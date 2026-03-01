using SystemMain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class SystemMainDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SystemMainDbContext>
{
    public SystemMainDbContext CreateDbContext(string[] args)
    {
        var configuration = DBContextSettingsHelper.GetSettings();

        var builder = new DbContextOptionsBuilder<SystemMainDbContext>();

        var connectionString = configuration.connectionStrings.DefaultConnection;

        //builder.UseSqlServer(connectionString);
        builder.UseNpgsql(connectionString, option =>
        {
            option.MigrationsHistoryTable("__EFMigrationsHistory", "Migrations");
        });

        return new SystemMainDbContext(builder.Options);
    }
}

