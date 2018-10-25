using niwrA.QueryManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.QueryManager
{
    public class IndexedQueryResult: IIndexedQueryResult
    {
        public IndexedQueryResult()
        {

        }
        public IndexedQueryResult(Guid guid, IQueryResult queryResult)
        {
            Guid = guid;
            QueryResult = queryResult;
        }
        public Guid Guid { get; set; }
        public IQueryResult QueryResult { get; set; }
    }
    public class QueryService : IQueryService
    {
        private readonly CommandManager.IDateTimeProvider _dateTimeProvider;

        public QueryService(CommandManager.IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }
        public IEnumerable<IIndexedQueryResult> ProcessQueries(IEnumerable<IQuery> queries)
        {
            var results = new List<IndexedQueryResult>();
            foreach (var query in queries)
            {
                var result = ProcessQuery(query);
                results.Add(new IndexedQueryResult(query.Guid, result));
            }
            return results;
        }
        /// <summary>
        /// Process a single query (execute it)
        /// </summary>
        /// <param name="query"></param>
        public IQueryResult ProcessQuery(IQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            query.ExecutedOn = _dateTimeProvider.GetServerDateTime();
            return query.Execute();
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
