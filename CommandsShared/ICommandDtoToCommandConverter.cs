using System.Collections.Generic;

namespace niwrA.CommandManager
{
    public interface ICommandDtoToCommandConverter
    {
        void AddCommandConfigs(IEnumerable<ICommandConfig> configs);
        void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
        IEnumerable<ICommand> ConvertCommand(CommandDto command);
        IEnumerable<ICommand> ConvertCommands(IEnumerable<CommandDto> commands);
        IEnumerable<ICommandConfig> GetCommandConfigs(string key);
        IEnumerable<IProcessorConfig> GetProcessorConfigs(string key);
        IEnumerable<CommandDto> GetUnprocessedCommands();
        //void MergeCommands(IEnumerable<CommandDto> commands);
    }
}