using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Lara.Handlers.Static;
public class FileTranslationStrategyHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return new List<DataSourceItem>()
        {
            new DataSourceItem("blackbird", "Blackbird interoperable (default)"),
            new DataSourceItem("lara", "Lara native"),
        };
    }
}
