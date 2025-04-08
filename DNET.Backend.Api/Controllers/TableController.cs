using System.Text.Json;
using Consul;
using DNET.Backend.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace DNET.Backend.Api.Controllers;

[ApiController]
[Route("tables")]
public class TableController : ControllerBase
{
    private readonly ITableService _tableService;
    
    public TableController(ITableService tableService)
    {
        _tableService = tableService;
    }
    
    
    //GET /tables (with pagination)
    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    public IActionResult GetTables(int page = 1, int size = 10)
    {
        var tables = _tableService.GetAllPaginatedTables(page, size);
        if (tables == null)
            return NotFound(new ErrorResponse { Message = "Tables not found" });
        
        var response = new
        {
            tables = tables.Tables,
            totalItems = tables.TotalItems,
            page = tables.Page,
            size = tables.Size,
            totalPages = tables.TotalPages
        };

        return Ok(response);
    }
    
    
    // GET /tables/1
    [HttpGet]
    [Route("{id:int}")]
    [Authorize(Roles = "User, Admin")]
    public IActionResult GetTable(int id)
    {
        var table = _tableService.GetTable(id);
        if (table == null)
            return NotFound(new ErrorResponse { Message = "Table not found" });
        
        return Ok(table);
    }
    
    
    // GET /tables/filter?capacity=4
    [HttpGet]
    [Route("filter")]
    [Authorize(Roles = "User, Admin")]
    public IActionResult GetTablesByCapacity(int capacity)
    {
        var tables = _tableService.GetTablesByCapacity(capacity);
        if (tables == null)
            return NotFound(new ErrorResponse { Message = "Table not found" });
        
        return Ok(tables);
    }
    
    
    // POST /tables
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateTable(Table table)
    {
        try
        {
            var newTable = _tableService.CreateTable(table);
            if (newTable == null)
                return StatusCode(403, new { error = "Table creation is currently disabled." });
            
            return Created($"/tables/{newTable.Item1}", table);
        }
        catch (ServerException badRequestException)
        {
            return BadRequest(new ErrorResponse
            {
                Message = badRequestException.WrongMessage,
                Status = badRequestException.WrongCode
            });
        }
    }
    
    
    // PUT /tables/1
    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateTable(int id, Table table)
    {
        try
        {
            var updatedTable = _tableService.UpdateTable(id, table);
            if (updatedTable == null)
                return NotFound(new ErrorResponse { Message = "Table not found" });
            
            return Ok(updatedTable);
        }
        catch (ServerException badRequestException)
        {
            return BadRequest(new ErrorResponse
            {
                Message = badRequestException.WrongMessage,
                Status = badRequestException.WrongCode
            });
        }
        
    }
    
    
    // PATCH /tables/1
    [HttpPatch]
    [Route("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult PatchTable(int id, JsonElement patch)
    {
        try
        {
            var updatedTable = _tableService.PatchTable(id, patch);
            if (updatedTable == null)
                return NotFound(new ErrorResponse { Message = "Table not found" });

            return Ok(updatedTable);

        }
        catch (ServerException badRequestException)
        {
            return BadRequest(new ErrorResponse
            {
                Message = badRequestException.WrongMessage,
                Status = badRequestException.WrongCode
            });
        }
    }
    
    
    // DELETE /tables/1
    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteTable(int id)
    {
        if (!_tableService.DeleteTable(id))
            return NotFound(new ErrorResponse { Message = "Table not found" });
        
        return NoContent();
    } 
}