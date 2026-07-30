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
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null)
                throw new EntityNotFoundException(nameof(Doctor));

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
            var doctor = await _persistence.GetById<Doctor>(doctorId);
            if (doctor == null)
                throw new EntityNotFoundException(nameof(Doctor));

            var now = DateTime.UtcNow;

            var rules = await _persistence.GetFiltered<AvailabilityRule>(
                r => r.DoctorId == doctorId &&
                r.Month == now.Month &&
                r.Year == now.Year &&
                !r.Deleted);

            return rules.Select(r => new AvailabilityModel.Response(
                r.Id,
                r.DayOfWeek.ToString(),
                r.StartTime,
                r.EndTime
            ));
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

            var cantidadReglasBorradas = reglasExistentes.Count();

            foreach (var regla in reglasExistentes)
            {
                try
                {
                    await regla.DeleteIfNoBookedSlots(_persistence);
                }
                catch (BusinessRuleException)
                {
                    _logger.LogWarning(
                        "No se pudo actualizar disponibilidad del doctor {DoctorId}: la regla {ReglaId} tiene turnos reservados",
                        request.DoctorId, regla.Id);
                    throw; 
                }
            }

            var totalSlots = await CreateRulesAndSlots(request, now.Month, now.Year,
                DateOnly.FromDateTime(now),
                DateTime.DaysInMonth(now.Year, now.Month));

            _logger.LogInformation(
                "Disponibilidad actualizada para doctor {DoctorId}: {ReglasReemplazadas} regla(s) reemplazada(s), {TotalSlots} slot(s) nuevo(s) para {Mes}/{Anio}",
                request.DoctorId, cantidadReglasBorradas, totalSlots, now.Month, now.Year);
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
                await _persistence.Add(rule);

                var slots = dayRequest.GenerateSlots(rule.Id, today, daysInMonth, month, year, _feriadoService);
                foreach (var slot in slots)
                {
                    await _persistence.Add(slot);
                    totalSlots++;
                }
            }

            return totalSlots;
        }
    }
    
}
