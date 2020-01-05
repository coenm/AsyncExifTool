namespace CoenM.ExifToolLib.Logging
{
    public enum LogLevel
    {
        /// <summary>
        /// For debugging.
        /// </summary>
        Debug,

        /// <summary>
        /// Normal behavior.
        /// </summary>
        Info,

        /// <summary>
        /// Something unexpected; application will continue.
        /// </summary>
        Warn,

        /// <summary>
        /// Something failed; application may or may not continue
        /// </summary>
        Error,

        /// <summary>
        /// Something bad happened; application is going down
        /// </summary>
        Fatal
    }
}
