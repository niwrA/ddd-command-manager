using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.EventManager.Contracts
{
    public interface IEvent
    {
        Guid? Guid { get; set; }
        string EntityRoot { get; set; }
        string Event { get; set; }
        string EventVersion { get; set; }
        string ParametersJson { get; set; }
        DateTime? ExecutedOn { get; set; }
        DateTime? ReceivedOn { get; set; }
        DateTime CreatedOn { get; set; }
        string UserName { get; set; }
        string TenantId { get; set; }
        string UserId { get; set; }
        string TransactionId { get; set; }
        string ConfigurationId { get; set; }
        IEventProcessor EventProcessor { get; set; }
        void Execute();
    }

    public interface IEventDto
    {
        Guid? Guid { get; set; }
        string Entity { get; set; }
        string EntityGuid { get; set; }
        string EntityRoot { get; set; }
        string EntityRootGuid { get; set; }
        string Event { get; set; }
        string EventVersion { get; set; }
        string ParametersJson { get; set; }
        DateTime? ExecutedOn { get; set; }
        DateTime? ReceivedOn { get; set; }
        DateTime CreatedOn { get; set; }
        string UserName { get; set; }
        string TenantId { get; set; }
        string UserId { get; set; }
        string TransactionId { get; set; }
        string ConfigurationId { get; set; }
    }
    public interface IEventProcessor
    {
    }

    public interface IEventConfig
    {
        string Key { get; }
        string EventName { get; }
        IEvent GetEvent(string parametersJson);
        IEventProcessor Processor { get; }
        string EntityRoot { get; }
        string NameSpace { get; }
        string Assembly { get; }
    }

    public interface IProcessorConfig
    {
        IEventProcessor Processor { get; }
        string EntityRoot { get; }
        string NameSpace { get; }
        string Assembly { get; }
        IEvent GetEvent(string name, string entityRoot, string parametersJson);
    }

    public interface IEventService
    {
        T CreateEvent<T>() where T : IEvent, new();
        void ProcessEvents(IEnumerable<IEvent> commands);
        void ProcessEvents(IEvent e);
    }

    public interface IEventDtoToEventConverter
    {
        void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
        void AddEventConfigs(IEnumerable<IEventConfig> configs);
        IEnumerable<IEvent> ConvertEvent(IEventDto e);
        IEnumerable<IEvent> ConvertEvents(IEnumerable<IEventDto> es);
        IEnumerable<IProcessorConfig> GetProcessorConfigs(string key);
        IEnumerable<IEventConfig> GetEventConfigs(string key);
    }

    public interface IEventManager
    {
        void ProcessEvents(IEnumerable<IEventDto> commands);
        void AddEventConfigs(IEnumerable<IEventConfig> configs);
        void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
    }
}
