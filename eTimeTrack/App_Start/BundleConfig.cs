using System.Web.Optimization;

namespace eTimeTrack
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui.min.js",
                        "~/Scripts/moment.min.js",
                        "~/Scripts/stacktable.js",
                        "~/Scripts/DatepickerSetup.js",
                        "~/Scripts/site.js"));

            bundles.Add(new ScriptBundle("~/bundles/withinviewport").Include(
                "~/Scripts/withinviewport.js",
                "~/Scripts/jquery.withinviewport.js"));

            bundles.Add(new ScriptBundle("~/bundles/tablesaw").Include(
                "~/Scripts/Tablesaw/tablesaw.jquery.js",
                "~/Scripts/Tablesaw/tablesaw-init.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js"
                ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.flatly.css",
                        "~/Content/stacktable.css",
                        "~/Content/tablesaw.css",
                        "~/Content/site.css",
                        "~/Content/jquery-ui.css"));

            bundles.Add(new ScriptBundle("~/bundles/notify").Include(
                        "~/Scripts/notify.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/Scripts/DataTables/jquery.dataTables.min.js",
                        "~/Scripts/DataTables/dataTables.responsive.min.js",
                        "~/Scripts/data-tables-initializer.js"
            ));

            bundles.Add(new StyleBundle("~/Content/tablegraphicscss").Include(
                        "~/Content/DataTables/css/dataTables.bootstrap.min.css",
                        "~/Content/DataTables/css/dataTables.jqueryui.css",
                        "~/Content/DataTables/css/responsive.bootstrap.min.css",
                        "~/Content/DataTables/css/responsive.dataTables.min.css"
            ));
        }
    }
}
