using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repository.Models;
using Repository.Interfaces;

namespace Repository.Implementations
{
    public class UserRepository:IUserInterface
    {
        private readonly NpgsqlConnection _conn;
        public UserRepository(NpgsqlConnection connection)
        {
            _conn = connection;
        }

        // public bool Register(t_Registration user)
        // {
        //     try
        //     {
        //         string query="";
        //         bool isEmployee= string.Equals(user.c_Role,"Employee", StringComparison.OrdinalIgnoreCase);
        //         if(isEmployee)
        //         {
        //             query=@"INSERT INTO t_employee (
        //             c_companyname, c_empname,c_email, c_password, c_role) VALUES
        //             (@name, @email, @password, @role)";
        //         }else
        //         {
        //             query=@"INSERT INTO t_user(c_companyname,c_empname, c_emailid, c_password) VALUES
        //             (@name, @email, @password)";
        //         }
        //         using (NpgsqlCommand cmd= new NpgsqlCommand(query, _conn))
        //         {
        //             cmd.Parameters.AddWithValue("@name", user.c_CompanyName);
        //             cmd.Parameters.AddWithValue("@email", user.c_EmailId);
        //             cmd.Parameters.AddWithValue("@password", user.c_Password);

        //             if (isEmployee)
        //             {
        //                 cmd.Parameters.AddWithValue("@role",user.c_Role.ToLower());
        //             }
        //             _conn.Open();
        //             int result=cmd.ExecuteNonQuery();
        //             _conn.Close();

        //             return result>0;
        //         }
        //     }
        //     catch
        //     {
        //         if(_conn.State == ConnectionState.Open)
        //         {
        //             _conn.Close();
        //         }
        //         return false;
        //     }
        // }
        public bool Register(t_Registration user)
        {
            try
            {
                string query = "";
                // Use case-insensitive comparison to ensure "Employee" is always caught
                bool isEmployee = string.Equals(user.c_Role, "Employee", StringComparison.OrdinalIgnoreCase);

                if (isEmployee)
                {
                    // Matches your t_employee table structure
                    query = @"INSERT INTO t_employee (c_companyname, c_empname, c_email, c_password, c_role) 
                            VALUES (@company, @empname, @email, @password, @role)";
                }
                else
                {
                    // Matches your t_user table structure
                    query = @"INSERT INTO t_user (c_companyname, c_empname, c_emailid, c_password) 
                            VALUES (@company, @empname, @email, @password)";
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, _conn))
                {
                    // Ensure you are using the exact property names from your t_Registration model
                    cmd.Parameters.AddWithValue("@company", user.c_CompanyName);
                    cmd.Parameters.AddWithValue("@empname", user.c_EmpName); 
                    cmd.Parameters.AddWithValue("@email", user.c_EmailId);
                    cmd.Parameters.AddWithValue("@password", user.c_Password);

                    if (isEmployee)
                    {
                        cmd.Parameters.AddWithValue("@role", user.c_Role);
                    }

                    if (_conn.State != ConnectionState.Open) _conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    _conn.Close();

                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                // Check ex.Message here; it will tell you if there is a 'Column not found' or 'Null constraint' error
                if (_conn.State == ConnectionState.Open) _conn.Close();
                return false;
            }
        }
    }
}