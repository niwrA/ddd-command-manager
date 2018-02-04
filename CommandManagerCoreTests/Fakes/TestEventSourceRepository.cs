using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommandManagerCoreTests.Fakes
{
  public class RootEntityState : IRootEntityState
  {
    public RootEntityState(Guid guid)
    {
      Guid = guid;
    }
    public string Name { get; set; }
    public string Email { get; set; }
    public Guid Guid { get; }
  }

  public class TestServiceEventSourceRepository : ITestServiceRepository
  {
    private ConcurrentDictionary<Guid, IRootEntityState> _rootEntities = new ConcurrentDictionary<Guid, IRootEntityState>();

    public IRootEntityState CreateRootEntityState(Guid guid, string name)
    {
      IRootEntityState state = GetOrCreateRootEntityState(guid);
      state.Name = name;
      return state;
    }

    private IRootEntityState GetOrCreateRootEntityState(Guid guid)
    {
      return _rootEntities.GetOrAdd(guid, new RootEntityState(guid));
    }

    public IRootEntityState GetRootEntityState(Guid guid)
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
