using Apps.Lara.Actions;
using Apps.Lara.Model;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Lara.Base;

namespace Tests.Lara;

[TestClass]
public class TranslateActionsTests : TestBase
{
    [TestMethod]
    public async Task TranslateText_IsSuccessful()
    {
        var action = new TranslateActions(InvocationContext, FileManager);

        var languageRequest = new LanguageRequest
        {
            TargetLanguage = "es",
            SourceLanguage = "en"
        };

        var textRequest = new TranslateTextRequest
        {
            Text = "Hello, how are you?",
            ContentType = "text/plain",
            Instructions = ["Translate as much friendly as possible"],
        };

        var response = await action.TranslateText(languageRequest, textRequest);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    [TestMethod]
    public async Task TranslateFile_IsSuccessful()
    {
        var action = new TranslateActions(InvocationContext, FileManager);

        var languageRequest = new LanguageRequest
        {
            TargetLanguage = "es",
            SourceLanguage = "en"
        };

        var textRequest = new TranslateFileRequest
        {
            File = new FileReference { Name= "test.html" },
            Instructions = ["Translate as much friendly as possible"],
        };

        var response = await action.TranslateFile(languageRequest, textRequest);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }


    [TestMethod]
    public async Task TranslateFileBlacklake_IsSuccessful()
    {
        var action = new TranslateActions(InvocationContext, FileManager);

        var languageRequest = new LanguageRequest
        {
            TargetLanguage = "es",
            SourceLanguage = "en"
        };

        var textRequest = new TranslateFileRequest
        {
            File = new FileReference { Name = "test.html" },
            Instructions = ["Translate as much friendly as possible"],
        };

        var response = await action.TranslateFileBlacklake(languageRequest, textRequest);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    [TestMethod]
    public async Task CreateMemory_IsSuccessful()
    {
        var action = new TranslateActions(InvocationContext, FileManager);
        var response = await action.CreateMemory("New memory 2");

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task AddTranslationToMemory_IsSuccessful()
    {
        var action = new TranslateActions(InvocationContext, FileManager);

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
        var action = new TranslateActions(InvocationContext, FileManager);

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
        var action = new TranslateActions(InvocationContext, FileManager);

        var memory = new MemoryRequest { MemoryId = "mem_1Xg3fQLOiUjIfnlWKrTH2A" };
        var translation = new ImportMemoryRequest { File = new FileReference { Name = "test.tmx" } };

        var response = await action.ImportMemory(memory, translation);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }
}
