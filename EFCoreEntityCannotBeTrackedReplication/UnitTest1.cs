using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCoreEntityCannotBeTrackedReplication
{
    public class UnitTest1 : BaseTest
    {
        [Fact]
        public async Task Run_Database()
        {
            await using var context = new MyDbContext(ContextOptions);

            var appointmentStatuses = context.AppointmentStatuses.ToList();

            var appointment = GetAppointment(context);

            appointment.SetStatus(AppointmentStatus.Occurring);

            await context.SaveChangesAsync();

            var appointmentAgain = GetAppointment(context);
        }

        private Appointment GetAppointment(MyDbContext context)
        {
            return context.Appointments
                .Include(x => x.AppointmentStatus)
                .Single();
        }
    }
}

// https://stackoverflow.com/questions/44549030/sqlite-memory-database-eager-loading-entities
// .Set<SomeEntity>().AsNoTracking().ToList();