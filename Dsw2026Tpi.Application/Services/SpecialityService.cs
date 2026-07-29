using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
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
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
                throw new ValidationException(ErrorCodes.APPOINTMENT_CONFLICT, nameof(ErrorCodes.APPOINTMENT_CONFLICT));

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
                throw new ValidationException(ErrorCodes.APPOINTMENT_CONFLICT, nameof(ErrorCodes.APPOINTMENT_CONFLICT));

            var speciality = new Speciality(request.Name, request.Description ?? string.Empty);

            await _persistence.Add(speciality); 

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
