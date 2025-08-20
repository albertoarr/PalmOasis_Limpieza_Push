using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using PalmOasis_Limpieza_Push.Models.Interfaces;

namespace PalmOasis_Limpieza_Push.Services
{
	public class PushService : IPushService
	{
		public PushService(IConfiguration cfg) 
		{
			if (FirebaseApp.DefaultInstance is null)
			{
				var credsPath = cfg["Firebase:CredentialsFile"];
				FirebaseApp.Create(new AppOptions
				{
					Credential = GoogleCredential.FromFile(credsPath)
				});
			}
		}


		public Task SendRoomCleanedToTopicAsync(string topic, object dto, CancellationToken ct = default) {
			var cadena = dto.GetType().GetProperty("Cadena")?.GetValue(dto)?.ToString() ?? "";
			var ts = dto.GetType().GetProperty("TimestampUtc")?.GetValue(dto)?.ToString() ?? "";

			var msg = new Message
			{
				Topic = topic,

				Notification = new Notification
				{
					Title = "Limpieza",
					Body = cadena
				},

				Data = new Dictionary<string, string>
				{
					["text"] = cadena,
					["ts"] = ts,
					["type"] = "room_cleaned"
				},

				Android = new AndroidConfig
				{
					Priority = Priority.High,
					Notification = new AndroidNotification { ChannelId = "limpiezas" }
				}
			};

			return FirebaseMessaging.DefaultInstance.SendAsync(msg, ct);
		}
	}
}
