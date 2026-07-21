using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Machines.Commands;
using PhoeNix.Application.Machines.Queries;
using PhoeNix.Contracts.Machines;
using PhoeNix.Contracts.VmHosts;
using PhoeNix.Domain.Entities.Machines;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Machines;

public class MachinesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost<CreateMachineRequest>("/machines/create", CreateMachine)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/machines", GetMachines)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/machines/{machineId:guid}", GetMachineById)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/machines/{machineId:guid}/metrics", GetMachineMetrics)
            .Produces<MachineMetricsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPut<UpdateMachineRequest>("/machines/{machineId:guid}", UpdateMachine)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost<AssignManagementProfileRequest>("/machines/{machineId:guid}/management-profile",
                AssignManagementProfile)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapDelete("/machines/{machineId:guid}/management-profile", ClearManagementProfile)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost<PowerManageRequest>("/machines/{machineId:guid}/power", PowerManage)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost<CreateMachineVmRequest>("/machines/create-vm", CreateMachineVm)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapDelete("/machines/{machineId:guid}/vm", DeleteMachineVm)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private async Task<IResult> CreateMachine(
        CreateMachineRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateMachineCommand(
            request.Title,
            request.Enabled,
            request.MacAddress,
            request.Architecture,
            request.InstallDiskSelectionPreference);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private async Task<IResult> UpdateMachine(
        Guid machineId,
        UpdateMachineRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMachineCommand(
            new MachineId(machineId),
            request.Title,
            request.Enabled,
            request.MacAddress,
            request.Architecture,
            request.InstallDiskSelectionPreference);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private async Task<IResult> GetMachines(
        [AsParameters] ListMachinesRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new ListMachinesQuery(request);
        var response = await sender.Send(query, cancellationToken);
        return response.AsHttpResult();
    }

    private async Task<IResult> GetMachineById(
        Guid machineId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetMachineQuery(new MachineId(machineId));
        var response = await sender.Send(query, cancellationToken);
        return response.AsHttpResult();
    }

    private async Task<IResult> GetMachineMetrics(
        Guid machineId,
        string? range,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var timeRange = range switch
        {
            "1h" => MetricsTimeRange.OneHour,
            "6h" => MetricsTimeRange.SixHours,
            "7d" => MetricsTimeRange.SevenDays,
            "30d" => MetricsTimeRange.ThirtyDays,
            _ => MetricsTimeRange.TwentyFourHours
        };

        var query = new GetMachineMetricsQuery(new MachineId(machineId), timeRange);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.AsHttpResult();

        var m = result.Value!;
        var response = new MachineMetricsResponse(
            m.IsUp,
            m.Uptime,
            m.DiskSpaceUsed,
            ToSeriesResponse(m.Cpu),
            ToSeriesResponse(m.Ram),
            ToSeriesResponse(m.NetRx),
            ToSeriesResponse(m.NetTx),
            ToSeriesResponse(m.DiskRead),
            ToSeriesResponse(m.DiskWrite));

        return Results.Ok(response);
    }

    private static MetricSeriesResponse ToSeriesResponse(
        PhoeNix.Application.Abstractions.Monitoring.PrometheusRangeSeries series)
    {
        return new MetricSeriesResponse(series.Timestamps, series.Values);
    }

    private async Task<IResult> AssignManagementProfile(
        Guid machineId,
        AssignManagementProfileRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new AssignManagementProfileCommand(machineId, request.VmHostId, request.ExternalId);
        var result = await sender.Send(command, ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> ClearManagementProfile(
        Guid machineId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ClearManagementProfileCommand(machineId), ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> PowerManage(
        Guid machineId,
        PowerManageRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new PowerManageMachineCommand(machineId, request.Action);
        var result = await sender.Send(command, ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> CreateMachineVm(
        CreateMachineVmRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateMachineVmCommand(
            request.VmHostId, request.Name, request.CpuCores, request.MemoryMb,
            request.DiskSizeGb, request.NetworkBridge, request.Architecture,
            request.Enabled, request.InstallDiskSelectionPreference);

        var result = await sender.Send(command, ct);
        return result.AsHttpResult();
    }

    private async Task<IResult> DeleteMachineVm(
        Guid machineId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteMachineVmCommand(machineId), ct);
        return result.AsHttpResult();
    }
}