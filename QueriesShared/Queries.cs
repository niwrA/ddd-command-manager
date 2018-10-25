using Newtonsoft.Json;
using niwrA.QueryManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.QueryManager
{
    public class QueryBase
    {
        public QueryBase()
        {
        }

        public DateTime CreatedOn { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }


        public virtual string ParametersJson { get; set; }
        public Guid Guid { get; set; }
        public string Entity { get; set; }
        public Guid EntityGuid { get; set; }

        public string EntityRoot { get; set; }
        public Guid EntityRootGuid { get; set; }

        public DateTime? ExecutedOn { get; set; }

        public string Query { get; set; }
        public string QueryVersion { get; set; }


        private IQueryProcessor _queryProcessor;
        public virtual IQueryProcessor QueryProcessor { get { return _queryProcessor; } set { _queryProcessor = value; } }

    }
    // the domain class for Querys


    public class ProcessorConfig : IProcessorConfig
    {
        public ProcessorConfig(string entityRoot, IQueryProcessor processor, string nameSpace, string assembly)
        {
            EntityRoot = entityRoot;
            Processor = processor;
            NameSpace = nameSpace;
            Assembly = assembly;
        }
        public IQueryProcessor Processor { get; }
        public string EntityRoot { get; }
        public string NameSpace { get; }
        public string Assembly { get; }
        public IQuery GetQuery(string name, string entity, string parametersJson)
        {
            var queryConfig = new QueryConfig(name, entity, this.Processor, this.NameSpace, this.Assembly);
            return queryConfig.GetQuery(parametersJson);
        }
    }
    public class QueryConfig : IQueryConfig
    {
        public QueryConfig(string queryName, string entity, IQueryProcessor processor, string nameSpace, string assembly)
        {
            QueryName = queryName;
            EntityRoot = entity;
            Processor = processor;
            NameSpace = nameSpace;
            Assembly = assembly;
        }
        public string Key { get { return QueryName + EntityRoot + "Query"; } }
        public string QueryName { get; }
        public string EntityRoot { get; }
        public string NameSpace { get; }
        public string Assembly { get; }
        public IQueryProcessor Processor { get; }
        public IQuery GetQuery(string json)
        {
            IQuery query;

            var type = Type.GetType(NameSpace + "." + Key + ", " + Assembly);

            if (type == null)
            {
                throw new CommandManager.TypeNotFoundException($"{Key} not found in {NameSpace} of {Assembly}");
            }

            if (!string.IsNullOrWhiteSpace(json))
            {
                var insert = @"'$type': '" + NameSpace + "." + Key + @", " + Assembly + "', ";
                json = json.Trim().Insert(1, insert);
                query = JsonConvert.DeserializeObject<IQuery>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            else
            {
                query = Activator.CreateInstance(type) as IQuery;
            }
            return query;
        }
    }
    [Serializable]
    public class QueryDto : IQueryDto
    {
        private string _entityRoot = "";

        public QueryDto()
        {
        }

        public Guid Guid { get; set; }
        public string Entity { get; set; }
        public string EntityGuid { get; set; }
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

        public string Query { get; set; }
        public string QueryVersion { get; set; }
        public string UserName { get; set; }
        public string TenantId { get; set; }
        public string ParametersJson { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ExecutedOn { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public CommandManager.Contracts.IParametersDto ParametersDto { get; set; }
        public IQueryResultDto Result { get; set; }
    }
}
