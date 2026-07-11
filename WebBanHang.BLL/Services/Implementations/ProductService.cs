using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Entities;
using WebBanHang.DAL.Repositories.Implementations;
using WebBanHang.DAL.Repositories.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ICategoryRepository _cateRepo;


        public ProductService(IProductRepository repo, ICategoryRepository cateRepo)
        {
            _repo = repo;
            _cateRepo = cateRepo;
        }

        public async Task<bool> CreateProductAsync(ProductDTO dto)
        {
            // Sửa: CategoryId là int, không so sánh với null được
            if (dto.CategoryId <= 0) return false;

            var existedCode = await _repo.GetByCodeAsync(dto.Code);
            if (existedCode != null) return false;

            var product = new Product
            {
                Code = dto.Code,
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                ProductImages = (dto.Images ?? Enumerable.Empty<ProductImageDTO>())
                    .Select(x => new ProductImage
                    {
                        ImageUrl = x.ImageUrl,
                        IsPrimary = x.IsPrimary
                    }).ToList(),
                SellingPrice = dto.SellingPrice,
                ImportPrice = dto.ImportPrice,
                StockQuantity = dto.StockQuantity,
                Status = dto.Status,
                Description = dto.Description,
                CreatedDate = DateTime.Now,
                CreatedBy = dto.CreatedBy,
            };

            await _repo.CreateAsync(product);
            return true;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _repo.GetAllAsync();

            return products.Select(u => new ProductDTO
            {
                ProductId = u.ProductId,
                Name = u.Name,
                Code = u.Code,
                SellingPrice = u.SellingPrice,
                Status = u.Status,
                StockQuantity = u.StockQuantity,
                Description = u.Description,
                Images = u.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }),
                CategoryName = u.Category.Name
            }).ToList();
        }

        public async Task<ProductDTO> GetProductByCodeAsync(string code)
        {
            var product = await _repo.GetByCodeAsync(code);
            if (product == null)
                return new ProductDTO(); // tránh null reference

            return new ProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
            };
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            Console.WriteLine("_________________");
            Console.WriteLine(id);
            var product = await _repo.GetByIdAsync(id);
            if (product == null)
                return new ProductDTO();

            return new ProductDTO
            {
                ProductId = id,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                Images = product.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }),
                Status = product.Status,
                SellingPrice= product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name
            };
        }

        public async Task<bool> UpdateProductAsync(ProductDTO dto)
        {
            var existedProduct = await _repo.GetByIdAsync(dto.ProductId);
            if (existedProduct == null) return false;

            var product = new Product
            {

                ProductId= dto.ProductId,
                Code = dto.Code,
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                ProductImages = (dto.Images ?? Enumerable.Empty<ProductImageDTO>())
                    .Select(x => new ProductImage
                    {
                        ImageUrl = x.ImageUrl,
                        IsPrimary = x.IsPrimary
                    }).ToList(),
                Description = dto.Description,
                SellingPrice = dto.SellingPrice,
                ImportPrice = dto.ImportPrice,
                StockQuantity = dto.StockQuantity,
                Status = dto.Status,
                
            };
            await _repo.UpdateAsync(product);
            return true;
        }

        public async Task<DetailProductDTO> GetDetailProductByIdAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);

            return new DetailProductDTO
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                CategoryName = product.Category.Name,
                SellingPrice = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                Description = product.Description,
                CreateBy = product.CreatedByNavigation.FullName,
                CreatedDate = product.CreatedDate ?? DateTime.Now,
                ExistingImages = product.ProductImages.Select(x => new ProductImageDTO
                {
                    ImageUrl = x.ImageUrl,
                    IsPrimary = x.IsPrimary
                }).ToList()
            };
            

        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return false;

            return await _repo.DeleteAsync(id);
            
        }
       
    }
}
