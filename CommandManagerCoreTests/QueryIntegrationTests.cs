using CommandManagerCoreTests.Fakes;
using Moq;
using niwrA.QueryManager;
using niwrA.QueryManager.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CommandManagerCoreTests
{
    public class QueryIntegrationTests
    {
        private string _assembly = "CommandManagerCoreTests";
        private string _namespace = "CommandManagerCoreTests.Fakes";
        [Fact(DisplayName = "ProcessQuery_CanTargetMultipleProcessorsForOneQuery")]
        public void ProcessQuery_CanTargetMultipleProcessorsForOneQuery()
        {
            var dateTimeProvider = new Mock<niwrA.CommandManager.IDateTimeProvider>().Object;
            var processor1 = new Mock<Fakes.ITestQueryService>();
            var queryResult = new Mock<IRootEntity>();
            processor1.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<string>())).Returns(queryResult.Object);

            var sut = new QueryDtoToQueryConverter(dateTimeProvider);
            var queryDto = new QueryDtoBuilder().Build();
            var queryConfigBuilder = new QueryConfigBuilder();

            var processorConfigs = new List<niwrA.QueryManager.Contracts.IProcessorConfig>
            {
                new ProcessorConfig("RootEntity", processor1.Object, _namespace, _assembly),
            };

            sut.AddProcessorConfigs(processorConfigs);

            var typedQueries = sut.ConvertQuery(queryDto);
            var sut2 = new QueryService(dateTimeProvider);

            var results = sut2.ProcessQueries(typedQueries);
            Assert.Single(results);
        }

    }
    public class QueryConfigBuilder
    {
        private string _assembly = "CommandManagerCoreTests";
        private string _namespace = "CommandManagerCoreTests.Fakes";
        private string _queryName = "GetAllContactNames";
        private string _entity = "RootEntity";

        private IQueryProcessor _processor;
        public IQueryConfig Build()
        {
            if (_processor == null) { _processor = new Mock<Fakes.ITestQueryService>().Object; }
            var queryConfig = new QueryConfig(_queryName, _entity, _processor, _namespace, _assembly);
            return queryConfig;
        }
        public QueryConfigBuilder WithNonExistingNameSpace()
        {
            _namespace = "thisnamespacedoesnotexist";
            return this;
        }
        public QueryConfigBuilder WithProcessor(IQueryProcessor processor)
        {
            _processor = processor;
            return this;
        }
        public QueryConfigBuilder WithQueryName(string name)
        {
            _queryName = name;
            return this;
        }
        public QueryConfigBuilder WithEntity(string entity)
        {
            _entity = entity;
            return this;
        }
    }
    public class QueryDtoBuilder
    {
        public QueryDto Build()
        {
            var queryDto = new QueryDto
            {
                Entity = "RootEntity",
                Query = "Get",
                EntityGuid = Guid.NewGuid().ToString(),
                EntityRoot = "RootEntity",
                Guid = Guid.NewGuid(),
                CreatedOn = new DateTime(2018, 1, 1),
                UserName = "userName",
                TenantId = "tenantId",
                ParametersJson = @"{Name: 'James Smith'}"
            };
            return queryDto;
        }
    }

    public class QueryDtoConverterBuilder
    {
        private string _assembly = "CommandManagerCoreTests";
        private string _namespace = "CommandManagerCoreTests.Fakes";
        public Mock<Fakes.ITestQueryService> TestServiceMock = new Mock<Fakes.ITestQueryService>();
        public IQueryDtoToQueryConverter Build()
        {
            var dateTimeProvider = new Mock<niwrA.CommandManager.IDateTimeProvider>().Object;
            TestServiceMock = new Mock<Fakes.ITestQueryService>();

            var sut = new QueryDtoToQueryConverter(dateTimeProvider);
            var processorConfigs = new List<IProcessorConfig>
                {
                    new ProcessorConfig("RootEntity", TestServiceMock.Object, _namespace, _assembly)
                };

            sut.AddProcessorConfigs(processorConfigs);
            return sut;
        }
    }
}
