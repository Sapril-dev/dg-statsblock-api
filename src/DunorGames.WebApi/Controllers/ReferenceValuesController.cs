using DunorGames.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DunorGames.WebApi.Controllers;

[ApiController]
[Route("api/v1/reference-values")]
public sealed class ReferenceValuesController(DunorGamesDbContext database) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ReferenceValueResponse[]>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ReferenceValueResponse[]>> List(
        [FromQuery] string system,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(system))
        {
            return BadRequest("Le système est obligatoire.");
        }

        var values = await database.ReferenceValues
            .AsNoTracking()
            .Where(value => value.SystemCode == system && value.IsActive)
            .OrderBy(value => value.Category)
            .ThenBy(value => value.SortOrder)
            .Select(value => new ReferenceValueResponse(
                value.Category,
                value.Code,
                value.DisplayName))
            .ToArrayAsync(cancellationToken);

        return Ok(values);
    }
}

public sealed record ReferenceValueResponse(string Category, string Code, string DisplayName);
