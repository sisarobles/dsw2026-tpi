using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
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

        public AvailabilityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task CreateAvailability(AvailabilityModel.Request request)
        {
            //Verifico la existencia del doctor
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null)
                throw new EntityNotFoundException("Doctor");

            //Obtengo mes y año actual
            DateTime CurrentDate = DateTime.Now;

            int month = CurrentDate.Month;
            int year = CurrentDate.Year;

            // Cuento la cantidad de días del mes
            var daysInMonth = DateTime.DaysInMonth(year, month);

            //Recorro cada día configurado por el administrador
            foreach (var dayRequest in request.Days)  
            {
                var rule = new AvailabilityRule(
                    request.DoctorId,
                    month,
                    year,
                    dayRequest.Day,
                    dayRequest.StartTime,
                    dayRequest.EndTime
                );
                await _persistence.Add(rule);

                //Valido de que StartTime<EndTime
                if (dayRequest.StartTime >= dayRequest.EndTime)
                    throw new ValidationException("El horario de inicio debe ser menor al de fin",
                        "VALIDATION_ERROR");

                // buscar qué fechas del mes tienen ese día de la semana
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateOnly(year, month, day);

                    if (date.DayOfWeek == dayRequest.Day)
                    {
                        //Genero slots cada 30min
                        var StartTime = dayRequest.StartTime;

                        while (StartTime < dayRequest.EndTime)
                        {
                            var SlotEndTime = StartTime.AddMinutes(30);
                            var slot = new AvailabilitySlot(rule.Id, date, StartTime, SlotEndTime);
                            await _persistence.Add(slot);
                            StartTime = SlotEndTime;
                        }

                    }
                }
            }


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
                throw new EntityNotFoundException("Doctor");

            //Obtengo mes y año actual
            DateTime CurrentDate = DateTime.Now;

            int month = CurrentDate.Month;
            int year = CurrentDate.Year;

            // Cuento la cantidad de días del mes
            var daysInMonth = DateTime.DaysInMonth(year, month);

            //Borro reglas y slots ya existentes de ese doctor

            var reglasExistentes = await _persistence.GetFiltered<AvailabilityRule>(
                r => r.DoctorId == request.DoctorId &&
                r.Month == month &&
                r.Year == year &&
                !r.Deleted);

            foreach (var regla in reglasExistentes)
            {
                var slotsExistentes = await _persistence.GetFiltered<AvailabilitySlot>(
                    s => s.AvailabilityRuleId == regla.Id && !s.Deleted);
                
                foreach (var slot in slotsExistentes)
                {
                    await _persistence.Delete(slot);
                }

                await _persistence.Delete(regla);
            }
            //Recorro cada día configurado por el administrador
            foreach (var dayRequest in request.Days)
            {
                var rule = new AvailabilityRule(
                    request.DoctorId,
                    month,
                    year,
                    dayRequest.Day,
                    dayRequest.StartTime,
                    dayRequest.EndTime
                );
                await _persistence.Add(rule);

                //Valido de que StartTime<EndTime
                if (dayRequest.StartTime >= dayRequest.EndTime)
                    throw new ValidationException("El horario de inicio debe ser menor al de fin",
                        "VALIDATION_ERROR");

                // buscar qué fechas del mes tienen ese día de la semana
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateOnly(year, month, day);

                    if (date.DayOfWeek == dayRequest.Day)
                    {
                        //Genero slots cada 30min
                        var StartTime = dayRequest.StartTime;

                        while (StartTime < dayRequest.EndTime)
                        {
                            var SlotEndTime = StartTime.AddMinutes(30);
                            var slot = new AvailabilitySlot(rule.Id, date, StartTime, SlotEndTime);
                            await _persistence.Add(slot);
                            StartTime = SlotEndTime;
                        }

                    }
                }
            }

        }
    }
}

