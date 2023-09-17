using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using serverMaestro.Data;
using serverMaestro.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using static serverMaestro.Controllers.MaestroController;

namespace serverMaestro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MaestroContext _context;
        private const int _KEY_SIZE = 64;
        private const int _ITERATIONS = 350000;
        private const int _ADMIN = 1;
        private const int _USER = 2;
        private System.Security.Cryptography.HashAlgorithmName _hashAlgorithm = System.Security.Cryptography.HashAlgorithmName.SHA512;

        public UsersController(MaestroContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> PostUser(PostUserDBO postUser)
        {
            string isUnique = _context.User.Select(u => u.Email).Where(e => e == postUser.Email).FirstOrDefault();
            if (isUnique != null)
                return BadRequest("Such email is already exist");
            OrderAddress address = new OrderAddress()
            {
                Address = postUser.Address,
                FirstName = postUser.FirstName,
                City = postUser.City,
                LastName = postUser.LastName,
                Phone = postUser.Phone,
                ZipCode = postUser.ZipCode
            };
            _context.OrderAddress.Add(address);
            _context.SaveChanges();
            User user = new User()
            {
                RoleId = _USER,
                Email = postUser.Email,
                OrderAddressId = address.Id,
                Password = HashPassword(postUser.Password, out byte[] salt)
            };
            _context.User.Add(user);
            _context.SaveChanges();
            System.IO.File.WriteAllBytes($"./HashData/{user.Id}.bin", salt);

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return await _context.User.ToListAsync();
        }

        [HttpGet("getuser")]
        public async Task<ActionResult> GetUser(int userId)
        {
            User user = _context.User.Find(userId);
            if (user == null)
                return NotFound();

            OrderAddress address = _context.OrderAddress.Where(oa => oa.Id == user.OrderAddressId).FirstOrDefault();
            if (address == null)
                return NotFound();
            return Ok(
                new
                {
                    UserId = user.Id,
                    RoleId = user.RoleId,
                    Email = user.Email,
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    Address = address.Address,
                    City = address.City,
                    ZipCode = address.ZipCode,
                    Phone = address.Phone
                }
            );
        }

        [HttpPost("updateuser")]
        public async Task<IActionResult> UpdateUser(UpdateUserDBO toUpdate)
        {
            User user = _context.User.Find(toUpdate.UserId);
            if (user == null)
                return NotFound();
            
            OrderAddress address = _context.OrderAddress.Find(user.OrderAddressId);
            if (address == null)
                return NotFound();

            address.Phone = toUpdate.ToUpdate.Phone;
            address.Address = toUpdate.ToUpdate.Address;
            address.LastName = toUpdate.ToUpdate.LastName;
            address.FirstName = toUpdate.ToUpdate.FirstName;
            address.ZipCode = toUpdate.ToUpdate.ZipCode;
            address.City = toUpdate.ToUpdate.City;

            _context.OrderAddress.Update(address);
            await _context.SaveChangesAsync();

            return Ok(address);
        }

        [HttpPost]
        [Route("user")]
        public async Task<ObjectResult> Login(UserDBO userDBO)
        {
            User user = _context.User.Where(u => u.Email == userDBO.Email).FirstOrDefault();
            if (user == null)
                return BadRequest(new { Message = "User do not exist" });
            byte[] salt = System.IO.File.ReadAllBytes($"./HashData/{user.Id}.bin");

            if (VerifyPassword(userDBO.Password, user.Password, salt))
                return new ObjectResult(user);
            else
                return BadRequest(new { Message = "Password is incorrect" });
            //CreatedAtAction()
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            CleanUser(id);
            CleanCardItems(id);
            CleanWishlistItems(id);
            CancelOrders(id);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(UpdateUserPasswordDBO toUpdate)
        {
            User user = _context.User.Find(toUpdate.UserId);
            if (user == null)
                return NotFound();
            byte[] salt = System.IO.File.ReadAllBytes($"./HashData/{user.Id}.bin");
            bool isCorrectCurrentPassword = VerifyPassword(toUpdate.OldPassword, user.Password, salt);
            if (!isCorrectCurrentPassword)
                return BadRequest();
            System.IO.File.Delete($"./HashData/{user.Id}.bin");
            user.Password = HashPassword(toUpdate.NewPassword, out salt);
            System.IO.File.WriteAllBytes($"./HashData/{user.Id}.bin", salt);
            _context.User.Update(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }


        private void CleanUser(int id)
        {
            User user = _context.User.Find(id);
            _context.User.Remove(user);
        }

        private void CancelOrders(int id)
        {
            List<Order> orders = _context.Order.Where(o => o.UserId == id).ToList();
            foreach (Order order in orders)
                order.OrderStatus = OrderStatus.Canceled.ToString();
        }

        private void CleanWishlistItems(int id)
        {
            List<Wishlist> userWishlistItems = _context.Wishlist.Where(w => w.UserId == id).ToList();
            foreach (Wishlist wishlistItem in userWishlistItems)
                _context.Wishlist.Remove(wishlistItem);
        }

        private void CleanCardItems(int id)
        {
            List<Card> userCardItems = _context.Card.Where(c => c.UserId == id).ToList();
            foreach (Card cardItem in userCardItems)
                _context.Card.Remove(cardItem);
        }

        private bool VerifyPassword(string password, string hash, byte[] salt)
        {
            byte[] hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                _ITERATIONS,
                _hashAlgorithm,
                _KEY_SIZE
            );
            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
        }

        private string HashPassword(string password, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(_KEY_SIZE);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                _ITERATIONS,
                _hashAlgorithm,
                _KEY_SIZE
            );
            return Convert.ToHexString(hash);
        }


        public sealed class UserDBO
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public sealed class PostUserDBO
        {
            public string Password { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
            public string Phone { get; set; }
        }

        public sealed class UpdateUserDBO
        {
            public int UserId { get; set; }
            public PostUserDBO ToUpdate { get; set; }
        }

        public sealed class UpdateUserPasswordDBO
        {
            public int UserId { get; set; }
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
