
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests
{
    public class AppointmentPersistenceTests
    {
        [Fact]
        public async Task Add_CuandoExisteOtraCitaActivaParaElSlot_LanzaConflictException()
        {
            await using var connection =
                new SqliteConnection("Data Source=:memory:");

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = OFF;";
            await command.ExecuteNonQueryAsync();

            var options = new DbContextOptionsBuilder<Dsw2026TpiDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new Dsw2026TpiDbContext(options);

            await context.Database.MigrateAsync();

            var persistence = new PersistenceEf(context);
            var slotId = Guid.NewGuid();

            var firstAppointment = new Appointment(
                Guid.NewGuid(),
                slotId,
                "Primera reserva");

            await persistence.Add(firstAppointment);

            var secondAppointment = new Appointment(
                Guid.NewGuid(),
                slotId,
                "Segunda reserva");

            var exception = await Assert.ThrowsAsync<ConflictException>(
                async () => await persistence.Add(secondAppointment));

            Assert.Equal(
                nameof(Dsw2026Tpi.CrossCutting.Resources.ErrorCodes.APPOINTMENT_CONFLICT),
                exception.Error.ErrorCode);
        }

        [Fact]
        public async Task Add_CuandoLaCitaAnteriorEstaCancelada_PermiteNuevaReserva()
        {
            await using var connection =
                new SqliteConnection("Data Source=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<Dsw2026TpiDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new Dsw2026TpiDbContext(options);

            await context.Database.ExecuteSqlRawAsync(
                "PRAGMA foreign_keys = OFF;");

            await context.Database.EnsureCreatedAsync();

            var persistence = new PersistenceEf(context);
            var slotId = Guid.NewGuid();

            var previousAppointment = new Appointment(
                Guid.NewGuid(),
                slotId,
                "Reserva anterior");

            await persistence.Add(previousAppointment);

            previousAppointment.Cancel();
            await persistence.Update(previousAppointment);

            var newAppointment = new Appointment(
                Guid.NewGuid(),
                slotId,
                "Nueva reserva");

            var result = await persistence.Add(newAppointment);

            Assert.Equal(newAppointment.Id, result.Id);
            Assert.Equal(Estado.CANCELLED, previousAppointment.Estado);
            Assert.Equal(Estado.BOOKED, newAppointment.Estado);
        }

        [Fact]
        public async Task Migrations_CreanIndiceUnicoParaTurnosActivos()
        {
            await using var connection =
                new SqliteConnection("Data Source=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<Dsw2026TpiDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new Dsw2026TpiDbContext(options);

            await context.Database.MigrateAsync();

            var indexes = await context.Database
                .SqlQueryRaw<string>(
                    """
                    SELECT name AS Value
                    FROM sqlite_master
                    WHERE type = 'index'
                      AND name = 'IX_Appointments_AvailabilitySlotId'
                      AND sql LIKE '%WHERE%'
                    """)
                .ToListAsync();

            Assert.Contains(
                "IX_Appointments_AvailabilitySlotId",
                indexes);
        }

        [Fact]
        public async Task TryBookAvailabilitySlot_CuandoDosOperacionesCompiten_SoloUnaReservaElTurno()
        {
            await using var connection =
                new SqliteConnection("Data Source=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<Dsw2026TpiDbContext>()
                .UseSqlite(connection)
                .Options;

            await using (var setupContext = new Dsw2026TpiDbContext(options))
            {
                await setupContext.Database.MigrateAsync();

                await setupContext.Database.ExecuteSqlRawAsync(
                    "PRAGMA foreign_keys = OFF;");

                var slot = new AvailabilitySlot(
                    Guid.NewGuid(),
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                    new TimeOnly(9, 0),
                    new TimeOnly(9, 30));

                setupContext.Set<AvailabilitySlot>().Add(slot);
                await setupContext.SaveChangesAsync();
            }

            var slots = await GetSlotIds(options);
            var slotId = Assert.Single(slots);

            await using var context1 = new Dsw2026TpiDbContext(options);
            await using var context2 = new Dsw2026TpiDbContext(options);

            var persistence1 = new PersistenceEf(context1);
            var persistence2 = new PersistenceEf(context2);

            var results = await Task.WhenAll(
                persistence1.TryBookAvailabilitySlot(slotId),
                persistence2.TryBookAvailabilitySlot(slotId));

            Assert.Equal(1, results.Count(result => result));
            Assert.Equal(1, results.Count(result => !result));

            await using var verificationContext =
                new Dsw2026TpiDbContext(options);

            var savedSlot = await verificationContext
                .Set<AvailabilitySlot>()
                .AsNoTracking()
                .SingleAsync(slot => slot.Id == slotId);

            Assert.Equal(SlotStatus.BOOKED, savedSlot.Status);
        }

        private static async Task<List<Guid>> GetSlotIds(
            DbContextOptions<Dsw2026TpiDbContext> options)
        {
            await using var context = new Dsw2026TpiDbContext(options);

            return await context.Set<AvailabilitySlot>()
                .Select(slot => slot.Id)
                .ToListAsync();
        }
    }
}
