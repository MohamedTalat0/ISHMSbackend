using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISHMS.Core.DTOs.Auth;

namespace ISHMS.Core.Interfaces;

public interface ILdapAuthenticationService
{
    bool Authenticate(string username, string password);
    AdUserInfoDto? GetUserInfo(string username);

}