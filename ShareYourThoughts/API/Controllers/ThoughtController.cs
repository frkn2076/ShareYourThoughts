using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ThoughtController : ControllerBase
    {
        private readonly string _connectionString;

        private readonly int PAGINATION_COUNT = 10;

        public ThoughtController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgresContext");
        }

        [HttpGet("{id}")]
        public IActionResult GetMessages(int id)
        {
            var connection = new NpgsqlConnection(_connectionString);

            try
            {
                if (id <= 0)
                    return BadRequest();

                var sql = $"SELECT message FROM public.messages LIMIT {PAGINATION_COUNT} OFFSET {(id - 1) * PAGINATION_COUNT}";

                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = sql;
                var reader = command.ExecuteReader();

                var messsages = new List<string>();
                messsages.Capacity = PAGINATION_COUNT;
                while (reader.Read())
                {
                    messsages.Add((string)reader["message"]);
                }

                return Ok(messsages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        [HttpPost]
        public IActionResult PostMessage(PostModel request)
        {
            var connection = new NpgsqlConnection(_connectionString);

            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 1000)
                    return BadRequest();

                var sql = $"INSERT INTO public.messages (message) VALUES ('{request.Message}')";

                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
            finally
            {
                connection.Close();
            }
        }

        [HttpPost("filter")]
        public IActionResult FilterMessages(FilterModel request)
        {
            var connection = new NpgsqlConnection(_connectionString);

            try
            {
                if (request == null || request.PageNo <= 0 || string.IsNullOrWhiteSpace(request.Filter) || request.Filter.Length > 1000)
                    return BadRequest();

                var sql = $"SELECT message FROM public.messages WHERE message LIKE '%{request.Filter}%' LIMIT {PAGINATION_COUNT} OFFSET {(request.PageNo - 1) * PAGINATION_COUNT}";

                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = sql;
                var reader = command.ExecuteReader();

                var messsages = new List<string>();
                messsages.Capacity = PAGINATION_COUNT;
                while (reader.Read())
                {
                    messsages.Add((string)reader["message"]);
                }

                return Ok(messsages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}