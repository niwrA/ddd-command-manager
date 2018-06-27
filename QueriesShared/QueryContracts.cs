using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.QueryManager.Contracts
{
    public interface IQuery
    {
        Guid? Guid { get; set; }
        string EntityRoot { get; set; }
        string Query { get; set; }
        string QueryVersion { get; set; }
        string ParametersJson { get; set; }
        DateTime? ExecutedOn { get; set; }
        DateTime? ReceivedOn { get; set; }
        DateTime CreatedOn { get; set; }
        string UserName { get; set; }
        string TenantId { get; set; }
        IQueryProcessor QueryProcessor { get; set; }
        void Execute();
    }
    public enum ResultBodyFormat
    {
        String = 0,
        NewtonSoftJson = 1,
        NewtonSoftBinary = 2,
        BinaryFormatter = 3,
        Xml = 4
    }
    public interface IQueryResultDto
    {
        ResultBodyFormat BodyFormat { get; }
        string Body { get; }
    }
    public interface IQueryDto
    {
        Guid? Guid { get; set; }
        string Entity { get; set; }
        string EntityGuid { get; set; }
        string EntityRoot { get; set; }
        string EntityRootGuid { get; set; }
        string Query { get; set; }
        string QueryVersion { get; set; }
        string ParametersJson { get; set; }
        DateTime? ExecutedOn { get; set; }
        DateTime? ReceivedOn { get; set; }
        DateTime CreatedOn { get; set; }
        string UserName { get; set; }
        string TenantId { get; set; }
        IQueryResultDto Result { get; set; }
    }

    public interface IQueryProcessor
    {
    }

    public interface IQueryConfig
    {
        string Key { get; }
        string QueryName { get; }
        IQuery GetQuery(string parametersJson);
        IQueryProcessor Processor { get; }
        string EntityRoot { get; }
        string NameSpace { get; }
        string Assembly { get; }
    }

    public interface IProcessorConfig
    {
        IQueryProcessor Processor { get; }
        string EntityRoot { get; }
        string NameSpace { get; }
        string Assembly { get; }
        IQuery GetQuery(string name, string entityRoot, string parametersJson);
    }

    public interface IQueryService
    {
        T CreateQuery<T>() where T : IQuery, new();
        void ProcessQueries(IEnumerable<IQuery> commands);
        void ProcessQueries(IQuery query);
    }

    public interface IQueryDtoToQueryConverter
    {
        void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
        void AddQueryConfigs(IEnumerable<IQueryConfig> configs);
        IEnumerable<IQuery> ConvertQuery(IQueryDto query);
        IEnumerable<IQuery> ConvertQueries(IEnumerable<IQueryDto> querys);
        IEnumerable<IProcessorConfig> GetProcessorConfigs(string key);
        IEnumerable<IQueryConfig> GetQueryConfigs(string key);
    }

    public interface IQueryManager
    {
        void ProcessQueries(IEnumerable<IQueryDto> commands);
        void AddQueryConfigs(IEnumerable<IQueryConfig> configs);
        void AddProcessorConfigs(IEnumerable<IProcessorConfig> configs);
    }
}
