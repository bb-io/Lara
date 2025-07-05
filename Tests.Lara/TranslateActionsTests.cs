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

        var textRequest = new TranslateTextRequest
        {
            Text = "Hello, how are you?",
            Instructions = "Translate as much friendly as possible",
            TargetLanguage = "es",
            SourceLanguage = "en"
        };

        var response = await action.TranslateText(textRequest);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    [TestMethod]
    public async Task TranslateFile_IsSuccessful()
    {
        var action = new TranslateActions(InvocationContext, FileManager);

        var textRequest = new TranslateFileRequest
        {
            File = new FileReference { Name = "contentful.html" },
            //GlossaryFile = new FileReference { Name = "Original.tbx" },
            //Instructions = "Translate as much friendly as possible",
            TargetLanguage = "es-ES",
            //MemoryId= "mem_17PV2mXVbF6J69A2fjWZaq"
        };

        var response = await action.TranslateFile( textRequest);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    [TestMethod]
    public async Task TranslateFile_original_format_IsSuccessful()
    {
        var action = new TranslateActions(InvocationContext, FileManager);

        var textRequest = new TranslateFileRequest
        {
            File = new FileReference { Name = "contentful.html" },
            //GlossaryFile = new FileReference { Name = "Original.tbx" },
            //Instructions = "Translate as much friendly as possible",
            TargetLanguage = "es-ES",
            //MemoryId= "mem_17PV2mXVbF6J69A2fjWZaq",
            OutputFileHandling = "original",
        };

        var response = await action.TranslateFile(textRequest);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    [TestMethod]
    public async Task TranslateFile_native_api()
    {
        var action = new TranslateActions(InvocationContext, FileManager);

        var textRequest = new TranslateFileRequest
        {
            File = new FileReference { Name = "contentful.html", ContentType = "text/html" },
            //GlossaryFile = new FileReference { Name = "Original.tbx" },
            //Instructions = "Translate as much friendly as possible",
            TargetLanguage = "es-ES",
            //MemoryId= "mem_17PV2mXVbF6J69A2fjWZaq",
            OutputFileHandling = "original",
            FileTranslationStrategy = "lara"
        };

        var response = await action.TranslateFile(textRequest);

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }
}
