using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace Dsw2026Tpi.Api.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentController : AppController
    {
        private readonly IAppointmentService _service;
        public AppointmentController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Policy = Policies.PatientPolicy)]
        [EnableRateLimiting("AppointmentPolicy")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentModel.Request request)
        {
            var result = await _service.CreateAppointment(request);
            return Created(string.Empty, result);
        }

        [HttpGet("patient")]
        [Authorize(Policy = Policies.PatientPolicy)]
        public async Task<IActionResult> GetAppointmentsByDni([FromQuery] long dni)
        {
            var result = await _service.GetAppointmentByDni(dni);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.PatientPolicy)]
        public async Task<IActionResult> DeleteAppointment([FromRoute] Guid id)
        {
            await _service.DeleteAppointment(id);
            return Ok("ok");
        }

        [HttpGet("search")]
        [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> GetAppointmentBySearch([FromQuery] Guid? specialtyId = null, [FromQuery] Guid? doctorId = null,
                                                                [FromQuery] long? dni = null, [FromQuery] DateOnly? date = null, 
                                                                [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1) {
            var request = new AppointmentSearchModel.Request(specialtyId, doctorId, dni, date);
            var result = await _service.GetAppointmentBySearch(request, pageSize, pageIndex);
            return Ok(result);
        }

        [HttpGet()]
        [Authorize(Policy = Policies.AdminPolicy)]
        public async Task<IActionResult> GetAppointmentsByDate([FromQuery] DateOnly date, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1)
        {
            var request = new AppointmentSearchModel.Request(null, null, null, date);
            var result = await _service.GetAppointmentBySearch(request, pageSize, pageIndex);
            return Ok(result);
        }
    }
}