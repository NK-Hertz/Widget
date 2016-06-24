using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using System.Collections.Generic;
using System.Web.Routing;

namespace Nop.Plugin.Widgets.HomePageNewProducts
{
    public class HomePageNewProductsPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;
        private readonly HomePageNewProductsSettings _homePageNewProductsSettings;

        public HomePageNewProductsPlugin(ISettingService settingService, 
            HomePageNewProductsSettings homePageNewProductsSettings)
        {
            this._settingService = settingService;
            this._homePageNewProductsSettings = homePageNewProductsSettings;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WidgetsHomePageNewProducts";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces" , "Nop.Plugin.Widgets.HomePageNewProducts.Controllers" },
                { "area", null }
            };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "WidgetsHomePageNewProducts";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "Nop.Plugin.Widgets.HomePageNewProducts.Controllers" },
                { "area", null },
                { "widgetZone", widgetZone }
            };
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                _homePageNewProductsSettings.WidgetZone.ToString()
            };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            var settings = new HomePageNewProductsSettings
            {
                WidgetZone = WidgetZone.home_page_top,
                NumberOfProducts = 0
            };
            _settingService.SaveSetting(settings);

            //this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.WidgetZoneId", "WidgetZoneId");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.WidgetZoneValues", "Widget Zone");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.WidgetZoneValues.Hint", "Choose widget zone in which the products to be displayed");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.NumberOfProducts", "NumberOfProducts");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.NumberOfProducts.Hint", "Enter number of products you want to show.");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<HomePageNewProductsSettings>();

            //this.DeletePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.WidgetZoneId");
            this.DeletePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.WidgetZoneValues");
            this.DeletePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.WidgetZoneValues.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.NumberOfProducts");
            this.DeletePluginLocaleResource("Plugins.Widgets.HomePageNewProducts.NumberOfProducts.Hint");

            base.Uninstall();
        }
    }
}
