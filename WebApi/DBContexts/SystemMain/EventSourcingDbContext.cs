using SystemMain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SystemMain
{
    public class EventSourcingDbContext : DbContext
    {
        public DbSet<EventSourcingEvent> EventSourcingEvents { get; set; }

        public EventSourcingDbContext(DbContextOptions<EventSourcingDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }

            modelBuilder.Entity<EventSourcingEvent>(entity =>
            {
                entity.ToTable("EventSourcingEvent");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AggregateIdentifier, e.Version })
                    .IsUnique()
                    .HasDatabaseName("UX_EventSourcingEvents_AggregateIdentifier_Version");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
                entity.Property(e => e.AggregateIdentifier).HasColumnName("aggregate_identifier");
                entity.Property(e => e.AggregateType).HasColumnName("aggregate_type");
                entity.Property(e => e.EventOperator).HasColumnName("event_operator");
                entity.Property(e => e.Version).HasColumnName("version");
                entity.Property(e => e.EventType).HasColumnName("event_type");
                entity.Property(e => e.EventData)
                    .HasColumnName("event_data")
                    .HasColumnType("jsonb");
            });
        }
    }
}


