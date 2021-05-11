using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCoreEntityCannotBeTrackedReplication
{
    public class BaseTest : IDisposable
    {
        private readonly DbConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        protected BaseTest()
        {
            ContextOptions = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .Options;

            connection = RelationalOptionsExtension.Extract(ContextOptions).Connection;

            Seed();
        }

        protected DbContextOptions<MyDbContext> ContextOptions { get; }

        public void Dispose() => connection.Dispose();

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("datasource=:memory:");

            connection.Open();

            return connection;
        }

        private async void Seed()
        {
            using var context = new MyDbContext(ContextOptions);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.Database.Migrate();

            var myAppointment = new Appointment(AppointmentStatus.Planned);

            context.Appointments.Attach(myAppointment);
            await context.SaveChangesAsync();
        }
    }
}