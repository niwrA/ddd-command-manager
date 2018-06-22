using niwrA.QueryManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.QueryManager
{
    public class QueryService : IQueryService
    {
        private readonly CommandManager.IDateTimeProvider _dateTimeProvider;

        public QueryService(CommandManager.IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }
        public void ProcessQueries(IEnumerable<IQuery> queries)
        {
            var processed = new List<IQuery>();
            foreach (var query in queries)
            {
                ProcessQueries(query);
            }
        }
        /// <summary>
        /// Process a single query (execute it)
        /// </summary>
        /// <param name="query"></param>
        public void ProcessQueries(IQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            query.Execute();
            query.ExecutedOn = _dateTimeProvider.GetServerDateTime();
        }
        /// <summary>
        /// Create a command of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateQuery<T>() where T : IQuery, new()
        {
            var query = new T()
            {
            };
            query.Guid = Guid.NewGuid();
            query.CreatedOn = _dateTimeProvider.GetServerDateTime();
            return query;
        }
    }
}
