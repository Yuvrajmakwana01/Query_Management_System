using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repository.Interfaces;
using Repository.Models;

namespace Repository.Implementations
{
    public class AccountRepository : IAccountInterface
    {
        private readonly NpgsqlConnection _conn;

        public AccountRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<bool> Register(t_User user)
        {

            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            var sql = @"INSERT INTO t_user (c_companyname, c_emailid, c_password)  
                        VALUES (@name, @email, @pass)";

            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("name", user.c_CompanyName);
            cmd.Parameters.AddWithValue("email", user.c_EmailId);
            cmd.Parameters.AddWithValue("pass", user.c_Password);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<t_User?> LoginUser(string email, string password)
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();


            var sql = "SELECT * FROM t_user WHERE c_emailid = @email AND c_password = @pass";
            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("pass", password);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new t_User
                {
                    c_UserId = (int)reader["c_userid"],
                    c_UserName = reader["c_empname"].ToString(),
                    c_CompanyName = reader["c_companyname"].ToString(),
                    c_EmailId = reader["c_emailid"].ToString()
                };
            }
            return null;
        }

        public async Task<t_Employee?> LoginEmployee(string email, string password)
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();


            // Using c_email and c_empname based on your t_employee table schema
            var sql = "SELECT * FROM t_employee WHERE c_email = @email AND c_password = @pass";
            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("pass", password);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new t_Employee
                {
                    c_EmpId = (int)reader["c_empid"],
                    c_EmpName = reader["c_empname"].ToString(),
                    c_Email = reader["c_email"].ToString(),
                    c_Role = reader["c_role"].ToString()
                };
            }
            return null;
        }



        // 🔍 Check email exists in both tables
        public async Task<string?> CheckEmailExists(string email)
        {
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open)
                    await _conn.OpenAsync();

                // Check in t_user
                var sqlUser = "SELECT COUNT(1) FROM t_user WHERE c_emailid = @email";
                using (var cmd = new NpgsqlCommand(sqlUser, _conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    var count = Convert.ToInt64(await cmd.ExecuteScalarAsync() ?? 0);

                    if (count > 0)
                        return "user";
                }

                // Check in t_employee
                var sqlEmp = "SELECT COUNT(1) FROM t_employee WHERE c_email = @email";
                using (var cmd = new NpgsqlCommand(sqlEmp, _conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    var count = Convert.ToInt64(await cmd.ExecuteScalarAsync() ?? 0);

                    if (count > 0)
                        return "employee";
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CheckEmailExists Error: " + ex.Message);
                return null;
            }
            finally
            {
                await _conn.CloseAsync();
            }

        }

        // 🔐 Reset password
        public async Task<bool> ResetPassword(string email, string newPassword, string userType)
        {
            try
            {
                if (_conn.State != System.Data.ConnectionState.Open)
                    await _conn.OpenAsync();

                string sql;

                if (userType == "employee")
                {
                    sql = "UPDATE t_employee SET c_password = @pass WHERE c_email = @email";
                }
                else
                {
                    sql = "UPDATE t_user SET c_password = @pass WHERE c_emailid = @email";
                }

                using var cmd = new NpgsqlCommand(sql, _conn);
                cmd.Parameters.AddWithValue("@pass", newPassword);
                cmd.Parameters.AddWithValue("@email", email);

                var rows = await cmd.ExecuteNonQueryAsync();

                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ResetPassword Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }

        }
    }
}