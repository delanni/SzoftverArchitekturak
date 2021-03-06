﻿using System.Web;
using System.Web.Optimization;

namespace PocsKft
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/ext/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/ext/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/ext/jquery.unobtrusive*",
                        "~/Scripts/ext/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/ext/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css").Include("~/Content/glyphreg.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new ScriptBundle("~/bundles/appmain")
                .Include("~/Scripts/appMain.js")
                .Include("~/Scripts/GlobalService.js")
                .Include("~/Scripts/BrowserController.js")
                .Include("~/Scripts/Communicator.js")
                .Include("~/Scripts/PropertiesController.js")
                .Include("~/Scripts/ActionBarController.js")
                .Include("~/Scripts/RevertController.js")
                .Include("~/Scripts/SearchController.js")
                .Include("~/Scripts/User.js")
                .Include("~/Scripts/File.js")
                .Include("~/Scripts/Project.js")
                .Include("~/Scripts/Version.js")
                .Include("~/Scripts/Property.js")
                .Include("~/Scripts/bootstrap.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Scripts/ext/angular.js"));
        }
    }
}