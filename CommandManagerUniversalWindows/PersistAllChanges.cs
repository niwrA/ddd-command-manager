using niwrA.CommandManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.CommandManager
{
  public class PlatformSpecific
  {
    public void PersistAllChanges(IEnumerable<ICommandProcessor> processors, ICommandService service)
    {
      foreach (var processor in processors)
      {
        processor.PersistChanges();
      }
      service.PersistChanges();
    }
  }
}
