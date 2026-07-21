using Dsw2026Tpi.Application.Dtos.Specialities;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class SpecialitiesController : ControllerBase
{
    private readonly ISpecialityService _specialityService;

    public SpecialitiesController(ISpecialityService specialityService)
    {
        _specialityService = specialityService;
    }

    // Petición GET: Devuelve la lista de especialidades
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var specialities = await _specialityService.GetAllAsync();
        return Ok(specialities);
    }

    // Petición POST: Crea una nueva especialidad
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSpecialityDto dto)
    {
        var result = await _specialityService.CreateAsync(dto);
        return Ok(result);
    }
}