using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.Service;
using Windows.ApplicationModel.Search;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Callisto.Controls;
using Windows.UI.Core;

namespace BetaSeriesW8
{
    public sealed partial class LoginPage : LayoutAwarePage
    {

        public LoginPage()
        {
            InitializeComponent();
            DefaultViewModel["EstConnecte"] = BetaSerieData.EstConnecte;
            SettingsPane.GetForCurrentView().CommandsRequested += SettingBS;
            VisibiliteGrille();
        }

        private void VisibiliteGrille()
        {
            if (BetaSerieData.EstConnecte)
            {
                if (ViewState == "Snapped")
                {
                    gridDeconnecteSnapped.Visibility = Visibility.Collapsed;
                    gridConnecteSnapped.Visibility = Visibility.Visible;
                }
                else
                {
                    gridDeconnecte.Visibility = Visibility.Collapsed;
                    gridConnecte.Visibility = Visibility.Visible;
                }

            }
            else
            {
                if (ViewState == "Snapped")
                {
                    gridDeconnecteSnapped.Visibility = Visibility.Visible;
                    gridConnecteSnapped.Visibility = Visibility.Collapsed;
                }
                else
                {
                    gridDeconnecte.Visibility = Visibility.Visible;
                    gridConnecte.Visibility = Visibility.Collapsed;
                }
            }
        }

