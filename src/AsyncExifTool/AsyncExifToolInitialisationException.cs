namespace CoenM.ExifToolLib
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// AsyncExifTool Initialistion Exception.
    /// </summary>
    [Serializable]
    public sealed class AsyncExifToolInitialisationException : Exception
    {
        internal AsyncExifToolInitialisationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <remarks>Required because AsyncExifToolInitialisationException implements ISerializable interface.</remarks>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private AsyncExifToolInitialisationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
