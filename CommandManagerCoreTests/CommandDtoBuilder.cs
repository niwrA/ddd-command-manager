using niwrA.CommandManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommandManagerCoreTests
{
  public class CommandDtoBuilder
  {
    private string _entity = "RootEntity";
    private string _command = "Create";
    private string _entityGuid = Guid.NewGuid().ToString();
    private Guid _guid = Guid.NewGuid();
    private DateTime _createdOn = new DateTime(2018, 1, 1);
    private string _parametersJson = @"{Name: 'James Smith'}";
    public CommandDto Build()
    {
      var commandDto = new CommandDto
      {
        Entity = _entity,
        Command = _command,
        EntityGuid = _entityGuid,
        Guid = _guid,
        CreatedOn = _createdOn,
        ParametersJson = _parametersJson
      };
      return commandDto;
    }
    public CommandDtoBuilder WithEntityGuid(string guid)
    {
      _entityGuid = guid;
      return this;
    }
    public CommandDtoBuilder WithCommand(string name)
    {
      _command = name;
      return this;
    }
    public CommandDtoBuilder WithParametersJson(string json)
    {
      _parametersJson = json;
      return this;
    }
    public CommandDtoBuilder WithCreatedOn(DateTime createdOn)
    {
      _createdOn = createdOn;
      return this;
    }
  }
}
