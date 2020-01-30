namespace CoenM.ExifToolLib.Internals.Stream
{
    internal interface IBytesWriter
    {
        void Write(byte[] buffer, int offset, int count);
    }
}
