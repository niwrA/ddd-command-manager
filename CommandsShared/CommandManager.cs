using niwrA.CommandManager.Helpers;
using niwrA.CommandManager.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.CommandManager
{
  public class CommandManager : ICommandManager
  {
    private ICommandDtoToCommandConverter _converter;
    private ICommandService _service;
    public CommandManager()
    {
      var repo = new CommandStateRepositoryInMemory();
      var dateTimeProvider = new DefaultDateTimeProvider();
      _converter = new CommandDtoToCommandConverter(repo, dateTimeProvider);
      _service = new CommandService(repo, dateTimeProvider);
    }
    public CommandManager(ICommandStateRepository repo, IDateTimeProvider dateTimeProvider)
    {
      _converter = new CommandDtoToCommandConverter(repo, dateTimeProvider);
      _service = new CommandService(repo, dateTimeProvider);
    }
    public CommandManager(ICommandDtoToCommandConverter converter, ICommandService service)
    {
      _converter = converter;
      _service = service;
    }
    public void ProcessCommands(IEnumerable<CommandDto> commands)
    {
      var typedCommands = _converter.ConvertCommands(commands);
      _service.ProcessCommands(typedCommands);
    }
    public int ProcessImportedCommands()
    {
      var importedCommands = _converter.GetUnprocessedCommands();
      ProcessCommands(importedCommands);
      return ((ICollection<CommandDto>)importedCommands).Count;
    }
    public void MergeCommands(IEnumerable<CommandDto> commands)
    {
      var typedCommands = _converter.ConvertCommands(commands);
      _service.MergeCommands(typedCommands, _converter);
    }
    public void AddCommandConfigs(IEnumerable<ICommandConfig> configs)
    {
      _converter.AddCommandConfigs(configs);
    }
    public void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs)
    {
      _converter.AddProcessorConfigs(configs);
    }
    public void PersistChanges()
    {
      _service.PersistChanges();
    }
  }
}
