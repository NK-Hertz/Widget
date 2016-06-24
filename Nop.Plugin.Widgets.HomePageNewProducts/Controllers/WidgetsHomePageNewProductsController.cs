using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Plugin.Widgets.HomePageNewProducts.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.HomePageNewProducts.Controllers
{
    public class WidgetsHomePageNewProductsController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly HomePageNewProductsSettings _homePageNewProductsSettings;
        private readonly IRepository<Product> _productRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;

        public WidgetsHomePageNewProductsController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService,
            ISettingService settingService,
            ILocalizationService localizationService,
            HomePageNewProductsSettings homePageNewProductsSettings,
            IRepository<Product> productRepository,
            IProductService productService,
            ICategoryService categoryService,
            ISpecificationAttributeService specificationAttributeService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IPermissionService permissionService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPictureService pictureService,
            IWebHelper webHelper,
            ICacheManager cacheManager,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._homePageNewProductsSettings = homePageNewProductsSettings;
            this._productRepository = productRepository;
            this._cacheManager = cacheManager;
            this._productService = productService;
            this._categoryService = categoryService;
            this._specificationAttributeService = specificationAttributeService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._permissionService = permissionService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._webHelper = webHelper;
            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var homePageNewProductsSettings = _settingService.LoadSetting<HomePageNewProductsSettings>(storeScope);

            var model = new ConfigurationModel();
            model.WidgetZoneId = Convert.ToInt32(homePageNewProductsSettings.WidgetZone);
            model.NumberOfProducts = homePageNewProductsSettings.NumberOfProducts;
            model.WidgetZoneValues = homePageNewProductsSettings.WidgetZone.ToSelectList();

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.WidgetZoneId_OverrideForStore = _settingService.SettingExists(homePageNewProductsSettings, x => x.WidgetZone, storeScope);
                model.NumberOfProducts_OverrideForStore = _settingService.SettingExists(homePageNewProductsSettings, x => x.NumberOfProducts, storeScope);
            }

            return View("~/Plugins/Widgets.HomePageNewProducts/Views/WidgetsHomePageNewProducts/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var homePageNewProductsSettings = _settingService.LoadSetting<HomePageNewProductsSettings>(storeScope);

            // save settings
            homePageNewProductsSettings.WidgetZone = (WidgetZone)model.WidgetZoneId;
            homePageNewProductsSettings.NumberOfProducts = model.NumberOfProducts;

            /* We do not clear cache after each setting update.
            * This behavior can increase performance because cached settings will not be cleared 
            * and loaded from database after each update */
            if (model.WidgetZoneId_OverrideForStore || storeScope == 0)
            { 
                _settingService.SaveSetting(homePageNewProductsSettings, x => x.WidgetZone, storeScope, false);
            }
            else if (storeScope > 0)
            {
                _settingService.DeleteSetting(homePageNewProductsSettings, x => x.WidgetZone, storeScope);
            }

            if (model.NumberOfProducts_OverrideForStore || storeScope == 0)
            { 
                _settingService.SaveSetting(homePageNewProductsSettings, x => x.NumberOfProducts, storeScope, false);
            }
            else if (storeScope > 0)
            { 
                _settingService.DeleteSetting(homePageNewProductsSettings, x => x.NumberOfProducts, storeScope);
            }

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            if (_homePageNewProductsSettings.NumberOfProducts <= 0)
            {
                return Content("");
            }

            var products = this.GetNewestProducts();

            if (products.Count > _homePageNewProductsSettings.NumberOfProducts)
            {
                for (int i = products.Count - 1; i >= _homePageNewProductsSettings.NumberOfProducts; i--)
                {
                    products.RemoveAt(i);
                }
            }

            var preparedProductOverviewModels = ControllerExtensions.PrepareProductOverviewModels(this, _workContext,
                _storeContext, _categoryService, _productService, _specificationAttributeService,
                _priceCalculationService, _priceFormatter, _permissionService,
                _localizationService, _taxService, _currencyService,
                _pictureService, _webHelper, _cacheManager,
                _catalogSettings, _mediaSettings, products);

            var model = new List<ProductOverviewModel>();
            model.AddRange(preparedProductOverviewModels);

            return View("~/Plugins/Widgets.HomePageNewProducts/Views/WidgetsHomePageNewProducts/PublicInfo.cshtml", model);
        }

        private IList<Product> GetNewestProducts()
        {
            var query = from p in _productRepository.Table
                            orderby p.CreatedOnUtc descending
                            where p.Published &&
                            !p.Deleted &&
                            p.VisibleIndividually == true &&
                            p.MarkAsNew == true
                            select p;
            var products = query.ToList();
            return products;
        }
    }
}
