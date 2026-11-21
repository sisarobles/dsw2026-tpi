using System.Text.Json;
using Dsw2026Tpi.Application.Dtos;
using Xunit;

namespace Dsw2026Tpi.Tests.ServicesTests
{
    public class AppointmentModelSerializationTests
    {
        [Theory]
        [InlineData("availabilityId")]
        [InlineData("availabilitySlotId")]
        public void Request_AceptaAmbosNombresParaElIdDeDisponibilidad(
            string propertyName)
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var availabilityId = Guid.NewGuid();

            var json = $$"""
            {
                "doctorId": "{{doctorId}}",
                "{{propertyName}}": "{{availabilityId}}",
                "patient": null,
                "reason": "Consulta"
            }
            """;

            var options = new JsonSerializerOptions(
                JsonSerializerDefaults.Web);

            // Act
            var request = JsonSerializer.Deserialize<AppointmentModel.Request>(
                json,
                options);

            // Assert
            Assert.NotNull(request);
            Assert.Equal(doctorId, request.DoctorId);
            Assert.Equal(
                availabilityId,
                request.EffectiveAvailabilitySlotId);
        }
    }
}
