/*
 *  Contains the bits of code to hook the connector into Umbraco
 * 
 *  these methods allow us to use a RazorClassLibrary to add the 
 *  files to an Umbraco 10+ site without the need for build 
 *  targets etc.
 * 
 */

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Routing;

namespace ReferenceConnector;

/// <summary>
///  Umbracon Composer
/// </summary>
public class ReferenceSetup : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddReferenceConnector();
    }
}

public static class ReferenceBuilderExtensions
{
    public static IUmbracoBuilder AddReferenceConnector(this IUmbracoBuilder builder)
    {
        // v10 sites need a manifest fileter as they cannot read package.manifests
        // from the RCL folders (v11 can, but this still works for v11).
        if (!builder.ManifestFilters().Has<ReferenceConnectorManifestFilter>())
            builder.ManifestFilters().Append<ReferenceConnectorManifestFilter>();

        return builder;
    }
}

public class ReferenceConnectorManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new PackageManifest 
        {
            PackageName = ReferenceConstants.Name,
            AllowPackageTelemetry = false,
            Version = GetType().Assembly.GetName().Version?.ToString(3) ?? "1.0",
            Scripts = new[] {
                WebPath.Combine(ReferenceConstants.AppPlugins, "config.controller.js"),
                WebPath.Combine(ReferenceConstants.AppPlugins, "pending.controller.js"),
                WebPath.Combine(ReferenceConstants.AppPlugins, "submitted.controller.js"),
            }
        });
    }
}



