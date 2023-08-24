using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using serverMaestro.Data;
using serverMaestro.Models;
using System.Drawing.Imaging;
using System.Security.Principal;

namespace serverMaestro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaestroController : ControllerBase
    {
        private readonly MaestroContext _context;

        public MaestroController(MaestroContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<ProductCategoryDBO> GetProducts()
        {
            List<ProductDBO> productsDBO = new List<ProductDBO>();
            List<Product> products = _context.Product.ToList();
            foreach (Product product in products)
            {
                productsDBO.Add(new ProductDBO()
                {
                    ProductName = product.Name,
                    Price = product.Price,
                    Sale = product.SalePricePercent,
                    Image = _context.Picture.ToList().Where(
                        p => p.Id == _context.PictureProduct.ToList().Where(pp => pp.ProductId == product.Id).FirstOrDefault()?.ProductId
                    )?.FirstOrDefault()?.PictureUrl,
                    Category = _context.Category.ToList().Where(
                        c => c.Id == _context.ProductCategory.ToList().Where(pc => pc.ProductId == product.Id).FirstOrDefault()?.CategoryId
                    )?.FirstOrDefault()?.Title,
                    IsNew = (DateTime.Now - product.AppearedDate).Days <= 30 ? true : false
                });
            }
            List<string> categories = _context.Category.ToList().Select(c => c.Title).ToList();
            ProductCategoryDBO toSend = new ProductCategoryDBO()
            {
                ProductsDBO = productsDBO,
                Categories = categories
            };

            return toSend;
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProducts(int id)
        {
            return null;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutArt(int id, Product product)
        {
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ExpectedProductDBO expectedProduct)
        {
            if (_context.Product == null)
                return Problem("Entity set 'MaestroContext.Product'  is null.");

            Product product = new Product()
            {
                Name = expectedProduct.ProductName,
                Price = expectedProduct.Price,
                SalePricePercent = expectedProduct.SalePricePercent,
                Description = expectedProduct.Description,
                CountOnStock = expectedProduct.CountOnStock,
                AppearedDate = DateTime.UtcNow
            };
            _context.Product.Add(product);
            _context.SaveChanges();
            
            AddCategoriesToContext(expectedProduct, product);
            AddPicturesToContext(expectedProduct, product);
            List<string[]> splitedFeatures = SplitFeatures(expectedProduct.Features);

            foreach (var feature in splitedFeatures)
            {
                Feature newFeature = new Feature()
                {
                    FeatureName = feature[0],
                    FeatureValue = feature[1],
                    ProductId = product.Id,
                };
                _context.Feature.Add(newFeature);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Split string by separator
        /// </summary>
        /// <param name="features"></param>
        /// <param name="separator"></param>
        /// <returns>splited[0] - featureName, splited[1] - featureValue for each row</returns>
        private List<string[]> SplitFeatures(string[] features, string separator = "____")
        {
            List<string[]> splitedFeatures = new List<string[]>();
            foreach (string feature in features)
                splitedFeatures.Add(feature.Split(separator)); 
            return splitedFeatures;
        }

        private void AddPicturesToContext(ExpectedProductDBO expectedProduct, Product product)
        {
            foreach (string picture in expectedProduct.Pictures)
            {
                Picture newPicture = new Picture() { PictureUrl = picture };
                _context.Picture.Add(newPicture);
                _context.SaveChanges();
                _context.PictureProduct.Add(new PictureProduct()
                {
                    PictureId = newPicture.Id,
                    ProductId = product.Id
                });
            }
        }

        private void AddCategoriesToContext(ExpectedProductDBO expectedProduct, Product product)
        {
            foreach (string category in expectedProduct.Categories)
            {
                int categoryId = _context.Category.ToList().Where(c => c.Title == category).FirstOrDefault().Id;
                _context.ProductCategory.Add(new ProductCategory()
                {
                    CategoryId = categoryId,
                    ProductId = product.Id
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {

            return NoContent();
        }

        public sealed class ProductCategoryDBO
        {
            public List<ProductDBO> ProductsDBO { get; set; }
            public List<string> Categories { get; set; }
        }

        public sealed class ProductDBO
        {
            public string Image { get; set; }
            public int Sale { get; set; }
            public string Category { get; set; }
            public string ProductName { get; set; }
            public double Price { get; set; }
            public bool IsNew { get; set; }
            public List<string> Categories { get; set; }
        }

        public sealed class ExpectedProductDBO
        {
            public string ProductName { get; set; }
            public string[] Categories { get; set; }
            public int Price { get; set; }
            public int SalePricePercent { get; set; }
            public string Description { get; set; }
            public int CountOnStock { get; set; }
            public string[] Pictures { get; set; }
            public string[] Features { get; set; }
        }

    }
}
