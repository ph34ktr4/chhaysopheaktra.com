using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WepAPIEntityFrameworkCore.Models;

namespace WepAPIEntityFrameworkCore.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDdContext _dbcontext;

        public ProductController(ApplicationDdContext dbcontext)
        {
            _dbcontext = dbcontext;
        }


        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _dbcontext.Products.ToList();
            if (products == null || !products.Any())
            {
                return NotFound(new { Message = "No Product Found" });
            }
            var baseURl = $"{Request.Scheme}://{Request.Host}/";
            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    product.ImageUrl = baseURl + product.ImageUrl.TrimStart('/');
                }
            }
            return Ok(products);
        }



        [HttpPost]
        public async Task<IActionResult> PostProduct([FromForm] PostProducts product)
        {
            var file = product.Image;
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Image File is Required" });
            }

            string[] allowedExtensions = { "image/jpg", "image/jpeg", "image/png" };
            if (!allowedExtensions.Contains(file.ContentType))
            {
                return BadRequest(new { message = "Invalid image format. Only JPG and PNG allowed." });
            }

            var uploadpath = Path.Combine
                (Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadpath))
            {
                Directory.CreateDirectory(uploadpath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); //generate a unit file name
            var filePath = Path.Combine(uploadpath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var newProduct = new Product
            {
                Name = product.Name,
                Price = product.Price,
                Qty = product.Qty,
                ImageUrl = $"/images/{fileName}"
            };
            _dbcontext.Products.Add(newProduct);
            await _dbcontext.SaveChangesAsync();

            String imageFullUrl = $"{Request.Scheme}://{Request.Host}{newProduct.ImageUrl}";
            newProduct.ImageUrl = imageFullUrl;

            return CreatedAtAction(nameof(GetAllProducts), new { product = newProduct });
        }



        [HttpPut]
        public async Task<IActionResult> PutProduct([FromForm] PutProducts product)
        {
            var existingProduct = await _dbcontext.Products.FindAsync(product.Id);
            if (existingProduct == null)
            {
                return NotFound(new { message = "Product id not found", id = product.Id });
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Qty = product.Qty;



            if (product.Image != null && product.Image.Length > 0)
            {
                var allowedExtensions = new[] { "image/jpg", "image/jpeg", "image/png" };
                if (!allowedExtensions.Contains(product.Image.ContentType))
                {
                    return BadRequest(new { message = "Invalid image format. Only JPG and PNG allowed." });
                }
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(product.Image.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.Image.CopyToAsync(stream);
                }


                //delete old image file if exists
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                existingProduct.ImageUrl = $"/images/{fileName}";
            }

            _dbcontext.Products.Update(existingProduct);
            await _dbcontext.SaveChangesAsync();

            var fullImageUrl = $"{Request.Scheme}://{Request.Host}{existingProduct.ImageUrl}";
            existingProduct.ImageUrl = fullImageUrl;

            return Accepted(new { message = "Product Updated Successfully", product = existingProduct });
        }




        [HttpGet("{id}"), HttpGet("id/{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _dbcontext.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found", id });
            }
            if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.StartsWith("http"))
            {
                var fullImageUrl = $"{Request.Scheme}://{Request.Host}{product.ImageUrl}";
                product.ImageUrl = fullImageUrl;
            }
            return Ok(product);
        }




        [HttpGet("name/{name}")]
        public IActionResult GetProductByName(string name)
        {
            var product = _dbcontext.Products.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
            if (product == null)
            {
                return NotFound(new { message = "Product name not found", name = name });
            }
            if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.StartsWith("http"))
            {
                var fullImageUrl = $"{Request.Scheme}://{Request.Host}{product.ImageUrl}";
                product.ImageUrl = fullImageUrl;
            }
            return Ok(product);
        }



        [HttpDelete("{id}"), HttpDelete("id/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _dbcontext.Products.Find(id);
            if (product == null)
            {
                return NotFound(new { message = "Product id not found", id = id });
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            _dbcontext.Products.Remove(product);
            _dbcontext.SaveChanges();
            return Accepted(new { message = "Product Deleted Successfully", product = product });
        }
    }
}

