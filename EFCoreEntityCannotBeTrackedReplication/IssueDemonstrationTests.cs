using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCoreEntityCannotBeTrackedReplication
{
    /// <summary>
    /// Links to info about issue:
    /// https://github.com/dotnet/efcore/issues/12459
    /// </summary>
    public class IssueDemonstrationTests : BaseTest
    {
        [Fact]
        public async Task Demonstrate_Issue()
        {
            await AddSingleAppointmentToDatabase(AppointmentStatus.Planned);

            await using var context = new MyDbContext(ContextOptions);

            // Pull all statuses into the context, this line is needed to demonstrate the issue.
            // This situation could occur when if multiple appointments are retrieved and they cover all the different statuses.
            var appointmentStatuses = context.AppointmentStatuses.ToList();
            var appointment = GetAppointment(context);

            appointment.SetStatus(AppointmentStatus.Occurring);

            await context.SaveChangesAsync();

            // The following does not get reached
            var appointmentAgain = GetAppointment(context);

            appointmentAgain.AppointmentStatus.Should().Be(AppointmentStatus.Occurring);
        }

        [Fact]
        public async Task Demonstrate_Issue_With_Multiple_Appointments()
        {
            await AddSingleAppointmentToDatabase(AppointmentStatus.Planned);
            await AddSingleAppointmentToDatabase(AppointmentStatus.Occurring);
            await AddSingleAppointmentToDatabase(AppointmentStatus.Finished);

            await using var context = new MyDbContext(ContextOptions);

            // Change all appointments to finished
            var appointments = context.Appointments.Include(x => x.AppointmentStatus).ToList();

            foreach (var appointment in appointments)
            {
                // Even adding this if statement does not avoid the issue
                if (appointment.AppointmentStatus == AppointmentStatus.Finished)
                {
                    continue;
                }

                appointment.SetStatus(AppointmentStatus.Finished);
            }

            await context.SaveChangesAsync();

            context.Dispose();

            // The following does not get reached
            await using var context2 = new MyDbContext(ContextOptions);

            var appointments2 = context2.Appointments.Include(x => x.AppointmentStatus).ToList();

            appointments2.Select(x => x.AppointmentStatus).Should().AllBeEquivalentTo(AppointmentStatus.Finished);
        }

        /// <summary>
        /// In our production code this approach does not work, unsure why.
        /// We are using NpgSql if that matters.
        /// </summary>
        [Fact]
        public async Task Demonstrate_Issue_Occurring_When_Using_Set_AsNoTracking()
        {
            await using var context = new MyDbContext(ContextOptions);

            context.Set<AppointmentStatus>().AsNoTracking();

            // Pull all statuses into the context, this line is needed to demonstrate the issue.
            var appointmentStatuses = context.AppointmentStatuses.ToList();

            var myAppointment = new Appointment(AppointmentStatus.Planned);

            context.Appointments.Attach(myAppointment);
            await context.SaveChangesAsync();

            var appointment = GetAppointment(context);

            appointment.SetStatus(AppointmentStatus.Occurring);

            await context.SaveChangesAsync();

            // The following does not get reached
            var appointmentAgain = GetAppointment(context);

            appointmentAgain.AppointmentStatus.Should().Be(AppointmentStatus.Occurring);
        }

        [Fact]
        public async Task Demonstrate_Issue_Occurring_When_AsNoTracking_On_Query()
        {
            await using var context = new MyDbContext(ContextOptions);

            // Pull all statuses into the context, this line is needed to demonstrate the issue.
            var appointmentStatuses = context.AppointmentStatuses.ToList();

            var myAppointment = new Appointment(AppointmentStatus.Planned);

            context.Appointments.Attach(myAppointment);
            await context.SaveChangesAsync();

            var appointment = GetAppointmentWithAsNoTracking(context);

            appointment.SetStatus(AppointmentStatus.Occurring);

            await context.SaveChangesAsync();

            // The following code is reached as none of the changes are tracked
            var appointmentAgain = GetAppointmentWithAsNoTracking(context);

            appointmentAgain.AppointmentStatus.Should().Be(AppointmentStatus.Occurring);
        }

        private async Task AddSingleAppointmentToDatabase(AppointmentStatus status)
        {
            await using var context = new MyDbContext(ContextOptions);
            var myAppointment = new Appointment(status);

            context.Appointments.Attach(myAppointment);
            await context.SaveChangesAsync();
            context.Dispose();
        }

        private Appointment GetAppointment(MyDbContext context)
        {
            return context.Appointments
                .Include(x => x.AppointmentStatus)
                .Single();
        }

        private Appointment GetAppointmentWithAsNoTracking(MyDbContext context)
        {
            return context.Appointments
                .Include(x => x.AppointmentStatus)
                .AsNoTracking()
                .Single();
        }
    }
}
