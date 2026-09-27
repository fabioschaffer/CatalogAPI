using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogAPI.Controllers
{
    public class LibraryController(ILibrarySevice LibrarySevice) : Controller
    {
        [Authorize]
        [HttpPost("criar")]
        public async Task<IActionResult> Create([FromBody] LibraryRequest LibraryRequest)
        {
            var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (loggedUserId == null || !int.TryParse(loggedUserId, out var parsedId) || parsedId != LibraryRequest.IDUsuario)
                return Forbid();

            try
            {
                await LibrarySevice.CriarLibraryAsync(LibraryRequest);
                return Created("", null);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException?.Message}");
            }
        }

        [Authorize]
        [HttpGet("todos")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var retorno = await LibrarySevice.ObterTodosAsync();
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException?.Message}");
            }
        }

        [Authorize]
        [HttpGet("/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var retorno = await LibrarySevice.ObterPorIdAsync(id);

            if (retorno == null) return NotFound();

            return Ok(retorno);
        }

    }
}
