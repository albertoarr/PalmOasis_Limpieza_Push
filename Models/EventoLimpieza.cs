using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PalmOasis_Limpieza_Push.Models
{
[Table("COM_Eventos")]
	public class EventoLimpieza
	{
		[Key]
		[Column("CodigoId")]                 
		public long CodigoId { get; set; }    // bigint -> long (Int64)

		[Column("FechaHora")]
		public DateTime FechaHora { get; set; }  // en DB permite NULL

		[Column("Usuario")]
		[MaxLength(50)]
		public string? Usuario { get; set; }      // en DB permite NULL

		[Column("Evento")]
		[MaxLength(255)]
		[Required]                                // en DB NOT NULL
		public string Evento { get; set; } = string.Empty;
	}
}