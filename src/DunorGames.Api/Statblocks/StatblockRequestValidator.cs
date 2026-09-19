using System.Text.Json;
using System.Text.Json.Nodes;
using DunorGames.Contracts.Statblocks;

namespace DunorGames.Api.Statblocks;

public interface IStatblockRequestValidator
{
    StatblockRequestValidationResult Validate(CreateStatblockRequest request);

    StatblockRequestValidationResult Validate(UpdateStatblockRequest request);
}

public sealed record StatblockRequestValidationResult(
    IReadOnlyDictionary<string, string[]> RequestErrors,
    IReadOnlyDictionary<string, string[]> SystemDataErrors)
{
    public bool HasRequestErrors => RequestErrors.Count > 0;

    public bool HasSystemDataErrors => SystemDataErrors.Count > 0;
}

public sealed class StatblockRequestValidator : IStatblockRequestValidator
{
    private const string SupportedSchemaVersion = "1.0";
    private readonly SystemDataSchemaValidator systemDataValidator = new();

    public StatblockRequestValidationResult Validate(CreateStatblockRequest request) =>
        Validate(
            request.SchemaVersion,
            request.System,
            request.Identity,
            request.Tags,
            request.Source,
            request.SystemData,
            requireCompleteSystemData: false);

    public StatblockRequestValidationResult Validate(UpdateStatblockRequest request) =>
        Validate(
            request.SchemaVersion,
            request.System,
            request.Identity,
            request.Tags,
            request.Source,
            request.SystemData,
            requireCompleteSystemData: request.Status == StatblockStatusDto.Published);

    private StatblockRequestValidationResult Validate(
        string? schemaVersion,
        StatblockSystemDto system,
        StatblockIdentityDto? identity,
        IReadOnlyList<string>? tags,
        StatblockSourceDto? source,
        JsonObject? systemData,
        bool requireCompleteSystemData)
    {
        var requestErrors = new ValidationErrors();

        if (!string.Equals(schemaVersion, SupportedSchemaVersion, StringComparison.Ordinal))
        {
            requestErrors.Add(
                "schemaVersion",
                $"La version de schéma doit être {SupportedSchemaVersion}.");
        }

        ValidateIdentity(identity, requestErrors);
        ValidateTags(tags, requestErrors);
        ValidateSource(source, requestErrors);

        if (systemData is null)
        {
            requestErrors.Add("systemData", "Les données du système sont obligatoires.");
        }

        var systemErrors = systemData is null
            ? new ValidationErrors()
            : systemDataValidator.Validate(system, systemData, requireCompleteSystemData);

        return new StatblockRequestValidationResult(
            requestErrors.ToDictionary(),
            systemErrors.ToDictionary());
    }

    private static void ValidateIdentity(
        StatblockIdentityDto? identity,
        ValidationErrors errors)
    {
        if (identity is null)
        {
            errors.Add("identity", "L’identité du Stats Block est obligatoire.");
            return;
        }

        ValidateRequiredText(identity.Name, "identity.name", 200, errors);
        ValidateOptionalText(identity.Subtitle, "identity.subtitle", 200, errors);

        if (identity.Aliases is null)
        {
            return;
        }

        ValidateUniqueTextList(identity.Aliases, "identity.aliases", 200, errors);
    }

    private static void ValidateTags(
        IReadOnlyList<string>? tags,
        ValidationErrors errors)
    {
        if (tags is null)
        {
            errors.Add("tags", "La liste des tags est obligatoire, même si elle est vide.");
            return;
        }

        ValidateUniqueTextList(tags, "tags", 64, errors);
    }

    private static void ValidateSource(
        StatblockSourceDto? source,
        ValidationErrors errors)
    {
        if (source is null)
        {
            errors.Add("source", "La provenance du Stats Block est obligatoire.");
            return;
        }

        ValidateOptionalText(source.Reference, "source.reference", 1000, errors);
    }

    private static void ValidateRequiredText(
        string? value,
        string path,
        int maximumLength,
        ValidationErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(path, "Le champ est obligatoire.");
            return;
        }

        if (value.Length > maximumLength)
        {
            errors.Add(path, $"Le champ ne peut pas dépasser {maximumLength} caractères.");
        }
    }

    private static void ValidateOptionalText(
        string? value,
        string path,
        int maximumLength,
        ValidationErrors errors)
    {
        if (value is not null && value.Length > maximumLength)
        {
            errors.Add(path, $"Le champ ne peut pas dépasser {maximumLength} caractères.");
        }
    }

    private static void ValidateUniqueTextList(
        IReadOnlyList<string> values,
        string path,
        int maximumItemLength,
        ValidationErrors errors)
    {
        var uniqueValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            var itemPath = $"{path}[{index}]";
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(itemPath, "La valeur ne peut pas être vide.");
                continue;
            }

            if (value.Length > maximumItemLength)
            {
                errors.Add(
                    itemPath,
                    $"La valeur ne peut pas dépasser {maximumItemLength} caractères.");
            }

            if (!uniqueValues.Add(value.Trim()))
            {
                errors.Add(itemPath, "La valeur est déjà présente dans la liste.");
            }
        }
    }
}

