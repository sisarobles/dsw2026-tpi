
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

            Assert.Contains("IX_Appointments_AvailabilitySlotId", indexes);
        }
    }
}
