using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

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

            var slots = slotsExistentes?.ToList()
                ?? new List<AvailabilitySlot>();

            if (slots.Any(s => s.Status == SlotStatus.BOOKED))
            {
                throw new BusinessRuleException(
                    ErrorCodes.AVAILABILITY_BOOKED_SLOTS,
                    nameof(ErrorCodes.AVAILABILITY_BOOKED_SLOTS));
            }

            foreach (var slot in slots)
            {
                slot.MarkAsDeleted();
                await persistence.Update(slot);
            }

            regla.MarkAsDeleted();
            await persistence.Update(regla);
        }
    }
}
