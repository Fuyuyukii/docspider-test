using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace backend.Models.DTOs.Document
{
  public class DocumentUploadRequest
  {
    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    [Required]
    [StringLength(2000)]
    public string Description { get; set; }

    [Required]
    public IFormFile Archive { get; set; } 
  }
}
