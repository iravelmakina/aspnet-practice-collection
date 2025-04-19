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
    private readonly ILogger<TableController> _logger;

    public TableController(ITableService tableService, ILogger<TableController> logger)
    {
        _tableService = tableService;
        _logger = logger;
    }


    //GET /tables (with pagination)
    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    [SwaggerHeader("If-None-Match", "ETag", false, "string")]
    [ServiceFilter(typeof(ETagFilter))]
    public IActionResult GetTables(int page = 1, int size = 10)
    {
        _logger.LogInformation("Fetching {PageSize} tables for page {Page}", size, page);
        
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
        
        _logger.LogInformation("Fetched {Count} tables for page {Page}", tables.TotalItems, page);

        return Ok(response);
    }


    // GET /tables/1
    [HttpGet]
    [Route("{id:int}")]
    [Authorize(Roles = "User, Admin")]
    [SwaggerHeader("If-None-Match", "ETag", false, "string")]
    [ServiceFilter(typeof(ETagFilter))]
    public IActionResult GetTable(int id)
    {
        _logger.LogInformation("Fetching table with ID={Id}", id);
        
        var table = _tableService.GetTable(id);
        if (table == null)
            return NotFound(new ErrorResponse { Message = "Table not found" });

        _logger.LogInformation("Fetched table with ID={Id}", id);
        
        return Ok(table);
    }


    // GET /tables/filter?capacity=4
    [HttpGet]
    [Route("filter")]
    [Authorize(Roles = "User, Admin")]
    [SwaggerHeader("If-None-Match", "ETag", false, "string")]
    [ServiceFilter(typeof(ETagFilter))] 
    public IActionResult GetTablesByCapacity(int capacity)
    {
        _logger.LogInformation("Fetching tables with capacity={Capacity}", capacity);
        
        var tables = _tableService.GetTablesByCapacity(capacity);
        if (tables == null)
            return NotFound(new ErrorResponse { Message = "Table not found" });
        
        _logger.LogInformation("Fetched {Count} tables with capacity={Capacity}", tables.Count, capacity);

        return Ok(tables);
    }


    // POST /tables
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateTable(Table table)
    {
        _logger.LogInformation("Creating table No{TableNumber}", table.Number);
        
        var newTable = _tableService.CreateTable(table);
        if (newTable == null)
            return StatusCode(403, new { error = "Table creation is currently disabled." });

        _logger.LogInformation("Created table No{TableNumber}", newTable.Item2.Number);
        
        return Created($"/tables/{newTable.Item1}", table);
    }


    // PUT /tables/1
    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateTable(int id, Table table)
    {
        _logger.LogInformation("Updating table No{TableNumber} with ID={Id}", table.Number, id);

       var updatedTable = _tableService.UpdateTable(id, table);
        if (updatedTable == null)
            return NotFound(new ErrorResponse { Message = "Table not found" });

        _logger.LogInformation("Updated table No{TableNumber} with ID={Id}", updatedTable.Number, id);
        
        return Ok(updatedTable);
    }


    // PATCH /tables/1
    [HttpPatch]
    [Route("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult PatchTable(int id, JsonElement patch)
    {
        _logger.LogInformation("Patching table with ID={Id}", id);
        
        var updatedTable = _tableService.PatchTable(id, patch);
        if (updatedTable == null)
            return NotFound(new ErrorResponse { Message = "Table not found" });
        
        _logger.LogInformation("Patched table with ID={Id}", id);
        
        return Ok(updatedTable);
    }


    // DELETE /tables/1
    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteTable(int id)
    {
        _logger.LogInformation("Deleting table with ID {Id}", id);
        
        if (!_tableService.DeleteTable(id))
            return NotFound(new ErrorResponse { Message = "Table not found" });

        _logger.LogInformation("Deleted table with ID {Id}", id);
        
        return NoContent();
    }
}