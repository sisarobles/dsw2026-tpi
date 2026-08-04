using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/doctors")]
[Authorize(Policy = Policies.AdminPolicy)]
public class DoctorController : AppController
{
    private readonly IDoctorService _service;
    private readonly IAvailabilityService _availabilityService;

    public DoctorController(IDoctorService service, IAvailabilityService availabilityService)
    {
        _service = service;
        _availabilityService = availabilityService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery]int pageSize = 10, [FromQuery]int pageIndex = 1, [FromQuery]string? name = null)
    {
        var doctors = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(doctors);
    }

    [HttpGet("{id}/availabilities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailabilities(
    [FromRoute] Guid id)
    {
        var result = await _availabilityService.GetAvailabilitiesByDoctor(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DoctorModel.Request request)
    {
        var result = await _service.CreateAsync(request);
        return Created(string.Empty, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await _service.DeactivateAsync(id);

        if (!success)
        {
            return NotFound(); 
        }

        return Ok("ok"); 
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] DoctorModel.Request request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(result);
    }
}
