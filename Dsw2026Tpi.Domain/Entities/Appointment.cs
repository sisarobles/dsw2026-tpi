using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;

namespace Dsw2026Tpi.Domain.Entities
{
    public enum Estado 
    {
        BOOKED = 1,
        CANCELLED,
        ATTENDED,
        NO_SHOW
    }
    public class Appointment : EntityBase
    {
        public Estado Estado { get; private set; }
        public string Reason { get; private set; }
        public byte[] RowVersion { get; private set; }
        public Guid PatientId { get; private set; }
        public Guid AvailabilitySlotId { get; private set; }

        //constructor para EF
        private Appointment() { }

        public Appointment(Guid pacienteId, Guid availabilitySlotId, string reason, Guid? id = null) : base(id)
        {
            Estado = Estado.BOOKED;
            PatientId = pacienteId;
            AvailabilitySlotId = availabilitySlotId;
            Reason = reason;
        }

        public void Cancel()
        {
            if (Estado != Estado.BOOKED)
                throw new BusinessRuleException(ErrorCodes.CANCEL_APPOINTMENT, nameof(ErrorCodes.CANCEL_APPOINTMENT));
            Estado = Estado.CANCELLED;
        }
    }
}