        protected override string DetermineVisualState(Windows.UI.ViewManagement.ApplicationViewState viewState)
        {
            var layout = base.DetermineVisualState(viewState);
            if (layout == "Snapped")
            {
                if (BetaSerieData.EstConnecte)
                {
                    gridConnecteSnapped.Visibility = Visibility.Visible;
                    gridDeconnecteSnapped.Visibility = Visibility.Collapsed;
                }
                else
                {
                    gridConnecteSnapped.Visibility = Visibility.Collapsed;
                    gridDeconnecteSnapped.Visibility = Visibility.Visible;
                }
            }
            else
            {
                gridConnecteSnapped.Visibility = Visibility.Collapsed;
                gridDeconnecteSnapped.Visibility = Visibility.Collapsed;

                if (BetaSerieData.EstConnecte)
                {
                    gridConnecte.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    gridDeconnecte.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    gridConnecte.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    gridDeconnecte.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            return layout;

        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            liveTile2.ItemsSource = new List<Home>
                {
                    new Home("fringe", "http://thetvdb.com/banners/_cache/posters/82066-7.jpg"),
                    new Home("chuck", "http://thetvdb.com/banners/_cache/posters/80348-1.jpg"),
                    new Home("glee", "http://thetvdb.com/banners/_cache/posters/83610-17.jpg")
                    
                };

            liveTile3.ItemsSource = new List<Home>
                {
                    new Home("dexter", "http://thetvdb.com/banners/_cache/posters/79349-24.jpg"),
                    new Home("house", "http://thetvdb.com/banners/_cache/posters/73255-24.jpg"),
                    new Home("homeland", "http://thetvdb.com/banners/_cache/posters/247897-7.jpg")
                };

            liveTile4.ItemsSource = new List<Home>
                {
                    new Home("himym", "http://thetvdb.com/banners/_cache/posters/75760-1.jpg"),
                    new Home("greysanatomy", "http://thetvdb.com/banners/_cache/posters/73762-12.jpg"),
                    new Home("thementalist", "http://thetvdb.com/banners/_cache/posters/82459-6.jpg")
                };

            liveTile5.ItemsSource = new List<Home>
                {
                    new Home("trueblood", "http://thetvdb.com/banners/_cache/posters/82283-1.jpg"),
                    new Home("misfits", "http://thetvdb.com/banners/_cache/posters/124051-4.jpg"),
                    new Home("the-new-girl", "http://thetvdb.com/banners/_cache/posters/248682-9.jpg")
                };

            liveTile6.ItemsSource = new List<Home>
                {
                    new Home("bigbangtheory", "http://thetvdb.com/banners/_cache/posters/80379-13.jpg"),
                    new Home("californication", "http://thetvdb.com/banners/_cache/posters/80349-12.jpg"),
                    new Home("simpsons", "http://thetvdb.com/banners/_cache/posters/71663-10.jpg")
                };

            liveTile7.ItemsSource = new List<Home>
                {
                    new Home("gameofthrones", "http://thetvdb.com/banners/_cache/posters/121361-4.jpg"),
                    new Home("thewalkingdead", "http://thetvdb.com/banners/_cache/posters/153021-6.jpg"),
                    new Home("breakingbad", "http://thetvdb.com/banners/_cache/posters/81189-8.jpg")
                };


            stkLoading.Visibility = Visibility.Visible;
            textLoading.Text = "Récupération des informations pour le compte " + BetaSerieData.Login;

            await ChargerMesInformations();

            if (BetaSerieData.EstConnecte)
            {
                var mesSeriesIn = BetaSerieData.MesSeries;

                if (BetaSerieData.MesSeries == null)
                {
                    mesSeriesIn = new ObservableCollection<Serie>();
                    BetaSerieData.MesSeries = mesSeriesIn;

                    BetaSerieData.UiDispatcher = Dispatcher;

                    await ServicesBetaSeries.RecupererMesSeries(mesSeriesIn, textLoading);

                    textLoading.Text = string.Empty;
                    stkLoading.Visibility = Visibility.Collapsed;

                    var seriesNonArchives = mesSeriesIn.Where(x => !x.EstArchive).OrderByDescending(x => x.Note).ToList();
                    if (seriesNonArchives == null || seriesNonArchives.Count == 0 || !seriesNonArchives.Exists(x => x.BannieresFanArt != null && x.BannieresFanArt.Count > 0))
                        blocAnnonce.Visibility = Visibility.Visible;
                    else FlipView5.ItemsSource = seriesNonArchives.Where(x => x.BannieresFanArt != null && x.BannieresFanArt.Count > 0);

                    if (seriesNonArchives.Count > 0 && !seriesNonArchives.First().FondEnCoursDeTelechargement)
                        ServicesBetaSeries.RecupererLeFond(seriesNonArchives.First());

                    if (mesSeriesIn.Count == 0)
                        blocAnnonce.Visibility = Visibility.Visible;
                }
                else
                {
                    var seriesNonArchives = mesSeriesIn.Where(x => !x.EstArchive).OrderByDescending(x => x.Note).ToList();
                    if (seriesNonArchives == null || seriesNonArchives.Count == 0 || !seriesNonArchives.Exists(x => x.BannieresFanArt != null && x.BannieresFanArt.Count > 0))
                        blocAnnonce.Visibility = Visibility.Visible;
                    else FlipView5.ItemsSource = seriesNonArchives.Where(x => x.BannieresFanArt != null && x.BannieresFanArt.Count > 0);
                }

            }
            else blocAnnonce.Visibility = Visibility.Visible;
            stkLoading.Visibility = Visibility.Collapsed;
        }

        private async Task<bool> ChargerMesInformations()
        {
            if (BetaSerieData.EstConnecte)
            {
                if (BetaSerieData.PremiereSynchro)
                {
                    ServiceToast.Afficher("Bienvenue sur Mes Séries TV !");
                    BetaSerieData.PremiereSynchro = false;
                }
                DefaultViewModel["Utilisateur"] = await ServicesBetaSeries.RecupererMesInformations();
            }
            return true;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        public void Sincrire()
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;
            popupInscription.IsOpen = true;
        }

        private async void VoirLaSerie(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;
            Frame.Navigate(typeof(FicheSerie), ((Serie)FlipView5.SelectedItem).Url);
        }




        private void SettingBS(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            if (BetaSerieData.EstConnecte)
            {
                if (!args.Request.ApplicationCommands.ToList().Exists(x => x.Id == "deconnexion"))
                {
                    var cmd = new SettingsCommand("deconnexion", "Se déconnecter", x => SeDeconnecter_OnClick());
                    args.Request.ApplicationCommands.Add(cmd);
                }
            }
            else
            {
                if (!args.Request.ApplicationCommands.ToList().Exists(x => x.Id == "connexion"))
                {
                    var cmd = new SettingsCommand("connexion", "Se connecter", x => SeConnecter_OnClick());
                    args.Request.ApplicationCommands.Add(cmd);
                }
            }

            if (!args.Request.ApplicationCommands.ToList().Exists(x => x.Id == "Paramétrage"))
            {
                SettingsCommand command = new SettingsCommand("Paramétrage", "Paramétrage", e =>
                                                                                        {
                                                                                            SettingsFlyout flyout =
                                                                                                new SettingsFlyout();
                                                                                            flyout.FlyoutWidth =
                                                                                                SettingsFlyout
                                                                                                    .SettingsFlyoutWidth
                                                                                                    .Narrow;
                                                                                            flyout.HeaderText =
                                                                                                "Paramétrage";
                                                                                            flyout.Background =
                                                                                                new SolidColorBrush(
                                                                                                    Colors.White);
                                                                                            flyout.HeaderBrush =
                                                                                                new SolidColorBrush(
                                                                                                    Colors.Black);
                                                                                            flyout.Content =
                                                                                                new Parametrage();
                                                                                            flyout.IsOpen = true;
                                                                                        });
                args.Request.ApplicationCommands.Add(command);

            }

            if (!args.Request.ApplicationCommands.ToList().Exists(x => x.Id == "support"))
            {
                SettingsCommand command = new SettingsCommand("support", "Support", e =>
                {
                    SettingsFlyout flyout = new SettingsFlyout();
                    flyout.FlyoutWidth = SettingsFlyout.SettingsFlyoutWidth.Narrow;
                    flyout.HeaderText = "Support";
                    flyout.Background = new SolidColorBrush(Colors.White);
                    flyout.HeaderBrush = new SolidColorBrush(Colors.Black);
                    flyout.Content = new APropos();
                    flyout.IsOpen = true;
                });

                args.Request.ApplicationCommands.Add(command);
            }

            if (!args.Request.ApplicationCommands.ToList().Exists(x => x.Id == "confidentialite"))
            {
                SettingsCommand command = new SettingsCommand("confidentialite", "Confidentialité", e =>
                {
                    SettingsFlyout flyout = new SettingsFlyout();
                    flyout.FlyoutWidth = SettingsFlyout.SettingsFlyoutWidth.Narrow;
                    flyout.HeaderText = "Confidentialité";
                    flyout.Background = new SolidColorBrush(Colors.White);
                    flyout.HeaderBrush = new SolidColorBrush(Colors.Black);
                    flyout.Content = new Confidentialite();
                    flyout.IsOpen = true;
                });

                args.Request.ApplicationCommands.Add(command);
            }

            //var cmdAPropos = new SettingsCommand("apropos", "A propos de BetaSeries W8", x => AProposOnClick());
            //args.Request.ApplicationCommands.Add(cmdAPropos);
        }

        private object APropos()
        {
            throw new NotImplementedException();
        }

        private void AProposOnClick()
        {

        }

        private async void SeDeconnecter_OnClick()
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            await ServicesBetaSeries.SeDeconnecter();
            Cache.FaireExpirer(Cache.Utilisateur);
            Cache.FaireExpirer(Cache.Episodes);
            BetaSerieData.MesSeries = null;
            ServicesTiles.MettreAJourLesTilesEpisodes();
            Frame.Navigate(typeof(LoginPage));
        }

        private void SeConnecter_OnClick()
        {
            Frame.Navigate(typeof(LoginPage));
        }



        private void Parametrage_OnClick()
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private async void ValiderInscription(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BetaSerieData.VerificationConnexionInternet())
                    return;

                stkBtnInscriptionAnn.Visibility = Visibility.Collapsed;
                stkBtnInscriptionVal.Visibility = Visibility.Collapsed;
                progressInscription.Visibility = Visibility.Visible;
                progressInscription.IsActive = true;

                if (mdpInscription.Password != ConfirmationMdp.Password)
                    throw new Exception("Les mots de passe sont différents");

                if (!string.IsNullOrEmpty(nomUtilisateur.Text) && nomUtilisateur.Text.Length > 24)
                    throw new Exception("Le nom d'utilisateur ne peut pas dépasser 24 caractéres");

                await ServicesBetaSeries.Inscription(nomUtilisateur.Text, adresseMail.Text, mdpInscription.Password);
                DefaultViewModel["ErreurInscription"] = string.Empty;
                erreurInscription.Visibility = Visibility.Collapsed;
                ServiceToast.Afficher("Inscription validée ! Bienvenue sur Mes Séries TV !");
                popupInscription.IsOpen = false;
                await ServicesBetaSeries.Authentifier(nomUtilisateur.Text, SecuriteHelper.ComputeMd5(mdpInscription.Password));
                VisibiliteGrille();
            }
            catch (Exception ex)
            {
                DefaultViewModel["ErreurInscription"] = ex.Message;
                erreurInscription.Visibility = Visibility.Visible;
                stkBtnInscriptionAnn.Visibility = Visibility.Visible;
                stkBtnInscriptionVal.Visibility = Visibility.Visible;
                progressInscription.Visibility = Visibility.Collapsed;
                progressInscription.IsActive = false;
                VisibiliteGrille();
            }
        }

