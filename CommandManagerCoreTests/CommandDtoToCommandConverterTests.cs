﻿using niwrA.CommandManager;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System;
using niwrA.CommandManager.Contracts;

namespace CommandManagerCoreTests
{
    [Trait("Entity", "CommandDtoToCommandConverter")]
    public class CommandDtoToCommandConverterTests
    {
        const string _assembly = "CommandManagerCoreTests";
        const string _namespace = "CommandManagerCoreTests.Fakes";

        [Fact(DisplayName = "CanSerializeToTypedCommand_FromProcessorConfig")]
        public void CanSerializeToTypedCommand_FromProcessorConfig()
        {
            var json = @"{'Name': 'new name'}";
            var projectsMoq = new Mock<Fakes.ITestService>();
            var processorConfig = new ProcessorConfig("RootEntity", projectsMoq.Object, _namespace, _assembly);
            var command = processorConfig.GetCommand("Rename", "RootEntity", json) as Fakes.RenameRootEntityCommand;
            Assert.Equal("new name", command.Name);
        }

        [Fact(DisplayName = "CanFindCommand_ByEntityAndEntityRootName")]
        public void CanFindCommand_ByEntityAndEntityRootName()
        {
            // var json = @"{'Name': 'new name'}";
            CommandDtoConverterBuilder builder = new CommandDtoConverterBuilder();
            var processorConfig = new ProcessorConfig("RootEntity", builder.TestServiceMock.Object, _namespace, _assembly);
            var processorConfig2 = new ProcessorConfig("RootEntityBlah", builder.TestServiceMock.Object, _namespace, _assembly);
            var converter = builder
                .WithProcessorConfig(processorConfig)
                .WithProcessorConfig(processorConfig2)
                .Build();
            var commandDto = new CommandDtoBuilder().Build();
            var command = converter.ConvertCommand(commandDto);
            Assert.Equal("Create", command.First().Command);
        }

        [Fact(DisplayName = "ConvertCommand_CanConvertACommandDtoToACommand")]
        public void ProcessCommands_CanProcessCommand()
        {
            var commandDto = new CommandDtoBuilder().Build();

            var sutBuilder = new CommandDtoConverterBuilder();
            var sut = sutBuilder.Build();

            var typedCommand = sut.ConvertCommand(commandDto);

            Assert.True(!string.IsNullOrWhiteSpace(commandDto.EntityRootGuid));
            Assert.Equal(sutBuilder.TestServiceMock.Object, typedCommand.First().CommandProcessor);
            Assert.Equal(commandDto.CreatedOn, typedCommand.First().CreatedOn);
            Assert.Equal(commandDto.Entity, typedCommand.First().Entity);
            Assert.Equal(commandDto.EntityGuid, typedCommand.First().EntityGuid);
            Assert.Equal(commandDto.EntityRoot, typedCommand.First().EntityRoot);
            Assert.Equal(commandDto.EntityRootGuid, typedCommand.First().EntityRootGuid);
            Assert.Equal(commandDto.Guid, typedCommand.First().Guid);
            Assert.Equal(commandDto.Command, typedCommand.First().Command);
            Assert.Equal(commandDto.UserName, typedCommand.First().UserName);
            Assert.Equal(commandDto.TenantId, typedCommand.First().TenantId);
        }

        [Fact(DisplayName = "ConvertCommands_CanConvertMultipleCommands")]
        public void ConvertCommands_CanConvertMultipleCommands()
        {
            var commandDto = new CommandDtoBuilder().Build();
            var commandDto2 = new CommandDtoBuilder().Build();

            var sutBuilder = new CommandDtoConverterBuilder();
            var sut = sutBuilder.Build();

            var sutResult = sut.ConvertCommands(new List<CommandDto> { commandDto, commandDto2 });

            Assert.Equal(2, sutResult.Count());
        }

        public class CommandDtoBuilder
        {
            public CommandDto Build()
            {
                var commandDto = new CommandDto
                {
                    Entity = "RootEntity",
                    Command = "Create",
                    EntityGuid = Guid.NewGuid().ToString(),
                    EntityRoot = "RootEntity",
                    EntityRootGuid = Guid.NewGuid().ToString(),
                    Guid = Guid.NewGuid(),
                    CreatedOn = new DateTime(2018, 1, 1),
                    UserName = "userName",
                    TenantId = "tenantId",
                    ParametersJson = @"{Name: 'James Smith'}"
                };
                return commandDto;
            }
        }

        public class CommandDtoConverterBuilder
        {
            public Mock<Fakes.ITestService> TestServiceMock = new Mock<Fakes.ITestService>();
            private List<IProcessorConfig> _processorConfigs = new List<IProcessorConfig>();

            public ICommandDtoToCommandConverter Build()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>().Object;
                TestServiceMock = new Mock<Fakes.ITestService>();
                var repo = new Mock<ICommandStateRepository>();
                repo.Setup(s => s.CreateCommandState(It.IsAny<Guid>())).Returns(new Fakes.CommandState());

                var sut = new CommandDtoToCommandConverter(repo.Object, dateTimeProvider);
                if(!_processorConfigs.Any())
                {
                    _processorConfigs.Add(new ProcessorConfig("RootEntity", TestServiceMock.Object, _namespace, _assembly));
                }
                sut.AddProcessorConfigs(_processorConfigs);

                return sut;
            }
            public CommandDtoConverterBuilder WithProcessorConfig(IProcessorConfig processorConfig)
            {
                _processorConfigs.Add(processorConfig);
                return this;
            }
        }

        [Fact(DisplayName = "GetUnprocessedCommands_CanGetUnprocessedCommands")]
        public void CommandManager_CanGetUnprocessedCommands()
        {
            var dateTimeProvider = new Mock<IDateTimeProvider>().Object;

            var mockTest = new Mock<ICommandState>();
            mockTest.Setup(s => s.Entity).Returns("Test");
            var existingCommands = new List<ICommandState> { mockTest.Object };

            var repo = new Mock<ICommandStateRepository>();
            repo.Setup(s => s.GetUnprocessedCommandStates()).Returns(existingCommands);

            var sut = new CommandDtoToCommandConverter(repo.Object, dateTimeProvider);
            var sutResult = sut.GetUnprocessedCommands() as List<ICommandDto>;

            Assert.Single(sutResult);
            Assert.Equal("Test", sutResult.First().Entity);
        }
    }
}
