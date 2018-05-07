using niwrA.CommandManager.Contracts;
using System.Collections.Generic;

namespace niwrA.CommandManager
{
    public interface ICommandDtoToCommandConverter
    {
        void AddCommandConfigs(IEnumerable<ICommandConfig> configs);
        void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
        IEnumerable<ICommand> ConvertCommand(ICommandDto command);
        IEnumerable<ICommand> ConvertCommands(IEnumerable<ICommandDto> commands);
        IEnumerable<ICommandConfig> GetCommandConfigs(string key);
        IEnumerable<IProcessorConfig> GetProcessorConfigs(string key);
        IEnumerable<ICommandDto> GetUnprocessedCommands();
        //void MergeCommands(IEnumerable<CommandDto> commands);
    }
}