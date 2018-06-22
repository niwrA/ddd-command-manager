using niwrA.EventManager.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace niwrA.EventManager
{
    public class EventDtoToEventConverter : IEventDtoToEventConverter
    {
        private Dictionary<string, IProcessorConfig> _configs = new Dictionary<string, IProcessorConfig>();
        private Dictionary<string, IEventConfig> _eventConfigs = new Dictionary<string, IEventConfig>();
        private CommandManager.IDateTimeProvider _dateTimeProvider;
        private ILookup<string, IEventConfig> _eventLookups;
        private ILookup<string, IProcessorConfig> _processorLookups;

        public EventDtoToEventConverter(CommandManager.IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _eventLookups = new List<IEventConfig>().ToLookup(o => o.Key);
            _processorLookups = new List<IProcessorConfig>().ToLookup(o => o.EntityRoot);
        }
        public void AddEventConfigs(IEnumerable<IEventConfig> configs)
        {
            _eventLookups = configs.ToLookup(o => o.Key);
        }
        public IEnumerable<IEventConfig> GetEventConfigs(string key)
        {
            var configs = new List<IEventConfig>();
            configs.AddRange(_eventLookups[key]);
            return configs;
        }
        public void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs)
        {
            _processorLookups = configs.ToLookup(o => o.EntityRoot);
        }
        public IEnumerable<IProcessorConfig> GetProcessorConfigs(string key)
        {
            var configs = new List<IProcessorConfig>();
            configs.AddRange(_processorLookups[key]);
            return configs;
        }

        public IEnumerable<IEvent> ConvertEvents(IEnumerable<IEventDto> events)
        {
            var processed = new List<IEvent>();
            foreach (var e in events)
            {
                processed.AddRange(ConvertEvent(e));
            }
            return processed;
        }
        public IEnumerable<IEvent> ConvertEvent(IEventDto e)
        {
            //      IEvent typedEvent = null;
            var typedEvents = new List<IEvent>();
            // either take existing name from state, or construct from dto
            // the replace is a bit hacky, should probably clean the eventstore
            var eventName = e.Event;
            var parametersJson = e.ParametersJson;

            ProcessEventConfigs(e, typedEvents, eventName, parametersJson);
            ProcessProcessorConfigs(e, typedEvents, eventName, parametersJson);

            if (typedEvents.Any())
            {
                return typedEvents;
            }
            throw new EventNotConfiguredException($"The event named '{e.Event}' for entity root type '{e.EntityRoot}' does not have a matching configuration.");
        }

        private void ProcessProcessorConfigs(IEventDto e, List<IEvent> typedEvents, string eventName, string parametersJson)
        {
            var processorConfigs = GetProcessorConfigs(e.EntityRoot);
            foreach (var config in processorConfigs)
            {
                var typedEvent = config.GetEvent(eventName, e.EntityRoot, parametersJson);

                SetEventProperties(e, config.Processor, typedEvent);
                typedEvents.Add(typedEvent);
            }
        }

        private void ProcessEventConfigs(IEventDto e, List<IEvent> typedEvents, string eventName, string parametersJson)
        {
            var eventConfigs = GetEventConfigs(eventName + e.EntityRoot + "Event");
            foreach (var config in eventConfigs)
            {
                var typedEvent = config.GetEvent(parametersJson);

                SetEventProperties(e, config.Processor, typedEvent);
                typedEvents.Add(typedEvent);
            }
        }

        private void SetEventProperties(IEventDto e, IEventProcessor processor, IEvent typedEvent)
        {
            CopyEventDtoIntoEvent(e, processor, typedEvent);
        }

        private void CopyEventDtoIntoEvent(IEventDto e, IEventProcessor processor, IEvent typedEvent)
        {
            typedEvent.CreatedOn = e.CreatedOn;
            typedEvent.ReceivedOn = _dateTimeProvider.GetSessionDateTime();
            typedEvent.EntityRoot = e.EntityRoot;
            typedEvent.Guid = e.Guid;
            typedEvent.ParametersJson = e.ParametersJson;
            typedEvent.UserName = e.UserName;
            typedEvent.TenantId = e.TenantId;
            typedEvent.Event = e.Event;

            typedEvent.EventProcessor = processor;
        }
    }
}
