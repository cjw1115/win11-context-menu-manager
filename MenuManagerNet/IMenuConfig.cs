using Program;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MenuManagerNet
{
    public interface IMenuConfig
    {
        Task LoadAsync();
        Task SaveAsync();
        Task Add(MenuItem newItem);
        Task Remove(MenuItem item);
        Task<MenuItem> Get(Guid comServer);
        Task<List<MenuItem>> GetAll();
        Task Update(MenuItem newItem);
    }
}
