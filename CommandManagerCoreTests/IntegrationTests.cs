using niwrA.CommandManager;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CommandManagerCoreTests
{
  [Trait("Category", "IntegrationTests")]
  public class IntegrationTests
  {
    const string _assembly = "CommandManagerCoreTests";
    const string _namespace = "CommandManagerCoreTests.Fakes";

    [Fact(DisplayName = "ProcessCommand_CanTargetMultipleProcessorsForOneCommand")]
    public void ProcessCommand_CanTargetMultipleProcessorsForOneCommand()
    {
      var dateTimeProvider = new Mock<IDateTimeProvider>().Object;
      var processor1 = new Mock<Fakes.ITestService>();
      var processor2 = new Mock<Fakes.ITestService>();
      var processor3 = new Mock<Fakes.ITestService>();
      var processor4 = new Mock<Fakes.ITestService>();

      var repo = new Mock<ICommandStateRepository>();
      repo.Setup(s => s.CreateCommandState()).Returns(new Fakes.CommandState());

      var sut = new CommandDtoToCommandConverter(repo.Object, dateTimeProvider);
      var commandDto = new CommandDtoToCommandConverterTests.CommandDtoBuilder().Build();
      var commandConfigBuilder = new CommandConfigBuilder();
      var configs = new List<ICommandConfig>
            {
              commandConfigBuilder.WithCommandName("Create")
              .WithEntity("RootEntity")
              .WithProcessor(processor1.Object)
              .Build(),
              commandConfigBuilder
              .WithCommandName("Create")
              .WithEntity("RootEntity")
              .WithProcessor(processor2.Object).Build()            };

      var processorConfigs = new List<IProcessorConfig>
            {
                new ProcessorConfig("RootEntity", processor3.Object, _namespace, _assembly),
                new ProcessorConfig("RootEntity", processor4.Object, _namespace, _assembly)
            };

      sut.AddCommandConfigs(configs);
      sut.AddProcessorConfigs(processorConfigs);

      var typedCommands = sut.ConvertCommand(commandDto);
      var sut2 = new CommandService(repo.Object, dateTimeProvider);

      sut2.ProcessCommands(typedCommands);

      processor1.Verify(s => s.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
      processor2.Verify(s => s.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
      processor3.Verify(s => s.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
      processor4.Verify(s => s.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }

    // todo: consider creating test specific commands
    [Fact(DisplayName = "CanMergeCommands_WithExistingStateAndApply")]
    // basically we are testing full event/commandsourcing here, restoring state
    // from existing commands and applying new commands to that state
    public void CommandManager_CanMergeCommandsWithExistingStateAndApply()
    {
      Guid entityGuid = Guid.NewGuid();

      // create some existing commands
      // todo: builder
      var existingCreateCommand = new Fakes.CommandState { Entity = "RootEntity", Command = "Create", EntityGuid = entityGuid, Guid = Guid.NewGuid(), CreatedOn = DateTime.Now, ExecutedOn = DateTime.Now, ReceivedOn = DateTime.Now, ParametersJson = "{'Name': 'John Smith'}" };
      var existingRenameCommand = new Fakes.CommandState { Entity = "RootEntity", Command = "Rename", EntityGuid = entityGuid, Guid = Guid.NewGuid(), CreatedOn = DateTime.Now, ExecutedOn = DateTime.Now, ReceivedOn = DateTime.Now, ParametersJson = "{'Name': 'john Hughes'}" };
      var existingChangeEmailCommand = new Fakes.CommandState { Entity = "RootEntity", Command = "ChangeEmailFor", EntityGuid = entityGuid, Guid = Guid.NewGuid(), CreatedOn = DateTime.Now, ExecutedOn = DateTime.Now, ReceivedOn = DateTime.Now, ParametersJson = "{'Email': 'john.hughes@acme.com'}" };
      var existingCommands = new List<ICommandState> { existingCreateCommand, existingRenameCommand, existingChangeEmailCommand };
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
      var sut = new CommandDtoToCommandConverter(repo.Object, dateTimeProvider);

      // setup configuration for all Contact commands to use the service above
      // for command processing
      var processorConfig = new ProcessorConfig("RootEntity", processor, _namespace, _assembly);
      sut.AddProcessorConfigs(new List<IProcessorConfig> { processorConfig });

      // create a new command to merge/apply
      // todo: make builder
      var commandDtoBuilder = new CommandDtoBuilder();
      var renameCommandDto = commandDtoBuilder
        .WithEntityGuid(entityGuid)
        .WithCreatedOn(DateTime.Now.AddTicks(1))
        .WithCommand("Rename")
        .WithParametersJson(@"{Name: 'James Smith'}")
        .Build();

      var commandDtos = new List<CommandDto> { renameCommandDto };

      // convert the commands
      var typedCommands = sut.ConvertCommands(commandDtos);

      // merge the commands
      var sut2 = new CommandService(repo.Object, dateTimeProvider);
      sut2.MergeCommands(typedCommands, sut);

      // retrieve the contact from in-memory state to check if state is as expected
      var rootEntity = processor.GetRootEntity(entityGuid);

      Assert.Equal("James Smith", rootEntity.Name);
      Assert.Equal("john.hughes@acme.com", rootEntity.Email);
    }
  }
}
