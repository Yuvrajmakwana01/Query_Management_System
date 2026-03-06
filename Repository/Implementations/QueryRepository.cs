// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Repository.Interfaces;
// using Npgsql;
// using Repository.Models;
// using System.Data;

// namespace Repository.Implementations
// {
//     public class QueryRepository : IQueryInterface
//     {
//         private readonly NpgsqlConnection _conn;

//         public QueryRepository(NpgsqlConnection con)
//         {
//             _conn = con;
//         }

//         public List<t_Query> GetUserQueries(int userid)
//         {
//             List<t_Query> queries = new List<t_Query>();

//             _conn.Open();

//             string sql = "SELECT * FROM t_query WHERE c_userid=@userid";

//             using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
//             {
//                 cmd.Parameters.AddWithValue("@userid", userid);

//                 using (NpgsqlDataReader reader = cmd.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         queries.Add(new t_Query
//                         {
//                             c_QueryId = Convert.ToInt32(reader["c_queryid"]),
//                             c_UserId = Convert.ToInt32(reader["c_userid"]),
//                             c_Title = reader["c_title"].ToString(),
//                             c_Description = reader["c_description"].ToString(),
//                             c_Priority = reader["c_priority"].ToString(),
//                             // c_QueryDate = DateOnly.FromDateTime(Convert.ToDateTime(reader["c_querydate"])),
//                             // c_QueryDate = DateOnly.FromDateTime((DateTime)reader["c_querydate"]),
//                             c_QueryDate = (DateOnly)reader["c_querydate"],
//                             c_EmpId = reader["c_empid"] as int?,
//                             c_Status = reader["c_status"].ToString(),
//                             c_Comment = reader["c_comment"] == DBNull.Value ? null : reader["c_comment"].ToString()
//                         });
//                     }
//                 }
//             }

//             _conn.Close();

//             return queries;
//         }

//         public bool AddQuery(t_Query query)
//         {
//             if (_conn.State == ConnectionState.Closed)
//             {
//                 _conn.Open();
//             }

//             string sql = @"INSERT INTO t_query
//                 (c_userid,c_title,c_description,c_priority,c_status)
//                 VALUES(@userid,@title,@description,@priority,@status)";

//             using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
//             {
//                 cmd.Parameters.AddWithValue("@userid", query.c_UserId);
//                 cmd.Parameters.AddWithValue("@title", query.c_Title);
//                 cmd.Parameters.AddWithValue("@description", query.c_Description);
//                 cmd.Parameters.AddWithValue("@priority", query.c_Priority);
//                 cmd.Parameters.AddWithValue("@status", "Open");

//                 cmd.ExecuteNonQuery();
//             }

//             _conn.Close();

//             return true;
//         }

//         public bool DeleteQuery(int queryid)
//         {
//             if (_conn.State == ConnectionState.Closed)
//             {
//                 _conn.Open();
//             }

//             string sql = "DELETE FROM t_query WHERE c_queryid=@id";

//             using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
//             {
//                 cmd.Parameters.AddWithValue("@id", queryid);
//                 cmd.ExecuteNonQuery();
//             }

//             _conn.Close();

//             return true;
//         }

//         public bool UpdateQuery(t_Query query)
//         {
//             if (_conn.State == ConnectionState.Closed)
//             {
//                 _conn.Open();
//             }

//             string sql = @"UPDATE t_query
//                    SET c_title=@title,
//                        c_description=@desc,
//                        c_priority=@priority
//                    WHERE c_queryid=@id";

//             using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
//             {
//                 cmd.Parameters.AddWithValue("@title", query.c_Title);
//                 cmd.Parameters.AddWithValue("@desc", query.c_Description);
//                 cmd.Parameters.AddWithValue("@priority", query.c_Priority);
//                 cmd.Parameters.AddWithValue("@id", query.c_QueryId);

//                 cmd.ExecuteNonQuery();
//             }

//             _conn.Close();

//             return true;
//         }

//         public t_Query GetQueryById(int queryid)
//         {
//             t_Query query = null;

//             if (_conn.State == ConnectionState.Closed)
//             {
//                 _conn.Open();
//             }

//             string sql = "SELECT * FROM t_query WHERE c_queryid = @id";

//             using (var cmd = new NpgsqlCommand(sql, _conn))
//             {
//                 cmd.Parameters.AddWithValue("@id", queryid);

