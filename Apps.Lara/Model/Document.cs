using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Lara.Model;
public class Document
{
    public string Id { get; set; }
    public string Status { get; set; }
    public string? Source { get; set; }
    public string Target { get; set; }
    public string Filename { get; set; }
    public int TranslatedChars { get; set; }
    public int TotalChars { get; set; }

    [JsonProperty("error_reason")]
    public string ErrorReason { get; set; }
}

public static class DocumentStatus
{
    public const string INITIALIZED = "initialized";    // just been created
    public const string ANALYZING = "analyzing";        // being analyzed for language detection and chars count
    public const string PAUSED = "paused";              // paused after analysis, needs user confirm
    public const string READY = "ready";                // ready to be translated
    public const string TRANSLATING = "translating";
    public const string TRANSLATED = "translated";
    public const string ERROR = "error";
}