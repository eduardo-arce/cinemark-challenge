using Microsoft.AspNetCore.Mvc;
using MovieCatalog.Domain.DTO.Film;
using MovieCatalog.Domain.Entity;
using MovieCatalog.Domain.IUseCase.Account;
using MovieCatalog.Shared.Util;

namespace MovieCatalog.Api.Controllers
{
    [ApiController]
    [Route("api/v1/films")]
    public class FilmController : ControllerBase
    {
        private readonly IFilmCreate _filmCreate;
        private readonly IFilmRead _filmRead;
        private readonly IFilmUpdate _filmUpdate;
        private readonly IFilmDelete _filmDelete;

        public FilmController(
            IFilmCreate filmCreate,
            IFilmRead filmRead,
            IFilmUpdate filmUpdate,
            IFilmDelete filmDelete
        )
        {
            _filmCreate = filmCreate;
            _filmRead = filmRead;
            _filmUpdate = filmUpdate;
            _filmDelete = filmDelete;
        }

        [HttpPost]
        public async Task<ActionResult<FilmEntity>> Create(FilmCreateDTO input)
        {
            var result = await _filmCreate.CreateAsync(input);
            return StatusCode(GetHttpStatusCode.Get(result.Status), result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FilmEntity>> GetById(string id)
        {
            var result = await _filmRead.GetByIdAsync(id);
            return StatusCode(GetHttpStatusCode.Get(result.Status), result);
        }

        [HttpGet]
        public async Task<ActionResult<List<FilmEntity>>> GetAll([FromQuery] FilmFilterDTO filmFilterDTO)
        {
            var result = await _filmRead.GetAllAsync(filmFilterDTO);
            return StatusCode(GetHttpStatusCode.Get(result.Status), result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FilmEntity>> Update(string id, FilmUpdateDTO input)
        {
            var result = await _filmUpdate.UpdateAsync(id, input);
            return StatusCode(GetHttpStatusCode.Get(result.Status), result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<FilmEntity>> SoftDelete(string id)
        {
            var result = await _filmDelete.DeleteAsync(id);
            return StatusCode(GetHttpStatusCode.Get(result.Status), result);
        }
    }
}

