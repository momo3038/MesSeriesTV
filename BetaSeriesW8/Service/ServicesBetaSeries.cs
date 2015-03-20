using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BetaSeriesW8.Data;
using BetaSeriesW8.DataModel;
using BetaSeriesW8.Service.Helper;
using BetaSeriesW8.Service.TileContent;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

namespace BetaSeriesW8.Service
{
    public static class ServicesBetaSeries
    {
        public static async Task<string> Authentifier(string login, string md5)
        {
            string resultJson = await BetaSerieAPI.Instance.Login(login, md5);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            JsonObject member = root.GetNamedObject("member");
            BetaSerieData.Token = member.GetNamedString("token");
            BetaSerieData.Login = member.GetNamedString("login");
            return string.Empty;
        }

        public static async Task<IList<Episode>> RecupererMesEpisodesRestantARegarder(bool uniquementLesPremiers = true)
        {
            if (!BetaSerieData.EstConnecte)
                throw new Exception("Impossible de récupérer les série sans être connecté");

            if (Cache.EstExpire(Cache.Episodes))
            {
                var mesEpisode = await ApplicationBetaSeries.RecupererMesEpisodesRestantARegarder(uniquementLesPremiers);


                await Cache.Ajouter(mesEpisode);
                Cache.AEteMisAJour(Cache.Episodes);
                return mesEpisode;
            }
            var cacheEpisode = await Cache.Recuperer<List<Episode>>();

            if (cacheEpisode == null)
            {
                var mesEpisode = await ApplicationBetaSeries.RecupererMesEpisodesRestantARegarder(uniquementLesPremiers);
                await Cache.Ajouter(mesEpisode);
                Cache.AEteMisAJour(Cache.Episodes);
                return mesEpisode;
            }
            return cacheEpisode;
        }

        public static async Task<IList<Serie>> MesSeriesPossedantDesEpisodesNonVus(IEnumerable<Serie> series)
        {
            var mesEpisodesNonVus = await RecupererMesEpisodesRestantARegarder();
            var seriesAvecEpisodesRestant = new List<Serie>();

            foreach (var serie in series)
            {
                serie.PossedeDesEpisodesNonVus = mesEpisodesNonVus.Any(x => x.ShowUrl == serie.Url);
                if (serie.PossedeDesEpisodesNonVus)
                    seriesAvecEpisodesRestant.Add(serie);
            }

            return seriesAvecEpisodesRestant;
        }

        public static IList<Serie> MesSeriesEnCoursSansEpisodes(IEnumerable<Serie> series)
        {
            return series.Where(x => !x.EstArchive && !x.PossedeDesEpisodesNonVus).ToList();
        }

        public static IList<Serie> MesSeriesNonArchives(IEnumerable<Serie> series)
        {
            return series.Where(x => !x.EstArchive).ToList();
        }

        public static IList<Serie> MesSeriesArchives(IEnumerable<Serie> series)
        {
            return series.Where(x => x.EstArchive).ToList();
        }

        public static async Task<Serie> RecupererUneSerie(string serieName, Serie serieAMettreAJour = null)
        {
            Serie serieT = serieAMettreAJour ?? new Serie();

            var cacheDeLaSerieExiste = await Cache.Existe<Serie>(serieName);

            bool forcer = false;
            if (cacheDeLaSerieExiste)
            {
                try
                {
                    serieT = await Cache.Recuperer<Serie>(serieName);
                    if (serieAMettreAJour != null)
                    {
                        serieT.EstArchive = serieAMettreAJour.EstArchive;
                        serieT.EstDansMesSeries = serieAMettreAJour.EstDansMesSeries;
                        serieAMettreAJour.Copier(serieT);
                        await Cache.Ajouter(serieT, serieT.Url);
                    }
                }
                catch (Exception)
                {
                    forcer = true;
                }
            }
            else
            {
                await ApplicationBetaSeries.FicheSerie(serieName, serieT);
            }

            if (forcer)
                await ApplicationBetaSeries.FicheSerie(serieName, serieT);

            return serieT;
        }

        public static async Task<IList<Episode>> RecupererMonPlanning()
        {
            var episodesDansLePlanning = await ApplicationBetaSeries.MonPlanning();
            var mesInformations = await RecupererMesInformations();

            var episodes = new List<Episode>();
            foreach (var series in mesInformations.Series.Where(x => !x.EstArchive))
            {
                var seriesDansMonPlanning = episodesDansLePlanning.Where(x => x.ShowUrl == series.Url);
                if (seriesDansMonPlanning != null)
                    episodes.AddRange(seriesDansMonPlanning);
            }
            return episodes;
        }

