using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interfaces
{
    public interface IEmployeeInterface
    {

        Task<List<t_Query>> GetUnsolvedQueries();

        // Update status and add comments
        Task<bool> ResolveQuery(int queryId, int empId, string status, string comment);

        // Track personal performance (Count of solved queries by this emp)
        Task<int> GetResolvedCountByEmployee(int empId);


        Task<Dictionary<string, int>> GetEmployeeDashboardMetrics(int empId);

        Task<t_Query> GetQueryById(int id);
    }
}