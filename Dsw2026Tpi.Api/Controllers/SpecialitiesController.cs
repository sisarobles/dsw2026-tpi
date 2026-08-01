using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;


[Route("api/specialties")]
[Authorize(Policy = Policies.AdminPolicy)]
public class SpecialitiesController : AppController
{
    private readonly ISpecialityService _specialityService;

    public SpecialitiesController(ISpecialityService specialityService)
    {
        _specialityService = specialityService;
    }

    // Petición GET: Devuelve la lista de especialidades
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] string? name = null)
    {
        var specialities = await _specialityService.GetAllAsync(pageSize, pageIndex, name);
        return Ok(specialities);
    }

    // Petición POST: Crea una nueva especialidad
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SpecialityModel.Request request)
    {
        var result = await _specialityService.CreateAsync(request);
        return Created(string.Empty, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await _specialityService.DeactivateAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SpecialityModel.Request request)
    {
        var result = await _specialityService.UpdateAsync(id, request);
        return Ok(result);
    }
}