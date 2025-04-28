using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using backend.Models.Entities;
using backend.Models.DTOs.Document;
using backend.Repositories.Interfaces;
using System.Collections.Generic;
using System;
using backend.Common;

namespace backend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]

  public class DocumentController : ControllerBase
  {
    private readonly IDocumentRepository _repository;

    public DocumentController(IDocumentRepository repository)
    {
      _repository = repository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
      var document = await _repository.GetById(id);
      if (document == null)
      {
        var errorResponse = new ApiResponse<string>("Document not found");
        return NotFound(errorResponse);
      }

      var response = new ApiResponse<Document>(document);
      return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments(
      [FromQuery] int? page, 
      [FromQuery] int? pageSize,
      [FromQuery] string? searchTerm,
      [FromQuery] string? orderBy,
      [FromQuery] string? order
    )
    {
      var documents = await _repository.GetDocuments(page, pageSize, searchTerm, orderBy, order);
      var response = new ApiResponse<IEnumerable<Document>>(documents);
      return Ok(response);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
      var document = await _repository.GetById(id);
      if (document == null)
      {
        var errorResponse = new ApiResponse<string>("File not found");
        return NotFound(errorResponse);
      }

      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
      var archivePath = Path.Combine(uploadsFolder, document.ArchiveName);

      if (!System.IO.File.Exists(archivePath))
      {
        var errorResponse = new ApiResponse<string>("File not found");
        return NotFound(errorResponse);
      }

      var bytes = System.IO.File.ReadAllBytes(archivePath);
      return File(bytes, "application/octet-stream", document.ArchiveName);
    }

    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Create([FromForm] DocumentUploadRequest request)
    {
      if (await _repository.ExistsByTitle(request.Title))
      {
        var errorResponse = new ApiResponse<string>("A document already is using this title.");
        return BadRequest(errorResponse);
      }

      if (request.Archive == null || request.Archive.Length == 0)
      {
        var errorResponse = new ApiResponse<string>("Invalid archive.");
        return BadRequest(errorResponse);
      }

      var extension = Path.GetExtension(request.Archive.FileName).ToLower();
      if (extension == ".exe" || extension == ".zip" || extension == ".bat")
      {
        var errorResponse = new ApiResponse<string>("Archive type not allowed.");
        return BadRequest(errorResponse);
      }

      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
      if (!Directory.Exists(uploadsFolder))
        Directory.CreateDirectory(uploadsFolder);

      var uniqArchiveName = Guid.NewGuid().ToString() + Path.GetExtension(request.Archive.FileName);
      var fullPath = Path.Combine(uploadsFolder, uniqArchiveName);

      using (var stream = new FileStream(fullPath, FileMode.Create))
      {
        await request.Archive.CopyToAsync(stream);
      }

      var document = new Document
      {
        Title = request.Title,
        Description = request.Description,
        ArchiveName = uniqArchiveName,
        CreationDate = DateTime.UtcNow
      };

      await _repository.Add(document);

      var response = new ApiResponse<Document>(document);
      return CreatedAtAction(nameof(GetById), new { id = document.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] DocumentUpdateRequest request)
    {
      var document = await _repository.GetById(id);
      if (document == null)
      {
        var errorResponse = new ApiResponse<string>("Document not found");
        return NotFound(errorResponse);
      }

      if (!string.IsNullOrWhiteSpace(request.Title))
        document.Title = request.Title;

      if (!string.IsNullOrWhiteSpace(request.Description))
        document.Description = request.Description;

      if (request.Archive != null && request.Archive.Length > 0)
      {
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        var oldPath = Path.Combine(uploadsFolder, document.ArchiveName);
        if (System.IO.File.Exists(oldPath))
            System.IO.File.Delete(oldPath);

        var newName = Guid.NewGuid().ToString() + Path.GetExtension(request.Archive.FileName);
        var newPath = Path.Combine(uploadsFolder, newName);

        using (var stream = new FileStream(newPath, FileMode.Create))
        {
            await request.Archive.CopyToAsync(stream);
        }

        document.ArchiveName = newName;
      }

      await _repository.Update(document);

      var response = new ApiResponse<Document>(document);
      return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
      var document = await _repository.GetById(id);
      if (document == null)
      {
        var errorResponse = new ApiResponse<string>("Document not found");
        return NotFound(errorResponse);
      }

      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
      var archivePath = Path.Combine(uploadsFolder, document.ArchiveName);

      if (System.IO.File.Exists(archivePath))
          System.IO.File.Delete(archivePath);

      await _repository.Delete(id);

      return NoContent();
    }
  }
}