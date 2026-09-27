using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers
{
    [ApiController]
    public class ErrorSimulationController : ControllerBase
    {
        [HttpGet("test/error")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get()
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Erro simulado",
                detail: "Resposta 500 gerada intencionalmente para validar o monitoramento.");
        }
    }
}
