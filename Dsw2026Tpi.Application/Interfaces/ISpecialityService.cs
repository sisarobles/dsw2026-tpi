using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dsw2026Tpi.Application.Dtos.Specialities;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ISpecialityService
    {
        Task<IEnumerable<SpecialityResponseDto>> GetAllAsync();

        Task<SpecialityResponseDto> CreateAsync(CreateSpecialityDto dto);

        Task<bool> DeactivateAsync(Guid id);
        
    }
}
