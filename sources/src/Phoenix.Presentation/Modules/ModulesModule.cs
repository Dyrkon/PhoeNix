using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Modules.Commands;
using PhoeNix.Application.Modules.Queries;
using PhoeNix.Domain.Entities.Modules;
using Phoenix.Presentation.Contracts;
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

        group.MapPost(string.Empty, CreateModuleTemplate)
            .WithName("CreateModuleTemplate")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{moduleTemplateId:guid}", UpdateModuleTemplate)
            .WithName("UpdateModuleTemplate")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetModuleTemplates(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModuleTemplatesQuery(), cancellationToken);
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
            request.SupportedArchitectures, request.EditableValueTypes, request.Tests);
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
            request.Content, request.SupportedArchitectures, request.EditableValueTypes, request.Tests);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}