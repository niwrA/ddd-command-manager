using niwrA.CommandManager.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace niwrA.CommandManager
{
    public class CommandService : ICommandService
    {
        private ICommandStateRepository _repo;
        private IDateTimeProvider _dateTimeProvider;
        private ICommandManager _commandManager;
        public event Action<IEnumerable<ICommandDto>> ProcessorGeneratedCommands;

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
            _repo.PersistChanges();
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
            var processedEntities = new HashSet<string>();
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
            var pendingCommands = new List<ICommandDto>();
            command.CommandProcessor.AddCommandsToBatch += (commands) =>
            {
                pendingCommands.AddRange(commands);
            };

            command.Execute();
            command.ExecutedOn = _dateTimeProvider.GetServerDateTime();
            this.ProcessorGeneratedCommands?.Invoke(pendingCommands);
        }
        /// <summary>
        /// Create a command of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateCommand<T>() where T : ICommand, new()
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
