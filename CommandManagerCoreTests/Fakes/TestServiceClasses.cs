using niwrA.CommandManager;
using niwrA.CommandManager.Contracts;
using niwrA.QueryManager;
using niwrA.QueryManager.Contracts;
using niwrA.EventManager;
using niwrA.EventManager.Contracts;
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
    public class HandleRootEntityEvent : EventBase, IEvent
    {
        public string Name { get; set; }
        public void Execute()
        {
            ((ITestEventService)EventProcessor).Handle(EntityGuid, Name);
        }
    }
    public class GetRootEntityQuery : QueryBase, IQuery
    {
        public string Name { get; set; }
        public void Execute()
        {
            ((ITestQueryService)QueryProcessor).Get(EntityGuid, Name);
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
    public interface ITestQueryService : IQueryProcessor
    {
        IRootEntity Get(Guid guid);
        IRootEntity Get(Guid guid, string Name);
    }
    public interface ITestEventService : IEventProcessor
    {
        IRootEntity Handle(Guid guid);
        IRootEntity Handle(Guid guid, string Name);
    }
    public class TestEventService : ITestEventService
    {
        public IRootEntity Handle(Guid guid)
        {
            throw new NotImplementedException();
        }

        public IRootEntity Handle(Guid guid, string Name)
        {
            throw new NotImplementedException();
        }
    }
    public class TestQueryService : ITestQueryService
    {
        public IRootEntity Get(Guid guid)
        {
            throw new NotImplementedException();
        }

        public IRootEntity Get(Guid guid, string Name)
        {
            throw new NotImplementedException();
        }
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
