using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using TodoApi.Models;

namespace TodoApi.Services;

public interface IAuthService
{
  public Task<User?> GetUser(string id);
  public Task<User?> GetUserByEmail(string email);
  public Task CreateUser(User user);
}

public class AuthService : IAuthService
{
  private readonly IMongoCollection<User> _userCollection;
  public AuthService(IOptions<TodoApiDatabaseSettings> options)
  {
    var mongoClient = new MongoClient(options.Value.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(options.Value.DatabaseName);
    _userCollection = mongoDatabase.GetCollection<User>(options.Value.UsersCollectionName);
  }



  public async Task<User?> GetUser(string id) => await _userCollection.Find(user => user.Id == id).FirstOrDefaultAsync();

  public async Task<User?> GetUserByEmail(string email) => await _userCollection.Find(user => user.Email == email).FirstOrDefaultAsync();
  public async Task CreateUser(User user)
  {
    var existingUser = await GetUserByEmail(user.Email);
    if (existingUser != null)
    {
      throw new ArgumentException("User already existed");
    }
    user.Id = null;
    await _userCollection.InsertOneAsync(user);
  }
}
