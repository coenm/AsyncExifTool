// ReSharper disable once CheckNamespace
namespace EagleEye.TestHelper.XUnit
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Xunit.Sdk;

    [TraitDiscoverer(ExifToolDiscoverer.DiscovererTypeName, TestHelperSettings.AssemblyName)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "XUnit")]
    public class ExifToolAttribute : Attribute, ITraitAttribute
    {
    }
}
