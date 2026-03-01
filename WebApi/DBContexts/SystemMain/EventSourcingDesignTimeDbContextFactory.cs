using SystemMain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class EventSourcingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<EventSourcingDbContext>
{
    public EventSourcingDbContext CreateDbContext(string[] args)
    {
        var configuration = DBContextSettingsHelper.GetSettings();
        var builder = new DbContextOptionsBuilder<EventSourcingDbContext>();
        var connectionString = configuration.connectionStrings.EventStoreConnection;

        builder.UseNpgsql(connectionString, option =>
        {
            option.MigrationsHistoryTable("__EFMigrationsHistory", "Migrations");
        });

        return new EventSourcingDbContext(builder.Options);
    }
}