internal sealed class SystemDataSchemaValidator
{
    private const string SchemaSuffix = "-system-data.schema.json";
    private readonly IReadOnlyDictionary<StatblockSystemDto, JsonObject> schemas;

    public SystemDataSchemaValidator()
    {
        var resources = typeof(SystemDataSchemaValidator).Assembly
            .GetManifestResourceNames();

        schemas = new Dictionary<StatblockSystemDto, JsonObject>
        {
            [StatblockSystemDto.DungeonsAndDragons] = LoadSchema(
                resources,
                "dnd-5-5" + SchemaSuffix),
            [StatblockSystemDto.Daggerheart] = LoadSchema(
                resources,
                "daggerheart" + SchemaSuffix),
            [StatblockSystemDto.DrawSteel] = LoadSchema(
                resources,
                "draw-steel" + SchemaSuffix),
            [StatblockSystemDto.Dc20] = LoadSchema(
                resources,
                "dc20" + SchemaSuffix)
        };
    }

    public ValidationErrors Validate(
        StatblockSystemDto system,
        JsonObject systemData,
        bool requireComplete)
    {
        var errors = new ValidationErrors();
        if (!schemas.TryGetValue(system, out var schema))
        {
            if (requireComplete)
            {
                errors.Add(
                    "systemData",
                    "Le schéma Tales of the Valiant n’est pas encore disponible; ce brouillon ne peut pas être publié.");
            }

            return errors;
        }

        new JsonSchemaSubsetEvaluator(schema, requireComplete)
            .Validate(systemData, "systemData", errors);
        return errors;
    }

    private static JsonObject LoadSchema(
        IEnumerable<string> resources,
        string fileName)
    {
        var resourceName = resources.Single(name =>
            name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
        using var stream = typeof(SystemDataSchemaValidator).Assembly
            .GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"La ressource de schéma {fileName} est introuvable.");
        return JsonNode.Parse(stream)?.AsObject()
            ?? throw new InvalidOperationException(
                $"La ressource de schéma {fileName} est invalide.");
    }
}

