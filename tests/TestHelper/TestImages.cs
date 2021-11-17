namespace TestHelper
{
    using System.IO;

    public static class TestImages
    {
        private const string INPUT_IMAGES_RELATIVE_PATH = @"Images\data\";

        /// <summary>
        /// Gets the correct full path to the Images directory.
        /// </summary>
        public static string InputImagesDirectoryFullPath => TestEnvironment.GetFullPath(INPUT_IMAGES_RELATIVE_PATH);

        /// <summary>Read image from relative filename for testing purposes.</summary>
        /// <param name="relativeFilename">Filename relative to 'solution directory' + 'images/data/'. </param>
        /// <returns>FileStream or throws exception.</returns>
        public static FileStream ReadRelativeImageFile(string relativeFilename)
        {
            var fullFilename = TestEnvironment.GetFullPath(INPUT_IMAGES_RELATIVE_PATH, relativeFilename);
            return File.OpenRead(fullFilename);
        }
    }
}
