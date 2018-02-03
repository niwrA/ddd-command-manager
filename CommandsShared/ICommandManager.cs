using System.Collections.Generic;

namespace niwrA.CommandManager
{
    public interface ICommandManager
    {
        void MergeCommands(IEnumerable<CommandDto> commands);
        void ProcessCommands(IEnumerable<CommandDto> commands);
        void AddCommandConfigs(IEnumerable<ICommandConfig> configs);
        void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
    }
}