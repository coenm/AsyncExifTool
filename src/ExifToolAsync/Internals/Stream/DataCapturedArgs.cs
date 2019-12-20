namespace ExifToolAsync.Internals.Stream
{
    using System;

    public class DataCapturedArgs : EventArgs
    {
        public DataCapturedArgs(string key, string data)
        {
            Key = key;
            Data = data;
        }

        public string Key { get; }

        public string Data { get; }
    }
}
