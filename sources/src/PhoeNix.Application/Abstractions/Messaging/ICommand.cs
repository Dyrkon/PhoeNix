using System.Windows.Input;
using MediatR;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResult> : IRequest<Result<TResult>>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result> where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}