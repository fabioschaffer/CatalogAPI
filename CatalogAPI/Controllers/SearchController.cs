using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Authorize]
public sealed class SearchController(IGameService gameService) : ControllerBase
{
    [HttpGet("/search")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length > 200)
            return BadRequest("Informe q com 1 a 200 caracteres para buscar pelo nome do jogo.");
        if (size is < 1 or > 100)
            return BadRequest("size deve estar entre 1 e 100.");

        return Ok(await gameService.SearchAsync(q, size, cancellationToken));
    }
}
