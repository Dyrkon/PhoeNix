using PhoeNix.Common.Models;

namespace PhoeNix.Application.Models.Setup;

public sealed record CallbackModuleParameters(
    string FinalizeUrl,
    string BearerToken);

public sealed record DeployAccessModuleParameters(
    string DeployUser,
    string DeployCaPublicKey);

public sealed record BuiltInModuleParameters(
    CallbackModuleParameters? Callback = null,
    DeployAccessModuleParameters? DeployAccess = null);

public sealed record SetupSessionsRequest(
    int Page = 1,
    int PageSize = 15,
    string? Search = null,
    SortDirection SortDirection = SortDirection.Descending);