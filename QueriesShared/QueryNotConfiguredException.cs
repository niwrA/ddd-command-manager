using System;
using System.Runtime.Serialization;

namespace niwrA.QueryManager
{
    [Serializable]
    internal class QueryNotConfiguredException : Exception
    {
        public QueryNotConfiguredException()
        {
        }

        public QueryNotConfiguredException(string message) : base(message)
        {
        }

        public QueryNotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected QueryNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}