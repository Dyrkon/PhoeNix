using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Modules.Commands;
using PhoeNix.Application.Modules.Queries;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Entities.Modules;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Modules;

public sealed class ModulesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/modules");

        group.MapGet(string.Empty, GetModuleTemplates)
            .WithName("GetModuleTemplates")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{moduleTemplateId:guid}", GetModuleTemplateById)
            .WithName("GetModuleTemplateById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/templates/new", CreateModuleTemplate)
            .WithName("CreateModuleTemplate")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{moduleTemplateId:guid}", UpdateModuleTemplate)
            .WithName("UpdateModuleTemplate")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{moduleTemplateId:guid}/scaffolding", GetModuleScaffolding)
            .WithName("GetModuleScaffolding")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/scaffolding/preview", GetScaffoldingPreview)
            .WithName("GetScaffoldingPreview")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetModuleTemplates(
        [AsParameters] ListModuleTemplatesRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListModuleTemplatesQuery(request), cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> GetModuleTemplateById(
        Guid moduleTemplateId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModuleTemplateByIdQuery(new ModuleTemplateId(moduleTemplateId)),
            cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> CreateModuleTemplate(
        CreateModuleTemplateRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateModuleTemplateCommand(request.Name, request.Enabled, request.Type, request.Content,
            request.SupportedArchitectures, request.EditableValueTypes, request.Tests, request.RequiredInputs);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> UpdateModuleTemplate(
        Guid moduleTemplateId,
        UpdateModuleTemplateRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateModuleTemplateCommand(new ModuleTemplateId(moduleTemplateId), request.Name,
            request.Enabled, request.Type,
            request.Content, request.SupportedArchitectures, request.EditableValueTypes, request.Tests,
            request.RequiredInputs);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> GetModuleScaffolding(
        Guid moduleTemplateId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModuleScaffoldingQuery(new ModuleTemplateId(moduleTemplateId)),
            cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> GetScaffoldingPreview(
        [AsParameters] GetScaffoldingPreviewRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var testNames = string.IsNullOrWhiteSpace(request.TestNames)
            ? []
            : request.TestNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var result = await sender.Send(new GetScaffoldingPreviewQuery(request.Type, testNames), cancellationToken);
        return result.AsHttpResult();
    }
}