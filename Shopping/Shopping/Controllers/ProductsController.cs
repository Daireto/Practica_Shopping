﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Helpers;
using Shopping.Models;
using Vereyon.Web;

namespace Shopping.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IFlashMessage _flashMessage;

        public ProductsController(DataContext context, ICombosHelper combosHelper, IBlobHelper blobHelper, IFlashMessage flashMessage)
        {
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
            _flashMessage = flashMessage;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            CreateProductViewModel model = new()
            {
                Categories = await _combosHelper.GetComboCategoriesAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;
                if (model.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                }

                Product product = new()
                {
                    Description = model.Description,
                    Name = model.Name,
                    Price = model.Price,
                    Stock = model.Stock,
                };

                product.ProductCategories = new List<ProductCategory>()
                {
                    new ProductCategory
                    {
                        Category = await _context.Categories.FindAsync(model.CategoryId)
                    }
                };

                product.ProductImages = new List<ProductImage>()
                {
                    new ProductImage
                    {
                        ImageId = imageId
                    }
                };

                try
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe un producto con el mismo nombre");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }
            }

            model.Categories = await _combosHelper.GetComboCategoriesAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            EditProductViewModel model = new()
            {
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                Product product = await _context.Products.FindAsync(model.Id);
                product.Description = model.Description;
                product.Name = model.Name;
                product.Price = model.Price;
                product.Stock = model.Stock;
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                {
                    _flashMessage.Danger("Ya existe un producto con el mismo nombre");
                }
                else
                {
                    _flashMessage.Danger(dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                _flashMessage.Danger(exception.Message);
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> AddImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            AddProductImageViewModel model = new()
            {
                ProductId = product.Id,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddProductImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;
                if (model.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                }

                Product product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == model.ProductId);

                ProductImage currentProductImage = product.ProductImages.FirstOrDefault();

                if (currentProductImage != null && currentProductImage.ImageId == Guid.Empty)
                {
                    try
                    {
                        currentProductImage.ImageId = imageId;
                        _context.Update(currentProductImage);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Details), new { product.Id });
                    }
                    catch (Exception exception)
                    {
                        _flashMessage.Danger(exception.Message);
                        return View(model);
                    }
                }

                ProductImage productImage = new()
                {
                    Product = product,
                    ImageId = imageId,
                };

                try
                {
                    _context.Add(productImage);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { product.Id });
                }
                catch (Exception exception)
                {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductImage productImage = await _context.ProductImages
                .Include(pi => pi.Product)
                .FirstOrDefaultAsync(pi => pi.Id == id);
            if (productImage == null)
            {
                return NotFound();
            }

            await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();

            Product product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == productImage.Product.Id);

            ProductImage currentProductImage = product.ProductImages.FirstOrDefault();

            if (currentProductImage == null)
            {
                currentProductImage = new()
                {
                    Product = product,
                    ImageId = Guid.Empty,
                };
                _context.Add(currentProductImage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { productImage.Product.Id });
        }

        public async Task<IActionResult> AddCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            AddCategoryProductViewModel model = new()
            {
                ProductId = product.Id,
                Categories = await _combosHelper.GetComboCategoriesAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Product product = await _context.Products.FindAsync(model.ProductId);
                ProductCategory productCategory = new()
                {
                    Category = await _context.Categories.FindAsync(model.CategoryId),
                    Product = product,
                };

                try
                {
                    _context.Add(productCategory);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { product.Id });
                }
                catch (Exception exception)
                {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductCategory productCategory = await _context.ProductCategories
                .Include(pc => pc.Product)
                .FirstOrDefaultAsync(pc => pc.Id == id);
            if (productCategory == null)
            {
                return NotFound();
            }

            _context.ProductCategories.Remove(productCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { productCategory.Product.Id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if(product.ProductImages != null)
            {
                foreach (ProductImage productImage in product.ProductImages)
                {
                    try
                    {
                        await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
                    }
                    catch { }
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
