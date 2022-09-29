using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace TodoApi.Models;

public class Todo
{
  [BsonId]
  [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
  public string Id { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Detail { get; set; } = string.Empty;
  public DateTime Date { get; set; } = DateTime.Now;
  [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
  public string UserId { get; set; } = null!;
}
