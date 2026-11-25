
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class SqlServerDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<HelpDeskDbContext>
{
    public HelpDeskDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "AZURE_SQL_CONNECTION_STRING"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "AZURE_SQL_CONNECTION_STRING environment variable is missing."
            );
        }

        var migrationsAssembly =
            typeof(SqlServerDesignTimeDbContextFactory)
                .Assembly.GetName().Name!;

        var options =
            new DbContextOptionsBuilder<HelpDeskDbContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsAssembly(
                            migrationsAssembly
                        );
                    }
                )
                .Options;

        return new HelpDeskDbContext(options);
    }
}
