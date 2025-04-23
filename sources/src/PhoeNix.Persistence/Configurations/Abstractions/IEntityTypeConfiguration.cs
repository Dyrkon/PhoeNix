using Microsoft.EntityFrameworkCore;

namespace PhoeNix.Persistence.Configurations.Abstractions;

public interface IApplicationEntityTypeConfiguration;

public interface IApplicationEntityTypeConfiguration<T> : IApplicationEntityTypeConfiguration,
    IEntityTypeConfiguration<T> where T : class;