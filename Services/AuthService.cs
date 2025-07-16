using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
    public class AuthService
    {
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        // In-memory user list for demo (replace with DB in production)
        private static readonly List<UserModel> Users = new List<UserModel>();
        private static readonly object _userLock = new object();

        public bool ValidateAdmin(string username, string password)
        {
            return username == AdminUsername && password == AdminPassword;
        }

        public bool RegisterUser(UserModel model, out string error)
        {
            lock (_userLock)
            {
                if (Users.Any(u => u.Username == model.Username))
                {
                    error = "Username already exists.";
                    return false;
                }
                Users.Add(model);
                error = null;
                return true;
            }
        }

        public UserModel ValidateUser(string username, string password)
        {
            lock (_userLock)
            {
                return Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            }
        }
    }
} 