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
                    ProductId = product.Id,
                    ProductName = product.Name,
                    OldPrice = product.OldPrice,
                    NewPrice = product.NewPrice,
                    StockCount = product.CountOnStock,
                    Image = GetPictures(product).Length > 0 ? GetPictures(product)[0] : string.Empty,
                    Category = _context.Category.ToList().Where(
                        c => c.Id == _context.ProductCategory.ToList().Where(pc => pc.ProductId == product.Id).FirstOrDefault()?.CategoryId
                    )?.FirstOrDefault()?.Title ?? string.Empty,
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
        public ActionResult<object> GetProduct(int id)
        {
            try
            {
                Product product = _context.Product.ToList().Where(p => p.Id == id).FirstOrDefault();
                string[] categories = GetCategories(product);
                string[] pictures = GetPictures(product);
                string[] features = GetFeatures(product);

                var toSend = new
                {
                    ProductName = product.Name,
                    NewPrice = product.NewPrice,
                    OldPrice = product.OldPrice,
                    Description = product.Description,
                    ShortDescription = product.ShortDescription,
                    CountOnStock = product.CountOnStock,
                    AppearedDate = product.AppearedDate,
                    Categories = categories,
                    Pictures = pictures,
                    Features = features,
                    ProductId = product.Id
                };

                return Ok(toSend);
            }
            catch (Exception ex)
            {
                return NotFound(ex.StackTrace);
            }
        }

        [HttpGet("categories")]
        public ActionResult<object> GetAllCategories()
        {
            List<string> categories = _context.Category.Select(c => c.Title).ToList();
            return categories;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ExpectedProductDBO expectedProduct)
        {
            if (_context.Product == null)
                return Problem("Entity set 'MaestroContext.Product'  is null.");

            Product product = new Product()
            {
                Name = expectedProduct.ProductName,
                OldPrice = expectedProduct.Price,
                NewPrice = expectedProduct.Price,
                Description = expectedProduct.Description,
                CountOnStock = expectedProduct.CountOnStock,
                AppearedDate = DateTime.UtcNow,
                ShortDescription = expectedProduct.ShortDescription
            };
            _context.Product.Update(product);
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

            return Ok(product);
        }
        //TODO: UPDATE image
        [HttpPost("update/{id}")]
        public async Task<ActionResult<Product>> PostProductToUpdate(ExpectedProductDBO expectedProduct)
        {
            if (_context.Product == null)
                return Problem("Entity set 'MaestroContext.Product'  is null.");

            Product product = new Product()
            {
                Id = expectedProduct.ProductId,
                Name = expectedProduct.ProductName,
                OldPrice = expectedProduct.Price,
                NewPrice = expectedProduct.Price,
                Description = expectedProduct.Description,
                CountOnStock = expectedProduct.CountOnStock,
                AppearedDate = DateTime.UtcNow,
                ShortDescription = expectedProduct.ShortDescription
            };
            _context.Product.Update(product);
            _context.SaveChanges();

            DeleteRelationsFeature(product.Id);
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

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            Product toDelete = _context.Product.Find(id);
            if (toDelete == null)
                return BadRequest("Such object does not exist");
            DeleteRelationsOrder(id);
            DeleteRelationsFeature(id);
            DeleteRelationsPictureProduct(id);
            DeleteRelationsProductCategory(id);
            DeleteRelationsWishlist(id);
            _context.Product.Remove(toDelete);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("addtocard")]
        public async Task<IActionResult> AddToCard(ProductUserDBO productUser)
        {
            // product to add in order
            if (_context.Product.Where(p => p.Id == productUser.ProductId).FirstOrDefault() == null)
                return BadRequest("Such product does not exist");

            // the user in which card to add
            if (_context.User.Where(u => u.Id == productUser.UserId).FirstOrDefault() == null)
                return BadRequest("Such user does not exist");

            // order in process of creating
            Card card = new Card() { UserId = productUser.UserId, ProductId = productUser.ProductId };
            _context.Card.Add(card);
            await _context.SaveChangesAsync(true);

            return Ok("Successfully added");
        }


        [HttpPost("addtowishlist")]
        public async Task<IActionResult> AddToWishlist(ProductUserDBO productUser)
        {
            // product to add to wishlist
            if (_context.Product.Where(p => p.Id == productUser.ProductId).FirstOrDefault() == null)
                return BadRequest("Such product does not exist");

            // the user in which card to add
            if (_context.User.Where(u => u.Id == productUser.UserId).FirstOrDefault() == null)
                return BadRequest("Such user does not exist");

            if (_context.Wishlist.Where(w => w.ProductId == productUser.ProductId && w.UserId == productUser.UserId).FirstOrDefault() == null)
            {
                Wishlist wishlist = new Wishlist() { UserId = productUser.UserId, ProductId = productUser.ProductId };
                _context.Wishlist.Add(wishlist);
                await _context.SaveChangesAsync(true);
                return Ok("Successfully added");
            }
            return BadRequest();
        }

        [HttpPost("removefromcard")]
        public async Task<IActionResult> RemoveFromCard(ProductUserDBO productUser)
        {
            // product to remove from order
            if (_context.Product.Where(p => p.Id == productUser.ProductId).FirstOrDefault() == null)
                return BadRequest("Such product does not exist");

            // the user in which card to remove
            if (_context.User.Where(u => u.Id == productUser.UserId).FirstOrDefault() == null)
                return BadRequest("Such user does not exist");
            Card card = _context.Card.Where(c => c.UserId == productUser.UserId && c.ProductId == productUser.ProductId).FirstOrDefault();
            if (card == null)
                return NotFound();
            _context.Card.Remove(card);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("removefromwishlist")]
        public async Task<IActionResult> RemoveFromWishlist(ProductUserDBO productUser)
        {
            Wishlist wishtlist = _context.Wishlist.Where(w => w.UserId == productUser.UserId && w.ProductId == productUser.ProductId).FirstOrDefault();
            if (wishtlist == null)
                return NotFound();
            _context.Wishlist.Remove(wishtlist);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("isincard")]
        public IActionResult IsProductInCard(ProductUserDBO productUser)
        {
            bool isProductInCard = _context.Card
                .Where(c => c.ProductId == productUser.ProductId && c.UserId == productUser.UserId)
                .FirstOrDefault() != null
                ? true : false;
            return Ok(isProductInCard);
        }

        [HttpGet("card")]
        public async Task<IActionResult> GetCard(int userId)
        {
            User user = _context.User.Find(userId);
            if (user == null)
                return BadRequest("Such user does not exist");
            List<ProductImageDBO> inCard = new List<ProductImageDBO>();
            List<Card> cards = _context.Card.Where(c => c.UserId == userId).ToList();
            foreach (Card cardItem in cards)
            {
                Product toAdd = _context.Product.Find(cardItem.ProductId) ?? new Product();
                inCard.Add(new ProductImageDBO()
                {
                    Product = toAdd,
                    Image = GetPictures(toAdd).Length > 0 ? GetPictures(toAdd)[0] : string.Empty,
                });
            }
            return Ok(inCard);
        }

        [HttpPost("submitorder")]
        public async Task<IActionResult> PostOrder(OrderDTO orderDBO)
        {
            _context.OrderAddress.Add(orderDBO.Address);
            _context.SaveChanges();
            Order order = new Order()
            {
                OrderNotes = orderDBO.OrderNotes,
                OrderAddressId = orderDBO.Address.Id,
                OrderStatus = OrderStatus.InProcess.ToString(),
                UserId = orderDBO.UserId
            };
            _context.Order.Add(order);
            _context.SaveChanges();

            foreach (Product product in orderDBO.Products)
            {
                product.CountOnStock--;
                OrderProduct productInOrder = new OrderProduct()
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    SaledByPrice = product.NewPrice
                };
                _context.OrderProduct.Add(productInOrder);
            }
            List<Card> currentUserCard = _context.Card.Where(c => c.UserId == orderDBO.UserId).ToList();
            foreach (Card cardItem in currentUserCard)
                _context.Card.Remove(cardItem);

            await _context.SaveChangesAsync();

            return Ok(order);
        }

        [HttpGet("cancelorder")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            return await CompleteOrder(id, OrderStatus.Canceled.ToString());
        }

        [HttpGet("recieveorder")]
        public async Task<IActionResult> RecieveOrder(int id)
        {
            return await CompleteOrder(id, OrderStatus.Finished.ToString());
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(int userId)
        {
            List<OrderProductsDBO> orderProductsDBOList = GetOrdersList(userId);
            return Ok(orderProductsDBOList);
        }

        [HttpGet("getwishlist")]
        public async Task<IActionResult> GetWishlist(int userId)
        {
            List<ProductDBO> productsDBO = new List<ProductDBO>();
            List<Product> products = new List<Product>();
            List<Wishlist> wishlists = _context.Wishlist.Where(w => w.UserId == userId).ToList();
            foreach (Wishlist item in wishlists)
                products.Add(_context.Product.Find(item.ProductId));

            foreach (Product product in products)
            {
                productsDBO.Add(new ProductDBO()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    OldPrice = product.OldPrice,
                    NewPrice = product.NewPrice,
                    StockCount = product.CountOnStock,
                    Image = GetPictures(product).Length > 0 ? GetPictures(product)[0] : string.Empty,
                    Category = _context.Category.ToList().Where(
                        c => c.Id == _context.ProductCategory.ToList().Where(pc => pc.ProductId == product.Id).FirstOrDefault()?.CategoryId
                    )?.FirstOrDefault()?.Title ?? string.Empty,
                    IsNew = (DateTime.Now - product.AppearedDate).Days <= 30 ? true : false
                });
            }

            return Ok(productsDBO);
        }

        [HttpGet("trackorder")]
        public async Task<IActionResult> GetOrderStatus(int orderId)
        {
            Order order = _context.Order.Find(orderId);
            if (order == null)
                return BadRequest();
            string orderStatus = order.OrderStatus == OrderStatus.InProcess.ToString() ? "In Process" : order.OrderStatus;
            return Ok(new {OrderStatus = orderStatus });
        }

        [HttpGet("gethotdeals")]
        public async Task<IActionResult> GetHotDeals()
        {
            List<HotDeal> hotDealList = _context.HotDeal.ToList();
            List<ProductImageDBO> products = new List<ProductImageDBO>();
            DateTime finishesAt = DateTime.Now;
            foreach (HotDeal item in hotDealList)
            {
                if (item.FinishesAt < DateTime.Now)
                {
                    _context.HotDeal.Remove(item);
                    continue;
                }
                Product productFromHotDeal = _context.Product.Find(item.ProductId);
                if (productFromHotDeal == null)
                    return BadRequest();
                finishesAt = item.FinishesAt;
                ProductImageDBO product = new ProductImageDBO()
                {
                    Product = productFromHotDeal,
                    Image = GetPictures(productFromHotDeal).Length > 0 ? GetPictures(productFromHotDeal)[0] : string.Empty,
                };
                products.Add(product);
            }
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Products = products,
                FinishesAt = finishesAt
            });
        }

        [HttpPost("addtohotdeals")]
        public async Task<IActionResult> AddToHotDeals(ProductDateDTO productDate)
        {
            Product toAdd = _context.Product.Find(productDate.ProductId);
            if (toAdd == null)
                return BadRequest();
            _context.HotDeal.Add(new HotDeal()
            {
                FinishesAt = productDate.FinishesAt,
                ProductId = productDate.ProductId
            });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("removefromhotdeals")]
        public async Task<IActionResult> RemoveFromHotDeals(int productId)
        {
            Product toRemove = _context.Product.Find(productId);
            HotDeal hotDeal = _context.HotDeal.Where(hd => hd.ProductId == productId).FirstOrDefault();
            if (toRemove == null || hotDeal == null)
                return BadRequest();
            _context.HotDeal.Remove(hotDeal);
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

        private string[] GetPictures(Product product)
        {
            List<string> pictures = new List<string>();
            List<PictureProduct> pictureProducts = _context.PictureProduct.ToList().Where(pp => pp.ProductId == product.Id).ToList();
            foreach (PictureProduct pictureProduct in pictureProducts)
            {
                string pictureBase64 = _context.Picture.ToList().Where(p => p.Id == pictureProduct.PictureId).FirstOrDefault()?.PictureUrl ?? string.Empty;
                pictures.Add(pictureBase64);
            }
            return pictures.ToArray();
        }

        private string[] GetCategories(Product product)
        {
            List<string> categories = new List<string>();
            List<ProductCategory> productCategories = _context.ProductCategory.ToList().Where(pc => pc.ProductId == product.Id).ToList();
            foreach (ProductCategory productCategory in productCategories)
            {
                string categoryName = _context.Category.ToList().Where(c => c.Id == productCategory.CategoryId).FirstOrDefault()?.Title ?? string.Empty;
                categories.Add(categoryName);
            }
            return categories.ToArray();
        }

        private string[] GetFeatures(Product product)
        {
            string separator = "____";
            List<string> concatedFeatures = new List<string>();
            List<Feature> features = _context.Feature.ToList().Where(f => f.ProductId == product.Id).ToList() ?? new List<Feature>();
            foreach (Feature feature in features)
            {
                string concatedFeature = $"{feature.FeatureName}{separator}{feature.FeatureValue}";
                concatedFeatures.Add(concatedFeature);
            }
            return concatedFeatures.ToArray();
        }

        private void DeleteRelationsProductCategory(int id)
        {
            List<ProductCategory> relationsProductCategory = _context.ProductCategory.Where(pc => pc.ProductId == id).ToList();
            foreach (ProductCategory productCategory in relationsProductCategory)
                _context.ProductCategory.Remove(productCategory);
        }

        private void DeleteRelationsPictureProduct(int id)
        {
            List<PictureProduct> relationsPictureProduct = _context.PictureProduct.Where(pp => pp.ProductId == id).ToList();
            foreach (PictureProduct pictureProduct in relationsPictureProduct)
                _context.PictureProduct.Remove(pictureProduct);
        }

        private void DeleteRelationsFeature(int id)
        {
            List<Feature> relationsFeature = _context.Feature.Where(f => f.ProductId == id).ToList();
            foreach (Feature feature in relationsFeature)
                _context.Feature.Remove(feature);
        }

        private void DeleteRelationsOrder(int id)
        {
            OrderProduct productInOrder = _context.OrderProduct.Where(op => op.ProductId == id).FirstOrDefault();
            int orderId = productInOrder == null ? 0 : productInOrder.OrderId;
            List<OrderProduct> relationsOrderProduct = _context.OrderProduct.Where(op => op.ProductId == id).ToList();
            foreach (OrderProduct orderProduct in relationsOrderProduct)
                _context.OrderProduct.Remove(orderProduct);
            Order order = _context.Order.Find(orderId);
            if (order != null)
                order.OrderStatus = OrderStatus.Deleted.ToString();
        }

        private void DeleteRelationsWishlist(int id)
        {
            List<Wishlist> relationsWishlist = _context.Wishlist.Where(pc => pc.ProductId == id).ToList();
            foreach (Wishlist wishlist in relationsWishlist)
                _context.Wishlist.Remove(wishlist);
        }

        private async Task<IActionResult> CompleteOrder(int id, string orderStatus)
        {
            Order order = _context.Order.Find(id);
            if (order == null)
                return BadRequest();
            order.OrderStatus = orderStatus;
            History history = new History()
            {
                OrderId = order.Id,
                UserId = order.UserId
            };
            _context.History.Add(history);
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        private List<OrderProductsDBO> GetOrdersList(int userId)
        {
            List<OrderProductsDBO> orderProductsDBOList = new List<OrderProductsDBO>();

            List<Order> orders = _context.Order.Where(o => o.UserId == userId).ToList();
            foreach (Order order in orders)
            {
                List<OrderProduct> productsInOrder = _context.OrderProduct.Where(op => op.OrderId == order.Id).ToList();
                double totalOrderPrice = 0;
                List<Product> products = new List<Product>();
                List<double> saledPrices = new List<double>();
                foreach (OrderProduct orderProductItem in productsInOrder)
                {
                    products.Add(_context.Product.Find(orderProductItem.ProductId));
                    totalOrderPrice += orderProductItem.SaledByPrice;
                    saledPrices.Add(orderProductItem.SaledByPrice);
                }
                List<ProductImageDBO> productsImages = new List<ProductImageDBO>();
                int index = 0;
                foreach (Product product in products)
                {
                    PictureProduct firstPicture = _context.PictureProduct.Where(pp => pp.ProductId == product.Id).FirstOrDefault();
                    Picture picture = _context.Picture.Find(firstPicture.PictureId);
                    ProductImageDBO productImage = new ProductImageDBO()
                    {
                        Image = picture.PictureUrl,
                        Product = product,
                        SaledByPrice = saledPrices[index++]
                    };
                    productsImages.Add(productImage);
                }
                OrderProductsDBO orderProductsDBO = new OrderProductsDBO()
                {
                    OrderId = order.Id,
                    Products = productsImages,
                    OrderStatus = order.OrderStatus,
                    TotalPrice = totalOrderPrice
                };
                orderProductsDBOList.Add(orderProductsDBO);
            }

            return orderProductsDBOList;
        }


        public sealed class OrderDTO
        {
            public Product[] Products { get; set; }
            public int UserId { get; set; }
            public OrderAddress Address { get; set; }
            public string OrderNotes { get; set; }
        }

        public sealed class ProductUserDBO
        {
            public int UserId { get; set; }
            public int ProductId { get; set; }
        }

        public sealed class ProductCategoryDBO
        {
            public List<ProductDBO> ProductsDBO { get; set; }
            public List<string> Categories { get; set; }
        }

        public sealed class ProductDBO
        {
            public int ProductId { get; set; }
            public string Image { get; set; }
            public double OldPrice { get; set; }
            public string Category { get; set; }
            public string ProductName { get; set; }
            public double NewPrice { get; set; }
            public bool IsNew { get; set; }
            public int StockCount { get; set; }
            public List<string> Categories { get; set; }
        }

        public sealed class ExpectedProductDBO
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string[] Categories { get; set; }
            public double Price { get; set; }
            public string Description { get; set; }
            public string ShortDescription { get; set; }
            public int CountOnStock { get; set; }
            public string[] Pictures { get; set; }
            public string[] Features { get; set; }
        }

        public sealed class OrderProductsDBO
        {
            public int OrderId { get; set; }
            public string OrderStatus { get; set; }
            public double TotalPrice { get; set; }
            public List<ProductImageDBO> Products { get; set; }
        }

        public sealed class ProductImageDBO
        {
            public string Image { get; set; }
            public Product Product { get; set; }
            public double SaledByPrice { get; set; }
        }

        public sealed class ProductDateDTO
        {
            public int ProductId { get; set; }
            public DateTime FinishesAt { get; set; }
        }
    }
}