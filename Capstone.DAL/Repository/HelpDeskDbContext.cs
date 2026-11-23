
using HelpDesk.DAL.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class HelpDeskDbContext : DbContext
{
    public HelpDeskDbContext(
        DbContextOptions<HelpDeskDbContext> options)
        : base(options)
    {
    }

    public class HelpDeskDbContextFactory
        : IDesignTimeDbContextFactory<HelpDeskDbContext>
    {
        public HelpDeskDbContext CreateDbContext(string[] args)
        {
            var options =
                new DbContextOptionsBuilder<HelpDeskDbContext>()
                .UseSqlite(
                    "Data Source=..\\Capstone.DAL\\Database\\ServiceDeskDB.db"
                )


                .Options;

            return new HelpDeskDbContext(options);
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Service request indexes
        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(r => r.RaisedBy);

        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(r => r.ReqStatus);

        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(r => r.RaisedOn);

        // Audit log indexes
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.RequestId);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.PerformedAt);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.Action);
    }


    public DbSet<Role> Roles { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<ServiceRequest> ServiceRequests { get; set; }

    public DbSet<Status> Statuses { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }
}
