using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommandManagerCoreTests.Fakes
{
    public class RootEntityState : IRootEntityState
    {
        public RootEntityState(string guid)
        {
            Guid = guid;
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Guid { get; }
    }
    public class TestServiceRepository : ITestServiceRepository
    {
        private ConcurrentDictionary<string, IRootEntityState> _rootEntities = new ConcurrentDictionary<string, IRootEntityState>();

        public IRootEntityState CreateRootEntityState(string guid, string name)
        {
            IRootEntityState state = GetOrCreateRootEntityState(guid);
            state.Name = name;
            return state;
        }

        private IRootEntityState GetOrCreateRootEntityState(string guid)
        {
            return _rootEntities.GetOrAdd(guid, new RootEntityState(guid));
        }

        public IRootEntityState GetRootEntityState(string guid)
        {
            return GetOrCreateRootEntityState(guid);
        }

        public void PersistChanges()
        {
        }

        public Task PersistChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
    public class TestServiceEventSourceRepository : ITestServiceRepository
    {
        private ConcurrentDictionary<string, IRootEntityState> _rootEntities = new ConcurrentDictionary<string, IRootEntityState>();

        public IRootEntityState CreateRootEntityState(string guid, string name)
        {
            IRootEntityState state = GetOrCreateRootEntityState(guid);
            state.Name = name;
            return state;
        }

        private IRootEntityState GetOrCreateRootEntityState(string guid)
        {
            return _rootEntities.GetOrAdd(guid, new RootEntityState(guid));
        }

        public IRootEntityState GetRootEntityState(string guid)
        {
            return GetOrCreateRootEntityState(guid);
        }

        public void PersistChanges()
        {
        }

        public Task PersistChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
