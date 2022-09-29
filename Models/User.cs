using MongoDB.Bson.Serialization.Attributes;

namespace TodoApi.Models
{


  public class User
  {
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
  }

  public class UserSignupDto
  {
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    public User toUser() => new User
    {
      Username = this.Username,
      Email = this.Email,
      Password = this.Password
    };
  }

  public class UserSigninDto
  {
    public string Email { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;

    public User toUser() => new User
    {
      Email = this.Email,
      Password = this.Password
    };
  }

}