        public static async Task<Utilisateur> RecupererMesInformations()
        {
            var infosUtilisateur = await Cache.Recuperer<Utilisateur>();
            if (infosUtilisateur == null || infosUtilisateur.EstExpireDepuisPlusDUnHeure)
            {
                var utilisateur = await ApplicationBetaSeries.RecupererMesInformations();
                utilisateur.DateMiseAJour = DateTime.Now;
                Cache.Ajouter(utilisateur);
                var badgeContent = new BadgeNumericNotificationContent((uint)utilisateur.NombreEpisodeARegarder);
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badgeContent.CreateNotification());
                return utilisateur;
            }
            return infosUtilisateur;
        }

        public static async Task RecupererMesSeries(ObservableCollection<Serie> mesSeriesIn = null, TextBlock text = null)
        {
            if (Cache.EstExpire(Cache.Series))
            {
                await ApplicationBetaSeries.RecupererMesSeries(mesSeriesIn);
                if (mesSeriesIn != null && mesSeriesIn.Count > 0)
                {
                    ServiceToast.Afficher("Mise à jour de vos Séries en cours ...");

                    var factory = new TaskFactory();
                    var v = new Queue<Task>();
                    foreach (var mesSeries1 in mesSeriesIn)
                    {
                        v.Enqueue(RecupererUneSerie(mesSeries1.Url, mesSeries1).ContinueWith(task => ContinuationAction(task, text))
                                        .ContinueWith(task => RecupererLaffiche(task.Result)));
                    }
                    await factory.ContinueWhenAll(v.ToArray(),
                                                  tasks =>
                                                  {
                                                      List<string> series = mesSeriesIn.Select(x => x.Url).ToList();
                                                      Cache.Ajouter(series);
                                                      Cache.AEteMisAJour(Cache.Series);
                                                      ServiceToast.Afficher("Mise à jour des séries terminée !");
                                                  });
                }
            }
            else
            {
                var mesSeriesFromCache = await Cache.Recuperer<List<string>>();

                if (mesSeriesIn == null)
                    mesSeriesIn = new ObservableCollection<Serie>();

                if (mesSeriesFromCache != null && mesSeriesFromCache.Any())
                {
                    var v = new Queue<Task>();
                    foreach (var serie in mesSeriesFromCache)
                    {
                        try
                        {
                            var serieFromCache = await Cache.Recuperer<Serie>(serie);
                            if (serieFromCache == null)
                            {
                                var serieS = new Serie(string.Empty,serie);
                                mesSeriesIn.Add(serieS);
                                v.Enqueue(
                                    RecupererUneSerie(serie, serieS)
                                        .ContinueWith(task => ContinuationAction(task))
                                        .ContinueWith(task => RecupererLaffiche(task.Result)));
                            }
                            else
                            {
                                mesSeriesIn.Add(serieFromCache);
                            }
                        }
                        catch (Exception)
                        {
                            Cache.Supprimer<Serie>(serie);
                            var serieS = new Serie(string.Empty, serie);
                            mesSeriesIn.Add(serieS);
                            v.Enqueue(
                                RecupererUneSerie(serie, serieS)
                                    .ContinueWith(task => ContinuationAction(task))
                                    .ContinueWith(task => RecupererLaffiche(task.Result)));
                        }

                    }
                    if (v.Count > 0)
                    {
                        var factory = new TaskFactory();
                        ServiceToast.Afficher("Mise à jour de vos Séries en cours ...");
                        await factory.ContinueWhenAll(v.ToArray(),
                                                      tasks => ServiceToast.Afficher("Mise à jour des séries terminée !"));
                    }
                }
                else Cache.FaireExpirer(Cache.Series);
            }
        }


        private static Serie ContinuationAction(Task<Serie> task, TextBlock text = null)
        {
            Cache.Ajouter(task.Result, task.Result.Url);
            if (text != null)
            {
                BetaSerieData.UiDispatcher.RunAsync(CoreDispatcherPriority.High,
                                                    () => text.Text = "Mise à jour de la série " + task.Result.Titre);

            }
            return task.Result;
        }

