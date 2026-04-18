using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Behaviors;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.UnitTests.Behaviors;

public class UnitOfWorkBehaviorTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDbContextTransaction _transaction = Substitute.For<IDbContextTransaction>();

    public UnitOfWorkBehaviorTests()
    {
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(_transaction);
    }

    [Fact]
    public async Task Handle_Should_Commit_And_SaveChanges_On_Success()
    {
        var behavior = new UnitOfWorkBehavior<TestCommand, Result>(_unitOfWork);
        var response = Result.Success();

        var result = await behavior.Handle(
            new TestCommand(),
            ct => Task.FromResult(response),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Rollback_On_Failure_Result()
    {
        var behavior = new UnitOfWorkBehavior<TestCommand, Result>(_unitOfWork);
        var response = Result.Failure(new Error("Test.Error", "Something failed"));

        var result = await behavior.Handle(
            new TestCommand(),
            ct => Task.FromResult(response),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Rollback_And_Rethrow_On_Exception()
    {
        var behavior = new UnitOfWorkBehavior<TestCommand, Result>(_unitOfWork);

        var act = async () => await behavior.Handle(
            new TestCommand(),
            ct => throw new InvalidOperationException("Test exception"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Commit_Without_SaveChanges_For_SelfManaged_Command()
    {
        var behavior = new UnitOfWorkBehavior<SelfManagedCommand, Result>(_unitOfWork);
        var response = Result.Success();

        var result = await behavior.Handle(
            new SelfManagedCommand(),
            ct => Task.FromResult(response),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Commit_For_Non_Result_Response()
    {
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(_unitOfWork);

        var result = await behavior.Handle(
            new TestCommand(),
            ct => Task.FromResult("ok"),
            CancellationToken.None);

        result.Should().Be("ok");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    private sealed record TestCommand : ICommand;
    private sealed record SelfManagedCommand : ICommand, ISelfManagedUnitOfWorkCommand;
}
