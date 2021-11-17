// ReSharper disable once CheckNamespace
namespace EagleEye.TestHelper.XUnit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Xunit.Sdk;

    [TraitDiscoverer(PerformanceDiscoverer.DISCOVERER_TYPE_NAME, TestHelperSettings.ASSEMBLY_NAME)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Xunit")]
    public class PerformanceAttribute : Attribute, ITraitAttribute
    {
    }
}
