using Apps.Lara.Extensions;

namespace Tests.Lara;

[TestClass]
public class TranslationBodyExtensionsTests
{
    [TestMethod]
    public void AddTranslationMemories_MultipleIds_AddsAllIdsToAdaptTo()
    {
        var body = new Dictionary<string, object>();

        body.AddTranslationMemories(["mem_1", "mem_2"]);

        CollectionAssert.AreEqual(new[] { "mem_1", "mem_2" }, (string[])body["adapt_to"]);
    }

    [TestMethod]
    public void AddTranslationMemories_BlankAndDuplicateIds_AddsOnlyDistinctNonBlankIds()
    {
        var body = new Dictionary<string, object>();

        body.AddTranslationMemories(["mem_1", " ", "", "mem_1", "mem_2"]);

        CollectionAssert.AreEqual(new[] { "mem_1", "mem_2" }, (string[])body["adapt_to"]);
    }

    [TestMethod]
    public void AddTranslationMemories_NoIds_DoesNotAddAdaptTo()
    {
        var body = new Dictionary<string, object>();

        body.AddTranslationMemories(null);
        body.AddTranslationMemories([]);
        body.AddTranslationMemories([" "]);

        Assert.IsFalse(body.ContainsKey("adapt_to"));
    }
}
