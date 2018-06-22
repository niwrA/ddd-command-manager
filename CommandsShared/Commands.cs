using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using niwrA.CommandManager.Exceptions;
using niwrA.CommandManager.Contracts;

namespace niwrA.CommandManager
{
    // define the interface for holding a command's state for instance for storage in a repository

    // contains the shared logic for all commands
    public class CommandBase
    {
        private ICommandState _state;
        private ICommandStateRepository _repository;
        public CommandBase()
        {
        }
        public CommandBase(ICommandStateRepository repo) : this()
        {
            _repository = repo;
            InitState();
        }

        public CommandBase(ICommandStateRepository repo, ICommandState state) : this(repo)
        {
            this._state = state;
        }
        public DateTime CreatedOn { get { return _state.CreatedOn; } set { _state.CreatedOn = value; } }
        public DateTime? ReceivedOn { get { return _state.ReceivedOn; } set { _state.ReceivedOn = value; } }
        public string UserName { get { return _state.UserName; } set { _state.UserName = value; } }
        public string TenantId { get { return _state.TenantId; } set { _state.TenantId = value; } }

        private void InitState()
        {
            if (_state == null && _repository != null)
            {
                this._state = _repository.CreateCommandState(Guid.NewGuid());
            }
            if (_state != null)
            {
                this._state.Command = this.GetType().Name;
                if (_state.Guid == null || _state.Guid == Guid.Empty)
                {
                    _state.Guid = Guid.NewGuid();
                }
            }
        }

        public virtual string ParametersJson
        {
            get
            {
                return _state.ParametersJson;
            }
            set
            {
                _state.ParametersJson = value;
            }
        }
        public Guid Guid { get { return _state.Guid; } set { _state.Guid = value; } }
        public string Entity { get { return _state.Entity; } set { _state.Entity = value; } }
        public Guid EntityGuid { get { return _state.EntityGuid; } set { _state.EntityGuid = value; } }

        public string EntityRoot { get { return _state.EntityRoot; } set { _state.EntityRoot = value; } }
        public Guid EntityRootGuid { get { return _state.EntityRootGuid; } set { _state.EntityRootGuid = value; } }

        public DateTime? ExecutedOn { get { return _state.ExecutedOn; } set { _state.ExecutedOn = value; } }

        public string Command { get { return _state.Command; } set { _state.Command = value; } }
        public ICommandState State { get { return _state; } set { _state = value; } }

        public ICommandStateRepository CommandRepository { get { return _repository; } set { _repository = value; InitState(); } }

        private ICommandProcessor _commandProcessor;
        public virtual ICommandProcessor CommandProcessor { get { return _commandProcessor; } set { _commandProcessor = value; } }

    }
    // the domain class for Commands


    public class ProcessorConfig : IProcessorConfig
    {
        public ProcessorConfig(string entityRoot, ICommandProcessor processor, string nameSpace, string assembly)
        {
            EntityRoot = entityRoot;
            Processor = processor;
            NameSpace = nameSpace;
            Assembly = assembly;
        }
        public ICommandProcessor Processor { get; }
        public string EntityRoot { get; }
        public string NameSpace { get; }
        public string Assembly { get; }
        public ICommand GetCommand(string name, string entity, string parametersJson)
        {
            var commandConfig = new CommandConfig(name, entity, this.Processor, this.NameSpace, this.Assembly);
            return commandConfig.GetCommand(parametersJson);
        }
    }
    public class CommandConfig : ICommandConfig
    {
        public CommandConfig(string commandName, string entity, ICommandProcessor processor, string nameSpace, string assembly)
        {
            CommandName = commandName;
            Entity = entity;
            Processor = processor;
            NameSpace = nameSpace;
            Assembly = assembly;
        }
        public string Key { get { return CommandName + Entity + "Command"; } }
        public string CommandName { get; }
        public string Entity { get; }
        public string NameSpace { get; }
        public string Assembly { get; }
        public ICommandProcessor Processor { get; }
        public ICommand GetCommand(string json)
        {
            ICommand command;

            var type = Type.GetType(NameSpace + "." + Key + ", " + Assembly);

            if (type == null)
            {
                throw new TypeNotFoundException($"{Key} not found in {NameSpace} of {Assembly}");
            }

            if (!string.IsNullOrWhiteSpace(json))
            {
                var insert = @"'$type': '" + NameSpace + "." + Key + @", " + Assembly + "', ";
                json = json.Trim().Insert(1, insert);
                command = JsonConvert.DeserializeObject<ICommand>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            else
            {
                command = Activator.CreateInstance(type) as ICommand;
            }
            return command;
        }
    }
    [Serializable]
    public class CommandDto : ICommandDto
    {
        private ICommandState _state;
        private string _entityRoot = "";
        private Guid _entityRootGuid;

        public CommandDto()
        {
        }
        public CommandDto(ICommandState state)
        {
            this.Guid = state.Guid;
            this.Entity = state.Entity;
            this.EntityGuid = state.EntityGuid;
            this.EntityRoot = state.EntityRoot;
            this.EntityRootGuid = state.EntityRootGuid;
            this.ExecutedOn = state.ExecutedOn;
            this.Command = state.Command; // we already have the proper name, so perhaps this can be done more cleanly,
            this.UserName = state.UserName;
            this.TenantId = state.TenantId;
            // or we should save the CommandTypeId differently into the CommandState Table, ie. without EntityCommand suffix
            this.ParametersJson = state.ParametersJson;
            _state = state;
        }

        public Guid Guid { get; set; }
        public Guid EntityGuid { get; set; }
        public string Entity { get; set; }
        // default to Entity values if only Entity is provided,
        // for backward compatibility and ease of use
        public string EntityRoot
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_entityRoot)) { return Entity; } else { return _entityRoot; }
            }
            set { _entityRoot = value; }
        }
        public Guid EntityRootGuid
        {
            get
            {
                if (Guid.Empty == _entityRootGuid) { return EntityGuid; } else { return _entityRootGuid; }
            }
            set { _entityRootGuid = value; }
        }
        public string Command { get; set; }
        public string CommandVersion { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }
        public string ParametersJson { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ExecutedOn { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public IParametersDto ParametersDto { get; set; }
    }


    public interface IDateTimeProvider
    {
        DateTime GetSessionDateTime();
        DateTime GetServerDateTime();
    }
}
