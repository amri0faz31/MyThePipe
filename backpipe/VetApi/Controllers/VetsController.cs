using Microsoft.AspNetCore.Mvc;
using VetApi.Models;
using VetApi.Data;

namespace VetApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VetsController : ControllerBase
    {
        private readonly VetRepository _repo;

        public VetsController(VetRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_repo.GetAll());

        [HttpPost]
        public IActionResult Add([FromBody] Vet vet)
        {
            _repo.Add(vet);
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _repo.Delete(id);
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Vet vet)
        {
            vet.Id = id;
            _repo.Update(vet);
            return Ok();
        }
    }
}
