using System;
using System.Runtime.Serialization;

namespace niwrA.EventManager
{
    [Serializable]
    internal class EventNotConfiguredException : Exception
    {
        public EventNotConfiguredException()
        {
        }

        public EventNotConfiguredException(string message) : base(message)
        {
        }

        public EventNotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EventNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}