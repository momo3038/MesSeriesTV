using System;
using System.Collections.Generic;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.Service;
using BetaSeriesW8.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BetaSeriesW8
{
    public sealed partial class FicheSerie : LayoutAwarePage
    {
        public FicheSerie()
        {
            InitializeComponent();
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
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            try
            {
                if (!BetaSerieData.VerificationConnexionInternet())
                    return;

                Serie serie;

                if (navigationParameter is string)
                {
                    serie = await ServicesBetaSeries.RecupererUneSerie((string)navigationParameter);
                }
                else serie = ((Serie)navigationParameter);
                DefaultViewModel["EstConnecte"] = BetaSerieData.EstConnecte;

                ChargerLaSerie(serie);
                serie.EstComplete = true;
            }
            catch (Exception exception)
            {
                BetaSerieData.Erreur(exception);
            }
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

        private async void ChargerLaSerie(Serie serie)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            var vmSerie = new ViewModelFicheSerie { Serie = serie };
            DefaultViewModel["Serie"] = vmSerie;

            serie = await ServicesBetaSeries.RecupererUneSerie(serie.Url, serie);

            FlipView5.Tag = serie;
            FlipView5.ItemsSource = serie.BannieresFanArt;

            if (ViewState == "Snapped")
            {
                gridSerie.Visibility = Visibility.Collapsed;
                gridSerie.Opacity = 0;
                snappedGrid.Visibility = Visibility.Visible;
                snappedGridTop.Visibility = Visibility.Visible;
            }
            else
            {
                gridSerie.Visibility = Visibility.Visible;
                gridSerie.Opacity = 1;
                snappedGrid.Visibility = Visibility.Collapsed;
                snappedGridTop.Visibility = Visibility.Collapsed;
            }

            progressFiche.Visibility = Visibility.Collapsed;
        }

        private async void AjouterAMesSeries_Click_1(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            var serieAAjouter = ((ViewModelFicheSerie)DefaultViewModel["Serie"]).Serie;
            BottomAppBar.IsOpen = false;
            TopAppBar.IsOpen = false;
            popupAjoutSerie.IsOpen = true;
            stkBtnAvanceSerie.Visibility = Visibility.Visible;
            selectionnerEpisodeEtSaison.Visibility = Visibility.Collapsed;
            await ServicesBetaSeries.AjouterAMesSeries(serieAAjouter);
            ServiceToast.Afficher(string.Format("La série {0} vient d'être ajoutée à vos Série !",
                                                (serieAAjouter).Titre));
        }

        private void Sinscrire_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void SeConnecter_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private async void ArchiverCetteSerie_Click_1(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            var serieAAjouter = ((ViewModelFicheSerie)DefaultViewModel["Serie"]).Serie;

            BottomAppBar.IsOpen = false;
            TopAppBar.IsOpen = false;

            ServicesBetaSeries.ArchiverDeMesSeries(serieAAjouter);
            ServiceToast.Afficher("La série " + serieAAjouter.Titre + " vient d'être archivée");
        }

        private async void DesarchiverCetteSerie_Click_1(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            var serieAAjouter = ((ViewModelFicheSerie)DefaultViewModel["Serie"]).Serie;

            BottomAppBar.IsOpen = false;
            TopAppBar.IsOpen = false;


            await ServicesBetaSeries.DesarchiverDeMesSeries(serieAAjouter);
            ServiceToast.Afficher("La série " + serieAAjouter.Titre + " vient d'être désarchivée");
        }

        private async void SupprimerCetteSerie_Click_1(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            var serieAAjouter = ((ViewModelFicheSerie)DefaultViewModel["Serie"]).Serie;
            BottomAppBar.IsOpen = false;
            TopAppBar.IsOpen = false;

            await ServicesBetaSeries.SupprimerDeMesSeries(serieAAjouter);
            ServiceToast.Afficher("La série "+serieAAjouter.Titre + " vient d'être supprimée");
        }

        private void FlipView5_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null)
                return;

            var serie = FlipView5.Tag as Serie;
            if (serie != null && !serie.FondEnCoursDeTelechargement)
            {
                var banniere = FlipView5.SelectedItem as BanniereTvDB;
                var index = serie.BannieresFanArt.IndexOf(banniere);
                if (index < serie.BannieresFanArt.Count && index > 0)
                {
                    ServicesBetaSeries.RecupererLeFond(serie, index);
                }
                else ServicesBetaSeries.RecupererLeFond(serie);
            }
        }

        private void PleinEcran_Click_1(object sender, RoutedEventArgs e)
        {
            if (gridSerie.Visibility == Visibility.Collapsed)
            {
                gridSerie.Visibility = Visibility.Visible;
                gridSerie.Opacity = 1;
                SortirPleinEcran.Visibility = Visibility.Collapsed;
                PleinEcran.Visibility = Visibility.Visible;

                SortirPleinEcranNoconnect.Visibility = Visibility.Collapsed;
                PleinEcransNoconnect.Visibility = Visibility.Visible;
            }

            else
            {
                gridSerie.Visibility = Visibility.Collapsed;
                gridSerie.Opacity = 0;
                SortirPleinEcran.Visibility = Visibility.Visible;
                PleinEcran.Visibility = Visibility.Collapsed;

                SortirPleinEcranNoconnect.Visibility = Visibility.Visible;
                PleinEcransNoconnect.Visibility = Visibility.Collapsed;
            }

            BottomAppBar.IsOpen = false;
            TopAppBar.IsOpen = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MesEpisodes));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MonPlanning));
        }

        private void MesSeries(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MesSeries));
        }

        private void GlobalAppBar_Opened_1(object sender, object e)
        {
            if (ViewState == "Snapped")
            {
                PleinEcran.Visibility = Visibility.Collapsed;
            }
        }

        private void DemarrerLaSerie(object sender, RoutedEventArgs e)
        {
            popupAjoutSerie.IsOpen = false;
        }

        private void SelectionnerUnEpisodeEtUneSaison(object sender, RoutedEventArgs e)
        {
            selectionnerEpisodeEtSaison.Visibility = Visibility.Visible;
            var serie = FlipView5.Tag as Serie;
            var listeSaison = new List<Saison>();
            for (int i = 1; i <= serie.NombreTotalSaisons; i++)
            {
                listeSaison.Add(new Saison(i));
            }
            cbxSaison.ItemsSource = listeSaison;
        }

        private async void CbxSaison_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var saison = cbxSaison.SelectedItem as Saison;
            if (saison == null)
                return;

            var serie = FlipView5.Tag as Serie;
            progressRingOkaySaison.Visibility = Visibility.Visible;
            var episodes = await ServicesBetaSeries.RecupererLesEpisodes(saison.Numero, serie.Url);
            cbxEpisode.ItemsSource = episodes;
            progressRingOkaySaison.Visibility = Visibility.Collapsed;
            selectionnerEpisode.Visibility = Visibility.Visible;
        }

        private void CbxEpisode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var episode = cbxEpisode.SelectedItem as Episode;
            if (episode == null)
                return;

            btnCestParti.Visibility = Visibility.Visible;
        }

        private async void SelectionnerUnEpisodeDeLaSerie(object sender, RoutedEventArgs e)
        {
            var episode = cbxEpisode.SelectedItem as Episode;
            if (episode == null)
                return;

            btnCestParti.Visibility = Visibility.Collapsed;
            progressRingOkay.Visibility = Visibility.Visible;

            await ServicesBetaSeries.MarquerUnEpisodeCommeVu(episode);

            progressRingOkay.Visibility = Visibility.Collapsed;
            popupAjoutSerie.IsOpen = false;
        }

        private void GererMesEpisodes(object sender, RoutedEventArgs e)
        {
            TopAppBar.IsOpen = false;
            BottomAppBar.IsOpen = false;
            stkBtnAvanceSerie.Visibility = Visibility.Collapsed;
            popupAjoutSerie.IsOpen = true;
            SelectionnerUnEpisodeEtUneSaison(null, null);
        }
    }

    public class Saison
    {
        public Saison(int numero)
        {
            Numero = numero;
        }
        public int Numero { get; set; }
        public string Titre { get { return "Saison " + Numero; } }
    }
}