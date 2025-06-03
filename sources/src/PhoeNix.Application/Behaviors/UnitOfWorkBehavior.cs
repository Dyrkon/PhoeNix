using System.Transactions;
using MediatR;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
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
        // TODO transaction scope doesn't work is SQLite
        // using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var response = await next();

        if (response is Result { IsFailure: true })
            // Do not commit transaction when result of command is failure
            return response;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // transactionScope.Complete();

        return response;
    }
}