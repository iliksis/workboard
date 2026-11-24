using Microsoft.AspNetCore.Mvc;
using Workboard.Data.Cache;
using Workboard.Domain;

namespace Workboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        public BoardController(WorkspaceCache workspaces)
        {
            this.workspaces = workspaces;
        }
        private readonly WorkspaceCache workspaces;

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public ActionResult<Workspace> Get(Guid id)
        {
            Workspace? workspace = workspaces.Get(id);
            return workspace is null ? NotFound() : workspace;
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
