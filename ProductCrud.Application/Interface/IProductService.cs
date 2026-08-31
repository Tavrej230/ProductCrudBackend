using ProductCrud.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCrud.Application.Interface
{
    public interface IProductService
    {
        Task<List<ProductResponseDto>> GetAllAsync(string? search, string? category,string? status);

        Task<ProductResponseDto?> GetByIdAsync(int id);

        Task<ProductResponseDto> CreateAsync(ProductCreateDto dto);

        Task<bool> ChangeStatusAsync(int id, string status);

        Task<bool> UpdateAsync(int id, ProductCreateDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
