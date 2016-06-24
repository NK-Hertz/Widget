using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Web.Mvc;

namespace Nop.Plugin.Widgets.HomePageNewProducts.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public int WidgetZoneId { get; set; }
        public bool WidgetZoneId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.HomePageNewProducts.WidgetZoneValues")]
        public SelectList WidgetZoneValues { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.HomePageNewProducts.NumberOfProducts")]
        public int NumberOfProducts { get; set; }
        public bool NumberOfProducts_OverrideForStore { get; set; }
    }
}
