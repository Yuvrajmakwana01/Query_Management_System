using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repository.Interfaces;
using Npgsql;
using Repository.Models;
using System.Data;
using Repository.Services;

namespace Repository.Implementations
{
    public class QueryRepository : IQueryInterface
    {
        private readonly NpgsqlConnection _conn;
        private readonly RedisServices _redis;
        private readonly RabbitService _rabbit;
        // private readonly ElasticSearchServices _elastic;

        public QueryRepository(
            NpgsqlConnection conn,
            RedisServices redis,
            RabbitService rabbit
            )
        // ElasticSearchServices elastic
        {
            _conn = conn;
            _redis = redis;
            _rabbit = rabbit;
            // _elastic = elastic;
        }

        // ================= GET USER QUERIES =================
        public async Task<List<t_Query>> GetUserQueries(int userid)
        {
            List<t_Query> queries = new List<t_Query>();

            await _conn.OpenAsync();

            // string sql = "SELECT * FROM t_query WHERE c_userid=@userid";
            string sql = @"SELECT q.*, e.c_empname 
               FROM t_query q 
               LEFT JOIN t_employee e ON q.c_empid = e.c_empid 
               WHERE q.c_userid = @userid";

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

        // ================= ADD QUERY =================
        public async Task<bool> AddQuery(t_Query query)
        {
            if (_conn.State == ConnectionState.Closed)
                await _conn.OpenAsync();

            string sql = @"INSERT INTO t_query
                (c_userid,c_title,c_description,c_priority,c_status)
                VALUES(@userid,@title,@description,@priority,@status)
                RETURNING c_queryid";

            int newId = 0;

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("@userid", query.c_UserId);
                cmd.Parameters.AddWithValue("@title", query.c_Title);
                cmd.Parameters.AddWithValue("@description", query.c_Description);
                cmd.Parameters.AddWithValue("@priority", query.c_Priority);
                cmd.Parameters.AddWithValue("@status", "Open");

                newId = (int)await cmd.ExecuteScalarAsync();
            }

            await _conn.CloseAsync();

            // ================= AFTER DB SUCCESS =================

            // 🔔 Redis Notification
            // await _redis.AddNotification(query.c_UserId.ToString(), "New Query Created");
            await _redis.AddNotification("admin", "New Query Created");
            // await _redis.AddNotification("admin", $"New Query Created at {DateTime.Now:hh:mm tt}");

            // 📨 RabbitMQ Message
            await _rabbit.SendMessage("New Query Created");

            return true;
        }

        // ================= UPDATE QUERY =================
        public async Task<bool> UpdateQuery(t_Query query)
        {
            if (_conn.State == ConnectionState.Closed)
                await _conn.OpenAsync();

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

                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                await _conn.CloseAsync();

                // return true;
                // return newId;
                return newId > 0; //returns true if an ID was generated
            }

        }

        // ================= DELETE QUERY =================
        public async Task<bool> DeleteQuery(int queryid)
        {
            if (_conn.State == ConnectionState.Closed)
                await _conn.OpenAsync();

            string sql = "DELETE FROM t_query WHERE c_queryid=@id";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("@id", queryid);
                await cmd.ExecuteNonQueryAsync();
            }

            await _conn.CloseAsync();

            return true;
        }

        // ================= GET QUERY BY ID =================
        public async Task<t_Query> GetQueryById(int queryid)
        {
            t_Query query = null;

            if (_conn.State == ConnectionState.Closed)
                await _conn.OpenAsync();

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