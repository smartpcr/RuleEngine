namespace DataCenterHealth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.DocDb;
    using Models;

    public interface IDocDbRepository<T> where T : BaseEntity
    {
        IDocDbClient Client { get; }
        Task<IEnumerable<T>> GetAll();
        Task<int> Count(string whereSql);
        Task<IEnumerable<T>> Query(string whereSql);
        Task<IEnumerable<T>> Query(string whereSqlTemplate, IList<string> ids);

        Task<PagedResult<T>> QueryPaged(string whereSql, string orderByField, bool isDescending = false, int skip = 0,
            int take = 10);

        Task<T> GetById(string id);
        Task<T> Create(T newInstance);
        Task Update(T instance);
        Task Delete(string id);
        Task<int> DeleteByQuery(string query);
        Task BulkUpsert(IList<T> batch, CancellationToken cancellationToken);
        Task<IEnumerable<T>> ObtainLease(CancellationToken cancel);
        Task ReleaseLease(string id, CancellationToken cancel);
        Task<DateTime> GetLastModificationTime(string query, CancellationToken cancel);
        Task<IEnumerable<string>> GetCountsByField(string fieldName, string query = null, CancellationToken cancel = default);
    }

    public class PagedResult<T> where T : BaseEntity
    {
        public int Total { get; set; }
        public List<T> Items { get; set; }
    }
}