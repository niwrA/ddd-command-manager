using niwrA.CommandManager.Helpers;
using niwrA.EventManager.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace niwrA.EventManager
{
    public class EventManager : IEventManager
    {
        private IEventDtoToEventConverter _converter;
        private IEventService _service;
        private List<IEventProcessor> _eventprocessors = new List<IEventProcessor>();
        private List<IEventProcessor> _processors = new List<IEventProcessor>();
        public EventManager()
        {
            var dateTimeProvider = new DefaultDateTimeProvider();
            _converter = new EventDtoToEventConverter(dateTimeProvider);
            _service = new EventService(dateTimeProvider);
        }
        public EventManager(IEventService service, IEventDtoToEventConverter converter)
        {
            _service = service;
            _converter = converter;
        }
        public void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs)
        {
            _converter.AddProcessorConfigs(configs);
            _processors = configs.Select(s => s.Processor).Distinct().ToList();
        }

        public void AddEventConfigs(IEnumerable<IEventConfig> configs)
        {
            _converter.AddEventConfigs(configs);
            _eventprocessors = configs.Select(s => s.Processor).Distinct().ToList();
        }

        public void ProcessEvents(IEnumerable<IEventDto> events)
        {
            var typedEvents = _converter.ConvertEvents(events);
            _service.ProcessEvents(typedEvents);
        }
    }
}
