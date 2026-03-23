using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace Repository.Interfaces
{
    public interface IUserInterface
    {
        Task<bool> Register(t_Registration register);
    }
}