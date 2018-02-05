using niwrA.CommandManager;
using Moq;
using Xunit;
using System.Collections.Generic;
using System;

namespace CommandManagerCoreTests
{
  [Trait("Entity", "CommandConfig")]
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
  }
}