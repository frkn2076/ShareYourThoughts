using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ThoughtController : ControllerBase
    {
        private readonly IConnectionService _connectionService;
        private readonly int PAGINATION_COUNT = 10;

        public IDbConnection PostgresConnection => _connectionService.GetPostgresConnection();

        public ThoughtController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpGet("{id}")]
        public IActionResult GetMessages(int id)
        {
            if (id <= 0)
                return BadRequest();

            var sql = $"SELECT message FROM public.messages LIMIT {PAGINATION_COUNT} OFFSET {(id-1) * PAGINATION_COUNT}";

            var command = PostgresConnection.CreateCommand();
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

        [HttpPost]
        public IActionResult PostMessage(PostModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 1000)
                return BadRequest();

            var sql = $"INSERT INTO public.messages (message) VALUES ('{request.Message}')";

            var command = PostgresConnection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();

            return Ok();
        }

        [HttpPost("filter")]
        public IActionResult FilterMessages(FilterModel request)
        {
            if (request == null || request.PageNo <= 0 || string.IsNullOrWhiteSpace(request.Filter) || request.Filter.Length > 1000)
                return BadRequest();

            var sql = $"SELECT message FROM public.messages WHERE message LIKE '%{request.Filter}%' LIMIT {PAGINATION_COUNT} OFFSET {(request.PageNo - 1) * PAGINATION_COUNT}";

            var command = PostgresConnection.CreateCommand();
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
    }
}