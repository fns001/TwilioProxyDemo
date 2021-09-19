using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwilioProxyDemo.Models;

namespace TwilioProxyDemo.Services
{
    /// <summary>
    /// How it works:
    /// 1. This service is injected in relevant Pages and Controllers
    /// 2. The index page registers an event listener called OnShowAlert
    /// 3. API controllers and other services call CreateAlertByStatus or ShowAlert to invoke the OnShowAlert event.
    /// </summary>
    public class ProxyAlertService
    {
        public Dictionary<string, string> Participants = new Dictionary<string, string>();

        public event Action<ProxyAlertArgs> OnShowAlert;

        public void CreateProxyAlert(string message)
        {
            OnShowAlert.Invoke(new ProxyAlertArgs
            {
                Message = message,
                Description = "test",
                Color = Blazorise.Color.Info
            });
        }

        public void CreateAlertByStatus(string MessageStatus)
        {
            Blazorise.Color alertColor = MessageStatus switch
            {
                "delivered" => Blazorise.Color.Success,
                "failed" => Blazorise.Color.Danger,
                _ => Blazorise.Color.Info
            };

            bool success = MessageStatus == "delivered";

            string message = success ? "Success!" : "";
            string description = $"SMS {MessageStatus}";
            ShowAlert($"{message}", $"{description}", alertColor);
        }

        public void ShowAlert(string message, string description, Blazorise.Color color)
        {
            OnShowAlert.Invoke(new ProxyAlertArgs
            {
                Message = message,
                Description = description,
                Color = color
            });
        }
    }
}
