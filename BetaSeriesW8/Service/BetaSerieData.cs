using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.DataModel;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace BetaSeriesW8.Service
{
    public class BetaSerieData
    {
        public static BackgroundDownloader downloader;
        private static ObservableCollection<Serie> _mesSeries;
        private static Utilisateur _utilisateur;

        public static BackgroundDownloader Downloader
        {
            get { return downloader ?? (downloader = new BackgroundDownloader()); }
        }



        public static bool VerificationConnexionInternet()
        {
            if (!BetaSerieData.IsNetworkConnected())
            {
                MessageDialog dialog = new MessageDialog("Nous sommes désolé mais nous n'avons détecté aucun connexion à Internet.");
                dialog.ShowAsync();
                return false;
            }
            return true;
        }

        public static bool IsNetworkConnected()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            IReadOnlyList<ConnectionProfile> connectionProfile = NetworkInformation.GetConnectionProfiles();
            if (InternetConnectionProfile == null)
                return false;
            else
                return true;
        }

        public static string Token
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Token"))
                    return ApplicationData.Current.LocalSettings.Values["Token"] as string;
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    ApplicationData.Current.LocalSettings.Values["Token"] = value;
            }
        }

        public static string Login
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Login"))
                    return ApplicationData.Current.LocalSettings.Values["Login"] as string;
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    ApplicationData.Current.LocalSettings.Values["Login"] = value;
            }
        }

        public static bool EstConnecte
        {
            get
            {
                return !string.IsNullOrEmpty(Token);
            }
        }

        public static bool TacheRecuperationMesSeriesEnCours { get; set; }

        public static void SeDeconnecter()
        {
            ApplicationData.Current.LocalSettings.Values.Remove("Token");
            ApplicationData.Current.LocalSettings.Values.Remove("Login");
        }

        public static bool Erreur(Exception ex)
        {
            MessageDialog dialog = new MessageDialog("Une erreur est survenue ... Le développeur de cette application n'a semble t-il pas fait correctement son boulot ... Si vous avez quelques minutes, n'hesitez pas à le contacter (Selection A propos dans les options de l'application) pour lui expliquer comment reproduire cette erreur ! Merci. " + Environment.NewLine + " Signé : Le Développeur. " + Environment.NewLine + Environment.NewLine + " Détail de l'erreur : " + ex.Message)
                {
                    Title = "Aie Aie Aie !"
                };
            dialog.ShowAsync();
            return false;
        }

        public static Windows.UI.Core.CoreDispatcher UiDispatcher { get; set; }

        public static System.Threading.Tasks.TaskScheduler TaskSceduler { get; set; }

        public static bool PremiereSynchro
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("PremiereSynchro"))
                    return bool.Parse(ApplicationData.Current.LocalSettings.Values["PremiereSynchro"].ToString());
                return true;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["PremiereSynchro"] = value;
            }
        }

        public static ObservableCollection<Serie> MesSeries
        {
            get
            {
                return _mesSeries;
            }
            set { _mesSeries = value; }
        }

        public static Utilisateur Utilisateur
        {
            get
            {
                return _utilisateur;
            }
            set { _utilisateur = value; }
        }

        public static void Message(string message, LayoutAwarePage page)
        {
            var dialog = new MessageDialog(message) {Title = "Patience !"};
            dialog.Commands.Add(new UICommand("Ok, je vais patienter", command => page.Frame.Navigate(typeof(LoginPage))));
            dialog.ShowAsync();
        }
    }
}
