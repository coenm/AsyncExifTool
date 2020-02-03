// ReSharper disable once CheckNamespace
namespace EagleEye.TestHelper.XUnit
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Xunit.Sdk;

    [TraitDiscoverer(PerformanceDiscoverer.DiscovererTypeName, TestHelperSettings.AssemblyName)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Xunit")]
    public class PerformanceAttribute : Attribute, ITraitAttribute
    {
    }
}
