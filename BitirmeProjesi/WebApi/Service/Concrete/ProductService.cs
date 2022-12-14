using AutoMapper;
using Base;
using DataAccess;
using Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
    public class ProductService : BaseService<ProductDto, Product>, IProductService
    {
        IProductRepository _repository;
        public ProductService(IProductRepository productRepository, IMapper mapper, IUnitOfWork unitOfWork) : base(productRepository, mapper, unitOfWork)
        {
            _repository = productRepository;
        }

 
        [SecuredOperation("admin,user,product.add")]
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        [PerformanceAspect(5)]
        public override async Task<BaseResponse<ProductDto>> InsertAsync(ProductDto product)
        {
            try
            {
                var tempEntity = Mapper.Map<ProductDto, Product>(product);

                await _repository.InsertAsync(tempEntity);
                await UnitOfWork.CompleteAsync();

                return new BaseResponse<ProductDto>(Mapper.Map<Product, ProductDto>(tempEntity));
            }
            catch (Exception ex)
            {
                throw new MessageResultException("Product_Saving_Error", ex);
            }
        }

        [SecuredOperation("admin,product.add,user")]
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public override async Task<BaseResponse<ProductDto>> UpdateAsync(int id, ProductDto updateResource)
        {
            try
            {
                var tempEntity = await _repository.GetByIdAsync(id);
                if (tempEntity is null)
                    return new BaseResponse<ProductDto>("NoData");
                Mapper.Map(updateResource, tempEntity);

                await UnitOfWork.CompleteAsync();
                var resource = Mapper.Map<Product, ProductDto>(tempEntity);

                return new BaseResponse<ProductDto>(resource);
            }
            catch (Exception ex)
            {
                throw new MessageResultException("Product_Updating_Error", ex);
            }
        }

        [SecuredOperation("admin,product.add,user")]
        [CacheAspect]
        public IDataResult<List<Product>> GetProductsByCategoryId(int id)
        {
            return new SuccessDataResult<List<Product>>(_repository.GetAll(p => p.CategoryId == id));
        }
    }
}
