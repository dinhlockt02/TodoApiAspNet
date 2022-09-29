using Microsoft.AspNetCore.Identity;

namespace TodoApi.Common;

public class BcryptPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
{
  public string HashPassword(TUser user, string password)
  {
    return BCrypt.Net.BCrypt.HashPassword(password);
  }

  public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
  {
    var isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
    if (isValid)
    {
      return PasswordVerificationResult.Success;
    }
    return PasswordVerificationResult.Failed;
  }
}