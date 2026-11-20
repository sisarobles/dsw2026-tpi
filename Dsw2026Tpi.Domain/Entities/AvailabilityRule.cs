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
            = new List<AvailabilitySlot>();

        private AvailabilityRule()
        {
            Slots = new List<AvailabilitySlot>();
        }

        public AvailabilityRule(
            Guid doctorId,
            int month,
            int year,
            DayOfWeek dayOfWeek,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? id = null) : base(id)
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

        public void MarkAsDeleted()
        {
            Deleted = true;
        }

        public void GenerateSlots(IEnumerable<DateOnly> dates)
        {
            foreach (var date in dates)
            {
                if (date.DayOfWeek != DayOfWeek)
                    continue;

                var current = StartTime;

                while (current.AddMinutes(30) <= EndTime)
                {
                    var slotEnd = current.AddMinutes(30);

                    Slots.Add(new AvailabilitySlot(
                        Id,
                        date,
                        current,
                        slotEnd));

                    current = slotEnd;
                }
            }
        }
    }
}
