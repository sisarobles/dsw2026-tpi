using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.Application.Extensions;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Logging;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests
{
    public class AvailabilityServiceSqliteTests
    {
        [Fact]
        public async Task GetAvailableSlots_ConSQLite_FiltraCorrectamenteLosTurnos()
        {
            // Arrange: crear una base SQLite real en memoria.
            await using var connection =
                new SqliteConnection("Data Source=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<Dsw2026TpiDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new Dsw2026TpiDbContext(options);

            await context.Database.EnsureCreatedAsync();

            // Obtener fecha y hora de Argentina.
            var now = ArgentinaTime.Now;
            var today = DateOnly.FromDateTime(now);
            var currentTime = TimeOnly.FromDateTime(now);
            var tomorrow = today.AddDays(1);

            // Crear doctor activo.
            var doctor = new Doctor(
                "Dr. Test",
                "MP-123",
                null);

            // Crear regla de disponibilidad.
            var rule = new AvailabilityRule(
                doctor.Id,
                now.Month,
                now.Year,
                today.DayOfWeek,
                new TimeOnly(0, 0),
                new TimeOnly(23, 59));

            // Turno de ayer: debe excluirse.
            var yesterdaySlot = new AvailabilitySlot(
                rule.Id,
                today.AddDays(-1),
                new TimeOnly(9, 0),
                new TimeOnly(9, 30));

            // Turno de hoy anterior a la hora actual.
            var pastTime = currentTime > TimeOnly.MinValue
                ? currentTime.AddMinutes(-1)
                : TimeOnly.MinValue;

            var pastTodaySlot = new AvailabilitySlot(
                rule.Id,
                today,
                pastTime,
                pastTime.AddMinutes(30));

            // Turno de hoy posterior a la hora actual.
            AvailabilitySlot? futureTodaySlot = null;

            if (currentTime < new TimeOnly(23, 29))
            {
                var futureTime = currentTime.AddMinutes(1);

                futureTodaySlot = new AvailabilitySlot(
                    rule.Id,
                    today,
                    futureTime,
                    futureTime.AddMinutes(30));
            }

            // Turnos disponibles de mañana.
            var tomorrowSlot1 = new AvailabilitySlot(
                rule.Id,
                tomorrow,
                new TimeOnly(9, 0),
                new TimeOnly(9, 30));

            var tomorrowSlot2 = new AvailabilitySlot(
                rule.Id,
                tomorrow,
                new TimeOnly(11, 0),
                new TimeOnly(11, 30));

            var tomorrowSlot3 = new AvailabilitySlot(
                rule.Id,
                tomorrow,
                new TimeOnly(12, 0),
                new TimeOnly(12, 30));

            var tomorrowSlot4 = new AvailabilitySlot(
                rule.Id,
                tomorrow,
                new TimeOnly(13, 0),
                new TimeOnly(13, 30));

            // Turno reservado de mañana: debe excluirse.
            var bookedSlot = new AvailabilitySlot(
                rule.Id,
                tomorrow,
                new TimeOnly(10, 0),
                new TimeOnly(10, 30));

            bookedSlot.Book();

            // Guardar los datos en SQLite.
            context.Add(doctor);
            context.Add(rule);

            context.AddRange(
                yesterdaySlot,
                pastTodaySlot,
                tomorrowSlot1,
                tomorrowSlot2,
                tomorrowSlot3,
                tomorrowSlot4,
                bookedSlot);

            if (futureTodaySlot != null)
            {
                context.Add(futureTodaySlot);
            }

            await context.SaveChangesAsync();

            // Utilizar PersistenceEf real.
            IPersistence persistence = new PersistenceEf(context);

            // Sustituir solamente los servicios externos.
            var feriadoService =
                Substitute.For<IFeriadoService>();

            var logger =
                Substitute.For<ILogService>();

            var service = new AvailabilityService(
                persistence,
                feriadoService,
                logger);

            // Act: consultar todos los turnos disponibles.
            var result = await service.GetAvailableSlots(
                doctor.Id,
                50,
                1);

            var returnedIds = result.Data
                .Select(slot => slot.Id)
                .ToList();

            // Assert: filtros por fecha, hora y estado.
            Assert.Contains(tomorrowSlot1.Id, returnedIds);
            Assert.Contains(tomorrowSlot2.Id, returnedIds);
            Assert.Contains(tomorrowSlot3.Id, returnedIds);
            Assert.Contains(tomorrowSlot4.Id, returnedIds);

            Assert.DoesNotContain(yesterdaySlot.Id, returnedIds);
            Assert.DoesNotContain(pastTodaySlot.Id, returnedIds);
            Assert.DoesNotContain(bookedSlot.Id, returnedIds);

            if (futureTodaySlot != null)
            {
                Assert.Contains(futureTodaySlot.Id, returnedIds);
            }

            var expectedTotal = futureTodaySlot == null ? 4 : 5;

            Assert.Equal(expectedTotal, result.Total);

            // Act: filtrar únicamente por la fecha de mañana.
            var resultByDate = await service.GetAvailableSlots(
                doctor.Id,
                50,
                1,
                tomorrow);

            var returnedByDate = resultByDate.Data.ToList();

            // Assert: todos los resultados corresponden a mañana.
            Assert.Equal(4, resultByDate.Total);
            Assert.Equal(4, returnedByDate.Count);

            Assert.All(returnedByDate, slot =>
                Assert.Equal(tomorrow, slot.SlotDate));

            Assert.Contains(
                returnedByDate,
                slot => slot.Id == tomorrowSlot1.Id);

            Assert.DoesNotContain(
                returnedByDate,
                slot => slot.Id == bookedSlot.Id);

            // Act: solicitar la primera página.
            var firstPage = await service.GetAvailableSlots(
                doctor.Id,
                2,
                1,
                tomorrow);

            // Act: solicitar la segunda página.
            var secondPage = await service.GetAvailableSlots(
                doctor.Id,
                2,
                2,
                tomorrow);

            var firstPageIds = firstPage.Data
                .Select(slot => slot.Id)
                .ToList();

            var secondPageIds = secondPage.Data
                .Select(slot => slot.Id)
                .ToList();

            // Assert: tamaño y total de las páginas.
            Assert.Equal(2, firstPage.Data.Count());
            Assert.Equal(2, secondPage.Data.Count());

            Assert.Equal(4, firstPage.Total);
            Assert.Equal(4, secondPage.Total);

            // PageIndex interno: comienza en cero.
            Assert.Equal(0, firstPage.PageIndex);
            Assert.Equal(1, secondPage.PageIndex);

            // Las páginas no deben repetir turnos.
            Assert.Empty(firstPageIds.Intersect(secondPageIds));

            // Entre ambas páginas deben aparecer los cuatro turnos.
            Assert.Equal(
                4,
                firstPageIds
                    .Union(secondPageIds)
                    .Count());
        }
    }
}
