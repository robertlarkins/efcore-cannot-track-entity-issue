using CSharpFunctionalExtensions;

namespace EFCoreEntityCannotBeTrackedReplication
{
    public class Appointment : Entity<int>
    {
        public Appointment(AppointmentStatus appointmentStatus)
        {
            AppointmentStatus = appointmentStatus;
        }

        protected Appointment()
        {
        }

        public AppointmentStatus AppointmentStatus { get; private set; } = null!;

        public void SetStatus(AppointmentStatus appointmentStatus)
        {
            AppointmentStatus = appointmentStatus;
        }
    }
}