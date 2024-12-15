using Microsoft.EntityFrameworkCore;

namespace PhoeNix.Persistence.Configuration.Abstractions;

public interface IApplicationDbContextEntityTypeConfiguration;

public interface IApplicationDbContextEntityTypeConfiguration<T> : IApplicationDbContextEntityTypeConfiguration,
    IEntityTypeConfiguration<T> where T : class;