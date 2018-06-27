using Newtonsoft.Json;
using niwrA.EventManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.EventManager
{
    public class EventBase
    {
        public EventBase()
        {
        }

        public DateTime CreatedOn { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }


        public virtual string ParametersJson { get; set; }
        public Guid? Guid { get; set; }
        public string Entity { get; set; }
        public Guid EntityGuid { get; set; }

        public string EntityRoot { get; set; }
        public Guid EntityRootGuid { get; set; }

        public DateTime? ExecutedOn { get; set; }

        public string Event { get; set; }
        public string EventVersion { get; set; }


        private IEventProcessor _eventProcessor;
        public virtual IEventProcessor EventProcessor { get { return _eventProcessor; } set { _eventProcessor = value; } }

    }
    // the domain class for Events


    public class ProcessorConfig : IProcessorConfig
    {
        public ProcessorConfig(string entityRoot, IEventProcessor processor, string nameSpace, string assembly)
        {
            EntityRoot = entityRoot;
            Processor = processor;
            NameSpace = nameSpace;
            Assembly = assembly;
        }
        public IEventProcessor Processor { get; }
        public string EntityRoot { get; }
        public string NameSpace { get; }
        public string Assembly { get; }
        public IEvent GetEvent(string name, string entity, string parametersJson)
        {
            var eventConfig = new EventConfig(name, entity, this.Processor, this.NameSpace, this.Assembly);
            return eventConfig.GetEvent(parametersJson);
        }
    }
    public class EventConfig : IEventConfig
    {
        public EventConfig(string eventName, string entity, IEventProcessor processor, string nameSpace, string assembly)
        {
            EventName = eventName;
            EntityRoot = entity;
            Processor = processor;
            NameSpace = nameSpace;
            Assembly = assembly;
        }
        public string Key { get { return EventName + EntityRoot + "Event"; } }
        public string EventName { get; }
        public string EntityRoot { get; }
        public string NameSpace { get; }
        public string Assembly { get; }
        public IEventProcessor Processor { get; }
        public IEvent GetEvent(string json)
        {
            IEvent e;

            var type = Type.GetType(NameSpace + "." + Key + ", " + Assembly);

            if (type == null)
            {
                throw new CommandManager.TypeNotFoundException($"{Key} not found in {NameSpace} of {Assembly}");
            }

            if (!string.IsNullOrWhiteSpace(json))
            {
                var insert = @"'$type': '" + NameSpace + "." + Key + @", " + Assembly + "', ";
                json = json.Trim().Insert(1, insert);
                e = JsonConvert.DeserializeObject<IEvent>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            else
            {
                e = Activator.CreateInstance(type) as IEvent;
            }
            return e;
        }
    }
    [Serializable]
    public class EventDto : IEventDto
    {
        private string _entityRoot = "";

        public EventDto()
        {
        }

        public Guid? Guid { get; set; }
        public string EntityGuid { get; set; }
        public string Entity { get; set; }
        // default to Entity values if only Entity is provided,
        // for backward compatibility and ease of use
        public string EntityRoot
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_entityRoot)) { return Entity; } else { return _entityRoot; }
            }
            set { _entityRoot = value; }
        }
        public string EntityRootGuid { get; set; }
        public string Event { get; set; }
        public string EventVersion { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }
        public string ParametersJson { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ExecutedOn { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public CommandManager.Contracts.IParametersDto ParametersDto { get; set; }
    }


    //public interface IDateTimeProvider
    //{
    //    DateTime GetSessionDateTime();
    //    DateTime GetServerDateTime();
    //}
}
