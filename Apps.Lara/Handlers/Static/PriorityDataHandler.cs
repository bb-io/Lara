using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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