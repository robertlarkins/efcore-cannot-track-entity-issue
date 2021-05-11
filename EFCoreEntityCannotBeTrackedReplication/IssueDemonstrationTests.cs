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
            await using var context = new MyDbContext(ContextOptions);

            var myAppointment = new Appointment(AppointmentStatus.Planned);

            context.Appointments.Attach(myAppointment);
            await context.SaveChangesAsync();

            var appointment = GetAppointment(context);

            appointment.SetStatus(AppointmentStatus.Occurring);

            await context.SaveChangesAsync();

            // The following does not get reached
            var appointmentAgain = GetAppointment(context);

            // appointmentAgain.AppointmentStatus.Should().Be(AppointmentStatus.Occurring);
        }

        /// <summary>
        /// In our production code this approach does not work, unsure why.
        /// We are using NpgSql if that matters.
        /// </summary>
        [Fact]
        public async Task Demonstrate_Issue_Resolved_Using_Set_AsNoTracking()
        {
            await using var context = new MyDbContext(ContextOptions);

            context.Set<AppointmentStatus>().AsNoTracking();

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
        public async Task Demonstrate_AsNoTracking_Does_Not_Save_Change()
        {
            await using var context = new MyDbContext(ContextOptions);

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
