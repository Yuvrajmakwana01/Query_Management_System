using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repository.Interfaces;
using Repository.Models;
using Repository.Services;


namespace Repository.Implementations
{
    public class EmployeeRepository : IEmployeeInterface
    {
        private readonly NpgsqlConnection _conn;

        private readonly RedisServices _redis;
        private readonly RabbitService _rabbit;

        public EmployeeRepository(NpgsqlConnection conn, RedisServices redis, RabbitService rabbit)
        {
            _conn = conn;
            _redis = redis;
            _rabbit = rabbit;
        }

        public async Task<List<t_Query>> GetUnsolvedQueries()
        {
            var list = new List<t_Query>();
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();


            // Select queries that are NOT Solved
            var sql = @"SELECT q.*, u.c_companyname 
            FROM t_query q 
            JOIN t_user u ON q.c_userid = u.c_userid 
            WHERE LOWER(q.c_status) <> 'solved' 
            ORDER BY q.c_querydate DESC";

            using var cmd = new NpgsqlCommand(sql, _conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new t_Query
                {
                    c_QueryId = (int)reader["c_queryid"],
                    c_Title = reader["c_title"].ToString(),
                    c_Description = reader["c_description"].ToString(),
                    c_Priority = reader["c_priority"].ToString(),
                    c_Status = reader["c_status"].ToString(),
                    c_QueryDate = reader.GetFieldValue<DateOnly>("c_querydate"),
                    c_UserId = (int)reader["c_userid"]
                });
            }
            return list;
        }

        // public async Task<bool> ResolveQuery(int queryId, int empId, string status, string comment)
        // {
        //     if (_conn.State != System.Data.ConnectionState.Open)
        //         await _conn.OpenAsync();

        //     var sql = @"UPDATE t_query 
        //                 SET c_status = @status, c_comment = @comment, c_empid = @empId 
        //                 WHERE c_queryid = @id";

        //     using var cmd = new NpgsqlCommand(sql, _conn);
        //     cmd.Parameters.AddWithValue("status", status);
        //     cmd.Parameters.AddWithValue("comment", comment);
        //     cmd.Parameters.AddWithValue("empId", empId);
        //     cmd.Parameters.AddWithValue("id", queryId);

        //     return await cmd.ExecuteNonQueryAsync() > 0;
        // }

        public async Task<int> GetResolvedCountByEmployee(int empId)
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            var sql = "SELECT COUNT(*) FROM t_query WHERE c_empid = @empId AND c_status = 'Solved'";
            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("empId", empId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> ResolveQuery(int queryId, int empId, string status, string comment)
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            int? userId = null;
            string? title = null;
            string? previousStatus = null;

            var fetchSql = @"SELECT c_userid, c_title, c_status
                             FROM t_query
                             WHERE c_queryid = @id";

            using (var fetchCmd = new NpgsqlCommand(fetchSql, _conn))
            {
                fetchCmd.Parameters.AddWithValue("id", queryId);

                using var reader = await fetchCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userId = Convert.ToInt32(reader["c_userid"]);
                    title = reader["c_title"]?.ToString();
                    previousStatus = reader["c_status"]?.ToString();
                }
            }

            if (userId == null)
                return false;

            var sql = @"UPDATE t_query 
                        SET c_status = @status, c_comment = @comment, c_empid = @empId 
                        WHERE c_queryid = @id";

            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("comment", comment);
            cmd.Parameters.AddWithValue("empId", empId);
            cmd.Parameters.AddWithValue("id", queryId);

            var updated = await cmd.ExecuteNonQueryAsync() > 0;

            if (updated &&
                string.Equals(status, "Solved", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(previousStatus, "Solved", StringComparison.OrdinalIgnoreCase))
            {
                var queryTitle = string.IsNullOrWhiteSpace(title) ? $"Query #{queryId}" : title;
                var notificationMessage = $"Your query \"{queryTitle}\" has been solved.";

                await _rabbit.SendMessage(notificationMessage);
                await _redis.AddNotification(userId.Value.ToString(), notificationMessage);
            }

            return updated;
        }


        public async Task<Dictionary<string, int>> GetEmployeeDashboardMetrics(int empId)
        {
            var stats = new Dictionary<string, int>();
            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();

            // Fetches: Personal Solved, System-wide Unsolved, and High Priority Unsolved
            var sql = @"SELECT 
                COUNT(*) FILTER (WHERE c_empid = @empId AND c_status = 'Solved') as solved,
                COUNT(*) FILTER (WHERE c_status != 'Solved') as system_active,
                COUNT(*) FILTER (WHERE c_status != 'Solved' AND c_priority = 'High') as urgent
                FROM t_query";

            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("empId", empId);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                stats["solved"] = Convert.ToInt32(reader["solved"]);
                stats["active"] = Convert.ToInt32(reader["system_active"]);
                stats["urgent"] = Convert.ToInt32(reader["urgent"]);
            }
            return stats;
        }


        public async Task<t_Query> GetQueryById(int id)
        {
            if (_conn.State != ConnectionState.Open)
                await _conn.OpenAsync();

            var sql = @"SELECT * FROM t_query WHERE c_queryid = @id";

            using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new t_Query
                {
                    c_QueryId = (int)reader["c_queryid"],
                    c_Title = reader["c_title"].ToString(),
                    c_Description = reader["c_description"].ToString(),
                    c_Priority = reader["c_priority"].ToString(),
                    c_Status = reader["c_status"].ToString(),
                    c_QueryDate = (DateOnly)reader["c_querydate"],
                    c_UserId = (int)reader["c_userid"]
                };
            }

            return null;
        }

    }
}