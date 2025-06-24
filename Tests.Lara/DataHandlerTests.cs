using Apps.Lara.Handlers;
using Apps.Lara.Handlers.Static;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Lara.Base;

namespace Tests.Lara;

[TestClass]
public class DataHandlerTests : TestBase
{
    [TestMethod]
    public async Task LanguageDataHandler_IssSuccess()
    {
        var handler = new LanguageDataHandler(InvocationContext);

        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

        foreach (var item in response)
        {
            Console.WriteLine($"{item.DisplayName} - {item.Value}");
        }
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task MemoriesDataHandler_IssSuccess()
    {
        var handler = new MemoriesDataHandler(InvocationContext);

        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

        foreach (var item in response)
        {
            Console.WriteLine($"{item.DisplayName} - {item.Value}");
        }
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task ContentTypeDataHandler_IssSuccess()
    {
        var handler = new ContentTypeDataHandler();

        var response = handler.GetData();

        foreach (var item in response)
        {
            Console.WriteLine($"{item.DisplayName} - {item.Value}");
        }
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task PriorityDataHandler_IssSuccess()
    {
        var handler = new PriorityDataHandler();

        var response = handler.GetData();

        foreach (var item in response)
        {
            Console.WriteLine($"{item.DisplayName} - {item.Value}");
        }
        Assert.IsNotNull(response);
    }
}
