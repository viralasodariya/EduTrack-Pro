using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core.Models;

namespace MyProject.Core.Interface
{
    public interface IuserInterface
    {
        Task<(bool success, string message)> userSignup(UserModel newUser);
        Task<(bool success, string message, UserModel)> userLogin(UserLoginModel userlogin);
        

    }
}