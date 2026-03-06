// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Repository.Models;

// namespace Repository.Interfaces
// {
//     public interface IQueryInterface
//     {
//         List<t_Query> GetUserQueries(int userid);
//         bool AddQuery(t_Query query);
//         bool UpdateQuery(t_Query query);
//         bool DeleteQuery(int queryid);
//         t_Query GetQueryById(int queryid);
//     }
// }

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interfaces
{
    public interface IQueryInterface
    {
        Task<List<t_Query>> GetUserQueries(int userid);
        Task<bool> AddQuery(t_Query query);
        Task<bool> UpdateQuery(t_Query query);
        Task<bool> DeleteQuery(int queryid);
        Task<t_Query> GetQueryById(int queryid);
    }
}