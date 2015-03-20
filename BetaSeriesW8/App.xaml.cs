using System;
using BetaSeriesW8.Common;
using BetaSeriesW8.Service;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BetaSeriesW8
{
    /// <summary>
    /// Fournit un comportement spécifique à l'application afin de compléter la classe Application par défaut.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// à être exécutée. Elle correspond donc à l'équivalent logique de main() ou WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }


        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            Rechercher.Activate(args.QueryText);
        }

        /// <summary>
        /// Invoqué lorsque l'application est lancée normalement par l'utilisateur final. D'autres points d'entrée
        /// sont utilisés lorsque l'application est lancée pour ouvrir un fichier spécifique, pour afficher
        /// des résultats de recherche, etc.
        /// </summary>
        /// <param name="args">Détails concernant la requête et le processus de lancement.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();
                return;
            }

            // Create a Frame to act as the navigation context and associate it with
            // a SuspensionManager key
            var rootFrame = new Frame();
            SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

            //if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            //{
            //    // Restore the saved session state only when appropriate
            //    await SuspensionManager.RestoreAsync();
            //}

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // puis configurez la nouvelle page en transmettant les informations requises en tant que
                // paramètre
                if (!rootFrame.Navigate(typeof (LoginPage), "Login"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Placez le frame dans la fenêtre active, puis assurez-vous qu'il est actif
            Window.Current.Content = rootFrame;
            Window.Current.Activate();

            //ServicesTiles.MettreAJourLesTilesEpisodes();
            
        }

        /// <summary>
        /// Appelé lorsque l'exécution de l'application est suspendue. L'état de l'application est enregistré
        /// sans savoir si l'application pourra se fermer ou reprendre sans endommager 
        /// le contenu de la mémoire.
        /// </summary>
        /// <param name="sender">Source de la requête de suspension.</param>
        /// <param name="e">Détails de la requête de suspension.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}