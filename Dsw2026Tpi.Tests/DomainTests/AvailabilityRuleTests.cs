
using Dsw2026Tpi.Domain.Entities;
using Xunit;

namespace Dsw2026Tpi.Tests.DomainTests
{
    public class AvailabilityRuleTests
    {
        [Fact]
        public void GenerateSlots_CuandoHayFechasValidas_GeneraSlotsDeTreintaMinutos()
        {
            // Arrange
            var doctorId = Guid.NewGuid();

            var rule = new AvailabilityRule(
                doctorId,
                11,
                2026,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(11, 0));

            var dates = new List<DateOnly>
            {
                new(2026, 11, 2),
                new(2026, 11, 9)
            };

            // Act
            rule.GenerateSlots(dates);

            // Assert
            Assert.Equal(8, rule.Slots.Count);

            Assert.All(rule.Slots, slot =>
            {
                Assert.Equal(TimeSpan.FromMinutes(30), slot.EndTime - slot.StartTime);
                Assert.Equal(rule.Id, slot.AvailabilityRuleId);
                Assert.Equal(SlotStatus.AVAILABLE, slot.Status);
            });
        }

        [Fact]
        public void GenerateSlots_CuandoHayFechasDeOtrosDias_NoGeneraSlots()
        {
            // Arrange
            var rule = new AvailabilityRule(
                Guid.NewGuid(),
                11,
                2026,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(11, 0));

            var dates = new List<DateOnly>
            {
                new(2026, 11, 3),
                new(2026, 11, 4),
                new(2026, 11, 5)
            };

            // Act
            rule.GenerateSlots(dates);

            // Assert
            Assert.Empty(rule.Slots);
        }

        [Fact]
        public void GenerateSlots_CuandoNoHayFechas_NoGeneraSlots()
        {
            // Arrange
            var rule = new AvailabilityRule(
                Guid.NewGuid(),
                11,
                2026,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(11, 0));

            var dates = new List<DateOnly>();

            // Act
            rule.GenerateSlots(dates);

            // Assert
            Assert.Empty(rule.Slots);
        }

        [Fact]
        public void GenerateSlots_CuandoElHorarioNoCompletaUnBloque_NoExcedeHoraFin()
        {
            // Arrange
            var rule = new AvailabilityRule(
                Guid.NewGuid(),
                11,
                2026,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(10, 50));

            var dates = new List<DateOnly>
            {
                new(2026, 11, 2)
            };

            // Act
            rule.GenerateSlots(dates);

            // Assert
            Assert.Equal(3, rule.Slots.Count);
            Assert.Equal(new TimeOnly(10, 30), rule.Slots.Last().EndTime);
            Assert.All(rule.Slots, slot =>
                Assert.True(slot.EndTime <= rule.EndTime));
        }
    }
}
