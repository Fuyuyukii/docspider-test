using backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Repositories.Interfaces
{
  public interface IDocumentRepository
  {
    Task<IEnumerable<Document>> GetAll();
    Task<Document> GetById(int id);
    Task Add(Document document);
    Task Update(Document document);
    Task Delete(int id);
    Task<bool> ExistsByTitle(string title);
    Task<IEnumerable<Document>> GetDocuments(int? page, int? pageSize, string? searchTerm, string? orderBy, string? order);
  }
}
