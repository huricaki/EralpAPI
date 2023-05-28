using DataAccessLayer.Abstract;
using DataAccessLayer.Context;
using EntityLayer.Entities;
using EralpAPI.Models.Auth;
using EralpAPI.Models.Product;
using EralpAPI.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EralpAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ProductController : Controller
    {
        public AppDbContext Context { get; set; }
        private readonly IGenericRepository<Product> _repository;
        public ProductController(AppDbContext context, IGenericRepository<Product> repository)
        {
            this.Context = context;
            this._repository = repository;
        }

        [HttpPost]
        [Route("AddProductForUser")]
        public async Task<IActionResult> AddProductForUser(long userId,[FromBody] ProductDto productDto)
        {
            try
            {
                 var user= Context.Users.Where(x => x.Id == userId).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest("Could not find the user");

                }

                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (currentUserId != user.Id)
                {
                    return Unauthorized();
                }

                Product product = new Product
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    Category = productDto.Category,
                    Status = productDto.Status,
                    Stock = productDto.Stock,
                    CreatedDate = productDto.CreatedDate,
                    Description = productDto.Description,
                    User = user
                };

                await _repository.Add(product);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet]
        [Route("GetProduct/{id}")]
        public async Task<IActionResult> GetProduct(long id)
        {
            try
            {
                var product = await _repository.GetById(id);
                return Ok(product);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
      
        [HttpPut]
        [Route("UpdateProduct/{id}")]
        public async Task<IActionResult> Update(int id, ProductDto updatedProduct)
        {
            try
            {
                Product product = new Product
                {
                    Category = updatedProduct.Category,
                    Description = updatedProduct.Description,
                    Name = updatedProduct.Name,
                    Price = updatedProduct.Price,
                    Status = updatedProduct.Status,
                    Stock = updatedProduct.Stock,
                    UpdateDate = updatedProduct.UpdateDate,
                    Id = id
                };
                var existingProduct = await _repository.Update(id, product);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                return Ok(existingProduct);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        [Route("GetProductsByUser")]
        public IActionResult GetProducts(int currentPage, int pageSize, string productName)
        {
            // Pagination

            var queryable = Context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(productName))
            {
                queryable = queryable.Where(x => x.Name.Contains(productName));
            }

            var response = queryable
                .OrderByDescending(x => x.Id)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            return Ok(response);
        }

        [HttpDelete]
        [Route("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            try
            {
                await _repository.Delete(id);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
