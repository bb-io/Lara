using Apps.Lara.Actions;
using Apps.Lara.Model;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Lara.Base;

namespace Tests.Lara;

[TestClass]
public class MemoryActionsTests : TestBase
{   

    [TestMethod]
    public async Task CreateMemory_IsSuccessful()
    {
        var action = new MemoryActions(InvocationContext, FileManager);
        var response = await action.CreateMemory("Ceci testing TM");

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task AddTranslationToMemory_IsSuccessful()
    {
        var action = new MemoryActions(InvocationContext, FileManager);

        var languageRequest = new LanguageRequest
        {
            TargetLanguage = "es",
            SourceLanguage = "en"
        };

        var memory = new MemoryRequest { MemoryId = "mem_1Xg3fQLOiUjIfnlWKrTH2A" };
        var translation = new AddTranslationRequest
        {
            Setnence = "Hello, how are you?",
            Translation = "Hola, ¿cómo estás?"
        };

        var response = await action.AddTranslationToMemory(memory, languageRequest, translation);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    [TestMethod]
    public async Task DeleteTranslationMemory_IsSuccessful()
    {
        var action = new MemoryActions(InvocationContext, FileManager);

        var languageRequest = new LanguageRequest
        {
            TargetLanguage = "es",
            SourceLanguage = "en"
        };

        var memory = new MemoryRequest { MemoryId = "mem_1Xg3fQLOiUjIfnlWKrTH2A" };
        var translation = new AddTranslationRequest
        {
            Setnence = "Hello, how are you?",
            Translation = "Hola, ¿cómo estás?",
            TranslationId = "00000000-0166-ed39-0100-000068000001"
        };

        var response = await action.DeleteTranslation(memory, languageRequest, translation);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }


    [TestMethod]
    public async Task ImportMemory_IsSuccessful()
    {
        var action = new MemoryActions(InvocationContext, FileManager);

        var memory = new MemoryRequest { MemoryId = "mem_3MrnFflmQIoZSskbNYUwap" };
        var translation = new ImportMemoryRequest { File = new FileReference { Name = "test.tmx" } };

        var response = await action.ImportMemory(memory, translation);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }
}