        public static async Task RecupererLeFond(Serie serie, int index = 0, string type = null)
        {
            if (type == "poster")
            {
                if (serie != null && serie.BannieresPosterMiniature.Count > 0)
                {
                    var fond = await serie.BannieresPosterMiniature.First().Recuperer();
                    serie.BanniereString = fond.AbsoluteUri;
                }
                else serie.BanniereString = new Uri("http://coding-in.net/blog/wp-content/uploads/PosterVide1.png", UriKind.Absolute).AbsoluteUri;
            }
            else
            {
                if (serie != null && serie.BannieresFanArt.Count > 0)
                {
                    serie.FondEnCoursDeTelechargement = true;
                    IOrderedEnumerable<BanniereTvDB> orderByDescending = serie.BannieresFanArt.OrderByDescending(x => x.RatingD);

                    BanniereTvDB banniereTvDb;
                    if (index > 0)
                    {
                        banniereTvDb = orderByDescending.ElementAt(index);
                    }
                    else banniereTvDb = orderByDescending.First();

                    banniereTvDb.FondEnCoursDeTelechargement = true;
                    var fond = await banniereTvDb.Recuperer();
                    if (fond != null)
                    {
                        serie.FondString = fond.AbsoluteUri;
                        banniereTvDb.LocalPath = serie.FondString;
                        banniereTvDb.FondEnCoursDeTelechargement = false;
                        serie.FondEnCoursDeTelechargement = false;
                    }
                }
            }

        }

        public static async Task RecupererLaffiche(Serie serie)
        {
            if (serie != null && serie.BannieresPosterMiniature.Count > 0)
            {
                var fond = await serie.BannieresPosterMiniature.First().Recuperer();
                serie.PosterString = fond.AbsoluteUri;
            }
        }

        public static async Task RechercherUneSerie(string text, ObservableCollection<Serie> list)
        {
            var series = await ApplicationBetaSeries.RechercherUneSerie(text);
            foreach (var serie in series)
            {
                list.Add(serie);
            }
        }

        public static Uri RecupererUneBanniere(string url)
        {
            return new Uri(string.Format("http://api.betaseries.com/pictures/show/{0}.jpg?key={1}", url, "e84a30a05b33"));
        }

