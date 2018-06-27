using niwrA.CommandManager.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace niwrA.CommandManager.Repositories
{
    public class CommandState : ICommandState
    {
        public Guid Guid { get; set; }
        public string EntityGuid { get; set; }
        public string Entity { get; set; }
        public string EntityRootGuid { get; set; }
        public string EntityRoot { get; set; }
        public string Command { get; set; }
        public string CommandVersion { get; set; }
        public string ParametersJson { get; set; }
        public DateTime? ExecutedOn { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }
    }
    public class CommandStateRepositoryInMemory : ICommandStateRepository
    {
        private ConcurrentDictionary<Guid, ICommandState> _commandDictionary = new ConcurrentDictionary<Guid, ICommandState>();
        public ICommandState CreateCommandState(Guid guid)
        {
            if (_commandDictionary.TryGetValue(guid, out ICommandState state)) { }
            else
            {
                state = new CommandState { Guid = guid };
                if (!_commandDictionary.TryAdd(guid, state))
                {
                    // can only fail if the key already exists, in which case trygetvalue would have returned a value
                    // do we need to deal with the tiny chance that inbetween these two calls another thread added the
                    // same key when we always create new Guids?
                };
            }
            return state;
        }

        public IEnumerable<ICommandState> GetCommandStates()
        {
            return _commandDictionary.Values;
        }

        public IEnumerable<ICommandState> GetCommandStates(string entityGuid)
        {
            return _commandDictionary.Values.Where(s => s.EntityGuid == entityGuid).ToList();
        }

        public IEnumerable<ICommandState> GetUnprocessedCommandStates()
        {
            return _commandDictionary.Values.Where(s => s.ExecutedOn == null).ToList();
        }

        public void PersistChanges()
        {
            // no additional work needed, this is in-memory
        }
    }
}
