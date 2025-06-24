using Apps.Lara.Api;
using Apps.Lara.Model;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Lara.Handlers;
public class LanguageDataHandler(InvocationContext invocationContext) : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var laraClient = new LaraClient(Creds);
        var request = new RestRequest("/languages", Method.Get);
        var response = await laraClient.ExecuteWithErrorHandling<LanguageCodeResponse>(request);

        var codes = response.Content ?? new List<string>();

        return codes
            .Select(code => new DataSourceItem(code, code));
    }
}
