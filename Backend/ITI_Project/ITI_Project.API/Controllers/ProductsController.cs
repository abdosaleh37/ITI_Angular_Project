using ITI_Project.BLL.Interfaces;
using ITI_Project.BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ITI_Project.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _productService.GetAllAsync(ct);
            
            if (result.IsFailure)
            {
                return BadRequest(new { Message = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var result = await _productService.GetByIdAsync(id, ct);

            if (result.IsFailure)
            {
                return NotFound(new { Message = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpGet("sku/{sku}")]
        public async Task<IActionResult> GetBySku([FromRoute] string sku, CancellationToken ct)
        {
            var result = await _productService.GetBySkuAsync(sku, ct);

            if (result.IsFailure)
            {
                return NotFound(new { Message = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.CreateAsync(dto, ct);

            if (result.IsFailure)
            {
                if (result.Error!.Contains("already exists"))
                {
                    return Conflict(new { Message = result.Error });
                }
                return BadRequest(new { Message = result.Error });
            }

            return CreatedAtAction(
                nameof(GetById), 
                new { id = result.Value!.Id }, 
                result.Value
            );
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<ProductCreateDto> dtos, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.CreateBulkAsync(dtos, ct);

            if (result.IsFailure)
            {
                if (result.Error!.Contains("already exist"))
                {
                    return Conflict(new { Message = result.Error });
                }
                return BadRequest(new { Message = result.Error });
            }

            return Ok(new
            {
                Message = $"{result.Value!.Count} products created successfully.",
                result.Value.Count,
                Products = result.Value.Products
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProductCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.UpdateAsync(id, dto, ct);

            if (result.IsFailure)
            {
                if (result.Error!.Contains("not found"))
                {
                    return NotFound(new { Message = result.Error });
                }
                if (result.Error.Contains("already exists"))
                {
                    return Conflict(new { Message = result.Error });
                }
                return BadRequest(new { Message = result.Error });
            }

            return Ok(new 
            { 
                Message = "Product updated successfully.", 
                Product = result.Value 
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var result = await _productService.DeleteAsync(id, ct);

            if (result.IsFailure)
            {
                return NotFound(new { Message = result.Error });
            }

            return Ok(new { Message = "Product deleted successfully.", Id = id });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? category, 
            [FromQuery] string? brand, 
            CancellationToken ct)
        {
            var result = await _productService.SearchAsync(category, brand, ct);

            if (result.IsFailure)
            {
                return BadRequest(new { Message = result.Error });
            }

            return Ok(result.Value);
        }
    }
}