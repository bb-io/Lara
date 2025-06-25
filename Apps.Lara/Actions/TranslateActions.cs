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
        if (text.Instructions != null && text.Instructions.Any())
            body["instructions"] = text.Instructions;
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
    public async Task<FileResponse> TranslateFile([ActionParameter] LanguageRequest language, [ActionParameter] TranslateFileRequest file)
    {
        var detectedContentType = await DetectContentTypeAsync(file);


        using var stream = await fileManagementClient.DownloadAsync(file.File);
        string content;
        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            content = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(content))
                throw new PluginMisconfigurationException("The file content is empty or could not be read.");
        }
        catch (Exception ex)
        {
            throw new PluginMisconfigurationException($"Failed to read file content: {ex.Message}", ex);
        }

        var body = new Dictionary<string, object>
        {
            ["target"] = language.TargetLanguage!,
            ["q"] = content,
            ["content_type"] = detectedContentType
        };
        if (!string.IsNullOrWhiteSpace(language.SourceLanguage))
            body["source"] = language.SourceLanguage!;
        if (file.Instructions?.Any() == true)
            body["instructions"] = file.Instructions;
        if (!string.IsNullOrWhiteSpace(file.Priority))
            body["priority"] = file.Priority!;

        var request = new RestRequest("/translate", Method.Post)
            .AddJsonBody(body);
        var response = await Client.ExecuteWithErrorHandling<TranslationTextDtoResponse>(request);

        var translatedContent = response.Content?.Translation;
        if (string.IsNullOrWhiteSpace(translatedContent))
            throw new PluginMisconfigurationException("The translated content is empty or could not be extracted.");

        var byteContent = Encoding.UTF8.GetBytes(translatedContent);
        using var memoryStream = new MemoryStream(byteContent);

        var originalFileName = Path.GetFileNameWithoutExtension(file.File.Name);
        var fileExt = Path.GetExtension(file.File.Name)?.ToLowerInvariant();
        var outputFileName = $"translated_{originalFileName}{fileExt}";

        var translatedFile = await fileManagementClient.UploadAsync(memoryStream, detectedContentType, outputFileName);
        return new FileResponse { File = translatedFile };
    }


    [Action("Translate file (Blacklake)", Description = "Translates file")]
    public async Task<FileResponse> TranslateFileBlacklake([ActionParameter] LanguageRequest language, [ActionParameter] TranslateFileRequest file)
    {
        var stream = await fileManagementClient.DownloadAsync(file.File);
        var content = await Transformation.TryParse(stream);

        var segments = content.GetSegments().ToList();
        if (!segments.Any())
            throw new PluginMisconfigurationException("No segments found in the file.");

        var qArray = segments.Select(segment => new Dictionary<string, object>
        {
            ["text"] = segment.GetSource(),
            ["translatable"] = !segment.IsIgnorbale
        }).ToArray();

        var detectedContentType = await DetectContentTypeAsync(file);

        var body = new Dictionary<string, object>
        {
            ["target"] = language.TargetLanguage!,
            ["q"] = qArray,
            ["content_type"] = detectedContentType
        };
        if (!string.IsNullOrWhiteSpace(language.SourceLanguage))
            body["source"] = language.SourceLanguage!;
        if (file.Instructions?.Any() == true)
            body["instructions"] = file.Instructions;
        if (!string.IsNullOrWhiteSpace(file.Priority))
            body["priority"] = file.Priority!;

        var request = new RestRequest("/translate", Method.Post)
            .AddJsonBody(body);
        var response = await Client.ExecuteWithErrorHandling<TranslationTextsResponse>(request);

        var translatedSegments = response.Translation?.Translation;
        if (translatedSegments == null || !translatedSegments.Any())
            throw new PluginMisconfigurationException("No translated segments received from the server.");

        var idx = 0;
        foreach (var segment in segments)
        {
            if (!segment.IsIgnorbale && idx < translatedSegments.Count)
            {
                segment.SetTarget(translatedSegments[idx].Text);
                segment.State = SegmentState.Translated;
                idx++;
            }
        }

        string serialized;
        if (detectedContentType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
        {
            serialized = HtmlContentCoder
                .Serialize(content.Target())
                .ToString();
        }
        else if (detectedContentType.Equals("application/xliff+xml", StringComparison.OrdinalIgnoreCase))
        {
            serialized = Xliff2Serializer
                .Serialize(content)
                .ToString();
        }
        else
        {
            serialized = HtmlContentCoder
                .Serialize(content.Target())
                .ToString();
        }

        var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));

        var originalName = Path.GetFileNameWithoutExtension(file.File.Name);
        var fileExtension = Path.GetExtension(file.File.Name)?.ToLowerInvariant();
        var outputName = $"translated_{originalName}{fileExtension}";

        var translatedFile = await fileManagementClient
            .UploadAsync(resultStream, detectedContentType, outputName);

        return new FileResponse { File = translatedFile };
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


    private async Task<string> DetectContentTypeAsync(TranslateFileRequest file)
    {
        var contentTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".txt", "text/plain" },
                { ".xml", "text/xml" },
                { ".html", "text/html" },
                { ".htm", "text/html" },
                { ".xlf", "application/xliff+xml" }
            };

        var fileName = file.File.Name;
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(ext) && contentTypeMap.TryGetValue(ext, out var mapped))
        {
            return mapped;
        }
        var tempStream = await fileManagementClient.DownloadAsync(file.File);
        using var reader = new StreamReader(tempStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        char[] buffer = new char[1024];
        int read = await reader.ReadAsync(buffer, 0, buffer.Length);
        tempStream.Seek(0, SeekOrigin.Begin);
        string sample = new(buffer, 0, read);

        if (sample.Contains("<?xml", StringComparison.OrdinalIgnoreCase) && sample.Contains("<xliff", StringComparison.OrdinalIgnoreCase))
            return "application/xliff+xml";
        if (sample.Contains("<?xml", StringComparison.OrdinalIgnoreCase))
            return "text/xml";
        if (sample.Contains("<html", StringComparison.OrdinalIgnoreCase) || sample.Contains("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase))
            return "text/html";

        return "text/plain";
    }
}