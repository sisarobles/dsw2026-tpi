using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos.Doctors
{
    public class DoctorResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid SpecialityId { get; set; }
        public string SpecialityName { get; set; } = string.Empty;
    }
}
