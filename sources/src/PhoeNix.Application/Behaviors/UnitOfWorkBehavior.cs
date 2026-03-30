using MediatR;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Repositories;
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
        var response = await next();

        if (response is Result { IsFailure: true })
            return response;

        if (request is ISelfManagedUnitOfWorkCommand)
            return response;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}