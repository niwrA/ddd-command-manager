# ddd-command-manager
Opinionated library that facilitates converting command messages to commands that can be validated and executed by one or more services. It has been designed in such a way that some of the benefits of Event Sourcing can be harnassed even for small apps or apps that do not necessarily have extreme throughput requirements. Instead the focus is on designing apps that are aesy to maintain and test, and have a linear complexity so that cost of development remains highly predictable, that can be used on any device and at any scale starting from embedded devices. 

The library is designed to work both with commands coming from an external source, for instance as being posted to an endpoint, and from a desktop application environment. It has been designed to be particularly suitable for MicroService and Domain Driven Design scenarios, and has a strong focus on being able to be Unit Tested - the proper kind where only a method's single responsibility is tested at every step. This guarantees that any form of testing is automatically possible and dependency injection supported at almost every level.

In a typical scenario 

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

    public interface IContactService : ICommandProcessor
    {
      void CreateContact(this.EntityGuid, this.Name);
    }
    
    public class ContactService : IContactService
    {
      public void CreateContact(Guid guid, string name) 
      {
        // todo: your business logic for creating a contact goes here.
      }
    }

Already, you can now simply set an instance of the ContactService on an instance of CreateContactCommand and execute it, and a unit test would simply check that the command executes the method on the service with the provided parameters. 

But this is not the typical scenario that we are going for here. We want to defer the decision what business logic needs to execute the command and manage that with the CommandManager.

TIP: setup your commands and your service classes up in a Shared Project type project. This contains only code, and no dependencies. Link this project to a class library that targets your chosen platform (for instance .NET Core). This way you can easily share this code between multiple class libraries that target different files and or bundles functionality into one dll.

### Setup the CommandState repository and DateTime provider
Now that we have the basics setup, we are going to configure this command in the CommandManager. The CommandManager consists of two components: 
- CommandService that can (optionally) retrieve and store commands and execute them.
- DTO to Command converter that can take a CommandDTO (e.g. posted from an external source such as a web / api controller) and serialise it into a command.

Some of the functionality in the CommandService needs it to be able to store and retrieve commands. What functionality it needs is defined in the ICommandStateRepository interface and so this functionality can be injected and use any database technology you require. If you don't actually need to store the commands, this is a minimal implementation:

    class CommandState : ICommandState
    {
      public Guid Guid { get; set; }
      public Guid EntityGuid { get; set; }
      public string Entity { get; set; }
      public Guid EntityRootGuid { get; set; }
      public string EntityRoot { get; set; }
      public string Command { get; set; }
      public string CommandVersion { get; set; }
      public string ParametersJson { get; set; }
      public DateTime? ExecutedOn { get; set; }
      public DateTime? ReceivedOn { get; set; }
      public DateTime CreatedOn { get; set; }
      public string UserName { get; set; }
    }
    class CommandStateRepository : ICommandStateRepository
    {
      public ICommandState CreateCommandState()
      {
        return new CommandState();
      }

      public IEnumerable<ICommandState> GetCommandStates()
      {
        throw new NotImplementedException();
      }

      public IEnumerable<ICommandState> GetCommandStates(Guid entityGuid)
      {
        throw new NotImplementedException();
      }

      public IEnumerable<ICommandState> GetUnprocessedCommandStates()
      {
        throw new NotImplementedException();
      }

      public void PersistChanges()
      {
        throw new NotImplementedException();
      }
    }
  
TODO: I will provide a configuration option that implements a simple in-memory implementation, so this is even easier and faster to setup.

In addition, the logic for setting the correct timestamps on the commands as they are processed is also injected. The contract for this is defined in IDateTimeProvider. An implementation of it could look like this:

    class DateTimeProvider : IDateTimeProvider
    {
      private DateTime utcNow = DateTime.UtcNow;
      public DateTime GetServerDateTime()
      {
        return DateTime.UtcNow;
      }

      public DateTime GetSessionUtcDateTime()
      {
        return utcNow;
      }
    }
  
Now we can create our CommandManager:

    var testService = new TestService();
    var dateTimeProvider = new DateTimeProvider();
    var commandStateRepository = new CommandStateRepository();
    var commandManager = new CommandManager(commandStateRepository, dateTimeProvider);

### Match commands with processors
Finally, we are going to configure which commands need to be processed by which services (commmand processors). To do so, let's look a bit more closely at our CommandDto. 

    var commandDto = new CommandDto
    { Command = "Create",
      Entity = "Contact",
      CreatedOn = DateTime.Now,
      EntityGuid = Guid.NewGuid(),
      ParametersJson = @"{Name: 'James Smith'}" };

Dto stands for Data Transfer Object - this object is merely designed to pass information from one system to the next, in this case from a mobile, web, desktop client, controller or other service to the command processor. In the DTO, the most important data is what command we want to execute, against which entity we want to execute it, and what the parameters are. 
In our example, we want to Create (=Command) a Contact (=Entity) and the parameters are the Guid for this Contact (=EntityGuid) and the new contact's name. The contact's name is specific to this create command, and therefore we accept it as part of the ParametersJson property that we later deserialize into the executable command we configured for it. 

All the fixed properties on the CommandDto help facilitate functionality we can use if we store our commands, such as auditing (when was what value changed by whom), recreating a historical version of an object (replay commands up to a specific point in time), or replaying (some) commands for undo/redo purposes, or creating a new projection (a cache, a lookup table, a report, or any other specialised read-only representation of data). 

The simplest way to configure the CommandManager is just telling it to send all commands for a specific EntityRoot to a specific processor. We can do that like this:

    commandManager.AddProcessorConfigs(
      new List<IProcessorConfig> {
        new ProcessorConfig("Contact", testService, "CommandManagerTest", "CommandManagerTest")
      });

And now we are ready to post our command:

    commandManager.ProcessCommands(new List<CommandDto> { commandDto });

And that's it! Note that we didn't specify an EntityRoot in our CommandDto - in this case it will assume Entity is also the EntityRoot. 
