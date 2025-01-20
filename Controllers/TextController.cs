using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Back.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TextController : ControllerBase
    {
        public string cacheKey { get; set; }
        private readonly IMemoryCache _memoryCache;

        public TextController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            cacheKey = "ToDoList";
        }
        

        [HttpGet("")]
        public ActionResult Get([FromQuery] string? filter = null) {
            var ToDoList = GetToDoList();

            // Filter logic based on the parameter
            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter.ToLower())
                {
                    case "checked":
                        ToDoList = ToDoList.Where(item => item.Checked).ToList();
                        break;
                    case "unchecked":
                        ToDoList = ToDoList.Where(item => !item.Checked).ToList();
                        break;
                    default:
                        return BadRequest("Invalid filter parameter. Use 'checked' or 'unchecked'.");
                }
            }

            return Ok(ToDoList);
        }

        [HttpPost("")]
        public ActionResult Post([FromBody] ListItemDto model) {            
            var ToDoList = GetToDoList();
            ToDoList.Add(model);
            _memoryCache.Set(cacheKey, ToDoList);

            return Ok("Text inserted");
        }

        [HttpPut("{index}")]
        public ActionResult Update([FromRoute] int index, [FromBody] ListItemDto novaModel) {
            var ToDoList = GetToDoList();

            if (index < 0 || index >= ToDoList.Count) {
                return BadRequest("Index invalid.");
            }

            ToDoList[index] = novaModel;
            _memoryCache.Set(cacheKey, ToDoList);

            return Ok("Text updated.");
        }

        [HttpDelete("{index}")]
        public ActionResult Delete([FromRoute] int index) {
            var ToDoList = GetToDoList();

            if (index < 0 || index >= ToDoList.Count) {
                return BadRequest("Index invalid.");
            }
            ToDoList.RemoveAt(index);
            _memoryCache.Set(cacheKey, ToDoList);

            return Ok("Text deleted.");
        }

        // [HttpGet("/filter")]
        // public ActionResult Filter([FromQuery] bool? isChecked) {
        //     var List = GetList();
        //     if (isChecked.HasValue)
        //     {
        //         List = List.Where(item => item.Checked == isChecked.Value).ToList();
        //     }
        //     return Ok(List);
        // }

        // add jwt authentication

        private List<ListItemDto> GetToDoList() {
            if (!_memoryCache.TryGetValue(cacheKey, out List<ListItemDto> ToDoList))
            {
                ToDoList = new List<ListItemDto>();
                
                // Set cache options
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                // Save data in cache
                _memoryCache.Set(cacheKey, ToDoList, cacheOptions);
            }

            return ToDoList;
        }
        // put this private in a service layer
    }
}