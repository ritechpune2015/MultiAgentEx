using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiAgentEx.Models;
using MultiAgentEx.Services;

namespace MultiAgentEx.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class MultiAgentController : ControllerBase
    {
        private readonly MultiAgentOrchestrator _orchestrator;

        public MultiAgentController(
            MultiAgentOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<MultiAgentResult>> Analyze(
            [FromBody] MultiAgentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _orchestrator.ExecuteAsync(
                request.Question,
                cancellationToken);

            return Ok(result);
        }
    }

}