        public static async Task<bool> AjouterAMesSeries(Serie serie)
        {
            string resultJson = await BetaSerieAPI.Instance.AjouterUneSeries(serie.Url);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);
            serie.EstDansMesSeries = true;
            Cache.AjouterUneSerie(serie);
            Cache.FaireExpirer(Cache.Episodes);
            Cache.FaireExpirer(Cache.Series);
            BetaSerieData.MesSeries = null;
            return true;
        }

        public static async Task<bool> SupprimerDeMesSeries(Serie serie)
        {
            string resultJson = await BetaSerieAPI.Instance.SupprimerUneSeries(serie.Url);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            if (ServiceErreur.RecupererCodeErreur(root) == 2004 || ServiceErreur.RecupererCodeErreur(root) == null)
            {
                serie.EstDansMesSeries = false;
                Cache.FaireExpirer(Cache.Utilisateur);
                Cache.Supprimer<Utilisateur>();
                Cache.FaireExpirer(Cache.Episodes);
                Cache.FaireExpirer(Cache.Series);
                Cache.SupprimerUneSerie(serie);
                BetaSerieData.MesSeries = null;
            }
            else ServiceErreur.LeverExceptionSiErreur(root);
            return true;
        }

        public static async Task<bool> ArchiverDeMesSeries(Serie serie)
        {
            string resultJson = await BetaSerieAPI.Instance.ArchiverUneSerie(serie.Url);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            Cache.FaireExpirer(Cache.Utilisateur);
            Cache.Supprimer<Utilisateur>();
            Cache.FaireExpirer(Cache.Episodes);
            Cache.ArchiverUneSerie(serie);
            BetaSerieData.MesSeries = null;
            return true;
        }

        public static async Task<bool> DesarchiverDeMesSeries(Serie serie)
        {
            string resultJson = await BetaSerieAPI.Instance.DesarchiverUneSerie(serie.Url);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            serie.EstDansMesSeries = true;
            serie.EstArchive = false;
            Cache.FaireExpirer(Cache.Utilisateur);
            Cache.Supprimer<Utilisateur>();
            Cache.FaireExpirer(Cache.Episodes);
            Cache.DesarchiverUneSerie(serie);
            BetaSerieData.MesSeries = null;
            return true;
        }

        public static async Task<Episode> RecupererLeProchainEpisodePourLaSerie(string serieUrl)
        {
            string resultJson = await BetaSerieAPI.Instance.ProchainEpisodePourLaSerie(serieUrl);

            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            JsonObject episodes = root.GetNamedObject("episodes");
            foreach (string e in episodes.Keys)
            {
                JsonObject objEpisode = episodes.GetNamedObject(e);
                string numeroEpisode = objEpisode.GetNamedString("episode");
                string numeroSaison = objEpisode.GetNamedString("season");
                string titre = objEpisode.GetNamedString("title");
                string date = objEpisode.GetNamedString("date");
                string showUrl = objEpisode.GetNamedString("url");
                string showName = objEpisode.GetNamedString("show");

                var nEpisode = new Episode(titre, DateHelper.UnixTimeStampToDateTime(Double.Parse(date)),
                                           int.Parse(numeroSaison), int.Parse(numeroEpisode), showUrl, showName);
                return nEpisode;
            }

            return null;
        }

        public async static Task<IList<SousTitre>> RecupererLesSousTitre(Episode episode)
        {
            string resultJSon = await BetaSerieAPI.Instance.RecupererLesSousTitre(episode);

            JsonObject root = JsonObject.Parse(resultJSon).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            JsonObject subtitles = root.GetNamedObject("subtitles");

            var listeSousTitre = new List<SousTitre>();
            foreach (string subtitle in subtitles.Keys)
            {
                JsonObject objEpisode = subtitles.GetNamedObject(subtitle);
                string season = objEpisode.GetNamedString("season");
                string numepisode = objEpisode.GetNamedString("episode");
                string langue = objEpisode.GetNamedString("language");
                string url = objEpisode.GetNamedString("url");
                string source = objEpisode.GetNamedString("source");
                string file = objEpisode.GetNamedString("file");

                listeSousTitre.Add(new SousTitre() { Langue = langue, Fichier = file, Url = url, Source = source, Episode = numepisode, Saison = season });
            }

            return listeSousTitre.Where(x => x.Episode == episode.NumeroEpisode.ToString() && x.Saison == episode.NumeroSaison.ToString()).ToList();
        }

        public static async Task<bool> Inscription(string nomUtilisateur, string email, string motDePasse)
        {
            string resultJson = await BetaSerieAPI.Instance.Inscription(nomUtilisateur, email, motDePasse);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);
            return true;
        }


        public static async Task<bool> MarquerUnEpisodeCommeVu(Episode episode)
        {
            string resultJSon = await BetaSerieAPI.Instance.MarquerUnEpisodeCommeVu(episode);
            JsonObject root = JsonObject.Parse(resultJSon).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);
            Cache.FaireExpirer(Cache.Episodes);
            Cache.FaireExpirer(Cache.Utilisateur);
            Cache.Supprimer<Utilisateur>();
            ServicesTiles.MettreAJourLesTilesEpisodes();
            return true;
        }

        public async static Task SeDeconnecter()
        {
            var result = await BetaSerieAPI.Instance.SeDeconnecter();
            BetaSerieData.SeDeconnecter();
            JsonObject root = JsonObject.Parse(result).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);
        }

        public async static Task<List<Episode>>  RecupererLesEpisodes(int numerosaison, string urlSerie)
        {
            string resultJSon = await BetaSerieAPI.Instance.RecupererLesEpisodes(urlSerie, numerosaison);
            JsonObject root = JsonObject.Parse(resultJSon).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            var episodest = new List<Episode>();
            JsonObject seasons = root.GetNamedObject("seasons");
            JsonObject episodes = seasons.GetNamedObject("0");
            episodes = episodes.GetNamedObject("episodes");
            foreach (string e in episodes.Keys)
            {
                JsonObject objEpisode = episodes.GetNamedObject(e);
                string numeroEpisode = objEpisode.GetNamedString("episode");
                string titre = objEpisode.GetNamedString("title");
                //string date = objEpisode.GetNamedString("date");
                //string showUrl = objEpisode.GetNamedString("url");
                //string showName = objEpisode.GetNamedString("show");

                var nEpisode = new Episode(titre, DateTime.Now,
                                           numerosaison, int.Parse(numeroEpisode), urlSerie, string.Empty);
                episodest.Add(nEpisode);
            }
            return episodest.OrderBy(x => x.NumeroEpisode).ToList();
        }
    }
}