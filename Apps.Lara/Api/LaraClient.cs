using Apps.Lara.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;

namespace Apps.Lara.Api;

public class LaraClient : BlackBirdRestClient
{
    private readonly string _accessKey;
    private readonly string _secret;

    public LaraClient(IEnumerable<AuthenticationCredentialsProvider> creds) : base(new()
    {
        BaseUrl = new Uri("https://api.laratranslate.com"),
    })
    {
        _accessKey = creds.Single(c => c.KeyName == CredsNames.AccessKey).Value;
        _secret = creds.Single(c => c.KeyName == CredsNames.AccessKeySecret).Value;
    }

    public override async Task<T> ExecuteWithErrorHandling<T>(RestRequest request)
    {
        string content = (await ExecuteWithErrorHandling(request)).Content;
        T val = JsonConvert.DeserializeObject<T>(content, JsonSettings);
        if (val == null)
        {
            throw new Exception($"Could not parse {content} to {typeof(T)}");
        }

        return val;
    }

    public override async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        SignRequest(request);
        RestResponse restResponse = await ExecuteAsync(request);
        if (!restResponse.IsSuccessStatusCode)
        {
            throw ConfigureErrorException(restResponse);
        }

        return restResponse;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var error = JsonConvert.DeserializeObject(response.Content);
        throw new PluginApplicationException(error.ToString());
    }

    private void SignRequest(RestRequest request)
    {
        var date = DateTime.UtcNow.ToString("r");
        request.AddOrUpdateHeader("X-Lara-Date", date);

        bool isMultipart = request.Files.Count > 0;
        var bodyParam = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);

        if (isMultipart)
        {
            const string multipartCt = "multipart/form-data";
            request.AddOrUpdateHeader("Content-Type", multipartCt);

        }
        else if (bodyParam is { Value: string bodyStr })
        {
            var contentType = bodyParam.ContentType ?? "application/json";
            request.AddOrUpdateHeader("Content-Type", contentType);

            if (!string.IsNullOrEmpty(bodyStr))
            {
                var contentMd5 = ComputeMD5(bodyStr);
                request.AddOrUpdateHeader("Content-MD5", contentMd5);
            }
        }
        else
        {
            request.AddOrUpdateHeader("Content-Type", "application/json");
        }

        var finalContentType = request.Parameters
            .FirstOrDefault(p => p.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            ?.Value?.ToString() ?? "";

        var path = request.Resource.StartsWith("/") ? request.Resource : "/" + request.Resource;
        var method = request.Method.ToString().ToUpperInvariant();
        var contentMd5Part = (!isMultipart && bodyParam is { Value: string })
            ? ComputeMD5(bodyParam.Value.ToString())
            : "";
        var challenge = $"{method}\n{path}\n{contentMd5Part}\n{finalContentType}\n{date}";
        var signature = ComputeHmacSha256(challenge, _secret);

        request.AddOrUpdateHeader("Authorization", $"Lara {_accessKey}:{signature}");
    }


    private string ComputeMD5(string input)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToBase64String(hashBytes);
    }

    private string ComputeHmacSha256(string challenge, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var challengeBytes = Encoding.UTF8.GetBytes(challenge);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(challengeBytes);
        return Convert.ToBase64String(hashBytes);
    }
}