using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAvailabilityService
    {
        Task CreateAvailability(AvailabilityModel.Request request);
        Task UpdateAvailability(AvailabilityModel.Request request);
        Task<IEnumerable<AvailabilityModel.Response>> GetAvailabilitiesByDoctor(Guid doctorId);
        
    }
}
