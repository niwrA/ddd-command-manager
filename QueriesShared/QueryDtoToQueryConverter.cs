using niwrA.QueryManager.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace niwrA.QueryManager
{
    public class QueryDtoToQueryConverter : IQueryDtoToQueryConverter
    {
        private Dictionary<string, IProcessorConfig> _configs = new Dictionary<string, IProcessorConfig>();
        private Dictionary<string, IQueryConfig> _queryConfigs = new Dictionary<string, IQueryConfig>();
        private IDateTimeProvider _dateTimeProvider;
        private ILookup<string, IQueryConfig> _queryLookups;
        private ILookup<string, IProcessorConfig> _processorLookups;

        public QueryDtoToQueryConverter(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
            _queryLookups = new List<IQueryConfig>().ToLookup(o => o.Key);
            _processorLookups = new List<IProcessorConfig>().ToLookup(o => o.EntityRoot);
        }
        public void AddQueryConfigs(IEnumerable<IQueryConfig> configs)
        {
            _queryLookups = configs.ToLookup(o => o.Key);
        }
        public IEnumerable<IQueryConfig> GetQueryConfigs(string key)
        {
            var configs = new List<IQueryConfig>();
            configs.AddRange(_queryLookups[key]);
            return configs;
        }
        public void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs)
        {
            _processorLookups = configs.ToLookup(o => o.EntityRoot);
        }
        public IEnumerable<IProcessorConfig> GetProcessorConfigs(string key)
        {
            var configs = new List<IProcessorConfig>();
            configs.AddRange(_processorLookups[key]);
            return configs;
        }

        public IEnumerable<IQuery> ConvertQueries(IEnumerable<IQueryDto> querys)
        {
            var processed = new List<IQuery>();
            foreach (var query in querys)
            {
                processed.AddRange(ConvertQuery(query));
            }
            return processed;
        }
        public IEnumerable<IQuery> ConvertQuery(IQueryDto query)
        {
            //      IQuery typedQuery = null;
            var typedQuerys = new List<IQuery>();
            // either take existing name from state, or construct from dto
            // the replace is a bit hacky, should probably clean the querystore
            var queryName = query.Query;
            var parametersJson = query.ParametersJson;

            ProcessQueryConfigs(query, typedQuerys, queryName, parametersJson);
            ProcessProcessorConfigs(query, typedQuerys, queryName, parametersJson);

            if (typedQuerys.Any())
            {
                return typedQuerys;
            }
            throw new QueryNotConfiguredException($"The query named '{query.Query}' for entity root type '{query.EntityRoot}' does not have a matching configuration.");
        }

        private void ProcessProcessorConfigs(IQueryDto query, List<IQuery> typedQuerys, string queryName, string parametersJson)
        {
            var processorConfigs = GetProcessorConfigs(query.EntityRoot);
            foreach (var config in processorConfigs)
            {
                var typedQuery = config.GetQuery(queryName, query.EntityRoot, parametersJson);

                SetQueryProperties(query, config.Processor, typedQuery);
                typedQuerys.Add(typedQuery);
            }
        }

        private void ProcessQueryConfigs(IQueryDto query, List<IQuery> typedQuerys, string queryName, string parametersJson)
        {
            var queryConfigs = GetQueryConfigs(queryName + query.EntityRoot + "Query");
            foreach (var config in queryConfigs)
            {
                var typedQuery = config.GetQuery(parametersJson);

                SetQueryProperties(query, config.Processor, typedQuery);
                typedQuerys.Add(typedQuery);
            }
        }

        private void SetQueryProperties(IQueryDto query, IQueryProcessor processor, IQuery typedQuery)
        {
            CopyQueryDtoIntoQuery(query, processor, typedQuery);
        }

        private void CopyQueryDtoIntoQuery(IQueryDto query, IQueryProcessor processor, IQuery typedQuery)
        {
            typedQuery.CreatedOn = query.CreatedOn;
            typedQuery.ReceivedOn = _dateTimeProvider.GetSessionDateTime();
            typedQuery.EntityRoot = query.EntityRoot;
            typedQuery.Guid = query.Guid;
            typedQuery.ParametersJson = query.ParametersJson;
            typedQuery.UserName = query.UserName;
            typedQuery.TenantId = query.TenantId;
            typedQuery.Query = query.Query;

            typedQuery.QueryProcessor = processor;
        }
    }
}
