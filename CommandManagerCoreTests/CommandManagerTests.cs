﻿using CommandsShared;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CommandManagerCoreTests
{
    [Trait("Entity", "Command")]
    public class CommandManagerTests
    {
        const string _assembly = "CommandManagerCoreTests";
        const string _namespace = "CommandManagerCoreTests.Fakes";
        // todo: consider creating test specific commands
        [Fact(DisplayName = "CanMergeCommands_WithExistingStateAndApply")]
        // basically we are testing full event/commandsourcing here, restoring state
        // from existing commands and applying new commands to that state
        [Trait("Category", "IntegrationTests")]
        public void CommandManager_CanMergeCommandsWithExistingStateAndApply()
        {
            Guid entityGuid = Guid.NewGuid();

            // create some existing commands
            // todo: builder
            var existingCreateCommand = new Fakes.CommandState { Entity = "RootEntity", Command = "Create", EntityGuid = entityGuid, Guid = Guid.NewGuid(), CreatedOn = DateTime.Now, ExecutedOn = DateTime.Now, ReceivedOn = DateTime.Now, ParametersJson = "{'Name': 'John Smith'}" };
            var existingRenameCommand = new Fakes.CommandState { Entity = "RootEntity", Command = "Rename", EntityGuid = entityGuid, Guid = Guid.NewGuid(), CreatedOn = DateTime.Now, ExecutedOn = DateTime.Now, ReceivedOn = DateTime.Now, ParametersJson = "{'Name': 'john Hughes'}" };
            var existingCommands = new List<ICommandState> { existingCreateCommand, existingRenameCommand };
            IDateTimeProvider dateTimeProvider = new Mock<IDateTimeProvider>().Object;

            // mock that these are returned by the repository
            var repo = new Mock<ICommandStateRepository>();
            repo.Setup(s => s.CreateCommandState()).Returns(new Fakes.CommandState());
            repo.Setup(s => s.GetCommandStates(entityGuid)).Returns(existingCommands);

            // create a real service, with an eventsourcing version of the state repository,
            // which basically only holds state in memory
            // todo: make builder
            var testServiceRepo = new Fakes.TestServiceEventSourceRepository();
            var processor = new Fakes.TestService(testServiceRepo);
            var sut = new CommandService(repo.Object, dateTimeProvider);

            // setup configuration for all Contact commands to use the service above
            // for command processing
            var processorConfig = new ProcessorConfig()
            {
                Assembly = _assembly,
                NameSpace = _namespace,
                EntityRoot = "RootEntity",
                Processor = processor
            };
            sut.AddProcessorConfigs(new List<IProcessorConfig> { processorConfig });

            // create a new command to merge/apply
            // todo: make builder
            var createCommandDto = new CommandDto { CreatedOn = DateTime.Now.AddTicks(1), Entity = "RootEntity", EntityGuid = entityGuid, Guid = Guid.NewGuid(), Name = "Create", ParametersJson = @"{Name: 'John Smith'}" };
            var renameCommandDto = new CommandDto { CreatedOn = DateTime.Now.AddTicks(1), Entity = "RootEntity", EntityGuid = entityGuid, Guid = Guid.NewGuid(), Name = "Rename", ParametersJson = @"{Name: 'James Smith'}" };
            var commandDtos = new List<CommandDto> { createCommandDto, renameCommandDto };

            // perform the actual merge
            sut.MergeCommands(commandDtos);

            // retrieve the contact from in-memory state to check if state is as expected
            var rootEntity = processor.GetRootEntity(entityGuid);

            Assert.Equal("James Smith", rootEntity.Name);
            // Assert.Equal("john@smith.com", contact.Email);
        }

        [Fact(DisplayName = "CommandManager_CanGetUnprocessedCommands")]
        // basically we are testing full event/commandsourcing here, restoring state
        // from existing commands and applying new commands to that state
        public void CommandManager_CanGetUnprocessedCommands()
        {
            var dateTimeProvider = new Mock<IDateTimeProvider>().Object;

            var mockTest = new Mock<ICommandState>();
            mockTest.Setup(s => s.Entity).Returns("Test");
            var existingCommands = new List<ICommandState> { mockTest.Object };

            var repo = new Mock<ICommandStateRepository>();
            repo.Setup(s => s.GetUnprocessedCommandStates()).Returns(existingCommands);

            var sut = new CommandService(repo.Object, dateTimeProvider);
            var sutResult = sut.GetUnprocessedCommands() as List<CommandDto>;

            Assert.Single(sutResult);
            Assert.Equal("Test", sutResult.First().Entity);
        }

        [Fact(DisplayName = "CommandManager_ProcessCommand_CanTargetMultipleProcessorsForOneCommand")]
        // basically we are testing full event/commandsourcing here, restoring state
        // from existing commands and applying new commands to that state
        public void CommandManager_ProcessCommand_CanTargetMultipleProcessorsForOneCommand()
        {
            var dateTimeProvider = new Mock<IDateTimeProvider>().Object;
            var processor1 = new Mock<Fakes.ITestService>();
            var processor2 = new Mock<Fakes.ITestService>();
            var processor3 = new Mock<Fakes.ITestService>();
            var processor4 = new Mock<Fakes.ITestService>();

            var repo = new Mock<ICommandStateRepository>();
            repo.Setup(s => s.CreateCommandState()).Returns(new Fakes.CommandState());

            var sut = new CommandService(repo.Object, dateTimeProvider);
            var commandDto = new CommandDto { Entity = "RootEntity", Name = "Create", EntityGuid = Guid.NewGuid(), Guid = Guid.NewGuid(), CreatedOn = new DateTime(2018, 1, 1), ParametersJson = @"{Name: 'James Smith'}" };

            var configs = new List<ICommandConfig>
            {
                new CommandConfig { CommandName = "Create", Entity = "RootEntity", NameSpace=_namespace, Assembly = _assembly, Processor = processor1.Object },
                new CommandConfig { CommandName = "Create", Entity = "RootEntity", NameSpace=_namespace, Assembly = _assembly, Processor = processor2.Object }
            };

            var processorConfigs = new List<IProcessorConfig>
            {
                new ProcessorConfig { EntityRoot = "RootEntity", NameSpace = _namespace, Assembly = _assembly, Processor = processor3.Object },
                new ProcessorConfig { EntityRoot = "RootEntity", NameSpace = _namespace, Assembly = _assembly, Processor = processor4.Object }
            };

            sut.AddCommandConfigs(configs);
            sut.AddProcessorConfigs(processorConfigs);

            var sutResult = sut.ProcessCommand(commandDto);

            processor1.Verify(s => s.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            processor2.Verify(s => s.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            processor3.Verify(s => s.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            processor4.Verify(s => s.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

    }
}
