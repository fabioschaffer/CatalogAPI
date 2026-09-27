using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers
{
    public class GameController(IGameService gameService) : Controller
    {
        [Authorize(Roles = "Administrador")]
        [HttpPost("game/criar")]
        public async Task<IActionResult> Create([FromBody] GameRequest GameRequest)
        {
            if (GameRequest.Price < 0)
                return BadRequest("O preço do jogo não pode ser negativo.");

            try
            {
                var game = await gameService.CriarGameAsync(GameRequest);
                return Created("", game);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException?.Message}");
            }
        }

        [Authorize]
        [HttpGet("game/todos")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var retorno = await gameService.ObterTodosAsync();
                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException?.Message}");
            }
        }

        [Authorize]
        [HttpGet("game/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var retorno = await gameService.ObterPorIdAsync(id);

            if (retorno == null) return NotFound();

            return Ok(retorno);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("game/atualizar")]
        public async Task<IActionResult> Update([FromBody] GameUpdateRequest usuario)
        {
            await gameService.AtualizarAsync(usuario);
            return NoContent();
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("game/deletar/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await gameService.DeletarAsync(id);
            return NoContent();
        }
    }
}
