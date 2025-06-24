using Apps.Lara.Api;
using Apps.Lara.Model;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Lara.Actions;

[ActionList]
public class TranslateActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Translate text", Description = "Translates text")]
    public async Task<TranslationTextResponse> TranslateText([ActionParameter] LanguageRequest language, [ActionParameter] TranslateTextRequest text)
    {
        var client = new LaraClient(Creds);
        var request = new RestRequest("/translate", Method.Post);
        var body = new Dictionary<string, object>
        {
            ["target"] = language.TargetLanguage!,
            ["q"] = text.Text
        };

        if (!string.IsNullOrWhiteSpace(language.SourceLanguage))
            body["source"] = language.SourceLanguage!;

        if (!string.IsNullOrWhiteSpace(text.ContentType))
            body["content_type"] = text.ContentType!;
        if (text.Instructions != null && text.Instructions.Any())
            body["instructions"] = text.Instructions;
        if (!string.IsNullOrWhiteSpace(text.Priority))
            body["priority"] = text.Priority!;

        request.AddJsonBody(body);

        var response = await client.ExecuteWithErrorHandling<TranslationTextDtoResponse>(request);

        return new TranslationTextResponse
        {
           Translation= response.Content
        };
    }
}