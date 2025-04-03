using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.API.Helper;
using MyProject.Core.Interface;
using MyProject.Core.Models;

namespace MyProject.API.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class userApiController : ControllerBase
    {
        private readonly IuserInterface _IuserInterface;
        private readonly IConfiguration _configuration;
        public userApiController(IuserInterface userInterface, IConfiguration configuration)
        {
            _IuserInterface = userInterface;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> signup([FromForm] UserModel user)
        {
            try
            {
                var (success, message) = await _IuserInterface.userSignup(user);
                if (success)
                {
                    return Ok(new { success, message });
                }
                else
                {
                    return BadRequest(new { success, message });
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine("error in userapi controller");
                return BadRequest(new { success = false, message = "There was an error during signup" });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] UserLoginModel userLoginModel)
        {
            try
            {
                var (success, message, user) = await _IuserInterface.userLogin(userLoginModel);
                if (success)
                {
                    string token = JwtHelper.GenerateJwtToken(user, _configuration);
                    return Ok(new { success, message, token });
                }
                else
                {
                    return Unauthorized(new { success, message });
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "An error occurred during login." });

            }
        }
    }
}