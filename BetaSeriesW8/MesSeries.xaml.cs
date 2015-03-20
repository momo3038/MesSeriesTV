using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.DataModel;
using BetaSeriesW8.Service;
using Windows.ApplicationModel.Search;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BetaSeriesW8
{

    public class VariableGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var viewModel = item as IResizable;


            element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
            element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);

            base.PrepareContainerForItemOverride(element, item);
        }
    }
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class MesSeries : LayoutAwarePage
    {
        private readonly ObservableCollection<GroupeDeSerie> _itemGroups = new ObservableCollection<GroupeDeSerie>();
        private Serie DroppedSerie;
        private GroupeDeSerie GroupeSerie;
        private GroupeDeSerie GroupeSerieArchive;


        public MesSeries()
        {
            InitializeComponent();
        }

        protected override async void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            var mesSeriesIn = BetaSerieData.MesSeries;

            if (BetaSerieData.MesSeries == null)
            {
                mesSeriesIn = new ObservableCollection<Serie>();
                BetaSerieData.MesSeries = mesSeriesIn;
                await ServicesBetaSeries.RecupererMesSeries(mesSeriesIn);
            }

            if (mesSeriesIn == null)
                return;

            foreach (var serie in mesSeriesIn)
                ServicesBetaSeries.RecupererLeFond(serie, type: "poster");



            if (mesSeriesIn.Count == 0)
            {
                stckSansSerie.Visibility = Visibility.Visible;
                itemGridScrollViewer.Visibility = Visibility.Collapsed;
                progressSeries.Visibility = Visibility.Collapsed;
            }
            else
            {
                //IList<Serie> seriesAvecEpisodesNonVus =
                //    await ServicesBetaSeries.MesSeriesPossedantDesEpisodesNonVus(mesSeriesIn);

                DefaultViewModel["GroupesDeSeries"] = _itemGroups;


                //if (seriesAvecEpisodesNonVus.Count > 0)
                //    _itemGroups.Add(new GroupeDeSerie("Series avec épisodes non vus", seriesAvecEpisodesNonVus));

                //IList<Serie> seriesEnCoursSansEpisodes =
                //    ServicesBetaSeries.MesSeriesEnCoursSansEpisodes(mesSeriesIn);
                //if (seriesEnCoursSansEpisodes.Count > 0)

                var mesSeries = ServicesBetaSeries.MesSeriesNonArchives(mesSeriesIn);
                if (mesSeries.Count > 0)
                    _itemGroups.Add(new GroupeDeSerie("Mes Séries", mesSeries));

                IList<Serie> seriesArchives = ServicesBetaSeries.MesSeriesArchives(mesSeriesIn);
                if (seriesArchives.Count > 0)
                    _itemGroups.Add(new GroupeDeSerie("Mes Séries archivées", seriesArchives));

                //GroupeSerieAvecEpisodesnonVus = ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).SingleOrDefault(x => x.TitreGroupe == "Series avec épisodes non vus");
                GroupeSerie = ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).SingleOrDefault(x => x.TitreGroupe == "Mes Séries");
                GroupeSerieArchive = ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).SingleOrDefault(x => x.TitreGroupe == "Mes Séries archivées");

                //RechercherPosters(mesSeries);

                progressSeries.Visibility = Visibility.Collapsed;
            }
        }


        public static async Task RechercherPosters(IList<Serie> series)
        {
            if (BetaSerieData.TacheRecuperationMesSeriesEnCours)
                return;

            var v = new TaskFactory();
            var queue = new Queue<Task>();

            var seriesAtraiter = series.Where(x => x.IsLoading);

            foreach (var series1 in seriesAtraiter)
            {
                queue.Enqueue(RechercherPoster(series1));
            }

            if (queue.Count > 0)
                BetaSerieData.TacheRecuperationMesSeriesEnCours = true;

            v.ContinueWhenAll(queue.ToArray(), completedTask =>
            {
                Cache.Ajouter(series.ToList());
                Cache.AEteMisAJour(Cache.Series);
                BetaSerieData.TacheRecuperationMesSeriesEnCours = false;
            });
        }

        public static async Task RechercherPoster(Serie serie, int? increment = null)
        {
            string idTvDb = await ApplicationBetaSeries.RecupererIdTvDB(serie.Url);
            serie.TvdbId = int.Parse(idTvDb);
            try
            {
                var bannieres = await ServicesTvDb.RecupererLesBannieres(serie.TvdbId, serie.Url);
                serie.Bannieres = bannieres.ToList();
                var ban = bannieres.Where(x => x.Type == "poster").OrderByDescending(x => x.Rating);
                if (ban != null && ban.Any())
                {
                    var uri = await BitMap.GetLocalImageAsync(ban.First().Path, serie.Url + "_Poster");
                    serie.BanniereString = uri.AbsoluteUri;
                }
                serie.IsLoading = false;
            }
            catch (Exception ex)
            {
            }

        }



        private void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            var serie = e.ClickedItem as Serie;
            Frame.Navigate(typeof(FicheSerie), serie);
        }

        private async void Rafraichir_OnClick(object sender, RoutedEventArgs e)
        {
            Cache.FaireExpirer(Cache.Series);
            Cache.FaireExpirer(Cache.Episodes);
            BetaSerieData.MesSeries = null;
            Frame.Navigate(typeof(MesSeries));
        }

        private void ajouterUneSerie_Click_1(object sender, RoutedEventArgs e)
        {
            SearchPane forCurrentView = SearchPane.GetForCurrentView();
            forCurrentView.Show();
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

        private void FilterBySerieName_OnClick(object sender, RoutedEventArgs e)
        {
            var mesSeries = ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).First().Series;
            _itemGroups.RemoveAt(0);
            _itemGroups.Insert(0, (new GroupeDeSerie("Series avec épisodes non vus", mesSeries.OrderBy(x => x.Titre).ToList())));
            DefaultViewModel["GroupesDeSeries"] = _itemGroups;
            GlobalAppBar.IsOpen = false;
            filtresEpisodes.IsOpen = false;
            TopAppBar.IsOpen = false;
        }

        private void FilterByDate_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void FilterBy_OnClick(object sender, RoutedEventArgs e)
        {
            filtresEpisodes.IsOpen = !filtresEpisodes.IsOpen;
        }

        private void ItemGridView_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var serieEnMvt = e.Items[0] as Serie;
            if (serieEnMvt != null)
                DroppedSerie = serieEnMvt;

            if (DroppedSerie.EstArchive)
            {
                stkArchiverLaSerie.Visibility = Visibility.Collapsed;
                stkDesarchiverLaSerie.Visibility = Visibility.Visible;
            }
            else
            {
                stkArchiverLaSerie.Visibility = Visibility.Visible;
                stkDesarchiverLaSerie.Visibility = Visibility.Collapsed;
            }
            actionSurSerie.Visibility = Visibility.Visible;
        }

        private async void ArchiverLaSerie_OnDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (!BetaSerieData.VerificationConnexionInternet())
                    return;

                if (DroppedSerie == null)
                    return;

                var series = RecupererGroupeDeSerie(DroppedSerie);

                actionSurSerie.Visibility = Visibility.Collapsed;

                if (GroupeSerieArchive == null || !((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).Contains(GroupeSerieArchive))
                {
                    ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).Add(new GroupeDeSerie("Mes Séries archivées", new List<Serie>()));
                    GroupeSerieArchive = ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).SingleOrDefault(x => x.TitreGroupe == "Mes Séries archivées");
                }

                if (series != null && GroupeSerieArchive != null)
                {
                    series.Remove(DroppedSerie);
                    GroupeSerieArchive.Series.Add(DroppedSerie);

                    if (GroupeSerie != null && GroupeSerie.Series.Count == 0)
                        ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).Remove(GroupeSerie);
                }
                await ServicesBetaSeries.ArchiverDeMesSeries(DroppedSerie);
                ServiceToast.Afficher("La série " + DroppedSerie.Titre + " vient d'être archivée");
            }
            catch (Exception ex)
            {
                BetaSerieData.Erreur(ex);
            }

        }

        private async void DesarchiverLaSerie_OnDrop(object sender, DragEventArgs e)
        {
            try
            {

                if (!BetaSerieData.VerificationConnexionInternet())
                    return;

                if (DroppedSerie == null)
                    return;

                var series = RecupererGroupeDeSerie(DroppedSerie);
                actionSurSerie.Visibility = Visibility.Collapsed;


                if (GroupeSerie == null || !((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).Contains(GroupeSerie))
                {
                    ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).Add(new GroupeDeSerie("Mes Séries", new List<Serie>()));
                    GroupeSerie = ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).SingleOrDefault(x => x.TitreGroupe == "Mes Séries");
                }

                if (series != null && GroupeSerie != null)
                {
                    GroupeSerieArchive.Series.Remove(DroppedSerie);
                    GroupeSerie.Series.Add(DroppedSerie);

                    if (GroupeSerieArchive != null && GroupeSerieArchive.Series.Count == 0)
                        ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).Remove(
                            GroupeSerieArchive);
                }
                await ServicesBetaSeries.DesarchiverDeMesSeries(DroppedSerie);
                ServiceToast.Afficher("La série " + DroppedSerie.Titre + " vient d'être désarchivée");
            }
            catch (Exception ex)
            {
                BetaSerieData.Erreur(ex);
            }
        }

        private ObservableCollection<Serie> RecupererGroupeDeSerie(Serie serie)
        {

            var estSerieenCours = serie.PossedeDesEpisodesNonVus;
            var estSerieEnPause = !serie.PossedeDesEpisodesNonVus;
            var estSerieArchive = serie.EstArchive;

            ObservableCollection<Serie> series = null;
            if (estSerieArchive && GroupeSerieArchive != null)
            {
                series = GroupeSerieArchive.Series;
            }
            else if (estSerieEnPause && GroupeSerie != null)
            {
                series = GroupeSerie.Series;
            }

            return series;
        }

        private void ArchiverLaSerie_OnDragEnter(object sender, DragEventArgs e)
        {
            archiverLaSerie.Foreground = new SolidColorBrush(Colors.Orange);
        }

        private void DesarchiverLaSerie_OnDragEnter(object sender, DragEventArgs e)
        {
            desarchiverLaSerie.Foreground = new SolidColorBrush(Colors.Green);
        }

        private async void supprimerLaSerie_Drop_1(object sender, DragEventArgs e)
        {
            try
            {
                if (!BetaSerieData.VerificationConnexionInternet())
                    return;

                if (DroppedSerie == null)
                    return;

                var series = RecupererGroupeDeSerie(DroppedSerie);
                actionSurSerie.Visibility = Visibility.Collapsed;
                if (series != null)
                {
                    series.Remove(DroppedSerie);

                    if (GroupeSerie != null && GroupeSerie.Series.Count == 0)
                        ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).Remove(GroupeSerie);

                    if (GroupeSerieArchive != null && GroupeSerieArchive.Series.Count == 0)
                        ((ObservableCollection<GroupeDeSerie>)DefaultViewModel["GroupesDeSeries"]).Remove(GroupeSerieArchive);

                    if ((GroupeSerie == null || GroupeSerie.Series.Count == 0) &&
                        (GroupeSerieArchive == null || GroupeSerieArchive.Series.Count == 0))
                    {
                        stckSansSerie.Visibility = Visibility.Visible;
                        itemGridScrollViewer.Visibility = Visibility.Collapsed;
                        progressSeries.Visibility = Visibility.Collapsed;
                    }
                    await ServicesBetaSeries.SupprimerDeMesSeries(DroppedSerie);
                    ServiceToast.Afficher("La série " + DroppedSerie.Titre + " vient d'être supprimée");
                }
            }
            catch (Exception ex)
            {
                BetaSerieData.Erreur(ex);
            }

        }

        private void SupprimerLaSerie_OnDragEnter(object sender, DragEventArgs e)
        {
            supprimerLaSerie.Foreground = new SolidColorBrush(Colors.DarkRed);
        }

        private void ItemGridView_OnDrop(object sender, DragEventArgs e)
        {
            DroppedSerie = null;
            actionSurSerie.Visibility = Visibility.Collapsed;
        }

        private void archiverLaSerie_DragLeave_1(object sender, DragEventArgs e)
        {
            archiverLaSerie.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void DesarchiverLaSerie_DragLeave_1(object sender, DragEventArgs e)
        {
            desarchiverLaSerie.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void supprimerLaSerie_DragLeave_1(object sender, DragEventArgs e)
        {
            supprimerLaSerie.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void GlobalAppBar_Closed_1(object sender, object e)
        {
            filtresEpisodes.IsOpen = false;
        }

        private void GlobalAppBar_Opened_1(object sender, object e)
        {
            if (stckSansSerie.Visibility == Visibility.Visible)
            {
                FilterBySerieName.Visibility = Visibility.Collapsed;
            }

            if (ViewState == "Snapped")
            {
                FilterBySerieName.Visibility = Visibility.Collapsed;
                rightCommandsInMySeries.Visibility = Visibility.Collapsed;
            }
        }
    }
}