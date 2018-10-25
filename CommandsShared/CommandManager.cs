using niwrA.CommandManager.Contracts;
using niwrA.CommandManager.Helpers;
using niwrA.CommandManager.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace niwrA.CommandManager
{
    public class CommandManager : ICommandManager
    {
        private ICommandDtoToCommandConverter _converter;
        private ICommandService _service;
        private List<ICommandProcessor> _commandprocessors = new List<ICommandProcessor>();
        private List<ICommandProcessor> _processors = new List<ICommandProcessor>();

        /// <summary>
        /// The default constructor creates an in memory commandstate repository
        /// and a default DateTimeProvider that uses UTC to provide timestamps
        /// </summary>
        public CommandManager()
        {
            var repo = new CommandStateRepositoryInMemory();
            var dateTimeProvider = new DefaultDateTimeProvider();
            _converter = new CommandDtoToCommandConverter(repo, dateTimeProvider);
            _service = new CommandService(repo, dateTimeProvider);
            _service.ProcessorGeneratedCommands += OnProcessorGeneratedCommands;
        }

        private void OnProcessorGeneratedCommands(IEnumerable<ICommandDto> commands)
        {
            ProcessCommands(commands);
        }

        /// <summary>
        /// Specify your own commandstate repository implementation and your own DateTimeProvider
        /// implementation with this constructor
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="dateTimeProvider"></param>
        public CommandManager(ICommandStateRepository repo, IDateTimeProvider dateTimeProvider)
        {
            _converter = new CommandDtoToCommandConverter(repo, dateTimeProvider);
            _service = new CommandService(repo, dateTimeProvider);
            _service.ProcessorGeneratedCommands += OnProcessorGeneratedCommands;
        }
        /// <summary>
        /// Specify your own CommandDtoToCommandConverter and your own CommandService with this constructor.
        /// This is mostly used for testing.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="service"></param>
        public CommandManager(ICommandDtoToCommandConverter converter, ICommandService service)
        {
            _converter = converter;
            _service = service;
            _service.ProcessorGeneratedCommands += OnProcessorGeneratedCommands;
        }
        /// <summary>
        /// Process the specified commands (execute them)
        /// </summary>
        /// <param name="commands"></param>
        public void ProcessCommands(IEnumerable<ICommandDto> commands)
        {
            var typedCommands = _converter.ConvertCommands(commands);
            _service.ProcessCommands(typedCommands);
        }
        /// <summary>
        /// Process all imported commands. This gets all commands that are currently in the commandstaterepository
        /// that have not yet been marked as processed (ExecutedOn is null)
        /// </summary>
        /// <returns></returns>
        public int ProcessImportedCommands()
        {
            var importedCommands = _converter.GetUnprocessedCommands();
            ProcessCommands(importedCommands);
            return ((ICollection<ICommandDto>)importedCommands).Count;
        }
        /// <summary>
        /// Merge the specified commands by applying them to a state that was rebuilt by executing all previous
        /// commands.
        /// </summary>
        /// <param name="commands"></param>
        public void MergeCommands(IEnumerable<ICommandDto> commands)
        {
            var typedCommands = _converter.ConvertCommands(commands);
            _service.MergeCommands(typedCommands, _converter);
        }
        /// <summary>
        /// Add configurations that target specific commands, so you can route specific commands for a particular
        /// (root) entity also to one or more additional domains.
        /// Use AddProcessorConfigs to route all domains for a specific (root) entity to a processor (service)
        /// </summary>
        /// <param name="configs"></param>
        public void AddCommandConfigs(IEnumerable<ICommandConfig> configs)
        {
            _converter.AddCommandConfigs(configs);
            _commandprocessors = configs.Select(s => s.Processor).Distinct().ToList();
        }
        /// <summary>
        /// Configure all commands for a specific entity (root) to use the specified assembly
        /// </summary>
        /// <param name="configs"></param>
        public void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs)
        {
            _converter.AddProcessorConfigs(configs);
            _processors = configs.Select(s => s.Processor).Distinct().ToList();
        }
        /// <summary>
        /// Call all services to persist their state. On supported platforms (.NET Framework and .NETCore 2.x)
        /// this is executed in a transactionscope, so if one fails no changes should be made.
        /// </summary>
        public void PersistChanges()
        {
            IEnumerable<ICommandProcessor> processors = new List<ICommandProcessor>();
            var persister = new PlatformSpecific();
            processors = _processors.Union(_commandprocessors);
            persister.PersistAllChanges(processors, _service);
        }
    }
}
