using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace backend.Models.DTOs.Document
{
  public class DocumentUpdateRequest
  {
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title can have at most 100 characters.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description can have at most 2000 characters.")]
    public string Description { get; set; }

    public IFormFile Archive { get; set; }
  }
}
