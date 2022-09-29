namespace TodoApi.Models;

public class TodoApiDatabaseSettings
{
  public string ConnectionString { get; set; } = null!;
  public string DatabaseName { get; set; } = null!;
  public string TodosCollectionName { get; set; } = null!;
  public string UsersCollectionName { get; set; } = null!;
}