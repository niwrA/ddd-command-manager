using niwrA.CommandManager;
using Moq;
using Xunit;

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
            var processorConfig = new ProcessorConfig("RootEntity", projectsMoq.Object, _namespace, _assembly);
            var command = processorConfig.GetCommand("Rename", "RootEntity", json) as Fakes.RenameRootEntityCommand;
            Assert.Equal("new name", command.Name);
        }

        [Fact(DisplayName = "CanSerializeToTypedCommand_FromCommandConfig")]
        public void CanSerializeToTypedCommand_FromCommandConfig()
        {
            var json = @"{'Name': 'new name'}";
            var projectsMoq = new Mock<Fakes.ITestService>();
            var commandConfig = new CommandConfig { Assembly = _assembly, NameSpace = _namespace, CommandName = "Rename", Entity = "RootEntity", Processor = projectsMoq.Object };
            var command = commandConfig.GetCommand(json) as Fakes.RenameRootEntityCommand;
            Assert.Equal("new name", command.Name);
        }

        [Fact(DisplayName = "ThrowsTypeNotFoundException_WhenTypeNotFound")]
        public void ThrowsTypeNotFoundException_WhenTypeNotFound()
        {
            var json = @"{'Name': 'new name'}";
            var projectsMoq = new Mock<Fakes.TestService>();
            var commandConfig = new CommandConfig { Assembly = "SomethingNotExisting", NameSpace = "SomethingNotExisting", CommandName = "Rename", Entity = "RootEntity", Processor = projectsMoq.Object };
            Assert.Throws<TypeNotFoundException>(() => commandConfig.GetCommand(json));
        }
    }
}
