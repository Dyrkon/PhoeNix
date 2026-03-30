namespace Phoenix.Presentation.Contracts;

public sealed record UserRegisterRequest(string Name, string Password);

public sealed record UserLoginRequest(string Name, string Password);