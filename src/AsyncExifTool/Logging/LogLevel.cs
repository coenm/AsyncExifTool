namespace CoenM.ExifToolLib.Logging
{
    public enum LogLevel
    {
        /// <summary>
        /// For trace debugging; begin method X, end method X
        /// </summary>
        Trace,

        /// <summary>
        /// For debugging.
        /// </summary>
        Debug,

        /// <summary>
        /// Normal behavior.
        /// </summary>
        Info,

        /// <summary>
        /// Something unexpected; AsyncExifTool library will continue.
        /// </summary>
        Warn,

        /// <summary>
        /// Something failed; AsyncExifTool library may or may not continue
        /// </summary>
        Error,

        /// <summary>
        /// Something bad happened; AsyncExifTool library is going down
        /// </summary>
        Fatal,
    }
}
