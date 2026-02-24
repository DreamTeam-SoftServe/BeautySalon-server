using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRepository<T>
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task CreateAsync(T entity);
        Task UpdateAsync(Guid id, T entity);
        Task DeleteAsync(Guid id);
    }
}
