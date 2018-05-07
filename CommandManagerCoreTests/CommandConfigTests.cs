using niwrA.CommandManager;
using Moq;
using Xunit;
using niwrA.CommandManager.Contracts;

namespace CommandManagerCoreTests.Commands
{
  [Trait("Entity", "CommandConfig")]
  public class CommandConfigTests
  {
    const string _assembly = "CommandManagerCoreTests";
    const string _namespace = "CommandManagerCoreTests.Fakes";

    [Fact(DisplayName = "CanSerializeToTypedCommand_FromProcessorConfig")]
    public void CanSerializeToTypedCommand_FromProcessorConfig()
    {
      var json = @"{'Name': 'new name'}";
      var projectsMoq = new Mock<Fakes.ITestService>();
      var sut = new ProcessorConfig("RootEntity", projectsMoq.Object, _namespace, _assembly);

      var sutResult = sut.GetCommand("Rename", "RootEntity", json) as Fakes.RenameRootEntityCommand;

      Assert.Equal("new name", sutResult.Name);
    }

    [Fact(DisplayName = "CanSerializeToTypedCommand_FromCommandConfig")]
    public void CanSerializeToTypedCommand_FromCommandConfig()
    {
      var json = @"{'Name': 'new name'}";
      var sut = new CommandConfigBuilder().Build();

      var sutResult = sut.GetCommand(json) as Fakes.RenameRootEntityCommand;

      Assert.Equal("new name", sutResult.Name);
    }

    [Fact(DisplayName = "ThrowsTypeNotFoundException_WhenTypeNotFound")]
    public void ThrowsTypeNotFoundException_WhenTypeNotFound()
    {
      var json = @"{'Name': 'new name'}";

      var sut = new CommandConfigBuilder()
        .WithNonExistingNameSpace()
        .Build();

      Assert.Throws<TypeNotFoundException>(() => sut.GetCommand(json));
    }
  }
  public class CommandConfigBuilder
  {
    private string _assembly = "CommandManagerCoreTests";
    private string _namespace = "CommandManagerCoreTests.Fakes";
    public ICommandConfig Build()
    {
      var projectsMoq = new Mock<Fakes.TestService>();
      var commandConfig = new CommandConfig("Rename", "RootEntity", projectsMoq.Object, _namespace, _assembly);
      return commandConfig;
    }
    public CommandConfigBuilder WithNonExistingNameSpace()
    {
      _namespace = "thisnamespacedoesnotexist";
      return this;
    }
  }
}
