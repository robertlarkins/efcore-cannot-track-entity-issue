using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace EFCoreEntityCannotBeTrackedReplication
{
    public class AppointmentStatus : Entity<int>
    {
        public static readonly AppointmentStatus Planned = new(1, "Planned");
        public static readonly AppointmentStatus Occurring = new(2, "Occurring");
        public static readonly AppointmentStatus Finished = new(3, "Finished");

        protected AppointmentStatus()
        {
        }

        private AppointmentStatus(int id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; } = string.Empty;

        public static List<AppointmentStatus> GetAll()
        {
            return new()
            {
                Planned,
                Occurring,
                Finished
            };
        }
    }
}