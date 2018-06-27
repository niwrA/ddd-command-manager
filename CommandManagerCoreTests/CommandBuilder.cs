using niwrA.CommandManager;
using Moq;
using System;
using niwrA.CommandManager.Contracts;

namespace CommandManagerCoreTests.Commands
{
    public class CommandBuilder<T> where T : ICommand, new()
    {
        public T Build(ICommandProcessor processor)
        {
            var commandRepoMock = new Mock<ICommandStateRepository>();
            var commandState = new Fakes.CommandState();

            commandRepoMock.Setup(t => t.CreateCommandState(Guid.NewGuid())).Returns(commandState);

            T cmd = new T()
            {
                CommandRepository = commandRepoMock.Object,
                EntityGuid = Guid.NewGuid().ToString(),
                CommandProcessor = processor
            };
            return cmd;
        }
    }

}
