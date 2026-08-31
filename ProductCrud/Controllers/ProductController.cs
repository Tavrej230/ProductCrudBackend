using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCrud.Application.Dtos;
using ProductCrud.Application.Interface;

namespace ProductCrud.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll( string? search,string? category,string? status)
        {
            var products = await _productService.GetAllAsync( search, category, status);

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create( [FromForm] ProductCreateDto dto)
        {
            try
            {
                var product = await _productService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetById),
                    new { id = product.Id },
                    product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update( int id,[FromForm] ProductCreateDto dto)
        {
            try
            {
                var result = await _productService.UpdateAsync(id, dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Product not found"
                    });
                }

                return Ok(new
                {
                    message = "Product updated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id,[FromBody] string status)
        {
            try
            {
                var result = await _productService.ChangeStatusAsync( id, status);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Product not found"
                    });
                }

                return Ok(new
                {
                    message = "Product status changed successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            return Ok(new
            {
                message = "Product deleted successfully"
            });
        }
    }
}