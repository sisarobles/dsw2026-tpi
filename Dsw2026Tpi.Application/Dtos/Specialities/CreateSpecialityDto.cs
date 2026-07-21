using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos.Specialities
{
    public class CreateSpecialityDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
