using Microsoft.AspNetCore.Mvc;
using Workboard.Data;
using Workboard.Domain;

namespace Workboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        public BoardController(BoardCache boardCache)
        {
            this.boardCache = boardCache;
        }
        private BoardCache boardCache;

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public async Task<Board> Get(int id)
        {
            Board board = await boardCache.GetBoardAsync(Guid.NewGuid());
            return board;
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
