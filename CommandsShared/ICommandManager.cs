using System.Collections.Generic;

namespace niwrA.CommandManager
{
  public interface ICommandManager
  {
    void MergeCommands(IEnumerable<ICommandDto> commands);
    void ProcessCommands(IEnumerable<ICommandDto> commands);
    int ProcessImportedCommands();
    void AddCommandConfigs(IEnumerable<ICommandConfig> configs);
    void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
    void PersistChanges();
  }
}