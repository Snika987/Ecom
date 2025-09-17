using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerce_Project.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
namespace ECommerce_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunctionsController : ControllerBase
    {
        ECommerceContext e = new ECommerceContext();

        //Adding Products 
        [HttpPost]
        [Route("AddProduct")]

        public IActionResult AddProduct([FromQuery] Product product)
        {
            try
            {
                e.Products.Add(product);
                int i = e.SaveChanges();
                return Ok(i);
            }

            catch (Exception ex)
            { 
                return BadRequest(ex.Message);
            }
            
        }


        //View All Products
        [HttpGet]
        [Route("ViewProducts")]

        [Authorize]
        public IActionResult ViewProducts()
        {
            try
            {
                var res = e.Products.Select(t => t);
                return Ok(res);
            }

            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }
           
        }


        //Registering User
        [AllowAnonymous]
        [HttpPost]
        [Route("RegisterUser")]

        public IActionResult RegisterUser([FromBody] RegisterRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid request");
                }

                if (string.IsNullOrWhiteSpace(request.Uid))
                {
                    return BadRequest("UId is required");
                }

                if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
                {
                    return BadRequest("Valid email is required");
                }

                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                {
                    return BadRequest("Password must be at least 6 characters");
                }

                var existing = e.Users.FirstOrDefault(u => u.Email == request.Email);
                if (existing != null)
                {
                    return Conflict("Email already registered");
                }

                var user = new User
                {
                    Uid = request.Uid,
                    Email = request.Email,
                };
                user.Password = SimplePasswordHasher.HashPassword(request.Password);

                e.Users.Add(user);
                int i = e.SaveChanges();
                return Ok(new { success = i > 0 });
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }


        //Login User
        [HttpGet]
        [Route("LoginUser")]

        public IActionResult LoginUser(string id)
        {
            try
            {
                var res = e.Users.FirstOrDefault(t => t.Uid == id);
                if (res == null)
                    return NotFound("User Not Registered. Register First");


                Console.WriteLine("Login Successful");
                return Ok(res);
            }

            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }

           
        }




    }
    
}
