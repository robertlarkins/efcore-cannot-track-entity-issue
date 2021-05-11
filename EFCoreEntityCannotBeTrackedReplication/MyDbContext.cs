using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EFCoreEntityCannotBeTrackedReplication
{
    public class MyDbContext : DbContext
    {
        private static readonly Type[] EnumerationTypes =
        {
            typeof(AppointmentStatus)
        };

        public MyDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public MyDbContext()
        {
        }

        public DbSet<Appointment> Appointments { get; set; } = null!;

        public DbSet<AppointmentStatus> AppointmentStatuses { get; set; } = null!;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var enumerationEntries = ChangeTracker.Entries()
                .Where(x => EnumerationTypes.Contains(x.Entity.GetType()));

            foreach (var enumerationEntry in enumerationEntries)
            {
                enumerationEntry.State = EntityState.Unchanged;
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Overrides DbContext OnModelCreating method.
        /// </summary>
        /// <param name="modelBuilder">modelBuilder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ApplyTableConfigurations(modelBuilder);
            SeedData(modelBuilder);
        }

        private void ApplyTableConfigurations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>().HasKey(k => k.Id);
            modelBuilder.Entity<Appointment>().HasOne(p => p.AppointmentStatus).WithMany();

            modelBuilder.Entity<AppointmentStatus>().HasKey(k => k.Id);
            modelBuilder.Entity<AppointmentStatus>().Property(p => p.Name);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppointmentStatus>().HasData(
                AppointmentStatus.Planned,
                AppointmentStatus.Occurring,
                AppointmentStatus.Finished);
        }
    }
}