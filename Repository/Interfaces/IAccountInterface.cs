using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interfaces
{
    public interface IAccountInterface
    {
        // Task<int> RegisterUser(t_User user);

        // Login check for Users
        Task<t_User?> LoginUser(string email, string password);

        // Login check for Employees/Admins
        Task<t_Employee?> LoginEmployee(string email, string password);

        Task<string?> CheckEmailExists(string email);

        Task<bool> ResetPassword(string email, string newPassword, string userType);
    }
}