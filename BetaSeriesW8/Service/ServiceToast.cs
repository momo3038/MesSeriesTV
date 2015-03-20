using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BetaSeriesW8.Service
{
    public static class ServiceToast
    {
        public static void Afficher(string text, bool? toastLong = false)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            XmlNodeList texts = toastXml.GetElementsByTagName("text");
            texts[0].InnerText = text;
            var toast = new ToastNotification(toastXml);
            ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
            toastNotifier.Show(toast);
        }
    }
}