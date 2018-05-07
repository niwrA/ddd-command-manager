using niwrA.CommandManager;
using niwrA.CommandManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandManagerCoreTests.Fakes
{
  public interface IRootEntityState
  {
    Guid Guid { get; }
    string Name { get; set; }
    string Email { get; set; }
  }

  public interface IRootEntity
  {
    string Name { get; }
    string Email { get; }

    void Rename(string name);
    void ChangeEmail(string email);
  }

  public class RootEntity : IRootEntity
  {
    private IRootEntityState _state;

    public RootEntity(IRootEntityState state)
    {
      _state = state;
    }
    public string Name { get { return _state.Name; } }

    public string Email { get { return _state.Email; } }

    public void ChangeEmail(string email)
    {
      _state.Email = email;
    }

    public void Rename(string name)
    {
      _state.Name = name;
    }
  }
  public class CreateRootEntityCommand : CommandBase, ICommand
  {
    public string Name { get; set; }
    public void Execute()
    {
      ((ITestService)CommandProcessor).CreateRootEntity(EntityGuid, Name);
    }
  }
  public class RenameRootEntityCommand : CommandBase, ICommand
  {
    public string Name { get; set; }
    public void Execute()
    {
      var testObject = ((ITestService)CommandProcessor).GetRootEntity(EntityGuid);
      testObject.Rename(Name);
    }
  }
  public class ChangeEmailForRootEntityCommand : CommandBase, ICommand
  {
    public string Email { get; set; }
    public void Execute()
    {
      var testObject = ((ITestService)CommandProcessor).GetRootEntity(EntityGuid);
      testObject.ChangeEmail(Email);
    }
  }
  public class RenameChildEntityCommand : CommandBase, ICommand
  {
    public string OriginalName { get; set; }
    public string Name { get; set; }
    public void Execute()
    {
      var testObject = ((ITestService)CommandProcessor).GetRootEntity(EntityGuid);
      // todo: get child
    }
  }
  public interface ITestServiceRepository : IEntityRepository
  {
    IRootEntityState CreateRootEntityState(Guid guid, string name);
    IRootEntityState GetRootEntityState(Guid guid);
  }
  public interface ITestService : ICommandProcessor
  {
    IRootEntity GetRootEntity(Guid guid);
    IRootEntity CreateRootEntity(Guid guid, string Name);
  }
  public class TestService : ITestService
  {
    private ITestServiceRepository _repo;
    public TestService() { }
    public TestService(ITestServiceRepository repo)
    {
      _repo = repo;
    }
    public IRootEntity GetRootEntity(Guid guid)
    {
      var state = _repo.GetRootEntityState(guid);
      return new RootEntity(state);
    }
    public IRootEntity CreateRootEntity(Guid guid, string name)
    {
      var state = _repo.CreateRootEntityState(guid, name);
      return new RootEntity(state);
    }
    public void PersistChanges()
    {
      _repo.PersistChanges();
    }
  }
}
