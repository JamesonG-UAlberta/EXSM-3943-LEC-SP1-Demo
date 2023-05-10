using Microsoft.AspNetCore.Mvc;
using ExampleAPI.Models;
using System.Xml.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExampleAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private static List<Product> products = new List<Product>();

        // GET: api/<ProductController>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(products);
        }
        // POST api/<ProductController>
        [HttpPost]
        public IActionResult Post(string name, string description)
        {
            try
            {
                Product toCreate = new Product()
                {
                    ID = products.Count + 1,
                    Name = name,
                    Description = description
                };
                products.Add(toCreate);
                return Created($"/product/{toCreate.ID}", toCreate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // PATCH 
        [HttpPatch]
        public IActionResult Patch(int oldId, int newID)
        {
            try
            {
                List<Product> targets = products.Where(product => product.ID == oldId).ToList();
                if (targets.Count == 0) return NotFound("Product ID was not found.");
                if (targets.Count > 1) return StatusCode(500, "Multiple Products with that ID were found. This should not happen.");
                Product target = targets.Single();
                target.ID = newID;
                return Ok(target);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (string.IsNullOrWhiteSpace(Request.Headers.ContentType)) return BadRequest("Must provide content-type header.");
            switch (Request.Headers.ContentType)
            {
                case "application/json":
                    return Ok(products.Where(product => product.ID == id).Single());
                case "text/plain":
                    return Ok(products.Where(product => product.ID == id).Single().ToString());
                default:
                    return BadRequest("Can only serve application/json and text/plain.");
            }
           
        }
        // PUT api/<ProductController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, string? name, string? description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description)) return BadRequest("Can't mutate record without at least one new value.");
                List<Product> targets = products.Where(product => product.ID == id).ToList();
                if (targets.Count == 0) return NotFound("Product ID was not found.");
                if (targets.Count > 1) return StatusCode(500, "Multiple Products with that ID were found. This should not happen.");
                
                Product target = targets.Single();
                if (!string.IsNullOrWhiteSpace(name)) target.Name = name;
                if (!string.IsNullOrWhiteSpace(description)) target.Description = description;
                return Ok(target);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                List<Product> targets = products.Where(product => product.ID == id).ToList();
                if (targets.Count == 0) return NotFound("Product ID was not found.");
                if (targets.Count > 1) return StatusCode(500, "Multiple Products with that ID were found. This should not happen.");
                Product target = targets.Single();
                products.Remove(target);
                return Ok(target);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
