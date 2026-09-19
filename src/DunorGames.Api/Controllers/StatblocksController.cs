using DunorGames.Api;
using DunorGames.Api.Statblocks;
using DunorGames.Contracts.Statblocks;
using Microsoft.AspNetCore.Mvc;

namespace DunorGames.Api.Controllers;

[ApiController]
[Route("api/v1/statblocks")]
public sealed class StatblocksController(
    IStatblockStore store,
    IStatblockRequestValidator validator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CursorPageResponse<StatblockSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPageResponse<StatblockSummaryResponse>>> List(
        [FromQuery] StatblockSystemDto? system,
        [FromQuery] StatblockStatusDto? status,
        [FromQuery] string? tag,
        [FromQuery(Name = "query")] string? textQuery,
        [FromQuery] string? cursor,
        CancellationToken cancellationToken)
    {
        var page = await store.ListAsync(
            new StatblockListQuery(system, status, tag, textQuery, cursor),
            cancellationToken);
        var summaries = page.Items
            .Select(item => new StatblockSummaryResponse(
                item.Resource.Id,
                item.Resource.System,
                item.Resource.Identity.Name,
                item.Resource.Identity.Subtitle,
                item.Resource.Tags,
                item.Resource.Metadata.Status,
                item.Resource.Metadata.UpdatedAt))
            .ToArray();

        return Ok(new CursorPageResponse<StatblockSummaryResponse>(summaries, page.NextCursor));
    }

    [HttpGet("{statblockId:guid}")]
    [ProducesResponseType<StatblockResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatblockResponse>> Get(
        Guid statblockId,
        CancellationToken cancellationToken)
    {
        var statblock = await store.GetAsync(statblockId, cancellationToken);
        if (statblock is null)
        {
            return NotFoundProblem(statblockId);
        }

        Response.Headers.ETag = statblock.ETag;
        return Ok(statblock.Resource);
    }

    [HttpPost]
    [ProducesResponseType<StatblockResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<StatblockResponse>> Create(
        CreateStatblockRequest request,
        CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request);
        if (validation.HasRequestErrors)
        {
            return ApiProblemResponses.RequestValidation(
                HttpContext,
                validation.RequestErrors);
        }

        if (validation.HasSystemDataErrors)
        {
            return ApiProblemResponses.SystemDataValidation(
                HttpContext,
                validation.SystemDataErrors);
        }

        var statblock = await store.CreateAsync(request, cancellationToken);
        Response.Headers.ETag = statblock.ETag;
        return CreatedAtAction(
            nameof(Get),
            new { statblockId = statblock.Resource.Id },
            statblock.Resource);
    }

    [HttpPut("{statblockId:guid}")]
    [ProducesResponseType<StatblockResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status412PreconditionFailed)]
    public async Task<ActionResult<StatblockResponse>> Update(
        Guid statblockId,
        UpdateStatblockRequest request,
        CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request);
        if (validation.HasRequestErrors)
        {
            return ApiProblemResponses.RequestValidation(
                HttpContext,
                validation.RequestErrors);
        }

        if (validation.HasSystemDataErrors)
        {
            return ApiProblemResponses.SystemDataValidation(
                HttpContext,
                validation.SystemDataErrors);
        }

        var result = await store.UpdateAsync(
            statblockId,
            Request.Headers.IfMatch.FirstOrDefault(),
            request,
            cancellationToken);

        if (result.Kind == StatblockStoreUpdateResultKind.NotFound)
        {
            return NotFoundProblem(statblockId);
        }

        if (result.Kind == StatblockStoreUpdateResultKind.PreconditionFailed)
        {
            return PreconditionFailedProblem();
        }

        Response.Headers.ETag = result.Statblock!.ETag;
        return Ok(result.Statblock.Resource);
    }

    [HttpDelete("{statblockId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> Delete(
        Guid statblockId,
        CancellationToken cancellationToken)
    {
        return await store.DeleteAsync(
            statblockId,
            Request.Headers.IfMatch.FirstOrDefault(),
            cancellationToken) switch
        {
            StatblockStoreDeleteResult.Deleted => NoContent(),
            StatblockStoreDeleteResult.NotFound => NotFoundProblem(statblockId),
            _ => PreconditionFailedProblem()
        };
    }

    private ObjectResult NotFoundProblem(Guid statblockId) =>
        ApiProblemResponses.Problem(
            HttpContext,
            StatusCodes.Status404NotFound,
            ApiProblemTypes.StatblockNotFound,
            "Stats Block introuvable",
            $"Aucun Stats Block ne correspond à l’identifiant {statblockId:D}.");

    private ObjectResult PreconditionFailedProblem() =>
        ApiProblemResponses.Problem(
            HttpContext,
            StatusCodes.Status412PreconditionFailed,
            ApiProblemTypes.PreconditionFailed,
            "Version du Stats Block périmée",
            "Rechargez le Stats Block avant de réessayer la modification.");
}
