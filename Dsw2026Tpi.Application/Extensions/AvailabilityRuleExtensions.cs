using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Extensions
{
    public static class AvailabilityRuleExtensions
    {
        public static async Task DeleteIfNoBookedSlots(
            this AvailabilityRule regla,
            IPersistence persistence)
        {
            var slotsExistentes = await persistence.GetFiltered<AvailabilitySlot>(
                s => s.AvailabilityRuleId == regla.Id && !s.Deleted);

            if (slotsExistentes.Any(s => s.Status == SlotStatus.BOOKED))
                throw new BusinessRuleException(ErrorCodes.AVAILABILITY_BOOKED_SLOTS,nameof(ErrorCodes.AVAILABILITY_BOOKED_SLOTS));

            foreach (var slot in slotsExistentes)
                await persistence.Delete(slot);

            await persistence.Delete(regla);
        }
    }
}
