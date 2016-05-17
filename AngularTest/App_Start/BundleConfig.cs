using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace ChallengerModeTest
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/scripts/jquery").IncludeDirectory("~/scripts/jquery", "*.js", true));
            bundles.Add(new StyleBundle("~/styles/jquery").IncludeDirectory("~/scripts/jquery", "*.css", true));
        }
    }
}