// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using RequireJsNet.Configuration;
using RequireJsNet.Helpers;
using RequireJsNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace RequireJsNet
{
    public static class RequireJsHtmlHelpers
    {
        public static MvcHtmlString RenderRequireJsSetup(this HtmlHelper html, string configurationName = HttpModule.RequireJsRouteHandler.DEFAULT_CONFIG_NAME, bool inlineConfigInHtml = true)
        {
            var handler = html.RouteCollection
                .OfType<System.Web.Routing.Route>()
                .Select(r => r.RouteHandler)
                .OfType<HttpModule.RequireJsRouteHandler>()
                .SingleOrDefault(); //TODO: Accept multiple registrations and use the one with the registered configuration?!

            if (handler == null)
                throw new ApplicationException("RequireJSRoutingHandler must be registered in Global.aspx before RenderRequreJsSetup() can be called with names.");


            var config = handler.Get(configurationName);

            var entryPointPath = html.RequireJsEntryPoint(config.BaseUrl, config.EntryPointRoot);
            if (entryPointPath == null)
                return new MvcHtmlString(string.Empty);

            if (inlineConfigInHtml)
                return RenderRequireJsSetup_Inline(html.ViewContext.HttpContext, config, entryPointPath);
            else
            {
                return RenderRequireJsSetup_Offline(handler, configurationName, config, entryPointPath);
            }
        }

        /// <summary>
        /// Setup RequireJS to be used in layouts
        /// </summary>
        /// <param name="config">
        /// Configuration object for various options.
        /// </param>
        /// <returns>
        /// Script code required to kickstart requirejs on the requested page.
        /// </returns>
        public static MvcHtmlString RenderRequireJsSetup(this HtmlHelper html, RequireRendererConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var entryPointPath = html.RequireJsEntryPoint(config.BaseUrl, config.EntryPointRoot);
            if (entryPointPath == null)
                return new MvcHtmlString(string.Empty);

            if (config.ConfigurationFiles == null || !config.ConfigurationFiles.Any())
                throw new Exception("No config files to load.");

            return RenderRequireJsSetup_Inline(html.ViewContext.HttpContext, config, entryPointPath);
        }

        private static MvcHtmlString RenderRequireJsSetup_Inline(System.Web.HttpContextBase httpContext, RequireRendererConfiguration config, MvcHtmlString entryPointPath)
        {
            string configScript = buildConfigScript(httpContext, config, entryPointPath).Render();

            return new MvcHtmlString(string.Concat(
                configScript + Environment.NewLine,
                bootstrappingScripts(config, entryPointPath)
            ));
        }

        private static MvcHtmlString RenderRequireJsSetup_Offline(HttpModule.RequireJsRouteHandler handler, string configName, RequireRendererConfiguration config, MvcHtmlString entryPointPath)
        {
            var prefix = System.Web.VirtualPathUtility.ToAbsolute($"~/{handler.RoutePrefix}");
            var configScript = $"<script src=\"{prefix}/{configName}/{entryPointPath}/\"></script>";

            return new MvcHtmlString(string.Concat(
                configScript + Environment.NewLine,
                bootstrappingScripts(config, entryPointPath)
            ));
        }

        private static string bootstrappingScripts(RequireRendererConfiguration config, MvcHtmlString entryPointPath)
        {
            string requireJsScript = buildRequireJsScript(config.RequireJsUrl);
            string requireEntrypointScript = buildRequireEntrypointScript(entryPointPath);

            return string.Concat(
                requireJsScript + Environment.NewLine,
                requireEntrypointScript);
        }

        internal static JavaScriptBuilder buildConfigScript(System.Web.HttpContextBase httpContext, RequireRendererConfiguration config, MvcHtmlString entryPointPath)
        {
            var processedConfigs = config.ConfigurationFiles.Select(r =>
            {
                var resultingPath = httpContext.MapPath(r);
                PathHelpers.VerifyFileExists(resultingPath);
                return resultingPath;
            }).ToList();

            var resultingConfig = GetCachedOverridenConfig(processedConfigs, config, entryPointPath.ToString());

            var locale = config.LocaleSelector(httpContext);

            var outputConfig = createOutputConfigFrom(resultingConfig, config, locale);

            var options = createOptionsFrom(httpContext, config, locale);

            var configBuilder = new JavaScriptBuilder();
            configBuilder.AddStatement(JavaScriptHelpers.SerializeAsVariable(options, "requireConfig"));
            configBuilder.AddStatement(JavaScriptHelpers.SerializeAsVariable(outputConfig, "require"));

            return configBuilder;
        }

        private static string buildRequireJsScript(string requireJsUrl)
        {
            var requireRootBuilder = new JavaScriptBuilder();
            requireRootBuilder.AddAttributesToStatement("src", requireJsUrl);
            return requireRootBuilder.Render();
        }

        private static string buildRequireEntrypointScript(MvcHtmlString entryPointPath)
        {
            var requireEntryPointBuilder = new JavaScriptBuilder();
            requireEntryPointBuilder.AddStatement(
                JavaScriptHelpers.MethodCall(
                "require",
                (object)new[] { entryPointPath.ToString() }));

            return requireEntryPointBuilder.Render();
        }

        internal static JsonRequireOptions createOptionsFrom(System.Web.HttpContextBase httpContext, RequireRendererConfiguration config, string locale)
        {
            var options = new JsonRequireOptions
            {
                Locale = locale,
                PageOptions = RequireJsOptions.GetPageOptions(httpContext),
                WebsiteOptions = RequireJsOptions.GetGlobalOptions(httpContext)
            };

            config.ProcessOptions(options);
            return options;
        }

        internal static JsonRequireOutput createOutputConfigFrom(ConfigurationCollection resultingConfig, RequireRendererConfiguration config, string locale)
        {
            var outputConfig = new JsonRequireOutput
            {
                BaseUrl = config.BaseUrl,
                Locale = locale,
                UrlArgs = config.UrlArgs,
                WaitSeconds = config.WaitSeconds,
                Paths = resultingConfig.Paths.PathList.ToDictionary(r => r.Key, r => r.Value),
                Packages = resultingConfig.Packages.PackageList,
                Shim = resultingConfig.Shim.ShimEntries.ToDictionary(
                        r => r.For,
                        r => new JsonRequireDeps
                        {
                            Dependencies = r.Dependencies.Select(x => x.Dependency).ToList(),
                            Exports = r.Exports
                        }),
                Map = resultingConfig.Map.MapElements.ToDictionary(
                         r => r.For,
                         r => r.Replacements.ToDictionary(x => x.OldKey, x => x.NewKey))
            };

            config.ProcessConfig(outputConfig);

            return outputConfig;
        }

        private static HashStore<ConfigurationCollection> configObjectHash = new HashStore<ConfigurationCollection>();

        private static ConfigurationCollection GetCachedOverridenConfig(
            List<string> processedConfigs,
            RequireRendererConfiguration config,
            string entryPointPath)
        {
            if (config.CacheConfigObject)
            {
                return configObjectHash.GetOrSet(
                    ComputeConfigObjectHash(processedConfigs, entryPointPath),
                    () => GetOverridenConfig(processedConfigs, config, entryPointPath));
            }

            return GetOverridenConfig(processedConfigs, config, entryPointPath);
        }

        private static string ComputeConfigObjectHash(List<string> processedConfigs, string entryPointPath)
        {
            return string.Join("|", processedConfigs) + "|" + entryPointPath;
        }

        private static ConfigurationCollection GetOverridenConfig(
            List<string> processedConfigs,
            RequireRendererConfiguration config,
            string entryPointPath)
        {
            var loader = new ConfigLoader(
                processedConfigs,
                config.Logger,
                new ConfigLoaderOptions
                    {
                        LoadOverrides = config.LoadOverrides,
                        CachingPolicy = config.ConfigCachingPolicy
                    });
            var resultingConfig = loader.Get();

            var overrider = new ConfigOverrider();
            overrider.Override(resultingConfig, entryPointPath.ToModuleName());

            return resultingConfig;
        }

        /// <summary>
        /// Returns entry point script relative path
        /// </summary>
        /// <param name="html">
        /// The HtmlHelper instance.
        /// </param>
        /// <param name="root">
        /// Relative root path ex. ~/Scripts/
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString RequireJsEntryPoint(this HtmlHelper html, string baseUrl, string root)
        {
            var result = RequireJsOptions.ResolverCollection.Resolve(html.ViewContext, baseUrl, root);

            return result != null ? new MvcHtmlString(result) : null;
        }

        public static Dictionary<string, int> ToJsonDictionary<TEnum>()
        {
            var enumType = typeof(TEnum);
            return Enum.GetNames(enumType).ToDictionary(r => r, r => Convert.ToInt32(Enum.Parse(enumType, r)));
        }


        
    }
}