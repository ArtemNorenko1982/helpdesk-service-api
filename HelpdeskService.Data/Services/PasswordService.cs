using HelpdeskService.Core.Common;
using HelpdeskService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelpdeskService.Data.Services
{
    public class PasswordService : IPasswordService
    {
        public string GeneratePassword(int passwordLength = 9)
        {
            var res = new StringBuilder();
            var rnd = new Random();
            while (0 < passwordLength--)
            {
                res.Append(AppConstants.PasswordPattern[rnd.Next(AppConstants.PasswordPattern.Length)]);
            }
            return res.ToString();
        }
    }
}
