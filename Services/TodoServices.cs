using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TodoApi.Models;

namespace TodoApi.Services;

public interface ITodoService
{
  /// <summary>
  /// Method <c>GetTodos</c> gets all provided user's todo items; 
  /// </summary>
  public Task<List<Todo>> GetTodos(User user);

  /// <summary>
  /// Method <c>GetTodo</c> gets all provided user's todo items; 
  /// </summary>
  public Task<Todo?> GetTodo(string id);

  /// <summary>
  /// Method <c>CreateTodo</c> create todo item ; 
  /// </summary>
  public Task CreateTodo(Todo todo);

  /// <summary>
  /// Method <c>UpdateTodo</c> update provided id todo item for provided user; 
  /// </summary>
  public Task UpdateTodo(Todo updateTodo);

  /// <summary>
  /// Method <c>DeleteTodo</c> delete provided id todo item for provided user; 
  /// </summary>
  public Task DeleteTodo(string id);


  /// <summary>
  /// Method <c>DeleteAllTodos</c> delete all todo items for provided user; 
  /// </summary>
  public Task DeleteAllTodos(User user);
}

public class TodoService : ITodoService
{
  private readonly IMongoCollection<Todo> _todoCollection;
  public TodoService(IOptions<TodoApiDatabaseSettings> todoApiDatabaseSetting)
  {
    var mongoClient = new MongoClient(todoApiDatabaseSetting.Value.ConnectionString);
    var mongoDatabase = mongoClient.GetDatabase(todoApiDatabaseSetting.Value.DatabaseName);
    _todoCollection = mongoDatabase.GetCollection<Todo>(todoApiDatabaseSetting.Value.TodosCollectionName);
  }

  public async Task CreateTodo(Todo todo)
  {
    await _todoCollection.InsertOneAsync(todo);
  }

  public async Task DeleteAllTodos(User user)
  {
    await _todoCollection.DeleteManyAsync(todo => todo.UserId == user.Id);
  }

  public async Task DeleteTodo(string id)
  {
    await _todoCollection.DeleteOneAsync(todo => todo.Id == id);
  }

  public async Task<Todo?> GetTodo(string id)
  {
    return await _todoCollection.Find<Todo>(todo => todo.Id == id).FirstOrDefaultAsync();
  }

  public async Task<List<Todo>> GetTodos(User user)
  {
    return await _todoCollection.Find<Todo>(todo => todo.UserId == user.Id).ToListAsync();
  }

  public async Task UpdateTodo(Todo updateTodo)
  {
    await _todoCollection.FindOneAndReplaceAsync(todo => todo.Id == updateTodo.Id, updateTodo);
  }
}