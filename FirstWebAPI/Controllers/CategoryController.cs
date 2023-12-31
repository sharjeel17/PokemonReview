﻿using AutoMapper;
using FirstWebAPI.Dto;
using FirstWebAPI.Interfaces;
using FirstWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public CategoryController(ICategoryRepository categoryRepository, IMapper mapper) 
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCategories() 
        {
            var categories = _mapper.Map<ICollection<CategoryDto>>(await _categoryRepository.GetCategoriesAsync());

            if(!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(categories);
        }

        [HttpGet("{categoryId}")]
        [ProducesResponseType(200, Type = typeof(CategoryDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            if (!(_categoryRepository.CategoryExists(categoryId))) return NotFound();

            var categories = _mapper.Map<CategoryDto>(await _categoryRepository.GetCategoryAsync(categoryId));

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(categories);
        }

        [HttpGet("pokemon/{categoryId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PokemonDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPokemonByCategory(int categoryId) 
        {
            if (!(_categoryRepository.CategoryExists(categoryId))) return NotFound();

            var pokemon = _mapper.Map<ICollection<PokemonDto>>(await _categoryRepository.GetPokemonByCategoryAsync(categoryId));
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(pokemon);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        //FromBody attribute ensures that the category key:value pairs
        //only come from the body and nowhere else
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto inputCategory) 
        {
            //not needed/redundant as ModelState itself will do the validation check
            //before ever executing code in this method.
            //Statement written for safety and housekeeping
            if (inputCategory == null) return BadRequest(ModelState);
            Console.WriteLine(inputCategory.Id);

            //EF will check and see if there are matching ID/Primary keys
            //at the createCategory and throw error if there is
            //but better to do self checking and returning early to reduce errors
            //IMPORANT: If InputCategory.Id is null/not present in request body
            //then Id will default to the value 0
            if (_categoryRepository.CategoryExists(inputCategory.Id))
                ModelState.AddModelError("ID Error", "ID Key already exists");

            //check for matching category names with the given input
            //if category name already exists then add an error to the ModelState
            //which in turn will turn the ModelState invalid
            if (_categoryRepository.CategoryExists(inputCategory.Name)) 
                ModelState.AddModelError("Name Error", "Category already exists");

            //if any above conditions are met then ModelState will be invalid
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //Identity_Insert is off, so Id needs to be 0
            //as we cannot set Id value ourselves.
            //If Identity_Insert is on, comment this line out
            inputCategory.Id = 0;

            //map CategoryDto to Category
            var categoryMap = _mapper.Map<Category>(inputCategory);

            //Add mapped Category object to database and check if successful
            if (! (await _categoryRepository.CreateCategoryAsync(categoryMap))) 
            {
                ModelState.AddModelError("Create Error", "Something went wrong while creating");
                return StatusCode(500, ModelState);
            }

            return StatusCode(201, categoryMap);
        }

        [HttpPut("{categoryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCategory([FromRoute] int categoryId, [FromBody] CategoryDto inputCategory) 
        {
            if (inputCategory == null)
                return BadRequest(ModelState);

            if (inputCategory.Id != categoryId)
            {
                ModelState.AddModelError("Incorrect IDS", "The given IDs do not match from the route and body");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var mappedCategory = _mapper.Map<Category>(inputCategory);

            if(!(await _categoryRepository.UpdateCategoryAsync(mappedCategory)))
            {
                ModelState.AddModelError("Update Error", "Something went wrong while updating category");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{categoryId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteCategory([FromRoute] int categoryId) 
        {
            if (!_categoryRepository.CategoryExists(categoryId))
                return NotFound();

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _categoryRepository.DeleteCategoryAsync(categoryId)) 
            {
                ModelState.AddModelError("Delete Error", "Something went wrong while deleting Category");
                return StatusCode(500, ModelState);
            }

            return Ok($"Deleted category {categoryId} successfully");
        }
    }
}
