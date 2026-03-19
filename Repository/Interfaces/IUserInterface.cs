using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interfaces
{
    public interface IUserInterface 
    {
        bool Register(t_Registration register);
        // Task<t_Registration> Login(vm_login user)
    }
}