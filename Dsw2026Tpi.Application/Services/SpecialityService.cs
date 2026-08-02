using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Extensions;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

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
            request.Validate();

            var speciality = new Speciality(request.Name, request.Description);
            await _persistence.Add(speciality);

            return new SpecialityModel.Response(
                speciality.Id,
                speciality.Name,
                speciality.Description,
                speciality.IsActive
            );
        }

        public async Task<Pagination<SpecialityModel.Response>> GetAllAsync(int pageSize, int pageIndex, string? name = null)
        {
            var specialities = await _persistence.Paginate<Speciality, string>(pageSize, pageIndex, s => s.IsActive &&
                (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)), x => x.Name);

            return specialities.Map(s => new SpecialityModel.Response(
                s.Id,
                s.Name,
                s.Description,
                s.IsActive
            ));
        }

        public async Task<SpecialityModel.Response> UpdateAsync(Guid id, SpecialityModel.Request request)
        {
            var speciality = await _persistence.GetById<Speciality>(id)
                ?? throw new EntityNotFoundException(nameof(Speciality));

            request.Validate();

            speciality.Update(request.Name, request.Description);
            await _persistence.Update(speciality);

            return new SpecialityModel.Response(
                speciality.Id,
                speciality.Name,
                speciality.Description,
                speciality.IsActive
            );
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