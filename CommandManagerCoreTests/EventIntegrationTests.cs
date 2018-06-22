using Moq;
using niwrA.EventManager;
using niwrA.EventManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CommandManagerCoreTests
{
    public class EventIntegrationTests
    {
        private string _assembly = "CommandManagerCoreTests";
        private string _namespace = "CommandManagerCoreTests.Fakes";
        [Fact(DisplayName = "ProcessEvent_CanTargetMultipleProcessorsForOneEvent")]
        public void ProcessEvent_CanTargetMultipleProcessorsForOneEvent()
        {
            var dateTimeProvider = new Mock<niwrA.CommandManager.IDateTimeProvider>().Object;
            var processor1 = new Mock<Fakes.ITestEventService>();
            var processor2 = new Mock<Fakes.ITestEventService>();
            var processor3 = new Mock<Fakes.ITestEventService>();
            var processor4 = new Mock<Fakes.ITestEventService>();

            var sut = new EventDtoToEventConverter(dateTimeProvider);
            var eventDto = new EventDtoBuilder().Build();
            var eventConfigBuilder = new EventConfigBuilder();
            var configs = new List<IEventConfig>
            {
              eventConfigBuilder
              .WithEventName("Handle")
              .WithEntity("RootEntity")
              .WithProcessor(processor1.As<IEventProcessor>().Object)
              .Build(),
              eventConfigBuilder
              .WithEventName("Handle")
              .WithEntity("RootEntity")
              .WithProcessor(processor2.As<IEventProcessor>().Object).Build()            };

            var processorConfigs = new List<niwrA.EventManager.Contracts.IProcessorConfig>
            {
                new ProcessorConfig("RootEntity", processor3.As<IEventProcessor>().Object, _namespace, _assembly),
                new ProcessorConfig("RootEntity", processor4.As<IEventProcessor>().Object, _namespace, _assembly)
            };

            sut.AddEventConfigs(configs);
            sut.AddProcessorConfigs(processorConfigs);

            var typedEvents = sut.ConvertEvent(eventDto);
            var sut2 = new EventService(dateTimeProvider);

            sut2.ProcessEvents(typedEvents);

            processor1.Verify(s => s.Handle(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            processor2.Verify(s => s.Handle(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            processor3.Verify(s => s.Handle(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            processor4.Verify(s => s.Handle(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }

    }
    public class EventConfigBuilder
    {
        private string _assembly = "CommandManagerCoreTests";
        private string _namespace = "CommandManagerCoreTests.Fakes";
        private string _eventName = "GetAllContactNames";
        private string _entity = "RootEntity";

        private IEventProcessor _processor;
        public IEventConfig Build()
        {
            if (_processor == null) { _processor = new Mock<Fakes.ITestEventService>().As<IEventProcessor>().Object; }
            var eventConfig = new EventConfig(_eventName, _entity, _processor, _namespace, _assembly);
            return eventConfig;
        }
        public EventConfigBuilder WithNonExistingNameSpace()
        {
            _namespace = "thisnamespacedoesnotexist";
            return this;
        }
        public EventConfigBuilder WithProcessor(IEventProcessor processor)
        {
            _processor = processor;
            return this;
        }
        public EventConfigBuilder WithEventName(string name)
        {
            _eventName = name;
            return this;
        }
        public EventConfigBuilder WithEntity(string entity)
        {
            _entity = entity;
            return this;
        }
    }
    public class EventDtoBuilder
    {
        public EventDto Build()
        {
            var eventDto = new EventDto
            {
                Entity = "RootEntity",
                Event = "Handle",
                EntityGuid = Guid.NewGuid().ToString(),
                EntityRoot = "RootEntity",
                Guid = Guid.NewGuid(),
                CreatedOn = new DateTime(2018, 1, 1),
                UserName = "userName",
                TenantId = "tenantId",
                ParametersJson = @"{Name: 'James Smith'}"
            };
            return eventDto;
        }
    }

    public class EventDtoConverterBuilder
    {
        private string _assembly = "CommandManagerCoreTests";
        private string _namespace = "CommandManagerCoreTests.Fakes";
        public Mock<Fakes.ITestEventService> TestServiceMock = new Mock<Fakes.ITestEventService>();
        public IEventDtoToEventConverter Build()
        {
            var dateTimeProvider = new Mock<niwrA.CommandManager.IDateTimeProvider>().Object;
            TestServiceMock = new Mock<Fakes.ITestEventService>();

            var sut = new EventDtoToEventConverter(dateTimeProvider);
            var processorConfigs = new List<IProcessorConfig>
                {
                    new ProcessorConfig("RootEntity", TestServiceMock.As<IEventProcessor>().Object, _namespace, _assembly)
                };

            sut.AddProcessorConfigs(processorConfigs);
            return sut;
        }
    }
}
