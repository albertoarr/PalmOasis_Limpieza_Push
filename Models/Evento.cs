using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PalmOasis_Limpieza_Push.Models
{
[Table("COM_Eventos")]
	public class Evento
	{
		[Key]
		public int Codigo { get; set; }

		[Required]
		public DateTime FechaHora { get; set; }

		[MaxLength(50)]
		public string Usuario { get; set; } = string.Empty;

		[MaxLength(255)]
		public string TextoEvento { get; set; } = string.Empty;
	}
}