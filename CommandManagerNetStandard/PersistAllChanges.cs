using niwrA.CommandManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.CommandManager
{
  public class PlatformSpecific
  {
    public void PersistAllChanges(IEnumerable<ICommandProcessor> processors, ICommandStateRepository repo)
    {
      foreach (var processor in processors)
      {
        processor.PersistChanges();
      }
      repo.PersistChanges();
    }
  }
}
