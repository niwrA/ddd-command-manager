[![Build Status](https://travis-ci.org/niwrA/ddd-command-manager.svg?branch=master)](https://travis-ci.org/niwrA/ddd-command-manager)
!Note: waiting for travis to update to Mono 2.7 so it supports strong naming

CommandManagerNetStandard
[![NuGet](https://img.shields.io/nuget/v/CommandManagerNetStandard.svg)](https://www.nuget.org/packages/CommandManagerNetStandard/)
[![NuGet](https://img.shields.io/nuget/dt/CommandManagerNetStandard.svg)](https://www.nuget.org/packages/CommandManagerNetStandard/)

CommandManagerCore
[![NuGet](https://img.shields.io/nuget/v/CommandManagerCore.svg)](https://www.nuget.org/packages/CommandManagerCore/)
[![NuGet](https://img.shields.io/nuget/dt/CommandManagerCore.svg)](https://www.nuget.org/packages/CommandManagerCore/)

CommandManagerNet
[![NuGet](https://img.shields.io/nuget/v/CommandManagerNet.svg)](https://www.nuget.org/packages/CommandManagerNet/)
[![NuGet](https://img.shields.io/nuget/dt/CommandManagerNet.svg)](https://www.nuget.org/packages/CommandManagerNet/)

# ddd-command-manager
Opinionated library that facilitates converting command messages to commands that can be validated and executed by one or more services. It has been designed in such a way that some of the benefits of Event Sourcing can be harnassed even for small apps or apps that do not necessarily have extreme throughput requirements. Instead the focus is on designing apps that are easy to maintain and test, and have a linear complexity so that cost of development remains highly predictable, that can be used on any device and at any scale starting from embedded devices.

The library is designed to work both with commands coming from an external source, for instance as being posted to an endpoint, and from a desktop application environment. It has been designed to be particularly suitable for MicroService and Domain Driven Design scenarios, and has a strong focus on being able to be Unit Tested - the proper kind where only a method's single responsibility is tested at every step. This guarantees that any form of testing is automatically possible and dependency injection supported at almost every level.

## Setup
Assuming that you have at least one command that is intended to perform an action against a backend service, here is what you will typically do to set this up. As an example, let's look at one or two  typical commands and how you would configure them.

### Create a command
The easiest way to do so is inherit them from CommandBase and add the ICommand interface. This will take care of almost everything and force you to implement your own Execute method. 

    public class CreateContactCommand : CommandBase, ICommand
    {
        public string Name { get; set; }
        public override void Execute()
        {
            ((IContactService)base.CommandProcessor).CreateContact(this.EntityGuid, this.Name);
            base.Execute();
        }
    }

### Pick, wrap or create a service
Next, create a service that implements ICommandProcessor, add ICommandProcessor to your interfaces on an existing class, or create a wrapper, whichever you prefer. ICommandProcessor does not currently have any members or properties, and is solely intended as a contract for dependency injection and such. If you don't have anything yet, create something like this:

    public interface IContactsService : ICommandProcessor
    {
      void CreateContact(this.EntityGuid, this.Name);
    }
    
    public class ContactsService : IContactsService
    {
      public void CreateContact(Guid guid, string name) 
      {
        // todo: your business logic for creating a contact goes here.
      }
    }

Already, you can now simply set an instance of the ContactsService on an instance of CreateContactCommand and execute it, and a unit test would simply check that the command executes the method on the service with the provided parameters. 

But this is not the typical scenario that we are going for here. We want to defer the decision what business logic needs to execute the command and manage that with the CommandManager.

TIP: setup your commands and your service classes up in a Shared Project type project. This contains only code, and no dependencies. Link this project to a class library that targets your chosen platform (for instance .NET Core). This way you can easily share this code between multiple class libraries that target different files and or bundles functionality into one dll.

  
Now we can create our service and CommandManager:

    var testService = new ContactsService();
    var commandManager = new CommandManager();

### Match commands with processors
Now that we have the basics setup, we are going to configure this command in the CommandManager. The CommandManager consists of two components: 
- CommandService that can (optionally) retrieve and store commands and execute them. By default, an InMemory repository is provided. We will use this for now, but see below for configuring your own.
- DTO to Command converter that can take a CommandDTO (e.g. posted from an external source such as a web / api controller) and serialise it into a command.

As a last step to get the command manager working, we are going to configure which commands need to be processed by which services (commmand processors). To do so, let's look a bit more closely at our CommandDto. 

    var commandDto = new CommandDto
    { Command = "Create",
      Entity = "Contact",
      CreatedOn = DateTime.Now,
      EntityGuid = Guid.NewGuid(),
      ParametersJson = @"{Name: 'James Smith'}" };

Dto stands for Data Transfer Object - this object is merely designed to pass information from one system to the next, in this case from a mobile, web, desktop client, controller or other service to the command processor. In the DTO, the most important data is what command we want to execute, against which entity we want to execute it, and what the parameters are. 
In our example, we want to Create (=Command) a Contact (=Entity) and the parameters are the Guid for this Contact (=EntityGuid) and the new contact's name. The contact's name is specific to this create command, and therefore we accept it as part of the ParametersJson property that we later deserialize into the executable command we configured for it. 

The simplest way to configure the CommandManager is just telling it to send all commands for a specific EntityRoot to a specific processor. We can do that like this:

    commandManager.AddProcessorConfigs(
      new List<IProcessorConfig> {
        new ProcessorConfig("Contact", testService, "CommandManagerTest", "CommandManagerTest")
      });

And now we are ready to post our command:

    commandManager.ProcessCommands(new List<CommandDto> { commandDto });

And that's it! This should call our CreateContact method on our ContactsService.

Note that we didn't specify an EntityRoot in our CommandDto - in this case it will assume Entity is also the EntityRoot.

### Queries and Events
Currently still in a separate branch but already on NuGet as version 2.1.0, support for Events and Queries has been added, each with their own manager (QueryManager, EventManager), with a similar setup in as far as applicable to those message flows. This allows you to use this library as a general setup for managing message flow, making it easy to support (Faster) non-http protocols in any circumstance. 

## Advanced Setup

### Per Command configuration
We can also configure additional processors on a command level. Say that we want a specific command to be executed not only by the processor we configured to handle all commands for this entity, but it also needs to be executed against a completely different processor. In this case, we use a CommandConfig, like so:

    new CommandConfig(<CommandName>, <Entity>, <Processor>, <NameSpace>, <Assembly>);

Where name is the command's name, in our previous example this would be "Create". So instead of or next to the AddProcessorConfig we saw above, we can add this configuration like this:

    commandManager.AddCommandConfigs(
      new List<CommandConfig> {
        new CommandConfig("Create", "Contact", contactsService, "CommandManagerTest", "CommandManagerTest");
      });

### CommandState repository 
All the fixed properties on the CommandDto help facilitate functionality we can use if we store our commands, such as auditing (when was what value changed by whom), recreating a historical version of an object (replay commands up to a specific point in time), or replaying (some) commands for undo/redo purposes, or creating a new projection (a cache, a lookup table, a report, or any other specialised read-only representation of data). 

However, the default repository implementation only keeps the commands in memory. If we want to save them, we need an implementation that does so. You can easily implement the ICommandStateRepository yourself, implement the four methods needed to support all functionality and support whatever database technology and/or distribution setup you need. 

### DateTime provider
In addition, the logic for setting the correct timestamps on the commands as they are processed is also injected. The contract for this is defined in IDateTimeProvider, which you can also provide your own. The default implementation simply assumes you will create a new CommandManager for each session (as in a typical WebApi Controller setup) and return the same session datetime for the duration. This value is used for setting the ReceivedOn datetime on a command. The ServerDateTime simply returns UtcNow and is used to set the ExecutedOn datetime.

## Testing
Testing a command is pretty easy - basically you want to know little more than that the command calls the service with the right arguments. If you setup a builder for your command, you can get really clean tests, like this:

    [Fact(DisplayName = "CreateContactCommand")]
    public void CreateCommand()
    {
        var contactsMock = new Mock<IContactService>();
        var sut = new CommandBuilder<CreateContactCommand>().Build(contactsMock.Object) as CreateContactCommand;

        sut.Name = "New Contact";
        sut.Execute();

        contactsMock.Verify(s => s.CreateContact(sut.EntityGuid, sut.Name), Times.Once);
    }

Here is the code for the builder, that you can reuse for any command:

    public class CommandBuilder<T> where T : ICommand, new()
    {
        public T Build(ICommandProcessor processor)
        {
          var commandRepoMock = new Mock<ICommandStateRepository>();
          var dateTimeProviderMock = new Mock<IDateTimeProvider>();
          var commandService = new CommandService(commandRepoMock.Object, dateTimeProviderMock.Object);

          dateTimeProviderMock.Setup(s => s.GetServerDateTime()).Returns(new DateTime(2017, 1, 1));
          CommandState commandState = new CommandState { Guid = Guid.NewGuid() };
          commandRepoMock.Setup(s => s.CreateCommandState(It.IsAny<Guid>())).Returns(commandState);

          var cmd = commandService.CreateCommand<T>();

          cmd.EntityGuid = Guid.NewGuid();
          cmd.CommandProcessor = processor;

          return (T)cmd;
        }
    }
