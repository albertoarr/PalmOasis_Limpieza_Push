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
		public async Task<ActionResult<IEnumerable<EventoLimpiezaDto>>> GetFiltrados(
			[FromQuery] int take = 50,
			[FromQuery] DateTime? sinceUtc = null,
			CancellationToken ct = default)
		{
			take = take <= 0 ? 50 : Math.Min(take, 500);

			IQueryable<EventoLimpieza> q = _context.EventosLimpieza.AsNoTracking();

			if (sinceUtc is not null)
			{
				var s = DateTime.SpecifyKind(sinceUtc.Value, DateTimeKind.Utc);
				q = q.Where(e => e.FechaHora >= s);
			}

			var lista = await q
				.OrderByDescending(e => e.FechaHora)
				.Take(take)
				.Select(e => new EventoLimpiezaDto
				{
					CodigoId = e.CodigoId,
					FechaHora = e.FechaHora,
					Usuario = e.Usuario == null ? null : e.Usuario.TrimEnd(),
					Evento = e.Evento
				})
				.ToListAsync(ct);	

			return Ok(lista);
		}

		// POST: api/limpieza
		[HttpPost]
		public async Task<ActionResult<EventoLimpiezaDto>> CreateLimpiezaGeneral([FromBody] EventoLimpiezaCreateDto nuevo, CancellationToken ct)
		{
			var usuario = (nuevo.Usuario ?? string.Empty).Trim();
			if (usuario.Length > 50) usuario = usuario[..50];

			var texto = (nuevo.Evento ?? string.Empty).Trim();
			if (texto.Length > 250) texto = texto[..250];

			var ts = (nuevo.FechaHora ?? DateTime.UtcNow.ToUniversalTime());

			var entidad = new EventoLimpieza
			{
				FechaHora = ts,
				Usuario = usuario,
				Evento = texto
			};
			_context.EventosLimpieza.Add(entidad);
			await _context.SaveChangesAsync(ct);

			var topic = string.IsNullOrWhiteSpace(nuevo.Topic) ? "limpieza-general" : nuevo.Topic!;
			await _push.SendToTopicAsync(topic, texto, ts, ct);

			var dto = new EventoLimpiezaDto
			{
				CodigoId = entidad.CodigoId,
				FechaHora = entidad.FechaHora,
				Usuario = entidad.Usuario,
				Evento = entidad.Evento
			};

			return CreatedAtAction(nameof(GetById), new { id = dto.CodigoId }, dto);
		}

		// Healthcheck simple
		[HttpGet("ping")]
		public IActionResult Ping() => Ok("pong");
	}
}
