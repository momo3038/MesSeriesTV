using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.Service;
using Windows.ApplicationModel.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BetaSeriesW8
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class MonPlanning : LayoutAwarePage
    {
        private readonly ObservableCollection<GroupeEpisode> _itemGroups = new ObservableCollection<GroupeEpisode>();

        public MonPlanning()
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
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            loadingEpisodes.Visibility = Visibility.Visible;

            var mesEpisodesAregarder = await ServicesBetaSeries.RecupererMonPlanning();
            if (mesEpisodesAregarder.Count > 0)
            {

                var debutDeSemaine = DateTime.Today;
                while (debutDeSemaine.DayOfWeek != DayOfWeek.Monday)
                {
                    debutDeSemaine = debutDeSemaine.AddDays(-1);
                }

                var finDeSemaine = debutDeSemaine.AddDays(7);

                var EpisodeDecetteSemaine = mesEpisodesAregarder.Where(x => x.Date >= debutDeSemaine && x.Date < finDeSemaine).OrderBy(x => x.Date).ToList();
                var EpisodeDeLaSemaineProchaine = mesEpisodesAregarder.Where(x => x.Date >= finDeSemaine && x.Date < finDeSemaine.AddDays(7)).OrderBy(x => x.Date).ToList();
                var EpisodeDansDeuxSemaines = mesEpisodesAregarder.Where(x => x.Date >= finDeSemaine.AddDays(7)).OrderBy(x => x.Date).ToList();

                if (EpisodeDecetteSemaine.Count > 0)
                    _itemGroups.Add(new GroupeEpisode("Cette semaine", EpisodeDecetteSemaine));

                if (EpisodeDeLaSemaineProchaine.Count > 0)
                    _itemGroups.Add(new GroupeEpisode("La semaine prochaine", EpisodeDeLaSemaineProchaine));

                if (EpisodeDansDeuxSemaines.Count > 0)
                    _itemGroups.Add(new GroupeEpisode("Dans deux semaines", EpisodeDansDeuxSemaines));

                DefaultViewModel["GroupesEpisodes"] = _itemGroups;
            }
            else
            {
                stckSansSerie.Visibility = Visibility.Visible;
                itemGridScrollViewer.Visibility = Visibility.Collapsed;
            }

            loadingEpisodes.Visibility = Visibility.Collapsed;
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

        public FrameworkElement FindVisualChildByName(DependencyObject obj, string name)
        {
            FrameworkElement ret = null;
            int numChildren = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < numChildren; i++)
            {
                DependencyObject objChild = VisualTreeHelper.GetChild(obj, i);
                var child = objChild as FrameworkElement;
                if (child != null && child.Name == name)
                {
                    return child;
                }

                ret = FindVisualChildByName(objChild, name);
                if (ret != null)
                    break;
            }
            return ret;
        }

        private async void Rafraichir_OnClick(object sender, RoutedEventArgs e)
        {

            Cache.FaireExpirer(Cache.Episodes);
            Frame.Navigate(typeof(MesEpisodes));
        }

        private async void BtnEpisodeRegarde_OnClick(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            var loader = RecupererLoader(sender);
            var btn = (Button)sender;
            if (loader != null)
            {
                loader.Visibility = Visibility.Visible;
                btn.Opacity = 0;
                //btn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            await ServicesBetaSeries.MarquerUnEpisodeCommeVu(((Button)sender).Tag as Episode);

            var mesEpisodes = ((ObservableCollection<GroupeEpisode>)DefaultViewModel["GroupesEpisodes"]).First().Episodes;

            var episodeAsupprimer = mesEpisodes.Single(x => x.ShowUrl == (((Button)sender).Tag as Episode).ShowUrl);
            var index = mesEpisodes.IndexOf(episodeAsupprimer);
            mesEpisodes.Remove(episodeAsupprimer);

            var episodeNew = await ServicesBetaSeries.RecupererLeProchainEpisodePourLaSerie((((Button)sender).Tag as Episode).ShowUrl);
            if (episodeNew != null)
                mesEpisodes.Insert(index, episodeNew);

            if (loader != null)
            {
                loader.Visibility = Visibility.Collapsed;
                btn.Opacity = 0;
                //btn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private ProgressRing RecupererLoader(object sender)
        {
            DependencyObject obj;
            if (ViewState == "Snapped")
            {
                obj = itemListView.ItemContainerGenerator.ContainerFromItem(((Button)(sender)).Tag) as ListViewItem;
            }
            else
            {
                obj = itemGridView.ItemContainerGenerator.ContainerFromItem(((Button)(sender)).Tag) as GridViewItem;
            }

            if (obj == null)
                return null;

            return FindVisualChildByName(obj, "loadingEpisode") as ProgressRing;
        }

        private void FilterBySerieName_OnClick(object sender, RoutedEventArgs e)
        {
            var mesEpisodes = ((ObservableCollection<GroupeEpisode>)DefaultViewModel["GroupesEpisodes"]).First().Episodes;
            _itemGroups.RemoveAt(0);
            var episodes = mesEpisodes.OrderBy(x => x.Titre).ToList();
            _itemGroups.Add(new GroupeEpisode("Mes épisodes non-vus", episodes));
            DefaultViewModel["GroupesEpisodes"] = _itemGroups;
            GlobalAppBar.IsOpen = false;
            filtresEpisodes.IsOpen = false;
            TopAppBar.IsOpen = false;
        }

        private void FilterByDate_OnClick(object sender, RoutedEventArgs e)
        {
            var mesEpisodes = ((ObservableCollection<GroupeEpisode>)DefaultViewModel["GroupesEpisodes"]).First().Episodes;
            _itemGroups.RemoveAt(0);
            _itemGroups.Add(new GroupeEpisode("Mes épisodes non-vus", mesEpisodes.OrderBy(x => x.Date).ToList()));
            DefaultViewModel["GroupesEpisodes"] = _itemGroups;
            GlobalAppBar.IsOpen = false;
            filtresEpisodes.IsOpen = false;
            TopAppBar.IsOpen = false;
        }

        private void ajouterUneSerie_Click_1(object sender, RoutedEventArgs e)
        {
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            SearchPane forCurrentView = SearchPane.GetForCurrentView();
            forCurrentView.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MesSeries));
        }

        private void FilterBy_OnClick(object sender, RoutedEventArgs e)
        {
            filtresEpisodes.IsOpen = !filtresEpisodes.IsOpen;
        }

        private void GlobalAppBar_Closed_1(object sender, object e)
        {
            filtresEpisodes.IsOpen = false;
        }

        private void GlobalAppBar_Opened_1(object sender, object e)
        {
            GlobalAppBar.Visibility = stckSansSerie.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            if (ViewState == "Snapped")
            {
                //FilterBySerieName.Visibility = Visibility.Collapsed;
                RightCommandsInMySeries.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (MesEpisodes));
        }
    }
}