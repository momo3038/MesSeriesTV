using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.Service;
using Windows.ApplicationModel.Search;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BetaSeriesW8
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class MesEpisodes : LayoutAwarePage
    {
        private readonly ObservableCollection<GroupeEpisode> _itemGroups = new ObservableCollection<GroupeEpisode>();


        internal bool EnsureUnsnapped()
        {
            // FilePicker APIs will not work if the application is in a snapped state.
            // If an app wants to show a FilePicker while snapped, it must attempt to unsnap first
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (!unsnapped)
            {

                //NotifyUser("Cannot unsnap the sample.", NotifyType.StatusMessage);
            }

            return unsnapped;
        }

        private async void PickAFileButton_Click(DownloadOperation download)
        {
            if (EnsureUnsnapped())
            {
                var openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.Desktop
                };

                openPicker.ViewMode = PickerViewMode.List;
                openPicker.FileTypeFilter.Add("*");
                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file != null)
                {
                    var folderPath = file.Path.Replace(file.Name, string.Empty);
                    var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);

                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                    await download.ResultFile.MoveAsync(folder);

                    await FolderZip.UnZipFile(folder, ((StorageFile)download.ResultFile).Name, file.DisplayName);
                }
                else
                {

                    //OutputTextBlock.Text = "Operation cancelled.";
                }
            }
        }

        private async void StartDownload_Click(SousTitre sousTitre)
        {
            try
            {

                StorageFile destinationFile =
                    await ApplicationData.Current.LocalFolder.CreateFileAsync(sousTitre.Fichier,
                                                                              CreationCollisionOption.ReplaceExisting);
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(new Uri(sousTitre.Url), destinationFile);
                await download.StartAsync();
                ResponseInformation response = download.GetResponseInformation();
                PickAFileButton_Click(download);
            }
            catch (Exception ex)
            {
                //LogException("Download Error", ex);
            }
        }

        public MesEpisodes()
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

            var mesEpisodesAregarder = await ServicesBetaSeries.RecupererMesEpisodesRestantARegarder();
            if (mesEpisodesAregarder.Count > 0)
            {
                _itemGroups.Add(new GroupeEpisode("Mes épisodes non-vus", mesEpisodesAregarder));
                DefaultViewModel["GroupesEpisodes"] = _itemGroups;
            }
            else
            {
                stckSansSerie.Visibility = Visibility.Visible;
                itemGridScrollViewer.Visibility = Visibility.Collapsed;
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
            {
                mesEpisodes.Insert(index, episodeNew);
            }

            if (loader != null)
            {
                loader.Visibility = Visibility.Collapsed;
                btn.Opacity = 0;
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
                FilterBySerieName.Visibility = Visibility.Collapsed;
                RightCommandsInMySeries.Visibility = Visibility.Collapsed;
            }
        }

        private async void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var episode = ((GridView)sender).SelectedItem as Episode;

            TexteInfosEpisode.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            loaderInfosEpisode.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var episodeComplet = await ServicesBetaSeries.RechercherUnEpisode(episode.ShowUrl, episode.NumeroSaison, episode.NumeroEpisode);
        }
    }
}