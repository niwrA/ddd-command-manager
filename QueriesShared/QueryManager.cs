using niwrA.CommandManager.Helpers;
using niwrA.QueryManager.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace niwrA.QueryManager
{
    public class QueryManager : IQueryManager
    {
        private IQueryDtoToQueryConverter _converter;
        private IQueryService _service;
        private List<IQueryProcessor> _queryprocessors = new List<IQueryProcessor>();
        private List<IQueryProcessor> _processors = new List<IQueryProcessor>();
        public QueryManager()
        {
            var dateTimeProvider = new DefaultDateTimeProvider();
            _converter = new QueryDtoToQueryConverter(dateTimeProvider);
            _service = new QueryService(dateTimeProvider);
        }
        public QueryManager(IQueryService service, IQueryDtoToQueryConverter converter)
        {
            _service = service;
            _converter = converter;
        }
        public void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs)
        {
            _converter.AddProcessorConfigs(configs);
            _processors = configs.Select(s => s.Processor).Distinct().ToList();
        }

        public void AddQueryConfigs(IEnumerable<IQueryConfig> configs)
        {
            _converter.AddQueryConfigs(configs);
            _queryprocessors = configs.Select(s => s.Processor).Distinct().ToList();
        }

        public void ProcessQueries(IEnumerable<IQueryDto> queries)
        {
            var typedQueries = _converter.ConvertQueries(queries);
            _service.ProcessQueries(typedQueries);
        }
    }
}
