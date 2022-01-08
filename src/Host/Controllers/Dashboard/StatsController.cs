using DN.WebApi.Application.Dashboard;
using DN.WebApi.Application.Wrapper;
using DN.WebApi.Shared.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace DN.WebApi.Host.Controllers.Dashboard;

public class StatsController : VersionedApiController
{
    private readonly IStatsService _service;

    public StatsController(IStatsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<Result<StatsDto>>> GetAsync()
    {
        var stats = await _service.GetDataAsync();
        return Ok(stats);
    }
}