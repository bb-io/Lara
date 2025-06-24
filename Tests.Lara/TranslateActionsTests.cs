using Apps.Lara.Actions;
using Apps.Lara.Model;
using Tests.Lara.Base;

namespace Tests.Lara;

[TestClass]
public class TranslateActionsTests : TestBase
{
    [TestMethod]
    public async Task TranslateText_IsSuccessful()
    {
        var action = new TranslateActions(InvocationContext);

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
}
