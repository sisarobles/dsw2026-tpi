using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAvailabilityService
    {
        Task<IEnumerable<AvailabilityModel.Response>> CreateAvailability(AvailabilityModel.Request request);
        Task<IEnumerable<AvailabilityModel.Response>> UpdateAvailability(AvailabilityModel.Request request);
        Task<IEnumerable<AvailabilityModel.Response>> GetAvailabilitiesByDoctor(Guid doctorId);
        Task<Pagination<AvailabilityModel.SlotResponse>> GetAvailableSlots(Guid doctorId, int pageSize,int pageIndex,DateOnly? date = null);
    }
}
