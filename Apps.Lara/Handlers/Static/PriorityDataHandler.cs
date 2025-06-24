using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Lara.Handlers.Static
{
    public class PriorityDataHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData()
        {
            return EnumValues.Select(item => new DataSourceItem(item.Key, item.Value));
        }
        protected Dictionary<string, string> EnumValues => new()
        {
                {"NORMAL", "Normal"},
                {"BACKGROUND", "Background" }
        };
    }
}