internal sealed class JsonSchemaSubsetEvaluator(
    JsonObject rootSchema,
    bool requireRequiredFields)
{
    public void Validate(
        JsonNode? instance,
        string path,
        ValidationErrors errors) =>
        ValidateNode(instance, rootSchema, path, errors);

    private void ValidateNode(
        JsonNode? instance,
        JsonObject schema,
        string path,
        ValidationErrors errors)
    {
        if (schema["$ref"] is JsonValue referenceValue &&
            referenceValue.TryGetValue<string>(out var reference))
        {
            schema = ResolveReference(reference);
        }

        var allowedTypes = ReadStrings(schema["type"]);
        if (allowedTypes.Count > 0 && !allowedTypes.Any(type => MatchesType(instance, type)))
        {
            errors.Add(
                path,
                $"La valeur doit être de type {string.Join(" ou ", allowedTypes.Select(FrenchType))}.");
            return;
        }

        if (schema["const"] is JsonNode constant && !JsonNode.DeepEquals(instance, constant))
        {
            errors.Add(path, $"La valeur doit être {constant.ToJsonString()}.");
        }

        if (schema["enum"] is JsonArray choices &&
            !choices.Any(choice => JsonNode.DeepEquals(instance, choice)))
        {
            errors.Add(path, "La valeur ne fait pas partie des choix permis.");
        }

        if (instance is JsonObject objectValue)
        {
            ValidateObject(objectValue, schema, path, errors);
        }
        else if (instance is JsonArray arrayValue)
        {
            ValidateArray(arrayValue, schema, path, errors);
        }
        else if (TryReadString(instance, out var stringValue))
        {
            ValidateString(stringValue, schema, path, errors);
        }
        else if (TryReadNumber(instance, out var numberValue))
        {
            ValidateNumber(numberValue, schema, path, errors);
        }
    }

    private void ValidateObject(
        JsonObject instance,
        JsonObject schema,
        string path,
        ValidationErrors errors)
    {
        var properties = schema["properties"] as JsonObject;

        if (requireRequiredFields && schema["required"] is JsonArray required)
        {
            foreach (var requiredNode in required)
            {
                var propertyName = requiredNode?.GetValue<string>();
                if (propertyName is not null &&
                    (!instance.TryGetPropertyValue(propertyName, out var value) || value is null))
                {
                    errors.Add(
                        ChildPath(path, propertyName),
                        "Le champ est obligatoire pour publier ce Stats Block.");
                }
            }
        }

        if (properties is not null)
        {
            foreach (var property in properties)
            {
                if (property.Value is JsonObject propertySchema &&
                    instance.TryGetPropertyValue(property.Key, out var propertyValue))
                {
                    ValidateNode(
                        propertyValue,
                        propertySchema,
                        ChildPath(path, property.Key),
                        errors);
                }
            }
        }

        if (schema["additionalProperties"] is JsonValue additionalProperties &&
            additionalProperties.TryGetValue<bool>(out var allowsAdditional) &&
            !allowsAdditional &&
            properties is not null)
        {
            foreach (var propertyName in instance.Select(property => property.Key))
            {
                if (!properties.ContainsKey(propertyName))
                {
                    errors.Add(
                        ChildPath(path, propertyName),
                        "Le champ n’est pas permis par le schéma du système.");
                }
            }
        }
    }

    private void ValidateArray(
        JsonArray instance,
        JsonObject schema,
        string path,
        ValidationErrors errors)
    {
        if (ReadInteger(schema["minItems"]) is int minimum && instance.Count < minimum)
        {
            errors.Add(path, $"La liste doit contenir au moins {minimum} élément(s).");
        }

        if (ReadInteger(schema["maxItems"]) is int maximum && instance.Count > maximum)
        {
            errors.Add(path, $"La liste ne peut pas contenir plus de {maximum} élément(s).");
        }

        if (schema["uniqueItems"]?.GetValue<bool>() == true)
        {
            for (var index = 0; index < instance.Count; index++)
            {
                if (instance.Take(index).Any(item => JsonNode.DeepEquals(item, instance[index])))
                {
                    errors.Add($"{path}[{index}]", "La valeur est déjà présente dans la liste.");
                }
            }
        }

        if (schema["items"] is JsonObject itemSchema)
        {
            for (var index = 0; index < instance.Count; index++)
            {
                ValidateNode(instance[index], itemSchema, $"{path}[{index}]", errors);
            }
        }
    }

    private static void ValidateString(
        string value,
        JsonObject schema,
        string path,
        ValidationErrors errors)
    {
        if (ReadInteger(schema["minLength"]) is int minimum && value.Length < minimum)
        {
            errors.Add(path, "La valeur ne peut pas être vide.");
        }
    }

    private static void ValidateNumber(
        decimal value,
        JsonObject schema,
        string path,
        ValidationErrors errors)
    {
        if (TryReadNumber(schema["minimum"], out var minimum) && value < minimum)
        {
            errors.Add(path, $"La valeur doit être supérieure ou égale à {minimum}.");
        }
    }

    private JsonObject ResolveReference(string reference)
    {
        const string prefix = "#/$defs/";
        if (!reference.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"La référence de schéma {reference} n’est pas supportée.");
        }

        var definitionName = reference[prefix.Length..];
        return rootSchema["$defs"]?[definitionName]?.AsObject()
            ?? throw new InvalidOperationException(
                $"La définition de schéma {reference} est introuvable.");
    }

    private static bool MatchesType(JsonNode? value, string type) => type switch
    {
        "null" => value is null,
        "object" => value is JsonObject,
        "array" => value is JsonArray,
        "string" => TryReadString(value, out _),
        "integer" => TryReadNumber(value, out var number) && decimal.Truncate(number) == number,
        "number" => TryReadNumber(value, out _),
        "boolean" => value is JsonValue booleanValue && booleanValue.TryGetValue<bool>(out _),
        _ => throw new InvalidOperationException($"Le type de schéma {type} n’est pas supporté.")
    };

    private static IReadOnlyList<string> ReadStrings(JsonNode? value)
    {
        if (value is JsonValue singleValue && singleValue.TryGetValue<string>(out var single))
        {
            return [single];
        }

        return value is JsonArray values
            ? values.Select(item => item?.GetValue<string>())
                .Where(item => item is not null)
                .Cast<string>()
                .ToArray()
            : [];
    }

    private static int? ReadInteger(JsonNode? value) =>
        value is JsonValue integerValue && integerValue.TryGetValue<int>(out var integer)
            ? integer
            : null;

    private static bool TryReadString(JsonNode? value, out string result)
    {
        if (value is JsonValue stringValue && stringValue.TryGetValue<string>(out var parsed))
        {
            result = parsed;
            return true;
        }

        result = string.Empty;
        return false;
    }

    private static bool TryReadNumber(JsonNode? value, out decimal result)
    {
        if (value is not JsonValue numberValue ||
            numberValue.GetValueKind() != JsonValueKind.Number)
        {
            result = default;
            return false;
        }

        return decimal.TryParse(
            value.ToJsonString(),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out result);
    }

    private static string ChildPath(string parent, string child) =>
        string.IsNullOrEmpty(parent) ? child : $"{parent}.{child}";

    private static string FrenchType(string type) => type switch
    {
        "null" => "nulle",
        "object" => "objet",
        "array" => "liste",
        "string" => "texte",
        "integer" => "nombre entier",
        "number" => "nombre",
        "boolean" => "booléen",
        _ => type
    };
}

internal sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> errors =
        new(StringComparer.Ordinal);

    public int Count => errors.Count;

    public void Add(string path, string message)
    {
        if (!errors.TryGetValue(path, out var messages))
        {
            messages = [];
            errors[path] = messages;
        }

        if (!messages.Contains(message, StringComparer.Ordinal))
        {
            messages.Add(message);
        }
    }

    public IReadOnlyDictionary<string, string[]> ToDictionary() =>
        errors.ToDictionary(
            error => error.Key,
            error => error.Value.ToArray(),
            StringComparer.Ordinal);
}
