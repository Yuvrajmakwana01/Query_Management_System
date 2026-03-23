using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repository.Interfaces;
using Repository.Models;

namespace Repository.Implementations
{
    public class AdminRepository : IAdminInterface
    {
        private readonly NpgsqlConnection _conn;

        public AdminRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<List<t_Query>> GetAllQuery()
        {
            try
            {
                var qry = @"SELECT q.c_queryid, q.c_userid, q.c_title, q.c_description, q.c_querydate, q.c_empid, e.c_empname, q.c_status, q.c_comment FROM t_query q LEFT JOIN t_employee e ON q.c_empid = e.c_empid";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    List<t_Query> list = new List<t_Query>();

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        list.Add(new t_Query
                        {
                            c_QueryId = reader.GetInt32(0),
                            c_UserId = reader.GetInt32(1),
                            c_Title = reader.GetString(2),
                            c_Description = reader.GetString(3),
                            // c_QueryDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["c_querydate"])),
                            c_QueryDate = (DateOnly)reader["c_querydate"],
                            // c_EmpId = reader.GetInt32(5),
                            c_EmpId = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                            c_EmpName = reader.IsDBNull(6) ? null : reader.GetString(6),
                            c_Status = reader.GetString(7),
                            // c_Comment = reader.GetString(7),
                            c_Comment = reader.IsDBNull(8) ? null : reader.GetString(8)
                        });
                    }
                    await _conn.CloseAsync();
                    return list;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error While Getting Queries for Admin..." + ex.Message);
                await _conn.CloseAsync();
                return null;
            }
        }

        public async Task<t_Query> GetOneQuery(int id)
        {
            try
            {
                var qry = @"SELECT c_queryid, c_userid, c_title, c_description, c_querydate, c_empid, c_status, c_comment FROM t_query WHERE c_queryid=@id";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        t_Query query = new t_Query()
                        {
                            c_QueryId = reader.GetInt32(0),
                            c_UserId = reader.GetInt32(1),
                            c_Title = reader.GetString(2),
                            c_Description = reader.GetString(3),
                            // c_QueryDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["c_querydate"])),
                            c_QueryDate = (DateOnly)reader["c_querydate"],
                            // c_EmpId = reader.GetInt32(5),
                            c_EmpId = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                            c_Status = reader.GetString(6),
                            // c_Comment = reader.GetString(7),
                            c_Comment = reader.IsDBNull(7) ? null : reader.GetString(7)
                        };

                        return query;
                    }
                    await _conn.CloseAsync();

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error While GEtting one Querie..." + ex.Message);
                await _conn.CloseAsync();
                return null;
            }
        }

        public async Task<int> Delete(t_Query query)
        {
            try
            {
                var qry = @"DELETE FROM t_query WHERE c_queryid=@id";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@id", query.c_QueryId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var result = await cmd.ExecuteNonQueryAsync();
                    await _conn.CloseAsync();

                    if (result == 1)
                    {
                        Console.WriteLine("Query Deleted Successfully...");
                        return result;
                    }
                    else if (result == 0)
                    {
                        Console.WriteLine("Query Deletion Failed...");
                        return result;
                    }

                    return -1;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error While Deleting Querie..." + ex.Message);
                await _conn.CloseAsync();
                return -1;
            }
        }

        public async Task<t_Dashboard> GetAll()
        {
            var state = new t_Dashboard();

            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();

                // total query
                using var cmd = new NpgsqlCommand(@"SELECT COUNT(*) FROM t_query;", _conn);
                state.TotalQueries = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // total today query
                using var cmd1 = new NpgsqlCommand(@"SELECT COUNT(*) FROM t_query WHERE DATE(c_querydate) = CURRENT_DATE;", _conn);
                state.TotalTodayQueries = Convert.ToInt32(await cmd1.ExecuteScalarAsync());

                // pending queries
                using var cmd2 = new NpgsqlCommand(@"SELECT COUNT(*) FROM t_query WHERE c_status IN ('Open','In Progress');", _conn);
                state.PendingQueries = Convert.ToInt32(await cmd2.ExecuteScalarAsync());

                // pending today queries
                using var cmd3 = new NpgsqlCommand(@"SELECT COUNT(*) FROM t_query WHERE c_status IN ('Open','In Progress') AND DATE(c_querydate) = CURRENT_DATE;", _conn);
                state.PendingTodayQueries = Convert.ToInt32(await cmd3.ExecuteScalarAsync());

                // solved queries
                using var cmd4 = new NpgsqlCommand(@"SELECT COUNT(*) FROM t_query WHERE c_status = 'Solved';", _conn);
                state.SolvedQuery = Convert.ToInt32(await cmd4.ExecuteScalarAsync());

                // solved today queries
                using var cmd5 = new NpgsqlCommand(@"SELECT COUNT(*) FROM t_query WHERE c_status = 'Solved' AND DATE(c_querydate) = CURRENT_DATE;", _conn);
                state.SolvedTodayQueries = Convert.ToInt32(await cmd5.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return state;
        }

        public async Task<List<t_User>> GetAllUsers()
        {
            List<t_User> list = new List<t_User>();

            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();

                var qry = @"SELECT c_userid, c_empname, c_companyname, c_emailid FROM t_user";

                NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn);

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new t_User
                    {
                        c_UserId = reader.GetInt32(0),
                        c_UserName = reader.GetString(1),
                        c_CompanyName = reader.GetString(2),
                        c_EmailId = reader.GetString(3),
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return list;
        }

        public async Task DeleteUser(int id)
        {
            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();

                var qry = "DELETE FROM t_user WHERE c_userid = @id";

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@id", id);

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<Dictionary<string, object>>> GetAllEmployee()
        {
            try
            {
                var qry = @"SELECT c_empid, c_empname, c_companyname, c_email FROM t_employee WHERE c_role = 'Employee'";

                using (var cmd = new NpgsqlCommand(qry, _conn))
                {
                    var list = new List<Dictionary<string, object>>();

                    if (_conn.State != System.Data.ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>
                            {
                                ["c_EmpId"] = reader["c_empid"],
                                ["c_EmpName"] = reader["c_empname"],
                                ["c_CompanyName"] = reader["c_companyname"],
                                ["c_Email"] = reader["c_email"]
                            };

                            list.Add(row);
                        }
                    }

                    await _conn.CloseAsync();
                    return list;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error While Getting Employees..." + ex.Message);

                if (_conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();

                return new List<Dictionary<string, object>>(); // NEVER return null
            }
        }

        public async Task DeleteEmployee(int id)
        {
            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();

                var qry = "DELETE FROM t_employee WHERE c_empid = @id";

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@id", id);

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
    }
}