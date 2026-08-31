using Microsoft.AspNetCore.Hosting;
using ProductCrud.Application.Dtos;
using ProductCrud.Application.Interface;
using ProductCrud.Domain.Entities;

namespace ProductCrud.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _environment;

        private readonly List<string> _categories = new()
        {
            "Electronics",
            "Clothing",
            "Grocery",
            "Furniture",
            "Books"
        };

        public ProductService(IProductRepository productRepository,IWebHostEnvironment environment)
        {
            _productRepository = productRepository;
            _environment = environment;
        }

        public async Task<List<ProductResponseDto>> GetAllAsync( string? search, string? category, string? status)
        {
            var products = await _productRepository.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(x => x.Name.Contains(search,
                        StringComparison.OrdinalIgnoreCase))
                       .ToList();
            }

            if (!string.IsNullOrWhiteSpace(category) &&
                category != "All")
            {
                products = products.Where(x => x.Category == category)
                    .ToList();
            }
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                products = products .Where(x => x.Status == status)
                    .ToList();
            }

            return products.Select(x => new ProductResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Picture = x.Picture,
                Description = x.Description,
                Category = x.Category,
                Brand = x.Brand,
                Price = x.Price,
                DiscountPrice = x.DiscountPrice,
                StockQuantity = x.StockQuantity,
                Status = x.Status
            }).ToList();
        }
        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return null;
            }

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Picture = product.Picture,
                Description = product.Description,
                Category = product.Category,
                Brand = product.Brand,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                Status = product.Status
            };
        }
        public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto)
        {
            if (!_categories.Contains(dto.Category))
            {
                throw new ArgumentException( "Invalid category. Allowed categories: Electronics, Clothing, Grocery, Furniture, Books.");
            }

            string? pictureName = null;

            if (dto.Picture != null)
            {
                
                var uploadsFolder = Path.Combine( _environment.WebRootPath,"images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                pictureName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Picture.FileName);
                var filePath = Path.Combine(uploadsFolder,pictureName);
                using var stream = new FileStream(filePath,FileMode.Create);
                await dto.Picture.CopyToAsync(stream);
            }

            var product = new Product
            {
                Name = dto.Name,
                Picture = pictureName,
                Description = dto.Description,
                Category = dto.Category,
                Brand = dto.Brand,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                StockQuantity = dto.StockQuantity,
                Status = dto.Status
            };

            var createdProduct = await _productRepository.AddAsync(product);

            return new ProductResponseDto
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Picture = createdProduct.Picture,
                Description = createdProduct.Description,
                Category = createdProduct.Category,
                Brand = createdProduct.Brand,
                Price = createdProduct.Price,
                DiscountPrice = createdProduct.DiscountPrice,
                StockQuantity = createdProduct.StockQuantity,
                Status = createdProduct.Status
            };
        }

        public async Task<bool> ChangeStatusAsync(int id, string status)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return false;
            }

            if (status != "Active" && status != "Inactive")
            {
                throw new ArgumentException("Status must be Active or Inactive.");
            }

            product.Status = status;

            await _productRepository.UpdateAsync(product);

            return true;
        }
        public async Task<bool> UpdateAsync(int id,ProductCreateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return false;
            }

            if (!_categories.Contains(dto.Category))
            {
                throw new ArgumentException("Invalid category. Allowed categories: Electronics, Clothing, Grocery, Furniture, Books." );
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Category = dto.Category;
            product.Brand = dto.Brand;
            product.Price = dto.Price;
            product.DiscountPrice = dto.DiscountPrice;
            product.StockQuantity = dto.StockQuantity;
            product.Status = dto.Status;
            if (dto.Picture != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath,"uploads","products");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (!string.IsNullOrEmpty(product.Picture))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, product.Picture);

                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                var pictureName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Picture.FileName);

                var filePath = Path.Combine(uploadsFolder,pictureName);

                using var stream = new FileStream(filePath,FileMode.Create);

                await dto.Picture.CopyToAsync(stream);

                product.Picture = pictureName;
            }

            await _productRepository.UpdateAsync(product);

            return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(product.Picture))
            {
                var uploadsFolder = Path.Combine( _environment.WebRootPath,"uploads","products");

                var filePath = Path.Combine(uploadsFolder,product.Picture);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            await _productRepository.DeleteAsync(id);

            return true;
        }
    }
}