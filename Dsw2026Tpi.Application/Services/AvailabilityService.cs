using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Extensions;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IPersistence _persistence;
        private readonly IFeriadoService _feriadoService;
        private readonly ILogger<AvailabilityService> _logger;

        public AvailabilityService(IPersistence persistence, IFeriadoService feriadoService, ILogger<AvailabilityService> logger)
        {
            _persistence = persistence;
            _feriadoService = feriadoService;
            _logger = logger;
        }
        public async Task CreateAvailability(AvailabilityModel.Request request)
        {
            //Verifico la existencia del doctor
            await GetActiveDoctorOrThrow(request.DoctorId);

            //Obtengo fecha actual
            var now = DateTime.UtcNow;
            var totalSlots = await CreateRulesAndSlots(request, now.Month, now.Year,
            DateOnly.FromDateTime(now),
            DateTime.DaysInMonth(now.Year, now.Month));

            _logger.LogInformation("Disponibilidad creada para doctor {DoctorId}: {DiasConfigurados} día(s), {TotalSlots} slot(s) generado(s) para {Mes}/{Anio}",
            request.DoctorId, request.Days.Count, totalSlots, now.Month, now.Year);
        }

        public async Task<IEnumerable<AvailabilityModel.Response>> GetAvailabilitiesByDoctor(Guid doctorId) 
        {
            await GetActiveDoctorOrThrow(doctorId);

            var now = DateTime.UtcNow;

            var rules = await _persistence.GetFiltered<AvailabilityRule>(
                r => r.DoctorId == doctorId &&
                r.Month == now.Month &&
                r.Year == now.Year &&
                !r.Deleted);

            return rules.Select(r => new AvailabilityModel.Response(
                r.Id,
                r.DayOfWeek.ToSpanish(),
                r.StartTime,
                r.EndTime
            ));
        }

        public async Task UpdateAvailability(AvailabilityModel.Request request)
        {
            await GetActiveDoctorOrThrow(request.DoctorId);

            var now = DateTime.UtcNow;
            var reglasBorradas = await DeleteExistingRules(request.DoctorId, now.Month, now.Year);

            var totalSlots = await CreateRulesAndSlots(
                request, now.Month, now.Year,
                DateOnly.FromDateTime(now),
                DateTime.DaysInMonth(now.Year, now.Month));

            _logger.LogInformation(
                "Disponibilidad actualizada para doctor {DoctorId}: {Reglas} reemplazada(s), {Slots} slot(s) para {Mes}/{Anio}",
                request.DoctorId, reglasBorradas, totalSlots, now.Month, now.Year);
        }

        private async Task<int> CreateRulesAndSlots(AvailabilityModel.Request request,int month,int year, DateOnly today,int daysInMonth)
        {
            var totalSlots = 0;

            foreach (var dayRequest in request.Days)
            {
                try
                {
                    await dayRequest.ValidateNoOverlap(_persistence, request.DoctorId, month, year);
                }
                catch (BusinessRuleException)
                {
                    _logger.LogWarning(
                        "Solapamiento de horarios al configurar disponibilidad: doctor {DoctorId}, día {Dia}, {Inicio}-{Fin}",
                        request.DoctorId, dayRequest.Day, dayRequest.StartTime, dayRequest.EndTime);
                    throw;
                }

                var rule = new AvailabilityRule(
                    request.DoctorId, month, year,
                    dayRequest.Day, dayRequest.StartTime, dayRequest.EndTime);

                var slots = dayRequest.GenerateSlots(rule.Id, today, daysInMonth, month, year, _feriadoService);

                if (!slots.Any())
                {
                    continue; 
                }

                await _persistence.Add(rule);

                foreach (var slot in slots)
                {
                    await _persistence.Add(slot);
                    totalSlots++;
                }
            }

            return totalSlots;
        }
        private async Task<Doctor> GetActiveDoctorOrThrow(Guid doctorId)
        {
            var doctor = await _persistence.GetById<Doctor>(doctorId);
            if (doctor == null || !doctor.IsActive)
                throw new EntityNotFoundException(nameof(Doctor));
            return doctor;
        }
        private async Task<int> DeleteExistingRules(Guid doctorId, int month, int year)
        {
            var reglasExistentes = await _persistence.GetFiltered<AvailabilityRule>(
                r => r.DoctorId == doctorId &&
                     r.Month == month &&
                     r.Year == year &&
                     !r.Deleted);

            var cantidad = reglasExistentes.Count();

            foreach (var regla in reglasExistentes)
            {
                try
                {
                    await regla.DeleteIfNoBookedSlots(_persistence);
                }
                catch (BusinessRuleException)
                {
                    _logger.LogWarning(
                        "No se pudo borrar regla {ReglaId} del doctor {DoctorId}: tiene turnos reservados",
                        regla.Id, doctorId);
                    throw;
                }
            }

            return cantidad;
        }

        public async Task<Pagination<AvailabilityModel.SlotResponse>> GetAvailableSlots(Guid doctorId,int pageSize,int pageIndex,DateOnly? date = null)
        {
            await GetActiveDoctorOrThrow(doctorId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return (await _persistence.Paginate<AvailabilitySlot, DateOnly>(
                pageSize,
                pageIndex,
                s => s.AvailabilityRule.DoctorId == doctorId &&
                     s.Status == SlotStatus.AVAILABLE &&
                     s.SlotDate >= today &&
                     !s.Deleted &&
                     (date == null || s.SlotDate == date),
                s => s.SlotDate,
                nameof(AvailabilitySlot.AvailabilityRule)))
            .Map(s => new AvailabilityModel.SlotResponse(
                s.Id,
                s.SlotDate,
                s.StartTime,
                s.EndTime));
        }
    }
    
}
