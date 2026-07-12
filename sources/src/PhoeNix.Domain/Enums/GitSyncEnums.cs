namespace PhoeNix.Domain.Enums;

public enum GitSyncMode
{
    None = 0,
    PushOnly = 1,
    PullOnly = 2
}

public enum GitAuthMethod
{
    None = 0,
    SshKey = 1,
    Token = 2
}

public enum ValidationTier
{
    None = 0,
    Syntax = 1,
    Module = 2,
    Configuration = 3
}
