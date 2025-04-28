using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities
{
  	public class Document
	{
		[Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Title { get; set; }

		[Required]
		[StringLength(2000)]
		public string Description { get; set; }

		[Required]
		public string ArchiveName { get; set; } // Nome do arquivo salvo
		
		public DateTime CreationDate { get; set; }
    }
}
