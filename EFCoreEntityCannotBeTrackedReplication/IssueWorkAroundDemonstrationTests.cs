using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCoreEntityCannotBeTrackedReplication
{
    public class IssueWorkAroundDemonstrationTests : BaseTest
    {
        /// <summary>
        /// Demonstrates how to avoid the issue with multiple appointments by only getting back appointments that
        /// do not have an appointment status of finished.
        /// </summary>
        [Fact]
        public async Task Demonstrate_How_To_Avoid_Issue_With_Multiple_Appointments()
        {
            await AddSingleAppointmentToDatabase(AppointmentStatus.Planned);
            await AddSingleAppointmentToDatabase(AppointmentStatus.Occurring);
            await AddSingleAppointmentToDatabase(AppointmentStatus.Finished);

            await using var context = new MyDbContext(ContextOptions);

            // Change all appointments to finished
            var appointments = context.Appointments
                .Include(x => x.AppointmentStatus)
                .Where(x => x.AppointmentStatus != AppointmentStatus.Finished)
                .ToList();

            foreach (var appointment in appointments)
            {
                appointment.SetStatus(AppointmentStatus.Finished);
            }

            await context.SaveChangesAsync();

            context.Dispose();

            // The following does not get reached
            await using var context2 = new MyDbContext(ContextOptions);

            var appointments2 = context2.Appointments.Include(x => x.AppointmentStatus).ToList();

            appointments2.Select(x => x.AppointmentStatus).Should().AllBeEquivalentTo(AppointmentStatus.Finished);
        }

        private async Task AddSingleAppointmentToDatabase(AppointmentStatus status)
        {
            await using var context = new MyDbContext(ContextOptions);
            var myAppointment = new Appointment(status);

            context.Appointments.Attach(myAppointment);
            await context.SaveChangesAsync();
            context.Dispose();
        }
    }
}
