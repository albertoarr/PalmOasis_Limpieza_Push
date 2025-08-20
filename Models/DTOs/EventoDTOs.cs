namespace PalmOasis_Limpieza_Push.Models.DTOs
{
	public class EventoCreateDto
	{
		public DateTime? FechaHora { get; set; }        // Si no viene se usa UtcNow
		public string TextoEvento { get; set; } = string.Empty; // Texto de evento
		public string Usuario { get; set; } = string.Empty;
		public string? Topic { get; set; }               // p.ej. "planta-3", "recepcion"…
	}

	public class EventoDto
	{
		public int Codigo { get; set; }
		public DateTime FechaHora { get; set; }
		public string? Usuario { get; set; }
		public string? TextoEvento { get; set; }
	}
}
