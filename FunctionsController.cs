using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerce_Project.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Project.Controllers
{
    /// <summary>
    /// Main controller for e-commerce operations - products, user registration, and basic functions
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FunctionsController : ControllerBase
    {
        // Database context for all operations
        ECommerceContext e = new ECommerceContext();

        /// <summary>
        /// Add a new product to the database
        /// </summary>
        /// <param name="product">Product details (name, price, description, image, stock)</param>
        /// <returns>Number of records affected (1 if successful)</returns>
        [HttpPost]
        [Route("AddProduct")]
        public IActionResult AddProduct([FromQuery] Product product)
        {
            try
            {
                // Add product to database
                e.Products.Add(product);
                int i = e.SaveChanges(); // Save changes and get count of affected records
                return Ok(i);
            }
            catch (Exception ex)
            { 
                return BadRequest(ex.Message); // Return error message if something goes wrong
            }
        }

        /// <summary>
        /// Get all products from database - requires user to be logged in
        /// </summary>
        /// <returns>List of all products</returns>
        [HttpGet]
        [Route("ViewProducts")]
        [Authorize] // This endpoint requires JWT token (user must be logged in)
        public IActionResult ViewProducts()
        {
            try
            {
                // Get all products from database
                var res = e.Products.Select(t => t);
                return Ok(res);
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message); // Return error message if something goes wrong
            }
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="request">User registration details (email and password)</param>
        /// <returns>Success message if registration successful</returns>
        [AllowAnonymous] // This endpoint doesn't require authentication
        [HttpPost]
        [Route("RegisterUser")]
        public IActionResult RegisterUser([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate request object
                if (request == null)
                {
                    return BadRequest("Invalid request");
                }

                // Validate email format
                if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
                {
                    return BadRequest("Valid email is required");
                }

                // Validate password length
                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                {
                    return BadRequest("Password must be at least 6 characters");
                }

                // Check if email already exists
                var existing = e.Users.FirstOrDefault(u => u.Email == request.Email);
                if (existing != null)
                {
                    return Conflict("Email already registered");
                }

                // Generate unique user ID from email and timestamp
                var uid = request.Email.Split('@')[0] + "_" + DateTime.Now.Ticks.ToString().Substring(10);

                // Create new user object
                var user = new User
                {
                    Uid = uid,
                    Email = request.Email,
                    Password = request.Password // Store password as plain text for simplicity
                };

                // Add user to database and save
                e.Users.Add(user);
                int i = e.SaveChanges();
                return Ok(new { success = i > 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Return error message if something goes wrong
            }
        }

        /// <summary>
        /// Basic user login check by user ID (legacy endpoint - not used in current flow)
        /// </summary>
        /// <param name="id">User ID to check</param>
        /// <returns>User details if found</returns>
        [HttpGet]
        [Route("LoginUser")]
        public IActionResult LoginUser(string id)
        {
            try
            {
                // Find user by ID
                var res = e.Users.FirstOrDefault(t => t.Uid == id);
                if (res == null)
                    return NotFound("User Not Registered. Register First");

                Console.WriteLine("Login Successful");
                return Ok(res);
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message); // Return error message if something goes wrong
            }
        }

        /// <summary>
        /// Create a new order when user clicks "Buy Now" - simple version
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Success message if order created</returns>
        [HttpPost]
        [Route("BuyNow")]
        public IActionResult BuyNow([FromQuery] string userId, [FromQuery] string productId)
        {
            try
            {
                // Get product price
                var product = e.Products.FirstOrDefault(p => p.Pid == productId);

                // Create simple order - always quantity 1
                var order = new UserOrder
                {
                    Uid = userId, // User ID from frontend
                    Pid = productId, // Product ID from frontend
                    Quantity = 1, // Always 1
                    OrderDate = DateTime.Now, // System date
                    TotalAmount = product.Price, // Just the product price
                    ShippingAddress = null // Leave blank (no not null constraint)
                };

                // Add order to database
                e.UserOrders.Add(order);
                int result = e.SaveChanges();

                if (result > 0)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Order placed successfully!"
                    });
                }
                else
                {
                    return BadRequest("Failed to create order");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
