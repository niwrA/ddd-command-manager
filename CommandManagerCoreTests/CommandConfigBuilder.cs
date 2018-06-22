using Moq;
using niwrA.CommandManager;
using niwrA.CommandManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandManagerCoreTests
{
    public class CommandConfigBuilder
    {
        private string _assembly = "CommandManagerCoreTests";
        private string _namespace = "CommandManagerCoreTests.Fakes";
        private string _commandName = "Rename";
        private string _entity = "RootEntity";

        private ICommandProcessor _processor;
        public ICommandConfig Build()
        {
            if (_processor == null) { _processor = new Mock<Fakes.TestService>().Object; }
            var commandConfig = new CommandConfig(_commandName, _entity, _processor, _namespace, _assembly);
            return commandConfig;
        }
        public CommandConfigBuilder WithNonExistingNameSpace()
        {
            _namespace = "thisnamespacedoesnotexist";
            return this;
        }
        public CommandConfigBuilder WithProcessor(ICommandProcessor processor)
        {
            _processor = processor;
            return this;
        }
        public CommandConfigBuilder WithCommandName(string name)
        {
            _commandName = name;
            return this;
        }
        public CommandConfigBuilder WithEntity(string entity)
        {
            _entity = entity;
            return this;
        }
    }
}
