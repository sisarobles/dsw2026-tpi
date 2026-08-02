using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ISpecialityService
    {
        Task<Pagination<SpecialityModel.Response>> GetAllAsync(int pageSize, int pageIndex, string? name = null);
        Task<SpecialityModel.Response> CreateAsync(SpecialityModel.Request request);
        Task<bool> DeactivateAsync(Guid id);
        Task<SpecialityModel.Response> UpdateAsync(Guid id, SpecialityModel.Request request);
    }
}
