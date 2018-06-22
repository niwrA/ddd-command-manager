using System;
using System.Collections.Generic;
using System.Text;

namespace CommandManagerCoreTests
{
    using niwrA.EventManager;
    using Moq;
    using Xunit;
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using niwrA.EventManager.Contracts;

    [Trait("Manager", "EventManager")]
    public class EventManagerTests
    {
        const string _assembly = "CommandManagerCoreTests";
        const string _namespace = "CommandManagerCoreTests.Fakes";

        [Fact(DisplayName = "IDateTimeProvider_DefaultsTo_DefaultDateTimeProvider_and_InMemoryRepository")]
        public void IDateTimeProvider_DefaultsTo_DefaultDateTimeProvider_and_InMemoryRepository()
        {
            var testService = new Mock<Fakes.ITestEventService>();
            var sut = new EventManager();
            var commandDto = new EventDtoBuilder().Build();
            var config = new ProcessorConfig(commandDto.Entity, testService.Object, _namespace, _assembly);
            sut.AddProcessorConfigs(new List<IProcessorConfig> { config });
            sut.ProcessEvents(new List<EventDto> { commandDto });

            testService.Verify(v => v.Handle(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        }
    }
}
