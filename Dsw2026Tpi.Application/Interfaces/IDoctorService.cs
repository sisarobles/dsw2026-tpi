using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IDoctorService
{
    Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null);
    Task<DoctorModel.Response> CreateAsync(DoctorModel.Request request);
    Task<bool> DeactivateAsync(Guid id);
}
