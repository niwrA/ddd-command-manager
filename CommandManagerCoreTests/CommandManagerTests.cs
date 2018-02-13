using niwrA.CommandManager;
using Moq;
using Xunit;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CommandManagerCoreTests
{
  [Trait("Manager", "CommandManager")]
  public class CommandManagerTests
  {
    const string _assembly = "CommandManagerCoreTests";
    const string _namespace = "CommandManagerCoreTests.Fakes";

    [Fact(DisplayName = "IDateTimeProvider_DefaultsTo_DefaultDateTimeProvider_and_InMemoryRepository")]
    public void IDateTimeProvider_DefaultsTo_DefaultDateTimeProvider_and_InMemoryRepository()
    {
      var sut = new CommandManager();
      var testService = new Mock<Fakes.ITestService>();
      var commandDto = new CommandDtoBuilder().Build();
      var config = new ProcessorConfig(commandDto.Entity, testService.Object, _namespace, _assembly);
      sut.AddProcessorConfigs(new List<IProcessorConfig> { config });
      sut.ProcessCommands(new List<CommandDto> { commandDto });

      testService.Verify(v => v.CreateRootEntity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public void PersistChanges_Calls_ServicePersistChanges()
    {
      var serviceMock = new Mock<ICommandService>();
      var converterMock = new Mock<ICommandDtoToCommandConverter>();
      var sut = new CommandManager(converterMock.Object, serviceMock.Object);
      sut.PersistChanges();
      serviceMock.Verify(s => s.PersistChanges(), Times.Once);
    }
    [Fact]
    public void PersistChanges_Calls_ProcessorsPersistChanges()
    {
      var serviceMock = new Mock<ICommandService>();
      var converterMock = new Mock<ICommandDtoToCommandConverter>();
      var sut = new CommandManager(converterMock.Object, serviceMock.Object);
      var processorInProcessorConfigMock = new Mock<ICommandProcessor>();
      var processorInCommandConfigMock = new Mock<ICommandProcessor>();

      var config = new ProcessorConfig("", processorInProcessorConfigMock.Object, "", "");
      sut.AddProcessorConfigs(new List<IProcessorConfig> { config });

      var commandConfig = new CommandConfigBuilder()
        .WithProcessor(processorInCommandConfigMock.Object)
        .Build();
      var commandConfigWithDuplicateProcessor = new CommandConfigBuilder()
        .WithProcessor(processorInProcessorConfigMock.Object)
        .Build();
      sut.AddCommandConfigs(new List<ICommandConfig> { commandConfig, commandConfigWithDuplicateProcessor });

      sut.PersistChanges();

      processorInProcessorConfigMock.Verify(s => s.PersistChanges(), Times.Once);
      processorInCommandConfigMock.Verify(s => s.PersistChanges(), Times.Once);
    }

    [Fact]
    public void ProcessImportedCommands()
    {
      var serviceMock = new Mock<ICommandService>();
      var converterMock = new Mock<ICommandDtoToCommandConverter>();
      var commandDtos = new List<ICommandDto> { new CommandDtoBuilder().Build() };

      Assert.Null(((CommandDto)commandDtos.First()).ExecutedOn);

      converterMock.Setup(s => s.GetUnprocessedCommands()).Returns(commandDtos);

      var sut = new CommandManager(converterMock.Object, serviceMock.Object);

      sut.ProcessImportedCommands();

      converterMock.Verify(v => v.GetUnprocessedCommands(), Times.Once);
      serviceMock.Verify(v => v.ProcessCommands(It.IsAny<IEnumerable<ICommand>>()), Times.Once);
    }
  }
}