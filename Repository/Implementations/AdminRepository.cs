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
                var qry = @"SELECT c_queryid, c_userid, c_title, c_description, c_querydate, c_empid, c_status, c_comment FROM t_query";

                using(var cmd = new NpgsqlCommand(qry, _conn))
                {
                    List<t_Query> list = new List<t_Query>();

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync();

                    while( await reader.ReadAsync())
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
                            c_Status = reader.GetString(6),
                            // c_Comment = reader.GetString(7),
                            c_Comment = reader.IsDBNull(7) ? null : reader.GetString(7)
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

                using(var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync();
                    
                    if(await reader.ReadAsync())
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

                using(var cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@id", query.c_QueryId);

                    await _conn.CloseAsync();
                    await _conn.OpenAsync();

                    var result = await cmd.ExecuteNonQueryAsync();
                    await _conn.CloseAsync();
                    
                    if(result == 1)
                    {
                        Console.WriteLine("Query Deleted Successfully...");
                        return result;
                    }
                    else if(result == 0)
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
    }
}