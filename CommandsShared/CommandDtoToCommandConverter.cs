using niwrA.CommandManager.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace niwrA.CommandManager
{
    public class CommandDtoToCommandConverter : ICommandDtoToCommandConverter
    {
        private Dictionary<string, IProcessorConfig> _configs = new Dictionary<string, IProcessorConfig>();
        private Dictionary<string, ICommandConfig> _commandConfigs = new Dictionary<string, ICommandConfig>();
        private ICommandStateRepository _repo;
        private IDateTimeProvider _dateTimeProvider;
        private ILookup<string, ICommandConfig> _commandLookups;
        private ILookup<string, IProcessorConfig> _processorLookups;

        public CommandDtoToCommandConverter(ICommandStateRepository repo, IDateTimeProvider dateTimeProvider)
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

        public IEnumerable<ICommand> ConvertCommands(IEnumerable<ICommandDto> commands)
        {
            var processed = new List<ICommand>();
            foreach (var command in commands)
            {
                processed.AddRange(ConvertCommand(command));
            }
            return processed;
        }
        public IEnumerable<ICommand> ConvertCommand(ICommandDto command)
        {
            //      ICommand typedCommand = null;
            var typedCommands = new List<ICommand>();
            // either take existing name from state, or construct from dto
            // the replace is a bit hacky, should probably clean the commandstore
            var commandName = command.Command;
            var parametersJson = command.ParametersJson;

            ProcessCommandConfigs(command, typedCommands, commandName, parametersJson);
            ProcessProcessorConfigs(command, typedCommands, commandName, parametersJson);

            if (typedCommands.Any())
            {
                return typedCommands;
            }
            throw new CommandNotConfiguredException($"The command named '{command.Command}' for entity '{command.Entity}' does not have a matching configuration.");
        }

        private void ProcessProcessorConfigs(ICommandDto command, List<ICommand> typedCommands, string commandName, string parametersJson)
        {
            var processorConfigs = GetProcessorConfigs(command.EntityRoot);
            foreach (var config in processorConfigs)
            {
                var typedCommand = config.GetCommand(commandName, command.Entity, parametersJson);

                SetCommandProperties(command, config.Processor, typedCommand);
                typedCommands.Add(typedCommand);
            }
        }

        private void ProcessCommandConfigs(ICommandDto command, List<ICommand> typedCommands, string commandName, string parametersJson)
        {
            var commandConfigs = GetCommandConfigs(commandName + command.Entity + "Command");
            foreach (var config in commandConfigs)
            {
                var typedCommand = config.GetCommand(parametersJson);

                SetCommandProperties(command, config.Processor, typedCommand);
                typedCommands.Add(typedCommand);
            }
        }

        private void SetCommandProperties(ICommandDto command, ICommandProcessor processor, ICommand typedCommand)
        {
            typedCommand.CommandRepository = _repo;
            CopyCommandDtoIntoCommand(command, processor, typedCommand);
        }

        private void CopyCommandDtoIntoCommand(ICommandDto command, ICommandProcessor processor, ICommand typedCommand)
        {
            typedCommand.CreatedOn = command.CreatedOn;
            typedCommand.ReceivedOn = _dateTimeProvider.GetSessionDateTime();
            typedCommand.Entity = command.Entity;
            typedCommand.EntityGuid = command.EntityGuid;
            typedCommand.EntityRoot = command.EntityRoot;
            typedCommand.EntityRootGuid = command.EntityRootGuid;
            typedCommand.Guid = command.Guid;
            typedCommand.ParametersJson = command.ParametersJson;
            typedCommand.UserName = command.UserName;
            typedCommand.TenantId = command.TenantId;
            typedCommand.Command = command.Command;

            typedCommand.CommandProcessor = processor;
        }

        public IEnumerable<ICommandDto> GetUnprocessedCommands()
        {
            var states = _repo.GetUnprocessedCommandStates();
            var dtos = new List<ICommandDto>();
            foreach (var state in states)
            {
                dtos.Add(new CommandDto(state));
            }
            return dtos;
        }
    }
}
