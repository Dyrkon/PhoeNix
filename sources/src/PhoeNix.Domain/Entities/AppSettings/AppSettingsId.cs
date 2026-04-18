using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.AppSettings;

public sealed record AppSettingsId(Guid Value) : StronglyTypedId(Value, "appsettings");
