using System.Collections.Generic;

namespace niwrA.CommandManager
{
  public interface ICommandManager
  {
    void MergeCommands(IEnumerable<CommandDto> commands);
    void ProcessCommands(IEnumerable<CommandDto> commands);
    int ProcessImportedCommands();
    void AddCommandConfigs(IEnumerable<ICommandConfig> configs);
    void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
    void PersistChanges();
  }
}