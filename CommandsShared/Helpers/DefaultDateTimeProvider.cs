using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.CommandManager.Helpers
{
  public class DefaultDateTimeProvider : IDateTimeProvider
  {
    private DateTime? _sessionDateTime;
    /// <summary>
    /// Will return the current UTC datetime
    /// </summary>
    /// <returns></returns>
    private DateTime GetUtcDateTime()
    {
      return DateTime.UtcNow;
    }
    public DateTime GetServerDateTime()
    {
      return GetUtcDateTime();
    }
    /// <summary>
    /// Will return the same UTC datetime for the duration of the session
    /// </summary>
    /// <returns></returns>
    public DateTime GetSessionDateTime()
    {
      if (_sessionDateTime == null)
      {
        _sessionDateTime = GetUtcDateTime();
      }
      return _sessionDateTime.Value;
    }
  }

}
