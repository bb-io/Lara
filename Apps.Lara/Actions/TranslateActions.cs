using Apps.Lara.Api;
using Apps.Lara.Model;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Coders;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff2;
using RestSharp;
using System.IO.Compression;
using System.Text;

namespace Apps.Lara.Actions;

[ActionList]
public class TranslateActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : Invocable(invocationContext)
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

        if (!string.IsNullOrWhiteSpace(text.Instructions))
            body["instructions"] = new[] { text.Instructions! };

        if (!string.IsNullOrWhiteSpace(text.Priority))
            body["priority"] = text.Priority!;

        request.AddJsonBody(body);

        var response = await client.ExecuteWithErrorHandling<TranslationTextDtoResponse>(request);

        return new TranslationTextResponse
        {
            Translation = response.Content
        };
    }


    [Action("Translate file", Description = "Translates file")]
    public async Task<FileResponse> TranslateFileBlacklake([ActionParameter] LanguageRequest language, [ActionParameter] TranslateFileRequest file)
    {
      
        var stream = await fileManagementClient.DownloadAsync(file.File);
        var content = await Transformation.Parse(stream);

        var segments = content.GetSegments().Where(s => !s.IsIgnorbale && s.IsInitial).ToList();

        if (!segments.Any())
            throw new PluginMisconfigurationException("No segments found in the file.");

        var textBlocks = segments.Select(s => new { text = s.GetSource(), translatable = true }).ToArray();
        string detectedContentType = DetectContentType(file.File.Name);

        var body = new Dictionary<string, object>
        {
            ["q"] = textBlocks,
            ["target"] = language.TargetLanguage,
            ["content_type"] = detectedContentType
        };

        if (!string.IsNullOrWhiteSpace(language.SourceLanguage))
            body["source"] = language.SourceLanguage;
        if (!string.IsNullOrWhiteSpace(file.Instructions))
            body["instructions"] = new[] { file.Instructions };
        if (!string.IsNullOrWhiteSpace(file.Priority))
            body["priority"] = file.Priority;


        var request = new RestRequest("/translate", Method.Post)
            .AddJsonBody(body);
        var response = await Client.ExecuteWithErrorHandling<TranslationTextsResponse>(request);

        var translatedSegments = response.Translation?.Translation;
        if (translatedSegments == null || !translatedSegments.Any())
            throw new PluginMisconfigurationException("No translated segments received from the server.");

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].SetTarget(translatedSegments[i].Text);
            segments[i].State = SegmentState.Translated;
        }

        Stream outputStream;
        string fileName = file.File.Name;

        if (detectedContentType.Equals("application/xliff+xml", StringComparison.OrdinalIgnoreCase))
        {
            outputStream = content.Serialize().ToStream();
            fileName = Path.ChangeExtension(file.File.Name, ".xliff");
        }
        else
        {
            outputStream = content.Target().Serialize().ToStream();
        }

        var uploadedFile = await fileManagementClient.UploadAsync(
            outputStream, detectedContentType, fileName);

        return new FileResponse
        {
            File = uploadedFile
        };
    }


    [Action("Add translation to memory", Description = "Adds translation to memory")]
    public async Task<MemoryTranslationResponse> AddTranslationToMemory([ActionParameter] MemoryRequest memory, [ActionParameter] LanguageRequest language,
        [ActionParameter] AddTranslationRequest translation)
    {
        if (string.IsNullOrWhiteSpace(memory.MemoryId))
            throw new PluginMisconfigurationException("Memory ID is required. Please check your input and try again");
        if (string.IsNullOrWhiteSpace(language.SourceLanguage))
            throw new PluginMisconfigurationException("Source language is required. Please check your input and try again");
        if (string.IsNullOrWhiteSpace(language.TargetLanguage))
            throw new PluginMisconfigurationException("Target language is required. Please check your input and try again");
        if (string.IsNullOrWhiteSpace(translation.Setnence))
            throw new PluginMisconfigurationException("Sentence is required. Please check your input and try again");
        if (string.IsNullOrWhiteSpace(translation.Translation))
            throw new PluginMisconfigurationException("Translation is required. Please check your input and try again");

        var client = new LaraClient(Creds);
        var request = new RestRequest($"/memories/{memory.MemoryId}/content", Method.Put);

        var body = new Dictionary<string, object>
        {
            ["source"] = language.SourceLanguage!,
            ["target"] = language.TargetLanguage,
            ["sentence"] = translation.Setnence,
            ["translation"] = translation.Translation
        };

        if (!string.IsNullOrWhiteSpace(translation.TranslationId))
            body["tuid"] = translation.TranslationId!;
        if (!string.IsNullOrWhiteSpace(translation.SentenceBefore))
            body["sentence_before"] = translation.SentenceBefore!;
        if (!string.IsNullOrWhiteSpace(translation.SentenceAfter))
            body["sentence_after"] = translation.SentenceAfter!;

        request.AddJsonBody(body);

        var response = await client.ExecuteWithErrorHandling<MemoryTranslationResponse>(request);

        return response;
    }


    [Action("Delete translation from memory", Description = "Deletes translation from memory")]
    public async Task<MemoryTranslationResponse> DeleteTranslation([ActionParameter] MemoryRequest memory, [ActionParameter] LanguageRequest language,
        [ActionParameter] AddTranslationRequest translation)
    {
        if (string.IsNullOrWhiteSpace(memory.MemoryId))
            throw new PluginMisconfigurationException("Memory ID is required. Please check your input and try again");
        if (string.IsNullOrWhiteSpace(language.SourceLanguage))
            throw new PluginMisconfigurationException("Source language is required. Please check your input and try again");
        if (string.IsNullOrWhiteSpace(language.TargetLanguage))
            throw new PluginMisconfigurationException("Target language is required. Please check your input and try again");
        if (string.IsNullOrWhiteSpace(translation.Setnence))
            throw new PluginMisconfigurationException("Sentence is required. Please check your input and try again");
        if (string.IsNullOrWhiteSpace(translation.Translation))
            throw new PluginMisconfigurationException("Translation is required. Please check your input and try again");

        var client = new LaraClient(Creds);
        var request = new RestRequest($"/memories/{memory.MemoryId}/content", Method.Delete);

        var body = new Dictionary<string, object>
        {
            ["source"] = language.SourceLanguage!,
            ["target"] = language.TargetLanguage,
            ["sentence"] = translation.Setnence,
            ["translation"] = translation.Translation
        };

        if (!string.IsNullOrWhiteSpace(translation.TranslationId))
            body["tuid"] = translation.TranslationId!;
        if (!string.IsNullOrWhiteSpace(translation.SentenceBefore))
            body["sentence_before"] = translation.SentenceBefore!;
        if (!string.IsNullOrWhiteSpace(translation.SentenceAfter))
            body["sentence_after"] = translation.SentenceAfter!;

        request.AddJsonBody(body);

        var response = await client.ExecuteWithErrorHandling<MemoryTranslationResponse>(request);

        return response;
    }

    [Action("Create memory", Description = "Creates memory")]
    public async Task<MemoryResponseDto> CreateMemory([ActionParameter][Display("Name")] string name)
    {
        var client = new LaraClient(Creds);
        var request = new RestRequest("/memories", Method.Post);

        var body = new Dictionary<string, object>
        {
            ["name"] = name
        };
        request.AddJsonBody(body);

        var response = await client.ExecuteWithErrorHandling<MemoryResponseDto>(request);

        return response;
    }

    [Action("Import memory", Description = "Imports memory from file")]
    public async Task<MemoryTranslationResponse> ImportMemory([ActionParameter] MemoryRequest memory,
        [ActionParameter] ImportMemoryRequest input)
    {
        if (string.IsNullOrWhiteSpace(memory.MemoryId))
            throw new PluginMisconfigurationException("Memory ID is required. Please check your input and try again");
        if (input.File == null)
            throw new PluginMisconfigurationException("TMX file is required. Please check your input and try again");

        var client = new LaraClient(Creds);
        var request = new RestRequest($"/memories/{memory.MemoryId}/import", Method.Post)
        {
            AlwaysMultipartFormData = true
        };

        await using var rawStream = await fileManagementClient.DownloadAsync(input.File);
        byte[] fileBytes;

        bool isGzipped = input.File.Name.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);

        if (isGzipped)
        {
            await using var msRaw = new MemoryStream();
            await rawStream.CopyToAsync(msRaw);
            fileBytes = msRaw.ToArray();
        }
        else
        {
            await using var msRaw = new MemoryStream();
            await rawStream.CopyToAsync(msRaw);
            byte[] rawBytes = msRaw.ToArray();

            await using var msGzip = new MemoryStream();
            using (var gzip = new GZipStream(msGzip, CompressionLevel.Optimal, leaveOpen: true))
            {
                await gzip.WriteAsync(rawBytes, 0, rawBytes.Length);
            }
            fileBytes = msGzip.ToArray();
        }
        var fileName = isGzipped ? input.File.Name : $"{input.File.Name}.gz";

        request.AddFile(name: "tmx", bytes: fileBytes, fileName: fileName, contentType: "application/gzip");
        request.AddParameter(name: "compression", value: "gzip", type: ParameterType.GetOrPost);

        return await client.ExecuteWithErrorHandling<MemoryTranslationResponse>(request);
    }


    private string DetectContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".html" or ".htm" => "text/html",
            ".xlf" or ".xliff" => "application/xliff+xml",
            _ => "text/plain"
        };
    }
}