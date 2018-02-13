using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

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
    /// <summary>
    /// Tells all services to persist all changes, including saving commands if a repository is used,
    /// all combined in the same transactionscope, so if one fails, all fail
    /// </summary>
    public void PersistChanges()
    {
      var processors = GetAllConfiguredProcessors();
      using (TransactionScope scope = new TransactionScope())
      {
        foreach (var processor in processors)
        {
          processor.PersistChanges();
        }
        _repo.PersistChanges();
        scope.Complete();
      }
    }
    /// <summary>
    /// Merge commands is used to rebuild the target (entity) of a command by rerunning all previous commands
    /// first and executing the new commands against the rebuilt target (entity).
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="converter"></param>
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
    /// <summary>
    /// Process the specified commands (Executes them)
    /// </summary>
    /// <param name="commands"></param>
    public void ProcessCommands(IEnumerable<ICommand> commands)
    {
      var processed = new List<ICommand>();
      foreach (var command in commands)
      {
        ProcessCommand(command);
      }
    }
    /// <summary>
    /// Process a single command (execute it)
    /// </summary>
    /// <param name="command"></param>
    public void ProcessCommand(ICommand command)
    {
      command.Execute();
      command.ExecutedOn = _dateTimeProvider.GetServerDateTime();
    }
    /// <summary>
    /// Create a command of the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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

    /// <summary>
    /// Gets all processors configured in the command configurations
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ICommandProcessor> GetAllConfiguredProcessors()
    {
      var processors = _configs.Values.Select(w => w.Processor).Distinct().ToList();
      var commandProcessors = _commandConfigs.Values.Select(w => w.Processor).Distinct().ToList();
      return processors.Union(commandProcessors).ToList();
    }
  }
}
