using Microsoft.EntityFrameworkCore;
using PalmOasis_Limpieza_Push.Models;

namespace PalmOasis_Limpieza_Push.Models;

public class LimpiezaContext : DbContext
{
	public LimpiezaContext(DbContextOptions<LimpiezaContext> options) : base(options) { }

	public DbSet<Evento> Eventos { get; set; } = null!;
}