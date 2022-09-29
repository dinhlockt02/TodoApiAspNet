using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Route("/api/[controller]")]
public class TodoController : ControllerBase
{
  private readonly ITodoService _todoService;
  private readonly IAuthService _authService;

  public TodoController(ITodoService todoService, IAuthService authService)
  {
    _todoService = todoService;
    _authService = authService;
  }

  [HttpGet]
  [ProducesResponseType(typeof(List<Todo>), StatusCodes.Status200OK)]
  public async Task<ActionResult<IEnumerable<Todo>>> Get()
  {
    try
    {
      var user = await getCurrentUser(HttpContext);
      if (user == null)
      {
        return BadRequest();
      }
      List<Todo> todos = await _todoService.GetTodos(user);
      return todos;
    }
    catch (System.Exception)
    {
      return StatusCode(StatusCodes.Status500InternalServerError);
    }
  }

  [HttpGet("{id}")]
  [ProducesResponseType(typeof(Todo), StatusCodes.Status200OK)]
  public async Task<ActionResult<Todo>> GetById(string id)
  {
    try
    {
      var todo = await _todoService.GetTodo(id);
      if (todo == null) return NotFound("Todo not found");
      return todo;
    }
    catch (Exception)
    {
      return StatusCode(StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Put(string id, UpdateTodoModel updateTodoModel)
  {
    try
    {
      var updateTodo = updateTodoModel.toTodo();
      var existingTodo = await _todoService.GetTodo(id);
      if (existingTodo == null)
      {
        return NotFound();
      }
      var user = await getCurrentUser(HttpContext);
      if (user?.Id != existingTodo.UserId)
      {
        return Unauthorized();
      }
      updateTodo.Id = id;
      updateTodo.UserId = user.Id;
      await _todoService.UpdateTodo(updateTodo);
      return NoContent();
    }
    catch (System.Exception)
    {
      return StatusCode(StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPost]
  public async Task<IActionResult> Post(CreateTodoModel createTodoModel)
  {
    try
    {
      var newTodo = createTodoModel.toTodo();
      var user = await getCurrentUser(HttpContext);
      if (user == null) return BadRequest();
      newTodo.UserId = user.Id!;
      await _todoService.CreateTodo(newTodo);
      return CreatedAtAction(nameof(GetById), newTodo);
    }
    catch (System.Exception)
    {

      return StatusCode(StatusCodes.Status500InternalServerError);
    }
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id)
  {
    try
    {
      var existingTodo = await _todoService.GetTodo(id);
      if (existingTodo == null)
      {
        return NotFound();
      }
      var user = await getCurrentUser(HttpContext);
      if (user?.Id != existingTodo.UserId)
      {
        return Unauthorized();
      }
      await _todoService.DeleteTodo(id);
      return NoContent();
    }
    catch (System.Exception)
    {
      return StatusCode(StatusCodes.Status500InternalServerError);
    }
  }

  [HttpDelete]
  public async Task<IActionResult> DeleteAll()
  {
    try
    {
      var user = await getCurrentUser(HttpContext);
      if (user == null)
      {
        return BadRequest();
      }
      await _todoService.DeleteAllTodos(user);
      return NoContent();
    }
    catch (System.Exception)
    {
      return StatusCode(StatusCodes.Status500InternalServerError);
      throw;
    }
  }
  private async Task<User?> getCurrentUser(HttpContext httpContext)
  {
    var userId = httpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
    {
      throw new Exception("Invalid token");
    }
    var user = await _authService.GetUser(userId);
    return user;
  }
}

public class CreateTodoModel
{
  public string Title { get; set; } = string.Empty;
  public string Detail { get; set; } = string.Empty;
  public DateTime Date { get; set; } = DateTime.Now;
  public Todo toTodo() => new Todo
  {
    Title = this.Title,
    Detail = this.Detail,
    Date = this.Date,
  };
}

public class UpdateTodoModel
{
  public string Title { get; set; } = string.Empty;
  public string Detail { get; set; } = string.Empty;
  public DateTime Date { get; set; } = DateTime.Now;
  public Todo toTodo() => new Todo
  {
    Title = this.Title,
    Detail = this.Detail,
    Date = this.Date,
  };
}