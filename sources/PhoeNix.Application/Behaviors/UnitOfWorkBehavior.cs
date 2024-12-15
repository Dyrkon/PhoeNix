using System.Transactions;
using Domain.Repositories;
using Domain.Shared;
using MediatR;
using PhoeNix.Application.Abstractions.Messaging;

namespace PhoeNix.Application.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var response = await next();

        if (response is Result { IsFailure: true })
        {
            // Do not commit transaction when result of command is failure
            return response;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        transactionScope.Complete();

        return response;
    }
}