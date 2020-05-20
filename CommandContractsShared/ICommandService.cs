using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace niwrA.CommandManager.Contracts
{
    public interface ICommand
    {
        Guid? Guid { get; set; }

        string Entity { get; set; }
        string EntityGuid { get; set; }

        string EntityRoot { get; set; }
        string EntityRootGuid { get; set; }

        string Command { get; set; }
        string ParametersJson { get; set; }

        DateTime? ExecutedOn { get; set; }
        DateTime? ReceivedOn { get; set; }
        DateTime CreatedOn { get; set; }

        string UserName { get; set; }
        string TenantId { get; set; }
        string UserId { get; set; }
        string TransactionId { get; set; }
        string ConfigurationId { get; set; }

        ICommandStateRepository CommandRepository { get; set; }
        ICommandProcessor CommandProcessor { get; set; }

        void Execute();
    }

    public interface ICommandState
    {
        Guid Guid { get; set; }
        string EntityGuid { get; set; }
        string Entity { get; set; }
        string EntityRootGuid { get; set; }
        string EntityRoot { get; set; }
        string Command { get; set; }
        string CommandVersion { get; set; }
        string ParametersJson { get; set; }
        DateTime? ExecutedOn { get; set; }
        DateTime? ReceivedOn { get; set; }
        DateTime CreatedOn { get; set; }
        string UserName { get; set; }
        string UserId { get; set; }
        string TenantId { get; set; }
        string TransactionId { get; set; }
        string ConfigurationId { get; set; }
    }
    // defines the contract for a Command Repository implementation
    public interface ICommandStateRepository
    {
        void PersistChanges();
        ICommandState CreateCommandState(Guid guid);
        IEnumerable<ICommandState> GetCommandStates();
        IEnumerable<ICommandState> GetCommandStates(string entityGuid);
        IEnumerable<ICommandState> GetUnprocessedCommandStates();
    }

    public interface ITimeStampedEntityState
    {
        Guid Guid { get; set; }
        DateTime CreatedOn { get; set; }
        DateTime UpdatedOn { get; set; }
    }
    public interface INamedEntity
    {
        DateTime CreatedOn { get; }
        DateTime UpdatedOn { get; }
        Guid Guid { get; }
        string Name { get; }
    }

    public interface INamedEntityState : ITimeStampedEntityState
    {
        string Name { get; set; }
    }
    // defines the contract for an Entity Repository implementation
    public interface IEntityRepository
    {
        void PersistChanges();
        Task PersistChangesAsync();
    }
    public interface IEntityReadOnlyRepository { }

    // defines the contract for entities compatible with commanding
    public interface ICommandableEntity
    {
        Guid Guid { get; }
    }

    public interface ICommandProcessor
    {
        void PersistChanges();

        event Action<IEnumerable<ICommandDto>> GeneratedCommandsForBatch;
    }

    public interface ICommandConfig
    {
        string Key { get; }
        string CommandName { get; }
        ICommand GetCommand(string parametersJson);
        ICommandProcessor Processor { get; }
        string Entity { get; }
        string NameSpace { get; }
        string Assembly { get; }
    }

    public interface IProcessorConfig
    {
        ICommandProcessor Processor { get; }
        string EntityRoot { get; }
        string NameSpace { get; }
        string Assembly { get; }
        ICommand GetCommand(string name, string entity, string parametersJson);
    }
    public interface IParametersDto
    {

    }
    public interface ICommandDto
    {
        Guid? Guid { get; set; }
        string Command { get; set; }
        string CommandVersion { get; set; }
        string EntityGuid { get; set; }
        string Entity { get; set; }
        string EntityRootGuid { get; set; }
        string EntityRoot { get; set; }
        string UserName { get; set; }
        string TenantId { get; set; }
        string TransactionId { get; set; }
        string ConfigurationId { get; set; }
        string UserId { get; set; }
        DateTime CreatedOn { get; set; }
        string ParametersJson { get; set; }
        IParametersDto ParametersDto { get; set; }
    }
    public interface ICommandService
    {
        void ProcessCommand(ICommand command);
        void ProcessCommands(IEnumerable<ICommand> commands);
        void PersistChanges();
        void MergeCommands(IEnumerable<ICommand> commands, ICommandDtoToCommandConverter converter);
        T CreateCommand<T>() where T : ICommand, new();
        event Action<IEnumerable<ICommandDto>> ProcessorGeneratedCommands;
    }
}
