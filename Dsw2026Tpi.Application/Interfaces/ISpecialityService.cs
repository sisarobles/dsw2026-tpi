using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ISpecialityService
    {
        Task<IEnumerable<SpecialityModel.Response>> GetAllAsync();

        Task<SpecialityModel.Response> CreateAsync(SpecialityModel.Request request);

        Task<bool> DeactivateAsync(Guid id);
        
    }
}
