using System;
using System.Threading;
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
				var credsPath = cfg["Firebase:CredentialsFile"] ?? "Secrets/firebase-admin.json";
				FirebaseApp.Create(new AppOptions
				{
					Credential = GoogleCredential.FromFile(credsPath)
				});
			}
		}

		public async Task SendToTopicAsync(string topic, string text, DateTime tsUtc, CancellationToken ct = default)
		{
			var data = new Dictionary<string, string>
			{
				["text"] = text,
				["ts"] = tsUtc.ToUniversalTime().ToString("o")
			};

			var msg = new Message
			{
				Topic = topic,
				Notification = new Notification { Title = "Aviso", Body = text },
				Data = data,
				Android = new AndroidConfig
				{
					Priority = Priority.High,
					Notification = new AndroidNotification { ChannelId = "limpiezas" }
				}
			};

			var id = await FirebaseMessaging.DefaultInstance.SendAsync(msg, ct);

			Console.WriteLine($"FCM OK id={id} topic={topic} text={text}");
		}
	}
}
