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
using RestSharp;
using System.IO.Compression;
using System.Text;
using Blackbird.Applications.Sdk.Common.Files;
using System.Text.RegularExpressions;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using System.Net.Mime;

namespace Apps.Lara.Actions;

[ActionList]
public class TranslateActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : Invocable(invocationContext)
{
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

        if (!string.IsNullOrWhiteSpace(input.ContentType))
            body["content_type"] = input.ContentType!;

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

    [Action("Translate", Description = "Translates file")]
    public async Task<FileResponse> TranslateFile([ActionParameter] TranslateFileRequest file)
    {
        using var fileStream = await fileManagementClient.DownloadAsync(file.File);
        var content = await Transformation.Parse(fileStream);
        
        async Task<IEnumerable<TranslationSegment>> BatchTranslate(IEnumerable<Segment> batch)
        {
            var instructionsList = new List<string>();
            if (file.GlossaryFile != null)
            {
                var allText = string.Join(" ", batch.Select(s => s.GetSource()));
                var glossaryPrompt = await GetGlossaryPromptPart(file.GlossaryFile, allText, filter: false);
                if (!string.IsNullOrWhiteSpace(glossaryPrompt))
                    instructionsList.Add(glossaryPrompt);
            }
            if (!string.IsNullOrWhiteSpace(file.Instructions))
                instructionsList.Add(file.Instructions);

            var blocks = batch
                .Select(s => new { text = s.GetSource(), translatable = true })
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

        var segmentTranslations = await content
            .GetSegments()
            .Where(x => !x.IsIgnorbale && x.IsInitial)
            .Batch(100).Process(BatchTranslate);

        foreach (var (segment, translation) in segmentTranslations)
        {
            segment.SetTarget(translation.Text);
            segment.State = SegmentState.Translated;
        }

        if (file.OutputFileHandling == null || file.OutputFileHandling == "xliff")
        {
            content.SourceLanguage = file.SourceLanguage;
            content.TargetLanguage = file.TargetLanguage;
            var xliffStream = content.Serialize().ToStream();
            var fileName = file.File.Name.EndsWith("xliff") || file.File.Name.EndsWith("xlf") ? file.File.Name : file.File.Name + ".xliff";
            var uploadedFile = await fileManagementClient.UploadAsync(xliffStream, "application/xliff+xml", fileName);
            return new FileResponse { File = uploadedFile };
        }
        else
        {
            var resultStream = content.Target().Serialize().ToStream();
            var uploadedFile = await fileManagementClient.UploadAsync(resultStream, file.File.ContentType, file.File.Name);
            return new FileResponse { File = uploadedFile };
        }
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