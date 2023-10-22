namespace Ford.WebApi.Repositories
{
    public interface IRepository<T, P>
    {
        Task<T?> RetrieveAsync(P id);
        Task<IEnumerable<T>?> RetrieveAllAsync();
        Task<T?> CreateAsync(T entity);
        Task<T?> UpdateAsync(T entity);
        Task<bool> DeleteAsync(P id);
        Task<bool> IsExistAsync(P id);
        Task<bool> IsExistAsync(T entity);
        Task<bool> SaveAsync();
    }
}
