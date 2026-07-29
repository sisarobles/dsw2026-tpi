using Dsw2026Tpi.Application.Dtos.Specialities;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Services
{
    public class SpecialityService : ISpecialityService
    {
        private readonly IPersistence _persistence;
        public SpecialityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<SpecialityResponseDto> CreateAsync(CreateSpecialityDto dto)
        {
            // 1. Validaciones 
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 3 || dto.Name.Length > 100)
                throw new Exception("El nombre debe tener entre 3 y 100 caracteres.");

            if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Length < 10 || dto.Description.Length > 100)
                throw new Exception("La descripción es obligatoria y debe tener entre 10 y 100 caracteres.");
            
            // 2. Mapeo de entrada: Convertimos el DTO en la entidad real de la base de datos
            var speciality = new Speciality(dto.Name, dto.Description ?? string.Empty);

            // 3. Guardamos usando el repositorio genérico
            await _persistence.Add(speciality); 

            // 4. Mapeo de salida: Devolvemos la respuesta segura
            return new SpecialityResponseDto
            {
                Id = speciality.Id,
                Name = speciality.Name,
                Description = speciality.Description,
                IsAcive = speciality.IsActive
            };
        }

        public async Task<IEnumerable<SpecialityResponseDto>> GetAllAsync()
        {
            // Traemos todas las especialidades de la base de datos
            var specialities = await _persistence.GetAll<Speciality>();

            if (specialities == null)
            {
                return new List<SpecialityResponseDto>();
            }

            // Transformamos cada entidad en un DTO para devolverlo
            return specialities.Select(s => new SpecialityResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var speciality = await _persistence.GetByIdAsync<Speciality>(id);
            if (speciality == null) return false;

            speciality.Deactivate();
            await _persistence.UpdateAsync(speciality);
            return true;
        }

    }
}
