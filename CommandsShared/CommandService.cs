using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace niwrA.CommandManager
{
  public class CommandService : ICommandService
  {
    private Dictionary<string, IProcessorConfig> _configs = new Dictionary<string, IProcessorConfig>();
    private Dictionary<string, ICommandConfig> _commandConfigs = new Dictionary<string, ICommandConfig>();
    private ICommandStateRepository _repo;
    private IDateTimeProvider _dateTimeProvider;

    public CommandService(ICommandStateRepository repo, IDateTimeProvider dateTimeProvider)
    {
      _repo = repo;
      _dateTimeProvider = dateTimeProvider;
    }
    public void PersistChanges()
    {
      // todo: wrap in transaction
      _repo.PersistChanges();
      // todo: persist changes on all services / repositories
      foreach(var config in _configs.Values)
      {
      };

    }
    public void MergeCommands(IEnumerable<ICommand> commands, ICommandDtoToCommandConverter converter)
    {
      // todo: replay should be done once for each unique entity, or we need to refresh
      // this can probably move to a higher lever for even better caching?
      var processedEntities = new HashSet<Guid>();
      // the existing entity for every command
      foreach (var command in commands)
      {
        if (!processedEntities.Contains(command.EntityGuid))
        {
          var states = _repo.GetCommandStates(command.EntityGuid);
          var commandDtos = states.Select(s => new CommandDto(s));
          var previousCommands = converter.ConvertCommands(commandDtos);
          ProcessCommands(previousCommands);
          processedEntities.Add(command.EntityGuid);
        }
        ProcessCommand(command);
      }
    }
    public void ProcessCommands(IEnumerable<ICommand> commands)
    {
      var processed = new List<ICommand>();
      foreach (var command in commands)
      {
        ProcessCommand(command);
      }
    }
    public void ProcessCommand(ICommand command)
    {
      command.Execute();
      command.ExecutedOn = _dateTimeProvider.GetServerDateTime();
    }
    public ICommand CreateCommand<T>() where T : ICommand, new()
    {
      var command = new T()
      {
        CommandRepository = _repo
      };
      command.Guid = Guid.NewGuid();
      command.CreatedOn = _dateTimeProvider.GetServerDateTime();
      return command;
    }
  }
}
