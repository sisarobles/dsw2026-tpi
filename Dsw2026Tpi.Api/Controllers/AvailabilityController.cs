using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("api/availabilities")]
    [ApiController]
    public class AvailabilityController : AppController
    {
        private IAvailabilityService _service;

        public AvailabilityController(IAvailabilityService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Policy = Policies.AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAvailability([FromBody] AvailabilityModel.Request request)
        {
            await _service.CreateAvailability(request);
            return Created(string.Empty, null);
        }

        [HttpPut]
        [Authorize(Policy = Policies.AdminPolicy)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> UpdateAvailability([FromBody] AvailabilityModel.Request request)
        {
            await _service.UpdateAvailability(request);
            return Ok();
        }

        [HttpGet("{doctorId}/slots")]
        [Authorize(Policy = Policies.PatientPolicy)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableSlots([FromRoute] Guid doctorId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1,
        [FromQuery] DateOnly? date = null)
        {
            var result = await _service.GetAvailableSlots(doctorId, pageSize, pageIndex, date);
            return Ok(result);
        }
    }
}