//                 using (var reader = cmd.ExecuteReader())
//                 {
//                     if (reader.Read())
//                     {
//                         query = new t_Query
//                         {
//                             c_QueryId = Convert.ToInt32(reader["c_queryid"]),
//                             c_UserId = Convert.ToInt32(reader["c_userid"]),
//                             c_Title = reader["c_title"].ToString(),
//                             c_Description = reader["c_description"].ToString(),
//                             c_Priority = reader["c_priority"].ToString(),
//                             c_QueryDate = (DateOnly)reader["c_querydate"],
//                             c_EmpId = reader["c_empid"] == DBNull.Value ? null : Convert.ToInt32(reader["c_empid"]),
//                             c_Status = reader["c_status"].ToString(),
//                             c_Comment = reader["c_comment"]?.ToString()
//                         };
//                     }
//                 }
//             }

//             _conn.Close();

//             return query;
//         }
//     }
// }


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Interfaces;
using Npgsql;
using Repository.Models;
using System.Data;

namespace Repository.Implementations
{
    public class QueryRepository : IQueryInterface
    {
        private readonly NpgsqlConnection _conn;

        public QueryRepository(NpgsqlConnection con)
        {
            _conn = con;
        }

        public async Task<List<t_Query>> GetUserQueries(int userid)
        {
            List<t_Query> queries = new List<t_Query>();

            await _conn.OpenAsync();

            string sql = "SELECT * FROM t_query WHERE c_userid=@userid";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("@userid", userid);

                using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        queries.Add(new t_Query
                        {
                            c_QueryId = Convert.ToInt32(reader["c_queryid"]),
                            c_UserId = Convert.ToInt32(reader["c_userid"]),
                            c_Title = reader["c_title"].ToString(),
                            c_Description = reader["c_description"].ToString(),
                            c_Priority = reader["c_priority"].ToString(),
                            c_QueryDate = (DateOnly)reader["c_querydate"],
                            c_EmpId = reader["c_empid"] as int?,
                            c_Status = reader["c_status"].ToString(),
                            c_Comment = reader["c_comment"] == DBNull.Value ? null : reader["c_comment"].ToString()
                        });
                    }
                }
            }

            await _conn.CloseAsync();

            return queries;
        }

        public async Task<bool> AddQuery(t_Query query)
        {
            if (_conn.State == ConnectionState.Closed)
            {
                await _conn.OpenAsync();
            }

            string sql = @"INSERT INTO t_query
                (c_userid,c_title,c_description,c_priority,c_status)
                VALUES(@userid,@title,@description,@priority,@status)";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("@userid", query.c_UserId);
                cmd.Parameters.AddWithValue("@title", query.c_Title);
                cmd.Parameters.AddWithValue("@description", query.c_Description);
                cmd.Parameters.AddWithValue("@priority", query.c_Priority);
                cmd.Parameters.AddWithValue("@status", "Open");

                await cmd.ExecuteNonQueryAsync();
            }

            await _conn.CloseAsync();

            return true;
        }

        public async Task<bool> DeleteQuery(int queryid)
        {
            if (_conn.State == ConnectionState.Closed)
            {
                await _conn.OpenAsync();
            }

            string sql = "DELETE FROM t_query WHERE c_queryid=@id";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("@id", queryid);
                await cmd.ExecuteNonQueryAsync();
            }

            await _conn.CloseAsync();

            return true;
        }

        public async Task<bool> UpdateQuery(t_Query query)
        {
            if (_conn.State == ConnectionState.Closed)
            {
                await _conn.OpenAsync();
            }

            string sql = @"UPDATE t_query
                   SET c_title=@title,
                       c_description=@desc,
                       c_priority=@priority
                   WHERE c_queryid=@id";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("@title", query.c_Title);
                cmd.Parameters.AddWithValue("@desc", query.c_Description);
                cmd.Parameters.AddWithValue("@priority", query.c_Priority);
                cmd.Parameters.AddWithValue("@id", query.c_QueryId);

                await cmd.ExecuteNonQueryAsync();
            }

            await _conn.CloseAsync();

            return true;
        }

        public async Task<t_Query> GetQueryById(int queryid)
        {
            t_Query query = null;

            if (_conn.State == ConnectionState.Closed)
            {
                await _conn.OpenAsync();
            }

            string sql = "SELECT * FROM t_query WHERE c_queryid = @id";

            using (var cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("@id", queryid);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        query = new t_Query
                        {
                            c_QueryId = Convert.ToInt32(reader["c_queryid"]),
                            c_UserId = Convert.ToInt32(reader["c_userid"]),
                            c_Title = reader["c_title"].ToString(),
                            c_Description = reader["c_description"].ToString(),
                            c_Priority = reader["c_priority"].ToString(),
                            c_QueryDate = (DateOnly)reader["c_querydate"],
                            c_EmpId = reader["c_empid"] == DBNull.Value ? null : Convert.ToInt32(reader["c_empid"]),
                            c_Status = reader["c_status"].ToString(),
                            c_Comment = reader["c_comment"]?.ToString()
                        };
                    }
                }
            }

            await _conn.CloseAsync();

            return query;
        }
    }
}