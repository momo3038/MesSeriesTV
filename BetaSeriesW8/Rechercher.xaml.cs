using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.Service;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace BetaSeriesW8
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class Rechercher : BetaSeriesW8.Common.LayoutAwarePage
    {
        /// <summary>
        /// Records the value of the active Window's Content property when the search started.
        /// </summary>
        private UIElement _previousContent;

        public Rechercher()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Records the value of the active Window's Content property when the search started.
        /// </summary>
        public static void Activate(String queryText)
        {
            // If the Window isn't already using Frame navigation, insert our own Frame
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;
            if (frame == null)
            {
                frame = new Frame();
                Window.Current.Content = frame;
            }

            // Use navigation to display the results, packing both the query text and the previous
            // Window content into a single parameter object
            frame.Navigate(typeof(Rechercher),
                new Tuple<String, UIElement>(queryText, previousContent));
            Window.Current.Activate();
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
            if (!BetaSerieData.VerificationConnexionInternet())
                return;

            DefaultViewModel["EstConnecte"] = BetaSerieData.EstConnecte;

 // Unpack the two values passed in the parameter object: query text and previous
            // Window content
            var parameter = (Tuple<String, UIElement>)navigationParameter;
            var queryText = parameter.Item1;
            this._previousContent = parameter.Item2;

            SearchInProgress.Visibility = Visibility.Visible;

            this.DefaultViewModel["CanGoBack"] = this._previousContent != null;
            if (string.IsNullOrEmpty(queryText))
            {
                SearchInProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return;
            }

            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';

            var series = new ObservableCollection<Serie>();

            this.DefaultViewModel["ResultsTous"] = series;
            this.DefaultViewModel["Results"] = series;

            await ServicesBetaSeries.RechercherUneSerie(queryText, series);

            var filterList = new List<Filter> { new Filter("Toutes les Séries", series.Count, true) };

            if (BetaSerieData.EstConnecte)
            {
                filterList.Add(new Filter("Série(s) suivie(s)", series.Count(x => x.EstDansMesSeries)));
                filterList.Add(new Filter("Série(s) non suivie(s)", series.Count(x => !x.EstDansMesSeries)));
            }

            SearchInProgress.Visibility = Visibility.Collapsed;

            // Communicate results through the view model
            this.DefaultViewModel["ShowFilters"] = true;
            this.DefaultViewModel["Filters"] = filterList;
        }

        private void GlobalAppBar_Closed_1(object sender, object e)
        {
            filtreSeries.IsOpen = false;
        }

        private void GlobalAppBar_Opened_1(object sender, object e)
        {
            //GlobalAppBar.Visibility = stckSansSerie.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            //if (ViewState == "Snapped")
            //{
            //    FilterBySerieName.Visibility = Visibility.Collapsed;
            //    rightCommandsInMySeries.Visibility = Visibility.Collapsed;
            //}
        }

        private void FilterBy_OnClick(object sender, RoutedEventArgs e)
        {

            filtreSeries.IsOpen = !filtreSeries.IsOpen;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MesSeries));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MesEpisodes));
        }

        private void FilterBySerieName_OnClick(object sender, RoutedEventArgs e)
        {
            var mesSeries = ((IList<Serie>)DefaultViewModel["Results"]);

            this.DefaultViewModel["Results"] =  mesSeries.OrderBy(x => x.Titre).ToList();
            GlobalAppBar.IsOpen = false;
            filtreSeries.IsOpen = false;
            TopAppBar.IsOpen = false;
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


        /// <summary>
        /// Invoked when the back button is pressed.
        /// </summary>
        /// <param name="sender">The Button instance representing the back button.</param>
        /// <param name="e">Event data describing how the button was clicked.</param>
        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            // Return the application to the state it was in before search results were requested
            var frame = this._previousContent as Frame;
            if (frame != null)
            {
                frame.GoBack();
            }
            else
            {
                Window.Current.Content = this._previousContent;
            }
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;
                var series = this.DefaultViewModel["ResultsTous"] as IList<Serie>;
                IList<Serie> selection = null;
                if (selectedFilter.Name == "Série(s) suivie(s)")
                {
                    selection = series.Where(x => x.EstDansMesSeries).ToList();
                }
                else if (selectedFilter.Name == "Série(s) non suivie(s)")
                {
                    selection = series.Where(x => !x.EstDansMesSeries).ToList();
                }
                else
                {
                    selection = series;
                }

                this.DefaultViewModel["Results"] = selection;
            }
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        private sealed class Filter : BetaSeriesW8.Common.BindableBase
        {
            private String _name;
            private int _count;
            private bool _active;

            public Filter(String name, int count, bool active = false)
            {
                this.Name = name;
                this.Count = count;
                this.Active = active;
            }

            public override String ToString()
            {
                return Description;
            }

            public String Name
            {
                get { return _name; }
                set { if (this.SetProperty(ref _name, value)) this.OnPropertyChanged("Description"); }
            }

            public int Count
            {
                get { return _count; }
                set { if (this.SetProperty(ref _count, value)) this.OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                get { return _active; }
                set { this.SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }
        }

        private void resultsGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var serie = e.ClickedItem as Serie;
            this.Frame.Navigate(typeof(FicheSerie), serie);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (MonPlanning));
        }

        private void BitmapImage_OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
           ( (BitmapImage)sender).UriSource = new Uri("http://coding-in.net/blog/wp-content/uploads/FondSansRchch.png");
        }
    }
}
