using niwrA.CommandManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace niwrA.CommandManager
{
  public class PlatformSpecific
  {
    public void PersistAllChanges(IEnumerable<ICommandProcessor> processors, ICommandStateRepository repo)
    {
      using (TransactionScope scope = new TransactionScope())
      {
        foreach (var processor in processors)
        {
          processor.PersistChanges();
        }
        repo.PersistChanges();
        scope.Complete();
      }
    }
  }
}
