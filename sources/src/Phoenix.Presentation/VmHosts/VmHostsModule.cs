using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.VmHosts.Commands;
using PhoeNix.Application.VmHosts.Queries;
using PhoeNix.Contracts.VmHosts;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.VmHosts;

public class VmHostsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost<RegisterVmHostRequest>("/vm-hosts", RegisterVmHost)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/vm-hosts", ListVmHosts)
            .Produces(StatusCodes.Status200OK);

        app.MapGet("/vm-hosts/{vmHostId:guid}", GetVmHost)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPut<UpdateVmHostRequest>("/vm-hosts/{vmHostId:guid}", UpdateVmHost)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapDelete("/vm-hosts/{vmHostId:guid}", RemoveVmHost)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/vm-hosts/{vmHostId:guid}/sync", SyncResources)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/vm-hosts/{vmHostId:guid}/test-connection", TestConnection)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/vm-hosts/{vmHostId:guid}/discover", DiscoverVms)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> RegisterVmHost(
        RegisterVmHostRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new RegisterVmHostCommand(
            request.Name, request.Provider, request.Host,
            request.Port, request.Username, request.Secret, request.ExtraConfig);

        var result = await sender.Send(command, ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> ListVmHosts(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ListVmHostsQuery(), ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> GetVmHost(Guid vmHostId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetVmHostQuery(vmHostId), ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> UpdateVmHost(
        Guid vmHostId,
        UpdateVmHostRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateVmHostCommand(
            vmHostId, request.Name, request.Host,
            request.Port, request.Username, request.Secret, request.ExtraConfig);

        var result = await sender.Send(command, ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> RemoveVmHost(Guid vmHostId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RemoveVmHostCommand(vmHostId), ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> SyncResources(Guid vmHostId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new SyncVmHostResourcesCommand(vmHostId), ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> TestConnection(Guid vmHostId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new TestVmHostConnectionCommand(vmHostId), ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> DiscoverVms(Guid vmHostId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DiscoverVmsQuery(vmHostId), ct);
        return result.AsHttpResult();
    }
}
