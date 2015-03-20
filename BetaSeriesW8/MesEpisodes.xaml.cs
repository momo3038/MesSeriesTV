using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.DataModel;
using BetaSeriesW8.Service;
using BetaSeriesW8.Service.TileContent;
using Windows.ApplicationModel.Search;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI;
using Windows.UI.Notifications;
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

            loadingEpisodes.Visibility = Visibility.Visible;

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
                btn.Visibility = Visibility.Collapsed;
            }
            await ServicesBetaSeries.MarquerUnEpisodeCommeVu(((Button)sender).Tag as Episode);

            var mesEpisodes = ((ObservableCollection<GroupeEpisode>)DefaultViewModel["GroupesEpisodes"]).First().Episodes;

            var episodeAsupprimer = mesEpisodes.Single(x => x.ShowUrl == (((Button)sender).Tag as Episode).ShowUrl);
            var index = mesEpisodes.IndexOf(episodeAsupprimer);
            mesEpisodes.Remove(episodeAsupprimer);

            var episodeNew = await ServicesBetaSeries.RecupererLeProchainEpisodePourLaSerie((((Button)sender).Tag as Episode).ShowUrl);
            if (episodeNew != null && mesEpisodes.Count >= index)
                mesEpisodes.Insert(index, episodeNew);

            if (loader != null)
            {
                loader.Visibility = Visibility.Collapsed;
                btn.Visibility = Visibility.Visible;
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

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MonPlanning));
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
            if (stckSansSerie.Visibility == Visibility.Visible)
            {
                FilterBySerieName.Visibility = Visibility.Collapsed;
            }

            if (ViewState == "Snapped")
            {
                FilterBySerieName.Visibility = Visibility.Collapsed;
                RightCommandsInMySeries.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnST_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var episode = ((Button)sender).Tag as Episode;
                popupSousTitre.IsOpen = true;
                titreCompletEpisode.Text = episode.ShowName + " (" + episode.NumeroComplet + ")";
                IList<SousTitre> itemsSource = await ServicesBetaSeries.RecupererLesSousTitre(episode);
                cbxST.ItemsSource = itemsSource;

                if (itemsSource == null || itemsSource.Count == 0)
                {
                    ErreurVideo.Text = "Il n'y a aucun sous-titre pour cet épisode.";
                    ErreurVideo.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {
                ErreurVideo.Text = "Impossible de récupérer les sous-titres. Merci de vérifier votre connexion.";
                ErreurVideo.Visibility = Visibility.Visible;
            }
        }

        private async void btnTelechargerEtAssocierSousTitre(object sender, RoutedEventArgs e)
        {
            if (Folder == null)
            {
                ErreurVideo.Text = "Merci de sélectionner le dossier ou vous souhaitez enregistrer le sous-titre" + Environment.NewLine;
                ErreurVideo.Visibility = Visibility.Visible;
            }

            if (cbxST.SelectedItem == null)
            {
                ErreurVideo.Text = "Merci de sélectionner le sous-titre à télécharger" + Environment.NewLine;
                ErreurVideo.Visibility = Visibility.Visible;
            }

            if (BtnSelectVideo.IsChecked.Value && video == null)
            {
                ErreurVideo.Text = "Il est nécessaire de séléctionner un épisode pour pouvoir renommer automatiquement le sous-titre" + Environment.NewLine;
                ErreurVideo.Visibility = Visibility.Visible;
            }

            if (Folder == null || cbxST.SelectedItem == null || (BtnSelectVideo.IsChecked.Value && video == null))
                return;

            progress1.Visibility = Visibility.Visible;
            VoirCetEpisode.Visibility = Visibility.Collapsed;

            DownloadOperation download = null;

            ErreurVideo.Text = string.Empty;
            try
            {
                download = await DownloadHelper.Telecharger(((SousTitre)cbxST.SelectedItem), Folder, !BtnExtraire.IsChecked.Value && !BtnSelectVideo.IsChecked.Value);
            }
            catch (Exception ex)
            {
                ErreurVideo.Text = "Erreur lors du téléchargement du sous-titre (" + ex.Message + ")";
                progress1.Visibility = Visibility.Collapsed;
                VoirCetEpisode.Visibility = Visibility.Visible;
                popupSousTitre.IsOpen = true;
                return;
            }
            var infosDl = download.GetResponseInformation();
            if (infosDl.StatusCode == 200)
            {
                try
                {
                    await SousTitreHelper.Gerer(download.ResultFile, video, Folder, BtnExtraire.IsChecked.Value, BtnSelectVideo.IsChecked.Value);
                }
                catch (Exception exception)
                {
                    ErreurVideo.Text = exception.Message;
                    progress1.Visibility = Visibility.Collapsed;
                    VoirCetEpisode.Visibility = Visibility.Visible;
                    popupSousTitre.IsOpen = true;
                    return;
                }
            }
            else
            {
                ErreurVideo.Text = "Erreur lors du téléchargement du sous-titre";
                progress1.Visibility = Visibility.Collapsed;
                VoirCetEpisode.Visibility = Visibility.Visible;
                popupSousTitre.IsOpen = true;
                return;
            }

            ServiceToast.Afficher("Sous-titre téléchargé.");
            progress1.Visibility = Visibility.Collapsed;
            VoirCetEpisode.Visibility = Visibility.Visible;
            popupSousTitre.IsOpen = false;
        }

        private StorageFile video = null;
        private StorageFolder Folder = null;


        private async void btnSelectFolder_Click_1(object sender, RoutedEventArgs e)
        {
            //if (rootPage.EnsureUnsnapped())
            //{
            var openPicker = new FolderPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add(".avi");
            openPicker.FileTypeFilter.Add(".mpeg");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.CommitButtonText = "Selectionner le dossier";
            var folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                Folder = folder;
                btnSelectFolder.Content = folder.DisplayName + " (" + folder.Path + ")";
            }
            else
            {
                //OutputTextBlock.Text = "Operation cancelled.";
            }
            popupSousTitre.IsOpen = true;

            //ActiverBtnValider();
        }

        private async void btnSelectVideo_Click_1(object sender, RoutedEventArgs e)
        {
            //if (rootPage.EnsureUnsnapped())
            //{
            var openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add("*");
            openPicker.CommitButtonText = "Selectionner l'épisode";
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                video = file;
                btnSelectVideo.Content = file.DisplayName;
            }
            else
            {
                //OutputTextBlock.Text = "Operation cancelled.";
            }
            popupSousTitre.IsOpen = true;

            //ActiverBtnValider();
        }

        private void ActiverBtnValider()
        {
            if (video != null && cbxST.SelectedItem != null)
            {
                VoirCetEpisode.Visibility = Visibility.Visible;
                VoirCetEpisode.Background = new SolidColorBrush(Colors.Green);
            }
            else VoirCetEpisode.Visibility = Visibility.Collapsed;
        }

        private void cbxST_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            //ActiverBtnValider();
        }

        private void BtnExtraire_Checked_1(object sender, RoutedEventArgs e)
        {
            if (BtnExtraire != null)
            {
                BtnSelectVideo.IsEnabled = BtnExtraire.IsChecked.Value;
                btnSelectVideo.IsEnabled = BtnExtraire.IsChecked.Value;
                BtnSelectVideo.IsChecked = BtnExtraire.IsChecked;
            }
        }

        private void BtnSelectVideo_Click_2(object sender, RoutedEventArgs e)
        {
            if (BtnSelectVideo != null)
            {
                btnSelectVideo.IsEnabled = BtnSelectVideo.IsChecked.Value;
            }
        }
    }

    internal class SousTitreHelper
    {
        public async static Task Gerer(IStorageFile fichierTelecharge, StorageFile fichierEpisode, StorageFolder folder, bool DoitDezipper, bool DoitRenommer)
        {
            var tabSplit = fichierTelecharge.Name.Split('.').LastOrDefault();
            if (tabSplit == null)
                throw new Exception();

            var directoryEpisodeTelecharge = fichierTelecharge.Path.Replace(fichierTelecharge.Name, string.Empty);

            var directoryEpisode = string.Empty;
            if (fichierEpisode != null)
                directoryEpisode = fichierEpisode.Path.Replace(fichierEpisode.Name, string.Empty);
            else directoryEpisode = folder.Path;

            var folderSousTitre = await StorageFolder.GetFolderFromPathAsync(directoryEpisodeTelecharge);
            var folderEpisode = await StorageFolder.GetFolderFromPathAsync(directoryEpisode);

            // Cas ou on traite un .zip
            if (DoitDezipper && tabSplit == "zip")
            {
                var displayName = "DefaultName.str";
                if (fichierEpisode != null)
                    displayName = fichierEpisode.DisplayName;
                else displayName = fichierTelecharge.Name;
                FolderZip.UnZipFile(folderSousTitre, fichierTelecharge.Name, displayName, folderEpisode, DoitRenommer);
                return;
            }
            // Cas ou on traite un .srt 
            else
            {
                var displayName = "DefaultName.str";
                if (fichierEpisode != null)
                    displayName = fichierEpisode.DisplayName;
                else displayName = fichierTelecharge.Name;


                if (DoitRenommer)
                {
                    var name = fichierTelecharge.Name.Split('.').Last();
                    displayName = string.Format("{0}.{1}", displayName, name);
                    await fichierTelecharge.RenameAsync(displayName, NameCollisionOption.ReplaceExisting);
                }


                //StorageFile outputFile = await folderEpisode.CreateFileAsync(displayName, CreationCollisionOption.ReplaceExisting);

                //CachedFileManager.DeferUpdates(outputFile);
                // write to file
                //await fichierTelecharge.MoveAsync(folderEpisode, displayName);
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                //FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(outputFile);
            }
        }
    }
}