        private void btnSeConnecter_Click(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            popupConnexion.IsOpen = true;
            popupInscription.IsOpen = !popupConnexion.IsOpen;
        }

        private void btnSeConnecterSnapped_Click(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;
            popupConnexionSnapped.IsOpen = true;
            popupInscription.IsOpen = !popupConnexionSnapped.IsOpen;
        }



        private void btnSincrire_Click(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            popupInscription.IsOpen = true;
            popupConnexion.IsOpen = !popupInscription.IsOpen;
            popupConnexionSnapped.IsOpen = !popupInscription.IsOpen;
        }

        private async void SeConnecter(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;
            DefaultViewModel["ErreurLogin"] = string.Empty;
            erreurLogin.Visibility = Visibility.Collapsed;

            btnSeConnecter.Visibility = Visibility.Collapsed;
            progress1.Visibility = Visibility.Visible;

            progress1.IsActive = true;
            string motDePasse = SecuriteHelper.ComputeMd5(mdp.Password);
            string loginS = login.Text;

            try
            {
                await ServicesBetaSeries.Authentifier(loginS, motDePasse);

            }
            catch (Exception ex)
            {
                btnSeConnecter.Visibility = Visibility.Visible;
                progress1.Visibility = Visibility.Collapsed;
                progress1.IsActive = false;
                DefaultViewModel["ErreurLogin"] = ex.Message;
                erreurLogin.Visibility = Visibility.Visible;
                return;
            }

            DefaultViewModel["EstConnecte"] = BetaSerieData.EstConnecte;
            VisibiliteGrille();
            progress1.Visibility = Visibility.Collapsed;
            progress1.IsActive = false;
            popupConnexion.Visibility = Visibility.Collapsed;
            popupInscription.Visibility = Visibility.Collapsed;
            ChargerMesInformations();
            ServicesTiles.MettreAJourLesTilesEpisodes();
            Frame.Navigate(typeof(LoginPage));
        }

