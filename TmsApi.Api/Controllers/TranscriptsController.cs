using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        [FromBody] object? request,
        CancellationToken ct)
    {
        // Temporary stub
        // Exercise 5 will replace this with:
        // 1. Create transcript job
        // 2. Put job into queue
        // 3. Return 202 Accepted + Location header

        await Task.CompletedTask;

        return Ok(new
        {
            message = "Transcript request received"
        });
    }
}