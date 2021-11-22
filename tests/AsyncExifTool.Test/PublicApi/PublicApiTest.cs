namespace CoenM.ExifToolLibTest.PublicApi
{
#if !NETCOREAPP2_1
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CoenM.ExifToolLib;
    using CoenM.ExifToolLibTest.TestInternals;
    using EagleEye.TestHelper.XUnit;
    using FluentAssertions;
    using PublicApiGenerator;
    using TestHelper;
    using TestHelper.XUnit.Facts;
    using VerifyXunit;
    using Xunit;
    using Xunit.Abstractions;

    [UsesVerify]
    public class PublicApiTest
    {
        [Fact]
        public async Task PublicApi_ShouldNotChangeUnexpected()
        {
            // arrange

            // act
            var publicApi = typeof(AsyncExifTool).Assembly.GeneratePublicApi();

            // assert
            await Verifier.Verify(publicApi).UniqueForTargetFrameworkAndVersion();
        }
    }
#endif
}