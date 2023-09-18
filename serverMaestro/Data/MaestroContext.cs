using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using serverMaestro.Models;

namespace serverMaestro.Data
{
    public class MaestroContext : DbContext
    {
        public MaestroContext (DbContextOptions<MaestroContext> options)
            : base(options)
        {
        }

        public DbSet<Card> Card { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderProduct> OrderProduct { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Picture> Picture { get; set; }
        public DbSet<PictureProduct> PictureProduct { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Wishlist> Wishlist { get; set; }
        public DbSet<Feature> Feature { get; set; }
        public DbSet<OrderAddress> OrderAddress { get; set; }
        public DbSet<HotDeal> HotDeal { get; set; }
        public DbSet<Review> Review { get; set; }
    }
}
