using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Extensions;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using static Dsw2026Tpi.Application.Dtos.AvailabilityModel;

namespace Dsw2026Tpi.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IPersistence _persistence;
        private readonly IFeriadoService _feriadoService;

        public AvailabilityService(IPersistence persistence, IFeriadoService feriadoService)
        {
            _persistence = persistence;
            _feriadoService = feriadoService;

        }
        public async Task CreateAvailability(AvailabilityModel.Request request)
        {
            //Verifico la existencia del doctor
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null)
                throw new EntityNotFoundException(nameof(Doctor));

            //Obtengo fecha actual
            var now = DateTime.UtcNow;
            await CreateRulesAndSlots(request, now.Month, now.Year,
            DateOnly.FromDateTime(now),
            DateTime.DaysInMonth(now.Year, now.Month));
        }

        public Task<IEnumerable<AvailabilityModel.Response>> GetAvailabilitiesByDni(Guid doctorId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAvailability(AvailabilityModel.Request request)
        {
            //Verifico la existencia del doctor
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null)
                throw new EntityNotFoundException(nameof(Doctor));

            //Obtengo fecha actual
            var now = DateTime.UtcNow;

            var reglasExistentes = await _persistence.GetFiltered<AvailabilityRule>(
               r => r.DoctorId == request.DoctorId &&
               r.Month == now.Month &&
               r.Year == now.Year &&
               !r.Deleted);

            foreach (var regla in reglasExistentes)
                await regla.DeleteIfNoBookedSlots(_persistence);

            await CreateRulesAndSlots(request,now.Month,now.Year,
              DateOnly.FromDateTime(now),
              DateTime.DaysInMonth(now.Year, now.Month));
        }

        private async Task CreateRulesAndSlots(AvailabilityModel.Request request,int month,int year, DateOnly today,int daysInMonth)
        {
            foreach (var dayRequest in request.Days)
            {
                dayRequest.ValidateTimeRange();
                await dayRequest.ValidateNoOverlap(_persistence, request.DoctorId, month, year);

                var rule = new AvailabilityRule(
                    request.DoctorId, month, year,
                    dayRequest.Day, dayRequest.StartTime, dayRequest.EndTime);
                await _persistence.Add(rule);

                var slots = dayRequest.GenerateSlots(rule.Id, today, daysInMonth, month, year, _feriadoService);
                foreach (var slot in slots)
                    await _persistence.Add(slot);
            }
        }
    }
}
