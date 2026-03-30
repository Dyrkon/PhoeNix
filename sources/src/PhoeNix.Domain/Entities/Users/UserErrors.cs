using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Users;

public static class UserErrors
{
    public static readonly Error NameRequired =
        new("UserNameRequired", "User name is required.");

    public static readonly Error NameLengthInvalid =
        new("UserNameLengthInvalid", "User name must be between 3 and 64 characters.");

    public static readonly Error PasswordRequired =
        new("UserPasswordRequired", "Password is required.");

    public static readonly Error PasswordTooShort =
        new("UserPasswordTooShort", "Password must be at least 8 characters long.");

    public static readonly Error NameAlreadyTaken =
        new("UserNameAlreadyTaken", "User name is already taken.");

    public static readonly Error InvalidCredentials =
        new("UserInvalidCredentials", "Invalid user name or password.");

    public static readonly Error UserNotFound =
        new("UserNotFound", "User was not found.");

    public static readonly Error Unauthenticated =
        new("UserUnauthenticated", "User is not authenticated.");
}