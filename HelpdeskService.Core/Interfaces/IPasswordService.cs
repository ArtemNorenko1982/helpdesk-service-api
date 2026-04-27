using System;
using System.Collections.Generic;
using System.Text;

namespace HelpdeskService.Core.Interfaces
{
    public interface IPasswordService
    {
        string GeneratePassword(int passwordLenght = 9);
    }
}
