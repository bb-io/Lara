using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Lara.Handlers.Static;
public class ContentTypeDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return EnumValues.Select(item => new DataSourceItem(item.Key, item.Value));
    }
    protected Dictionary<string, string> EnumValues => new()
        {
                {"text/plain", "text/plain"},
                {"text/xml", "text/xml"},
                {"text/html", "text/html"},
                {"application/xliff+xml", "application/xliff+xml"}
        };
}
