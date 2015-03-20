using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BetaSeriesW8.Data;
using BetaSeriesW8.DataModel;
using BetaSeriesW8.Service.Helper;
using Windows.Data.Json;

namespace BetaSeriesW8.Service
{
    public static class ApplicationBetaSeries
    {
        public static async Task<Utilisateur> RecupererMesInformations()
        {
            var result = await BetaSerieAPI.Instance.RecupererMesInformations();
            var root = JsonObject.Parse(result).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            var shows = root.GetNamedObject("member").GetNamedObject("shows");
            var stats = root.GetNamedObject("member").GetNamedObject("stats");

            var utilisateur = await RecupererInformationsUtilisateur(stats);
            var mesSeries = await RecupererMesSeries(shows);
            utilisateur.Series = mesSeries;
            return utilisateur;
        }

        private static async Task<Utilisateur> RecupererInformationsUtilisateur(JsonObject stats)
        {
            var utilisateur = new Utilisateur();
            utilisateur.NombreDEpisodesRegardes = (int)stats.GetNamedNumber("episodes");
            utilisateur.NombreDeBadgesGagnes = (int)stats.GetNamedNumber("badges");
            utilisateur.NombreDeSaisonsRegardes = (int)stats.GetNamedNumber("seasons");
            utilisateur.NombreDeSerieSuivies = (int)stats.GetNamedNumber("shows");

            if (stats.ContainsKey("episodes_to_watch"))
                utilisateur.NombreEpisodeARegarder = (int)stats.GetNamedNumber("episodes_to_watch");
            utilisateur.PogressionGenerale = stats.GetNamedString("progress");
            //utilisateur.TempsPasseARegarderDesSeries = DateHelper.UnixTimeStampToDateTime(stats.GetNamedNumber("time_on_tv"));
            //utilisateur.TempsRestantARegarderDesSeries = DateHelper.UnixTimeStampToDateTime(stats.GetNamedNumber("time_to_spend"));
            return utilisateur;
        }

        public static async Task<List<Serie>> RecupererMesSeries(ObservableCollection<Serie> mesSeriesIn = null)
        {
            var result = await BetaSerieAPI.Instance.RecupererMesInformations();
            var root = JsonObject.Parse(result).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);
            var shows = root.GetNamedObject("member").GetNamedObject("shows");
            var mesSeries = await RecupererMesSeries(shows, mesSeriesIn);
            return mesSeries;
        }

        private static async Task<List<Serie>> RecupererMesSeries(JsonObject jsonSeries, ObservableCollection<Serie> mesSeriesIn = null)
        {

            if(mesSeriesIn == null)
                mesSeriesIn = new ObservableCollection<Serie>();

            foreach (var show in jsonSeries.Keys)
            {
                JsonObject objShow = jsonSeries.GetNamedObject(show);
                string title = objShow.GetNamedString("title");
                string url = objShow.GetNamedString("url");

                bool isArchive = false;
                bool isInAccount = false;
                if (objShow.ContainsKey("archive"))
                    isArchive = objShow.GetNamedString("archive") == "1";
                if (objShow.ContainsKey("is_in_account"))
                    isInAccount = objShow.GetNamedNumber("is_in_account") == 1;
                var serie = new Serie(title, url) { EstDansMesSeries = isInAccount, EstArchive = isArchive };

                mesSeriesIn.Add(serie);
            }
            return mesSeriesIn.ToList();
        }



        public static async Task<IList<Episode>> MonPlanning()
        {
            string resultJson = await BetaSerieAPI.Instance.RecupererMonPlanning();
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            JsonObject shows = root.GetNamedObject("planning");

            IList<Episode> episodes = new List<Episode>();
            foreach (string episode in shows.Keys)
            {
                JsonObject objEpisode = shows.GetNamedObject(episode);
                string numeroEpisode = objEpisode.GetNamedString("episode");
                string numeroSaison = objEpisode.GetNamedString("season");
                string titre = objEpisode.GetNamedString("title");
                double date = objEpisode.GetNamedNumber("date");
                string showUrl = objEpisode.GetNamedString("url");
                string showName = objEpisode.GetNamedString("show");

                var nEpisode = new Episode(titre, DateHelper.UnixTimeStampToDateTime(date),
                                           int.Parse(numeroSaison), int.Parse(numeroEpisode), showUrl, showName);
                episodes.Add(nEpisode);
            }
            return episodes;
        }


