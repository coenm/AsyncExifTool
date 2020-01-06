// ReSharper disable once CheckNamespace
namespace EagleEye.TestHelper.XUnit
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Xunit.Abstractions;
    using Xunit.Sdk;

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "XUnit")]
    public class ExifToolDiscoverer : ITraitDiscoverer
    {
        public const string DiscovererTypeName = TestHelperSettings.Namespace + nameof(ExifToolDiscoverer);

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Category", "ExifTool");
        }
    }
}
