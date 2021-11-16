// ReSharper disable once CheckNamespace
namespace EagleEye.TestHelper.XUnit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Xunit.Sdk;

    [TraitDiscoverer(ExifToolDiscoverer.DISCOVERER_TYPE_NAME, TestHelperSettings.ASSEMBLY_NAME)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ExifToolAttribute : Attribute, ITraitAttribute
    {
    }
}
