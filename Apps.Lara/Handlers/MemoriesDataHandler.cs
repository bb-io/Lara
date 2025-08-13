using Apps.Lara.Api;
using Apps.Lara.Model;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Lara.Handlers;

public class MemoriesDataHandler(InvocationContext invocationContext) : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var laraClient = new LaraClient(Creds);
        var request = new RestRequest("/memories", Method.Get);
        var response = await laraClient.ExecuteWithErrorHandling<MemoriesResponseDto>(request);

        var memories = response.Content ?? new List<MemoryItem>();

        return memories.Select(m => new DataSourceItem(value: m.Id, displayName: m.Name));
    }
}
