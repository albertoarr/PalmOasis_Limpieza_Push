using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PalmOasis_Limpieza_Push.Models.Interfaces;

namespace PalmOasis_Limpieza_Push.Services
{
    public class PushService : IPushService
    {
        public PushService(IConfiguration cfg, IWebHostEnvironment env)
        {
            if (FirebaseApp.DefaultInstance is null)
            {
                // Ruta relativa desde configuración o valor por defecto
                var relativePath = cfg["Firebase:CredentialsFile"];

                // La convertimos en ruta absoluta
                var credsPath = Path.Combine(env.ContentRootPath, relativePath!);

                // (Opcional) Comprobar que existe y loguear algo útil
                if (!File.Exists(credsPath))
                {
                    throw new FileNotFoundException($"No se encuentra el fichero de credenciales de Firebase en: {credsPath}");
                }

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
