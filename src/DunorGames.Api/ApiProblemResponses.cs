using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DunorGames.Api;

internal static class ApiProblemTypes
{
    public const string InvalidRequest = "/problems/invalid-request";
    public const string InvalidSystemData = "/problems/invalid-system-data";
    public const string StatblockNotFound = "/problems/statblock-not-found";
    public const string PreconditionFailed = "/problems/precondition-failed";
    public const string InternalError = "/problems/internal-error";
}

internal static class ApiProblemResponses
{
    public static IActionResult InvalidModelState(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => NormalizeModelStatePath(entry.Key),
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "La valeur fournie n’est pas valide."
                        : error.ErrorMessage)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

        return Validation(
            context.HttpContext,
            errors,
            StatusCodes.Status400BadRequest,
            ApiProblemTypes.InvalidRequest,
            "Requête invalide",
            "Le corps de la requête est incomplet ou contient une valeur JSON invalide.");
    }

    public static ObjectResult RequestValidation(
        HttpContext context,
        IReadOnlyDictionary<string, string[]> errors) =>
        Validation(
            context,
            errors,
            StatusCodes.Status400BadRequest,
            ApiProblemTypes.InvalidRequest,
            "Requête invalide",
            "Corrigez les champs communs indiqués avant de réessayer.");

    public static ObjectResult SystemDataValidation(
        HttpContext context,
        IReadOnlyDictionary<string, string[]> errors) =>
        Validation(
            context,
            errors,
            StatusCodes.Status422UnprocessableEntity,
            ApiProblemTypes.InvalidSystemData,
            "Données du système invalides",
            "Les données de jeu ne respectent pas le schéma du système sélectionné.");

    public static ObjectResult Problem(
        HttpContext context,
        int status,
        string type,
        string title,
        string detail)
    {
        var problem = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path
        };
        AddTraceId(problem, context);
        return ProblemResult(problem, status);
    }

    private static ObjectResult Validation(
        HttpContext context,
        IReadOnlyDictionary<string, string[]> errors,
        int status,
        string type,
        string title,
        string detail)
    {
        var problem = new ValidationProblemDetails(
            errors.ToDictionary(
                error => error.Key,
                error => error.Value,
                StringComparer.Ordinal))
        {
            Type = type,
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path
        };
        AddTraceId(problem, context);
        return ProblemResult(problem, status);
    }

    private static ObjectResult ProblemResult(ProblemDetails problem, int status)
    {
        var result = new ObjectResult(problem)
        {
            StatusCode = status
        };
        result.ContentTypes.Add("application/problem+json");
        return result;
    }

    private static void AddTraceId(ProblemDetails problem, HttpContext context) =>
        problem.Extensions["traceId"] =
            Activity.Current?.Id ?? context.TraceIdentifier;

    private static string NormalizeModelStatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "$")
        {
            return "body";
        }

        return path.StartsWith("$.", StringComparison.Ordinal)
            ? char.ToLowerInvariant(path[2]) + path[3..]
            : path;
    }
}
