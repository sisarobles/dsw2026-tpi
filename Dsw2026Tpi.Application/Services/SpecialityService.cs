using Dsw2026Tpi.Application.Dtos;
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

        public async Task<SpecialityModel.Response> CreateAsync(SpecialityModel.Request request)
        {
            // 1. Validaciones 
            
            
            // 2. Mapeo de entrada: Convertimos el DTO en la entidad real de la base de datos
            var speciality = new Speciality(request.Name, request.Description ?? string.Empty);

            // 3. Guardamos usando el repositorio genérico
            await _persistence.Add(speciality); 

            // 4. Mapeo de salida: Devolvemos la respuesta segura
            return new SpecialityModel.Response
            (
               speciality.Id,
            speciality.Name,
            speciality.Description,
            speciality.IsActive
            );
        }

        public async Task<IEnumerable<SpecialityModel.Response>> GetAllAsync()
        {
            
            var specialities = await _persistence.GetAll<Speciality>();

            if (specialities == null)
            {
                return new List<SpecialityModel.Response>();
            }

            return specialities
             .Where(s => s.IsActive)
             .Select(s => new SpecialityModel.Response(
                 s.Id,
                 s.Name,
                 s.Description,
                 s.IsActive
             )).ToList();
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var speciality = await _persistence.GetById<Speciality>(id);
            if (speciality == null) return false;

            speciality.Deactivate();
            await _persistence.Update(speciality);
            return true;
        }

    }
}
