using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class AvailabilityRule : EntityBase
    {
        public Guid DoctorId { get; set; }
        public Doctor? Doctor { get; private set; }
        public int Month { get; private set; }
        public int Year { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }
        public bool Deleted { get; private set; }
        public ICollection<AvailabilitySlot> Slots { get; private set; }

        //Constructor para EF 
        private AvailabilityRule() { }

       public AvailabilityRule(Guid doctorId, int month, int year, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime,  Guid? id = null) : base(id)
        {
            DoctorId = doctorId;
            Month = month;
            Year = year;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            Deleted = false;
            Slots = new List<AvailabilitySlot>();
        }
    }
}