        public static async Task<string> RecupererIdTvDB(string url)
        {
            string resultJson = await BetaSerieAPI.Instance.RecupererUneFicheSerie(url);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            JsonObject show = root.GetNamedObject("show");

            return show.GetNamedString("id_thetvdb");
        }

        public static async Task FicheSerie(string serieName, Serie serie = null)
        {
            string resultJson = await BetaSerieAPI.Instance.RecupererUneFicheSerie(serieName);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            JsonObject show = root.GetNamedObject("show");

            string title = show.GetNamedString("title");
            string description = show.GetNamedString("description");
            string url = show.GetNamedString("url");
            string idTvDb = show.GetNamedString("id_thetvdb");
            string network = string.Empty;
            if (show.ContainsKey("network") && show.GetNamedValue("network").ValueType != JsonValueType.Null)
            {
                network = show.GetNamedString("network");
            }
            string duration = show.GetNamedString("duration");
            string statut = show.GetNamedString("status");
            JsonObject genreString = show.GetNamedObject("genres");
            string genreFinal = string.Empty;
            foreach (string genre in genreString.Keys)
            {
                genreFinal += genreString.GetNamedValue(genre).GetString() + " ";
            }

            int nbreVotant = 0;
            double resultatVote = 0;
            JsonObject notes = show.GetNamedObject("note");
            if (notes.ContainsKey("members") && notes.GetNamedValue("members").ValueType != JsonValueType.Null)
                nbreVotant = (int)notes.GetNamedNumber("members");
            if (notes.ContainsKey("mean") && notes.GetNamedValue("mean").ValueType != JsonValueType.Null)
                resultatVote = notes.GetNamedNumber("mean");

            int nbreEpisodes = 0;
            int nbreSaison = 0;
            JsonObject seasons = show.GetNamedObject("seasons");
            foreach (string season in seasons.Keys)
            {
                JsonObject ob = seasons.GetNamedObject(season);
                if (ob != null)
                {
                    nbreSaison++;
                    double nbEpisode = ob.GetNamedNumber("episodes");
                    nbreEpisodes += (int)nbEpisode;
                }
            }

            string estDansMesSeries = string.Empty;
            bool estArchive = false;
            if (show.ContainsKey("is_in_account"))
            {
                var mesInformations = await RecupererMesInformations();
                var series = mesInformations.Series;
                estDansMesSeries = show.GetNamedString("is_in_account");
                estArchive = series.ToList().Any(x => x.Url == url && x.EstArchive);
            }

            var bannieres = await ServicesTvDb.RecupererLesBannieres(int.Parse(idTvDb), url);


            if(serie == null)
                serie = new Serie();

            serie.Titre = title;
            serie.Description = description;
            serie.Url = url;
            serie.TvdbId = int.Parse(idTvDb);
            serie.EstDansMesSeries = (estDansMesSeries == "1");
            serie.EstArchive = estArchive;
            serie.NombreTotalEpisodes = nbreEpisodes;
            serie.NombreTotalSaisons = nbreSaison;
            serie.ChaineTV = network;
            serie.DureeMoyenne = duration;
            serie.Statut = statut;
            serie.Note = resultatVote;
            serie.Bannieres = bannieres.ToList();
            serie.DoitRecupererLesBannieres = true;
        }

        public static async Task<IList<Serie>> RechercherUneSerie(string text)
        {
            string resultJson = await BetaSerieAPI.Instance.Rechercher(text);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            JsonObject shows = root.GetNamedObject("shows");

            var series = new List<Serie>();
            foreach (string show in shows.Keys)
            {
                JsonObject objShow = shows.GetNamedObject(show);
                string title = objShow.GetNamedString("title");
                string url = objShow.GetNamedString("url");

                bool isInAccount = false;
                if (objShow.ContainsKey("is_in_account"))
                    isInAccount = objShow.GetNamedNumber("is_in_account") == 1;
                series.Add(new Serie(title, url) { EstDansMesSeries = isInAccount });
            }

            return series;
        }

