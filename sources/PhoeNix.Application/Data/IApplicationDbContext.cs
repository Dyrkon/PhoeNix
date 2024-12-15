using System.Data.Entity;
using Domain.Entities.Machine;

namespace PhoeNix.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Machine> Machines { get; }
}