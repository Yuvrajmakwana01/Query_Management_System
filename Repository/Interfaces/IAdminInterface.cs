using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interfaces
{
    public interface IAdminInterface
    {

        Task<t_Dashboard> GetAll();
        public Task<List<t_Query>> GetAllQuery();

        public  Task<t_Query> GetOneQuery(int id);

        public Task<int> Delete(t_Query query);

        public Task<List<t_User>> GetAllUsers();
        public Task DeleteUser(int id);
    }
}