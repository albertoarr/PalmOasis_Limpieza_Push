using Microsoft.EntityFrameworkCore;

namespace PalmOasis_Limpieza_Push.Models;

public class LimpiezaContext : DbContext
{
	public LimpiezaContext(DbContextOptions<LimpiezaContext> options) : base(options) { }

	public DbSet<EventoLimpieza> EventosLimpieza { get; set; } = null!;



}