        public static async Task<List<Episode>> RecupererMesEpisodesRestantARegarder(bool uniquementLesPremiers)
        {
            string resultJson = await BetaSerieAPI.Instance.RecupererMesEpisodesRestantARegarder(uniquementLesPremiers);
            JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
            ServiceErreur.LeverExceptionSiErreur(root);

            JsonObject episodes = root.GetNamedObject("episodes");
            var listeEpisodes = new List<Episode>();
            foreach (string episode in episodes.Keys)
            {
                JsonObject objEpisode = episodes.GetNamedObject(episode);
                string numeroEpisode = objEpisode.GetNamedString("episode");
                string numeroSaison = objEpisode.GetNamedString("season");
                string titre = objEpisode.GetNamedString("title");
                string date = objEpisode.GetNamedString("date");
                string showUrl = objEpisode.GetNamedString("url");
                string showName = objEpisode.GetNamedString("show");

                var nEpisode = new Episode(titre, DateHelper.UnixTimeStampToDateTime(Double.Parse(date)),
                                           int.Parse(numeroSaison), int.Parse(numeroEpisode), showUrl, showName);
                listeEpisodes.Add(nEpisode);
            }

            return listeEpisodes;
        }

        //public async static Task<Serie> FicheSerie(Serie serieP)
        //{
        //    return await FicheSerie(serieP.Url);

        //    //string resultJson = await BetaSerieAPI.Instance.RecupererUneFicheSerie(serieP.Url);
        //    //JsonObject root = JsonObject.Parse(resultJson).GetNamedObject("root");
        //    //ServiceErreur.LeverExceptionSiErreur(root);

        //    //JsonObject show = root.GetNamedObject("show");

        //    //string title = show.GetNamedString("title");
        //    //string description = show.GetNamedString("description");
        //    //string url = show.GetNamedString("url");
        //    //string idTvDb = show.GetNamedString("id_thetvdb");
        //    //string network = null;
        //    //if (show.ContainsKey("network"))
        //    //    network = show.GetNamedString("network");
        //    //string duration = show.GetNamedString("duration");
        //    //string statut = show.GetNamedString("status");
        //    //JsonObject genreString = show.GetNamedObject("genres");
        //    //string genreFinal = string.Empty;
        //    ////foreach (string genre in genreString.Keys)
        //    ////{
        //    ////    genreFinal += genreString.GetNamedValue(genre).GetString() + " ";
        //    ////}

        //    ////int nbreEpisodes = 0;
        //    ////int nbreSaison = 0;
        //    ////JsonObject seasons = show.GetNamedObject("seasons");
        //    ////foreach (string season in seasons.Keys)
        //    ////{
        //    ////    JsonObject ob = seasons.GetNamedObject(season);
        //    ////    if (ob != null)
        //    ////    {
        //    ////        nbreSaison++;
        //    ////        double nbEpisode = ob.GetNamedNumber("episodes");
        //    ////        nbreEpisodes += (int)nbEpisode;
        //    ////    }
        //    ////}

        //    //string estDansMesSeries = string.Empty;
        //    //bool estArchive = false;
        //    //if (show.ContainsKey("is_in_account"))
        //    //{
        //    //    var mesInformations = await RecupererMesInformations();
        //    //    var series = mesInformations.Series;
        //    //    estDansMesSeries = show.GetNamedString("is_in_account");
        //    //    estArchive = series.ToList().Any(x => x.Url == url && x.EstArchive);
        //    //}

        //    //serieP.Description = description;
        //    //serieP.TvdbId = int.Parse(idTvDb);
        //    //serieP.EstArchive = estArchive;
        //    //serieP.NombreTotalEpisodes = 0;
        //    //serieP.NombreTotalSaisons = 0;
        //    //serieP.ChaineTV = network;
        //    //serieP.DureeMoyenne = duration;
        //    //serieP.Statut = statut;
        //    //return serieP;
        //}
    }
}