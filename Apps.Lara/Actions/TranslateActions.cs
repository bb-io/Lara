using Apps.Lara.Api;
using Apps.Lara.Model;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using RestSharp;
using Blackbird.Applications.Sdk.Common.Files;
using System.Net.Http.Headers;
using Blackbird.Filters.Constants;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using System.Text.RegularExpressions;
using System.Text;

namespace Apps.Lara.Actions;

[ActionList("Translation")]
public class TranslateActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : Invocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.TranslateText)]
    [Action("Translate text", Description = "Translates text")]
    public async Task<TranslationContent> TranslateText([ActionParameter] TranslateTextRequest input)
    {
        var client = new LaraClient(Creds);
        var request = new RestRequest("/translate", Method.Post);
        var body = new Dictionary<string, object>
        {
            ["target"] = input.TargetLanguage!,
            ["q"] = input.Text
        };

        if (!string.IsNullOrWhiteSpace(input.SourceLanguage))
            body["source"] = input.SourceLanguage!;

        if (!string.IsNullOrWhiteSpace(input.Instructions))
            body["instructions"] = new[] { input.Instructions! };

        if (!string.IsNullOrWhiteSpace(input.Priority))
            body["priority"] = input.Priority!;

        if (!string.IsNullOrWhiteSpace(input.MemoryId))
            body["adapt_to"] = new[] { input.MemoryId };

        request.AddJsonBody(body);

        var response = await client.ExecuteWithErrorHandling<TranslationTextDtoResponse>(request);

        return response.Content;
    }

    [BlueprintActionDefinition(BlueprintAction.TranslateFile)]
    [Action("Translate", Description = "Translates file")]
    public async Task<FileResponse> TranslateFile([ActionParameter] TranslateFileRequest file)
    {
        if (file.FileTranslationStrategy == "lara") return await TranslateDocumentUsingLara(file);

        try
        {
            return await TranslateFileUsingBlackbird(file);
        } catch(NotImplementedException e)
        {
            throw new PluginMisconfigurationException("The file format is not supported by the Blackbird interoperable setting. Try setting the file translation strategy to Lara native.");
        }
    }

    public async Task<FileResponse> TranslateFileUsingBlackbird([ActionParameter] TranslateFileRequest file)
    {
        using var fileStream = await fileManagementClient.DownloadAsync(file.File);
        var content = await Transformation.Parse(fileStream, file.File.Name);
        
        async Task<IEnumerable<TranslationSegment>> BatchTranslate(IEnumerable<(Unit Unit, Segment Segment)> batch)
        {
            var instructionsList = new List<string>();
            if (file.GlossaryFile != null)
            {
                var allText = string.Join(" ", batch.Select(s => s.Segment.GetSource()));
                var glossaryPrompt = await GetGlossaryPromptPart(file.GlossaryFile, allText, filter: false);
                if (!string.IsNullOrWhiteSpace(glossaryPrompt))
                    instructionsList.Add(glossaryPrompt);
            }
            if (!string.IsNullOrWhiteSpace(file.Instructions))
                instructionsList.Add(file.Instructions);

            instructionsList.AddRange(content.Notes.Select(x => x.Text));

            var blocks = batch
                .Select(s => new { text = s.Segment.GetSource(), translatable = true })
                .ToArray();

            var body = new Dictionary<string, object>
            {
                ["target"] = file.TargetLanguage,
                ["q"] = blocks
            };

            if (!string.IsNullOrWhiteSpace(file.SourceLanguage))
                body["source"] = file.SourceLanguage;
            if (!string.IsNullOrWhiteSpace(file.Priority))
                body["priority"] = file.Priority;
            if (!string.IsNullOrWhiteSpace(file.MemoryId))
                body["adapt_to"] = new[] { file.MemoryId };
            if(instructionsList.Any())
                body["instructions"] = instructionsList.ToArray();

            var request = new RestRequest("/translate", Method.Post).AddJsonBody(body);
            var response = await Client.ExecuteWithErrorHandling<TranslationTextsResponse>(request);

            return response.Translation.Translation;
        }

        var unitTranslations = await content
            .GetUnits()
            .Batch(100, x => !x.IsIgnorbale && x.IsInitial).Process(BatchTranslate);

        foreach(var (unit, results) in unitTranslations)
        {
            foreach (var (segment, result) in results)
            {
                segment.SetTarget(result.Text);
                segment.State = SegmentState.Translated;
            }

            unit.Provenance.Translation.Tool = "Lara";
            unit.Provenance.Translation.ToolReference = "https://laratranslate.com";
        }

        if (file.OutputFileHandling == "original")
        {
            var targetContent = content.Target();
            return new FileResponse { File = await fileManagementClient.UploadAsync(targetContent.Serialize().ToStream(), targetContent.OriginalMediaType, targetContent.OriginalName) };
        }

        content.SourceLanguage ??= file.SourceLanguage;
        content.TargetLanguage ??= file.TargetLanguage;
        return new FileResponse { File = await fileManagementClient.UploadAsync(content.Serialize().ToStream(), MediaTypes.Xliff, content.XliffFileName) };
    }

    public async Task<FileResponse> TranslateDocumentUsingLara([ActionParameter] TranslateFileRequest file)
    {
        using var fileStream = await fileManagementClient.DownloadAsync(file.File);
        var content = await Transformation.Parse(fileStream, file.File.Name);

        var presignedUrlrequest = new RestRequest("/documents/upload-url", Method.Get).AddParameter("filename", file.File.Name);
        var presignedUrlResponse = await Client.ExecuteWithErrorHandling<ContentWrapper<UploadUrlData>>(presignedUrlrequest);
        presignedUrlResponse.Content.Fields.TryGetValue("key", out var s3Key);

        if (s3Key is null) throw new PluginApplicationException("Invalid S3 response returned from Lara");

        await UploadToS3PresignedUrl(presignedUrlResponse.Content, file.File);

        var body = new Dictionary<string, object>
        {
            ["target"] = file.TargetLanguage,
            ["s3key"] = s3Key
        };
        if (!string.IsNullOrWhiteSpace(file.SourceLanguage))
            body["source"] = file.SourceLanguage;
        if (!string.IsNullOrWhiteSpace(file.MemoryId))
            body["adapt_to"] = new[] { file.MemoryId };

        var postDocumentRequest = new RestRequest("/documents", Method.Post).AddJsonBody(body);
        var postDocumentResponse = await Client.ExecuteWithErrorHandling<ContentWrapper<Document>>(postDocumentRequest);

        var pollingInterval = 2000;
        var maxWaitTime = 1000 * 60 * 9; // 9 minutes
        var start = DateTime.Now;

        while((DateTime.Now - start).TotalMilliseconds < maxWaitTime)
        {
            await Task.Delay(pollingInterval);

            var statusRequest = new RestRequest($"/documents/{postDocumentResponse.Content.Id}", Method.Get);
            var statusResponse = await Client.ExecuteWithErrorHandling<ContentWrapper<Document>>(statusRequest);

            if (statusResponse.Content.Status == DocumentStatus.TRANSLATED) break;
            if (statusResponse.Content.Status == DocumentStatus.ERROR) throw new PluginApplicationException(statusResponse.Content.ErrorReason);
        }

        var downloadUrlRequest = new RestRequest($"/documents/{postDocumentResponse.Content.Id}/download-url", Method.Get);
        var downloadUrlResponse = await Client.ExecuteWithErrorHandling<ContentWrapper<UploadUrlData>>(downloadUrlRequest);

        var responseFile = new FileReference(new HttpRequestMessage(HttpMethod.Get, downloadUrlResponse.Content.Url), file.File.Name, file.File.ContentType);

        return new FileResponse
        {
            File = responseFile,
        };
    }

    public async Task UploadToS3PresignedUrl(UploadUrlData data, FileReference file)
    {
        using var fileStream = await fileManagementClient.DownloadAsync(file);
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();

        foreach (var field in data.Fields)
        {
            content.Add(new StringContent(field.Value), field.Key);
        }

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await client.PostAsync(data.Url, content);
        response.EnsureSuccessStatusCode();
    }

    protected async Task<string> GetGlossaryPromptPart(FileReference glossary, string sourceContent, bool filter)
    {
        if (!glossary.Name.EndsWith(".tbx", StringComparison.OrdinalIgnoreCase))
        {
            var extension = Path.GetExtension(glossary.Name);
            throw new PluginMisconfigurationException($"Glossary file must be in TBX format. But the provided file has {extension} extension.");
        }

        var glossaryStream = await fileManagementClient.DownloadAsync(glossary);
        var blackbirdGlossary = await glossaryStream.ConvertFromTbx();

        var glossaryPromptPart = new StringBuilder();
        glossaryPromptPart.AppendLine();
        glossaryPromptPart.AppendLine();
        glossaryPromptPart.AppendLine("Glossary entries (each entry includes terms in different language. Each " +
                                      "language may have a few synonymous variations which are separated by ;;):");

        var entriesIncluded = false;
        foreach (var entry in blackbirdGlossary.ConceptEntries)
        {
            var allTerms = entry.LanguageSections.SelectMany(x => x.Terms.Select(y => y.Term));
            if (filter && !allTerms.Any(x => Regex.IsMatch(sourceContent, $@"\b{x}\b", RegexOptions.IgnoreCase))) continue;
            entriesIncluded = true;

            glossaryPromptPart.AppendLine();
            glossaryPromptPart.AppendLine("\tEntry:");

            foreach (var section in entry.LanguageSections)
            {
                glossaryPromptPart.AppendLine(
                    $"\t\t{section.LanguageCode}: {string.Join(";; ", section.Terms.Select(term => term.Term))}");
            }
        }

        return entriesIncluded ? glossaryPromptPart.ToString() : null;
    }
}