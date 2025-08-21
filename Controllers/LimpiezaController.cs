using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PalmOasis_Limpieza_Push.Services;
using PalmOasis_Limpieza_Push.Models;
using PalmOasis_Limpieza_Push.Models.DTOs;
using PalmOasis_Limpieza_Push.Models.Interfaces;

namespace PalmOasis_Limpieza_Push.Controllers
{
	[ApiController]
	[Route("api/limpieza")]
	public class LimpiezaController : ControllerBase 
	{
		private readonly LimpiezaContext _context;
		private readonly IPushService _push;

		public LimpiezaController(LimpiezaContext context, IPushService push)
		{
			_context = context;
			_push = push;
		}

		// GET: api/limpieza?take=50
		[HttpGet]
		public async Task<ActionResult<IEnumerable<EventoDto>>> GetFiltrados(
			[FromQuery] int take = 50,
			[FromQuery] DateTime? sinceUtc = null,
			CancellationToken ct = default)
		{
			take = take <= 0 ? 50 : Math.Min(take, 500);

			IQueryable<Evento> q = _context.Eventos.AsNoTracking();

			if (sinceUtc is not null)
			{
				var s = DateTime.SpecifyKind(sinceUtc.Value, DateTimeKind.Utc);
				q = q.Where(e => e.FechaHora >= s);
			}

			var lista = await q
				.OrderByDescending(e => e.FechaHora)
				.Take(take)
				.Select(e => new EventoDto
				{
					Codigo = e.Codigo,
					FechaHora = e.FechaHora,
					Usuario = e.Usuario,
					TextoEvento = e.TextoEvento
				})
				.ToListAsync(ct);	

			return Ok(lista);
		}

		// GET: api/limpieza/12391
		[HttpGet("{id:int}")]
		public async Task<ActionResult<EventoDto>> GetById(int id, CancellationToken ct)
		{
			var e = await _context.Eventos.AsNoTracking()
			.FirstOrDefaultAsync(e => e.Codigo == id, ct);

			if (e is null) return NotFound();

			return Ok(new EventoDto
			{
				Codigo = e.Codigo,
				FechaHora = e.FechaHora,
				Usuario = e.Usuario,
				TextoEvento = e.TextoEvento
			});
		}

		// POST: api/limpieza
		[HttpPost]
		public async Task<ActionResult<EventoDto>> CreateLimpiezaGeneral([FromBody] EventoCreateDto nuevo, CancellationToken ct)
		{
			var usuario = (nuevo.Usuario ?? string.Empty).Trim();
			if (usuario.Length > 50) usuario = usuario[..50];

			var texto = (nuevo.TextoEvento ?? string.Empty).Trim();
			if (texto.Length > 250) texto = texto[..250];

			var ts = (nuevo.FechaHora ?? DateTime.UtcNow.ToUniversalTime());

			var entidad = new Evento
			{
				FechaHora = ts,
				Usuario = usuario,
				TextoEvento = texto
			};
			_context.Eventos.Add(entidad);
			await _context.SaveChangesAsync(ct);

			var topic = string.IsNullOrWhiteSpace(nuevo.Topic) ? "limpieza-general" : nuevo.Topic!;
			await _push.SendToTopicAsync(topic, texto, ts, ct);

			var dto = new EventoDto
			{
				Codigo = entidad.Codigo,
				FechaHora = entidad.FechaHora,
				Usuario = entidad.Usuario,
				TextoEvento = entidad.TextoEvento
			};

			return CreatedAtAction(nameof(GetById), new { id = dto.Codigo }, dto);
		}

		// PUT: api/limpieza/485240
		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] EventoCreateDto actualizado, CancellationToken ct)
		{
			var e = await _context.Eventos.FirstOrDefaultAsync(x => x.Codigo == id, ct);
			if (e is null) return NotFound();

			var usuario = (actualizado.Usuario ?? string.Empty).Trim();
			if (usuario.Length > 50) usuario = usuario[..50];

			var texto = (actualizado.TextoEvento ?? string.Empty).Trim();
			if (texto.Length > 255) texto = texto[..255];

			e.Usuario = usuario;
			e.TextoEvento = texto;
			e.FechaHora = (actualizado.FechaHora ?? e.FechaHora).ToUniversalTime();

			await _context.SaveChangesAsync(ct);
			return NoContent();
		}

		// DELETE: api/limpieza/485240
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id, CancellationToken ct)
		{
			var e = await _context.Eventos.FirstOrDefaultAsync(x => x.Codigo == id, ct);
			if (e is null) return NotFound();

			_context.Eventos.Remove(e);
			await _context.SaveChangesAsync(ct);
			return NoContent();
		}

		// Healthcheck simple
		[HttpGet("ping")]
		public IActionResult Ping() => Ok("pong");
	}
}
