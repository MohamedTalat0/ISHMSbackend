using System.DirectoryServices;
using ISHMS.Core.DTOs.Auth;
using ISHMS.Core.Interfaces;

namespace ISHMS.BLL.Services;

public class LdapAuthenticationService
    : ILdapAuthenticationService
{
    private const string LdapPath =
        "LDAP://4.232.82.68/DC=ishms,DC=local";

    private const string Domain =
        "ISHMS";

    public bool Authenticate(
        string username,
        string password)
    {
        try
        {
            string domainAndUsername =
                $@"{Domain}\{username}";

            using DirectoryEntry entry =
                new DirectoryEntry(
                    LdapPath,
                    domainAndUsername,
                    password,
                    AuthenticationTypes.Secure);

            object nativeObject =
                entry.NativeObject;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    public AdUserInfoDto? GetUserInfo(string username)
    {
        try
        {
        using DirectoryEntry entry =
    new DirectoryEntry(
        LdapPath,
        @"ISHMS\ali.it",
        "Asd123456@",
        AuthenticationTypes.Secure);
            using DirectorySearcher searcher =
                new DirectorySearcher(entry);

            searcher.Filter =
                $"(sAMAccountName={username})";

            var result = searcher.FindOne();

            if (result == null)
                return null;

            var user = new AdUserInfoDto();

            user.Username = username;

            if (result.Properties.Contains("displayName"))
            {
                user.FullName =
                    result.Properties["displayName"][0]
                        ?.ToString();
            }

            if (result.Properties.Contains("mail"))
            {
                user.Email =
                    result.Properties["mail"][0]
                        ?.ToString();
            }

            foreach (var group in result.Properties["memberOf"])
            {
                string groupName = group.ToString()!;

                if (groupName.Contains("Admins"))
                    user.Roles.Add("Admin");

                if (groupName.Contains("Doctors_Group"))
                    user.Roles.Add("Doctor");

                if (groupName.Contains("Nurses"))
                    user.Roles.Add("Nurse");

                if (groupName.Contains("Receptionists"))
                    user.Roles.Add("Receptionist");
            }

            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return null;
        }
    }
}