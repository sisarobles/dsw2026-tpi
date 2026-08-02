using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;

namespace Dsw2026Tpi.Domain.Entities
{
    public enum SlotStatus
    {
        AVAILABLE = 1,
        BOOKED,
        BLOCKED
    }
    public class AvailabilitySlot : EntityBase
    {
        public Guid AvailabilityRuleId { get; set; }
        public AvailabilityRule? AvailabilityRule { get; private set; }
        public SlotStatus Status { get; private set; }
        public DateOnly SlotDate { get; private set; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }
        public bool Deleted { get; private set; }

        //Contructor para EF
        private AvailabilitySlot() { }
        public AvailabilitySlot(Guid availabilityRuleId, DateOnly slotDate, TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
        {
            AvailabilityRuleId = availabilityRuleId;
            Status = SlotStatus.AVAILABLE;
            SlotDate = slotDate;
            StartTime = startTime;
            EndTime = endTime;
            Deleted = false;
        }
    
        public void Book()
        {
            if (Status != SlotStatus.AVAILABLE)
                throw new ConflictException(ErrorCodes.AVAILABILITY_SLOT_NOT_AVAILABLE, nameof(ErrorCodes.AVAILABILITY_SLOT_NOT_AVAILABLE));
            Status = SlotStatus.BOOKED;
        }

        public void Release()
        {
            if (Status != SlotStatus.BOOKED)
                throw new ConflictException(ErrorCodes.AVAILABILITY_SLOT_NOT_BOOKED, nameof(ErrorCodes.AVAILABILITY_SLOT_NOT_BOOKED));
            Status = SlotStatus.AVAILABLE;
        }
    }
}
