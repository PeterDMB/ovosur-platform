using Microsoft.EntityFrameworkCore;

namespace OVOSUR.Api.Infrastructure.Persistence;

public sealed class OvosurDbContext(DbContextOptions<OvosurDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        base.OnModelCreating(modelBuilder);
    }
}
