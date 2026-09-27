using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogAPI.Controllers
{
    public class GameReviewController(IGameReviewService gameReviewService) : Controller
    {
        [Authorize]
        [HttpPost("game/{gameId}/avaliacoes")]
        public async Task<IActionResult> Create(int gameId, [FromBody] GameReviewRequest request)
        {
            if (request.Nota < 1 || request.Nota > 5)
                return BadRequest("A nota deve estar entre 1 e 5.");

            request.GameId = gameId;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            try
            {
                var review = await gameReviewService.CriarAvaliacaoAsync(userId, request);
                return Created("", review);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException?.Message}");
            }
        }

        [Authorize]
        [HttpGet("game/{gameId}/avaliacoes")]
        public async Task<IActionResult> GetByGame(int gameId)
        {
            try
            {
                var avaliacoes = await gameReviewService.ObterPorJogoAsync(gameId);
                return Ok(avaliacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException?.Message}");
            }
        }
    }
}
