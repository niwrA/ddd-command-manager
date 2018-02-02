using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CommandsShared
{
    // define the interface for holding a command's state for instance for storage in a repository
    public interface ICommand
    {
        Guid Guid { get; set; }

        string Entity { get; set; }
        Guid EntityGuid { get; set; }

        string EntityRoot { get; set; }
        Guid EntityRootGuid { get; set; }

        string CommandTypeId { get; set; }
        string ParametersJson { get; set; }

        DateTime? ExecutedOn { get; set; }
        DateTime? ReceivedOn { get; set; }
        DateTime CreatedOn { get; set; }

        string UserName { get; set; }

        ICommandStateRepository CommandRepository { get; set; }
        ICommandProcessor CommandProcessor { get; set; }

        void Execute();
    }

    public interface ICommandState
    {
        Guid Guid { get; set; }
        Guid EntityGuid { get; set; }
        string Entity { get; set; }
        Guid EntityRootGuid { get; set; }
        string EntityRoot { get; set; }
        string Command { get; set; }
        string CommandVersion { get; set; }
        string ParametersJson { get; set; }
        DateTime? ExecutedOn { get; set; }
        DateTime? ReceivedOn { get; set; }
        DateTime CreatedOn { get; set; }
        string UserName { get; set; }
    }
    // defines the contract for a Command Repository implementation
    public interface ICommandStateRepository
    {
        void PersistChanges();
        ICommandState CreateCommandState();
        IEnumerable<ICommandState> GetCommandStates();
        IEnumerable<ICommandState> GetCommandStates(Guid entityGuid);
        IEnumerable<ICommandState> GetUnprocessedCommandStates();
    }

    public interface ITimeStampedEntityState
    {
        Guid Guid { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime UpdatedOn { get; set; }
    }
    public interface INamedEntity
    {
        DateTime CreatedOn { get; }
        DateTime UpdatedOn { get; }
        Guid Guid { get; }
        string Name { get; }
    }

    public interface INamedEntityState : ITimeStampedEntityState
    {
        string Name { get; set; }
    }
    // defines the contract for an Entity Repository implementation
    public interface IEntityRepository
    {
        void PersistChanges();
        Task PersistChangesAsync();
    }
    public interface IEntityReadOnlyRepository { }

    // defines the contract for entities compatible with commanding
    public interface ICommandableEntity
    {
        Guid Guid { get; }
    }
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

        private void InitState()
        {
            if (_state == null && _repository != null)
            {
                this._state = _repository.CreateCommandState();
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

        public string CommandTypeId { get { return _state.Command; } set { _state.Command = value; } }
        public ICommandState State { get { return _state; } set { _state = value; } }

        public ICommandStateRepository CommandRepository { get { return _repository; } set { _repository = value; InitState(); } }

        private ICommandProcessor _commandProcessor;
        public virtual ICommandProcessor CommandProcessor { get { return _commandProcessor; } set { _commandProcessor = value; } }

    }
    // the domain class for Commands
    public interface ICommandProcessor { }

    public interface ICommandConfig
    {
        string Key { get; }
        string CommandName { get; set; }
        ICommand GetCommand(string parametersJson);
        ICommandProcessor Processor { get; set; }
        string Entity { get; set; }
        string NameSpace { get; set; }
        string Assembly { get; set; }
    }

    public interface IProcessorConfig
    {
        ICommandProcessor Processor { get; set; }
        string EntityRoot { get; set; }
        string NameSpace { get; set; }
        string Assembly { get; set; }
        ICommand GetCommand(string name, string entity, string parametersJson);
    }

    public interface ICommandService
    {
        void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
        void AddCommandConfigs(IEnumerable<ICommandConfig> configs);
        IEnumerable<ICommand> ProcessCommand(CommandDto command);
        void PersistChanges();
        void MergeCommands(IEnumerable<CommandDto> commands);
        ICommand CreateCommand<T>() where T : ICommand, new();
        IEnumerable<CommandDto> GetUnprocessedCommands();
    }

    public class ProcessorConfig : IProcessorConfig
    {
        public ICommandProcessor Processor { get; set; }
        public string EntityRoot { get; set; }
        public string NameSpace { get; set; }
        public string Assembly { get; set; }
        public ICommand GetCommand(string name, string entity, string parametersJson)
        {
            var commandConfig = new CommandConfig()
            {
                Entity = entity,
                CommandName = name,
                NameSpace = this.NameSpace,
                Assembly = this.Assembly,
                Processor = this.Processor
            };
            return commandConfig.GetCommand(parametersJson);
        }
    }
    public class CommandConfig : ICommandConfig
    {
        public string Key { get { return CommandName + Entity + "Command"; } }
        public string CommandName { get; set; }
        public string Entity { get; set; }
        public string NameSpace { get; set; }
        public string Assembly { get; set; }
        public ICommandProcessor Processor { get; set; }
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

    public class CommandDto
    {
        private ICommandState _state;
        public CommandDto()
        {
        }
        public CommandDto(ICommandState state)
        {
            this.Guid = state.Guid;
            this.EntityGuid = state.EntityGuid;
            this.Entity = state.Entity;
            this.EntityRoot = state.EntityRoot;
            this.EntityRootGuid = state.EntityRootGuid;
            this.ExecutedOn = state.ExecutedOn;
            this.Name = state.Command; // we already have the proper name, so perhaps this can be done more cleanly,
            this.UserName = state.UserName;
            // or we should save the CommandTypeId differently into the CommandState Table, ie. without EntityCommand suffix
            this.ParametersJson = state.ParametersJson;
            _state = state;
        }

        public Guid Guid { get; set; }
        public Guid EntityGuid { get; set; }
        public string Entity { get; set; }
        public string EntityRoot { get; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string ParametersJson { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid EntityRootGuid { get; }
        public DateTime? ExecutedOn { get; set; }
        public DateTime? ReceivedOn { get; set; }

        //public string UserName { get; set; }
    }
    public class CommandService : ICommandService
    {
        private Dictionary<string, IProcessorConfig> _configs = new Dictionary<string, IProcessorConfig>();
        private Dictionary<string, ICommandConfig> _commandConfigs = new Dictionary<string, ICommandConfig>();
        private ICommandStateRepository _repo;
        private IDateTimeProvider _dateTimeProvider;
        private ILookup<string, ICommandConfig> _commandLookups;
        private ILookup<string, IProcessorConfig> _processorLookups;

        public CommandService(ICommandStateRepository repo, IDateTimeProvider dateTimeProvider)
        {
            _repo = repo;
            _dateTimeProvider = dateTimeProvider;
            _commandLookups = new List<ICommandConfig>().ToLookup(o => o.Key);
            _processorLookups = new List<IProcessorConfig>().ToLookup(o => o.EntityRoot);
        }
        public void AddCommandConfigs(IEnumerable<ICommandConfig> configs)
        {
            _commandLookups = configs.ToLookup(o => o.Key);
        }
        public IEnumerable<ICommandConfig> GetCommandConfigs(string key)
        {
            var configs = new List<ICommandConfig>();
            configs.AddRange(_commandLookups[key]);
            return configs;
        }
        public void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs)
        {
            _processorLookups = configs.ToLookup(o => o.EntityRoot);
        }
        public IEnumerable<IProcessorConfig> GetProcessorConfigs(string key)
        {
            var configs = new List<IProcessorConfig>();
            configs.AddRange(_processorLookups[key]);
            return configs;
        }
        public void PersistChanges()
        {
            _repo.PersistChanges();
        }
        public void MergeCommands(IEnumerable<CommandDto> commands)
        {
            // todo: replay should be done once for each unique entity, or we need to refresh
            var processedEntities = new HashSet<Guid>();
            // the existing entity for every command
            foreach (var command in commands)
            {
                if (!processedEntities.Contains(command.EntityGuid))
                {
                    var states = _repo.GetCommandStates(command.EntityGuid).Select(s => new CommandDto(s));
                    processedEntities.Add(command.EntityGuid);
                }
                ProcessCommand(command);
            }
        }

        public IEnumerable<ICommand> ProcessCommand(CommandDto command)
        {
            //      ICommand typedCommand = null;
            var typedCommands = new List<ICommand>();
            ICommandProcessor processor = null;
            // either take existing name from state, or construct from dto
            // the replace is a bit hacky, should probably clean the commandstore
            var commandName = command.Name;
            var parametersJson = command.ParametersJson;

            var commandConfigs = GetCommandConfigs(commandName + command.Entity + "Command");
            foreach (var config in commandConfigs)
            {
                var typedCommand = config.GetCommand(parametersJson);
                processor = config.Processor;

                ExecuteCommand(command, typedCommands, processor, typedCommand);
            }
            var processorConfigs = GetProcessorConfigs(command.Entity);
            foreach (var config in processorConfigs)
            {
                var typedCommand = config.GetCommand(commandName, command.Entity, parametersJson);
                processor = config.Processor;

                ExecuteCommand(command, typedCommands, processor, typedCommand);
            }
            if (typedCommands.Any())
            {
                return typedCommands;
            }
            throw new CommandNotConfiguredException($"The command named '{command.Name}' for entity '{command.Entity}' does not have a matching configuration.");
        }

        private void ExecuteCommand(CommandDto command, List<ICommand> typedCommands, ICommandProcessor processor, ICommand typedCommand)
        {
            typedCommand.CommandRepository = _repo;
            CopyCommandDtoIntoCommand(command, processor, typedCommand);

            typedCommand.Execute();
            typedCommand.ExecutedOn = _dateTimeProvider.GetServerDateTime();

            typedCommands.Add(typedCommand);
        }

        private void CopyCommandDtoIntoCommand(CommandDto command, ICommandProcessor processor, ICommand typedCommand)
        {
            typedCommand.CreatedOn = command.CreatedOn;
            typedCommand.ReceivedOn = _dateTimeProvider.GetSessionUtcDateTime();
            typedCommand.Entity = command.Entity;
            typedCommand.EntityGuid = command.EntityGuid;
            typedCommand.EntityRoot = command.EntityRoot;
            typedCommand.EntityRootGuid = command.EntityRootGuid;
            typedCommand.Guid = command.Guid;
            typedCommand.ParametersJson = command.ParametersJson;
            typedCommand.UserName = command.UserName;

            typedCommand.CommandProcessor = processor;
        }

        public ICommand CreateCommand<T>() where T : ICommand, new()
        {
            var command = new T()
            {
                CommandRepository = _repo
            };
            command.Guid = Guid.NewGuid();
            command.CreatedOn = DateTime.Now;
            return command;
        }

        public IEnumerable<CommandDto> GetUnprocessedCommands()
        {
            var states = _repo.GetUnprocessedCommandStates();
            var dtos = new List<CommandDto>();
            foreach (var state in states)
            {
                dtos.Add(new CommandDto(state));
            }
            return dtos;
        }
    }

    public interface IDateTimeProvider
    {
        DateTime? GetSessionUtcDateTime();
        DateTime? GetServerDateTime();
    }
}
