using backend.Models.Entities;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;

namespace backend.Repositories.Implementations
{
  public class DocumentRepository : IDocumentRepository
  {
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<Document>> GetAll()
    {
      return await _context.Documents.ToListAsync();
    }

    public async Task<Document> GetById(int id)
    {
      return await _context.Documents.FindAsync(id);
    }

    public async Task Add(Document document)
    {
      await _context.Documents.AddAsync(document);
      await _context.SaveChangesAsync();
    }

    public async Task Update(Document document)
    {
      _context.Documents.Update(document);
      await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
      var document = await _context.Documents.FindAsync(id);
      if (document != null)
      {
        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
      }
    }

    public async Task<bool> ExistsByTitle(string title)
    {
      return await _context.Documents.AnyAsync(d => d.Title == title);
    }

    public async Task<IEnumerable<Document>> GetDocuments(int? page, int? pageSize, string? searchTerm, string? orderBy, string? order)
    {
      var query = _context.Documents.AsQueryable();

      if (!string.IsNullOrWhiteSpace(searchTerm)) query = ApplyFilter(query, searchTerm);

      if (!string.IsNullOrWhiteSpace(orderBy) && !string.IsNullOrWhiteSpace(order)) {
        query = ApplyOrdering(query, orderBy, order);
      } else if (
        (string.IsNullOrWhiteSpace(orderBy) && !string.IsNullOrWhiteSpace(order)) ||
        (!string.IsNullOrWhiteSpace(orderBy) && string.IsNullOrWhiteSpace(order))
      ) throw new ArgumentException("Both 'orderBy' and 'order' must be provided together. Please ensure that both are passed to order.");

      if (page.HasValue && pageSize.HasValue) {
        query = ApplyPagination(query, page.Value, pageSize.Value);
      } else if (
          (page.HasValue && !pageSize.HasValue) ||
          (!page.HasValue && pageSize.HasValue)
      ) {
          throw new ArgumentException("Both 'page' and 'pageSize' must be provided together. Please ensure that both are passed to use pagination.");
      }

      return await query.ToListAsync();
    }

    private IQueryable<Document> ApplyFilter(IQueryable<Document> query, string searchTerm)
    {
      if (query == null) throw new ArgumentNullException(nameof(query));

      if (!string.IsNullOrWhiteSpace(searchTerm))
      {
          query = query.Where(d =>
              d.Title.Contains(searchTerm) ||
              d.Description.Contains(searchTerm) ||
              d.ArchiveName.Contains(searchTerm)
          );
      }

      return query;
    }

    private IQueryable<Document> ApplyOrdering(IQueryable<Document> query, string orderBy, string order)
    { 
      if (string.IsNullOrWhiteSpace(orderBy) || string.IsNullOrWhiteSpace(order))
      {
        throw new ArgumentException("Both 'orderBy' and 'order' parameters must be provided.");
      }

      bool ascending = order.ToLower() == "asc";

      switch (orderBy.ToLower())
      {
        case "title":
          query = ascending ? query.OrderBy(d => d.Title) : query.OrderByDescending(d => d.Title);
          break;

        case "description":
          query = ascending ? query.OrderBy(d => d.Description) : query.OrderByDescending(d => d.Description);
          break;

        case "archivename":
          query = ascending ? query.OrderBy(d => d.ArchiveName) : query.OrderByDescending(d => d.ArchiveName);
          break;

        case "creationdate":
          query = ascending ? query.OrderBy(d => d.CreationDate) : query.OrderByDescending(d => d.CreationDate);
          break;

        default:
          query = ascending ? query.OrderBy(d => d.CreationDate) : query.OrderByDescending(d => d.CreationDate);
          break;
      }

      return query;
    }


    private IQueryable<Document> ApplyPagination(IQueryable<Document> query, int page = 1, int pageSize = 10)
    { 
      if (query == null) throw new ArgumentNullException(nameof(query));

      return query
        .Skip((page - 1) * pageSize)
        .Take(pageSize);
    }

  }
}