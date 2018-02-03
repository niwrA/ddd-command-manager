﻿using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.CommandManager
{
    public class CommandManager : ICommandManager
    {
        private ICommandDtoToCommandConverter _converter;
        private ICommandService _service;

        public CommandManager(ICommandStateRepository repo, IDateTimeProvider dateTimeProvider)
        {
            _converter = new CommandDtoToCommandConverter(repo, dateTimeProvider);
            _service = new CommandService(repo, dateTimeProvider);
        }
        public CommandManager(ICommandDtoToCommandConverter converter, ICommandService service)
        {
            _converter = converter;
            _service = service;
        }
        public void ProcessCommands(IEnumerable<CommandDto> commands)
        {
            var typedCommands = _converter.ConvertCommands(commands);
            _service.ProcessCommands(typedCommands);
        }
        public void MergeCommands(IEnumerable<CommandDto> commands)
        {
            var typedCommands = _converter.ConvertCommands(commands);
            _service.MergeCommands(typedCommands);
        }
        public void AddCommandConfigs(IEnumerable<ICommandConfig> configs)
        {
            _converter.AddCommandConfigs(configs);
        }
        public void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs)
        {
            _converter.AddProcessorConfigs(configs);
        }
    }
}
