using MediatR;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        TResponse response;
        try
        {
            response = await next();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        if (response is Result { IsFailure: true })
        {
            await transaction.RollbackAsync(cancellationToken);
            return response;
        }

        if (request is ISelfManagedUnitOfWorkCommand)
        {
            await transaction.CommitAsync(cancellationToken);
            return response;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return response;
    }
}