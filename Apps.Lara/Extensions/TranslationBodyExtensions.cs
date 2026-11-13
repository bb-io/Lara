namespace Apps.Lara.Extensions;

public static class TranslationBodyExtensions
{
    public static void AddTranslationMemories(this Dictionary<string, object> body, IEnumerable<string>? memoryIds)
    {
        var distinctMemoryIds = memoryIds?
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct()
            .ToArray() ?? [];

        if (distinctMemoryIds.Length > 0)
            body["adapt_to"] = distinctMemoryIds;
    }
}
