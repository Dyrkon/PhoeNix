namespace PhoeNix.WebAPP.ApiClient.Contracts;

public sealed record UserLoginRequest(string Name, string Password);

public sealed record UserRegisterRequest(string Name, string Password);

public sealed record AuthenticatedUserResponse(Guid Id, string Name);

public sealed record ApiError(string Code, string Description);