        private async void SeConnecterSnapped(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            DefaultViewModel["ErreurLogin"] = string.Empty;
            erreurLoginSnapped.Visibility = Visibility.Collapsed;

            btnSeConnecterSnapped.Visibility = Visibility.Collapsed;
            progress1Snapped.Visibility = Visibility.Visible;
            progress1Snapped.IsActive = true;
            string motDePasse = SecuriteHelper.ComputeMd5(mdpSnapped.Password);
            string loginS = loginSnapped.Text;

            try
            {
                await ServicesBetaSeries.Authentifier(loginS, motDePasse);
            }
            catch (Exception ex)
            {
                btnSeConnecterSnapped.Visibility = Visibility.Visible;
                progress1Snapped.Visibility = Visibility.Collapsed;
                progress1Snapped.IsActive = false;
                DefaultViewModel["ErreurLogin"] = ex.Message;
                erreurLoginSnapped.Visibility = Visibility.Visible;
                return;
            }

            DefaultViewModel["EstConnecte"] = BetaSerieData.EstConnecte;
            if (BetaSerieData.EstConnecte)
            {
                gridDeconnecteSnapped.Visibility = Visibility.Collapsed;
                gridConnecteSnapped.Visibility = Visibility.Visible;
            }
            else
            {
                gridDeconnecteSnapped.Visibility = Visibility.Visible;
                gridConnecteSnapped.Visibility = Visibility.Collapsed;
            }
            progress1Snapped.Visibility = Visibility.Collapsed;
            progress1Snapped.IsActive = false;
            popupConnexionSnapped.Visibility = Visibility.Collapsed;
            popupInscription.Visibility = Visibility.Collapsed;
            ChargerMesInformations();
        }

        private void BlocMesSeries_OnClick(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            this.Frame.Navigate(typeof(MesSeries), "MesSeries");
        }

        private void BtnAjouterUneSerie_OnClick(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;
            SearchPane forCurrentView = SearchPane.GetForCurrentView();
            forCurrentView.Show();
        }

        private void BtnMesEpisodes_OnClick(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;
            this.Frame.Navigate(typeof(MesEpisodes));
        }

        private void FermerLaPopup(object sender, RoutedEventArgs e)
        {
            popupConnexionSnapped.IsOpen = false;
        }

        private void AnnulerInscription(object sender, RoutedEventArgs e)
        {
            popupInscription.IsOpen = false;
        }

        private void btnMonPlanning_Click_1(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            this.Frame.Navigate(typeof(MonPlanning), "MesSeries");
        }

        private void FlipView5_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var serie = ((FlipView)sender).SelectedItem as Serie;
            if (serie != null && !serie.FondEnCoursDeTelechargement)
                ServicesBetaSeries.RecupererLeFond(serie);

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            this.Frame.Navigate(typeof(FicheSerie), btn.Tag);
        }
    }

    public class Home
    {
        public string Titre { get; set; }
        public string Image { get; set; }

        public Home(string dexter, string httpThetvdbComBannersCachePostersJpg)
        {
            Titre = dexter;
            Image = httpThetvdbComBannersCachePostersJpg;
        }
    }
}