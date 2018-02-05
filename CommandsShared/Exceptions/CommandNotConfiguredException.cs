using System;

namespace niwrA.CommandManager.Exceptions
{
    internal class CommandNotConfiguredException : Exception
    {
        public CommandNotConfiguredException()
        {
        }

        public CommandNotConfiguredException(string message) : base(message)
        {
        }

        public CommandNotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}