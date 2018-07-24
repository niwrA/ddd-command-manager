using niwrA.CommandManager;
using niwrA.CommandManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace niwrA.CommandManager
{
    public class PlatformSpecific
    {
        public void PersistAllChanges(IEnumerable<ICommandProcessor> processors, ICommandService service)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,
                                         new System.TimeSpan(0, 60, 0)))
            {
                foreach (var processor in processors)
                {
                    processor.PersistChanges();
                }
                service.PersistChanges();
                scope.Complete();
            }
        }
    }
}
