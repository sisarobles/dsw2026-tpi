using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers
{
    [Route("availabilities")]
    [ApiController]
    [Authorize(Policy = Policies.AdminPolicy)]

    public class AvailabilityController : AppController
    {
        private IAvailabilityService _service;

        public AvailabilityController(IAvailabilityService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAvailability([FromBody] AvailabilityModel.Request request)
        {
            await _service.CreateAvailability(request);
            return Created(string.Empty, null);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> UpdateAvailability([FromBody] AvailabilityModel.Request request)
        {
            await _service.UpdateAvailability(request);
            return Ok();
        }

    }
}
