using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BetaSeriesW8.Data;

namespace BetaSeriesW8.Service
{
    public class BetaSerieAPI
    {
        public HttpClient Client;

        private BetaSerieAPI()
        {
           Client = new HttpClient { MaxResponseContentBufferSize = 1024 * 1024 };
        }

        private static BetaSerieAPI _instance;
        public static BetaSerieAPI Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new BetaSerieAPI();
                return _instance;
            }
        }

        private const string Key = "e84a30a05b33";

        private Uri AfficherToutesLesSeries
        {
            get { return new Uri(string.Format("http://api.betaseries.com/shows/display/all.json?key={0}", Key)); }
        }

        private Uri AfficherTousLesEpisodesDiffusés
        {
            get { return new Uri(string.Format("http://api.betaseries.com/planning/general.json?key={0}", Key)); }
        }


        private Uri MesInformations
        {
            get
            {
                return
                    new Uri(string.Format("http://api.betaseries.com/members/infos/{0}.json?key={1}&token={2}",
                                          BetaSerieData.Login, Key, BetaSerieData.Token));
            }
        }

        private Uri MesEpisodesRestantARegarder(bool uniquementLesPremiers)
        {
            string option = string.Empty;
            if (uniquementLesPremiers)
                option = "&view=next";

            return
                new Uri(string.Format("http://api.betaseries.com/members/episodes/all.json?key={0}&token={1}{2}",Key, BetaSerieData.Token, option));
        }

        private Uri ProchainEpisodeUri(string serieUrl)
        {
            return
                new Uri(string.Format("http://api.betaseries.com/members/episodes/all.json?key={0}&view=next&show={1}&view=next&token={2}", Key, serieUrl, BetaSerieData.Token));
        }

        private Uri Authentifier(string login, string md5)
        {
            return
                new Uri(string.Format("http://api.betaseries.com/members/auth.json?login={0}&password={1}&key={2}",
                                      login, md5, Key));
        }

        private Uri RecupererLaSerie(string nomSerie)
        {
            return
                new Uri(string.Format("http://api.betaseries.com/shows/display/{0}.json?key={1}&token={2}", nomSerie,
                                      Key, BetaSerieData.Token));
        }

        private Uri Ajouter(string url)
        {
            return
                new Uri(string.Format("http://api.betaseries.com/shows/add/{0}.json?key={1}&token={2}", url, Key,
                                      BetaSerieData.Token));
        }

        private Uri Supprimer(string url)
        {
            return
                new Uri(string.Format("http://api.betaseries.com/shows/remove/{0}.json?key={1}&token={2}", url, Key,
                                      BetaSerieData.Token));
        }

        private Uri Archiver(string url)
        {
            return
                new Uri(string.Format("http://api.betaseries.com/shows/archive/{0}.json?key={1}&token={2}", url, Key,
                                      BetaSerieData.Token));
        }

        private Uri Desarchiver(string url)
        {
            return
                new Uri(string.Format("http://api.betaseries.com/shows/unarchive/{0}.json?key={1}&token={2}", url, Key,
                                      BetaSerieData.Token));
        }

        private Uri Inscrire(string nomUtilisateur, string email, string motDePasse)
        {
            return
                new Uri(
                    string.Format(
                        "http://api.betaseries.com/members/signup.json?login={0}&password={1}&mail={2}&key={3}",
                        nomUtilisateur, motDePasse, email, Key));
        }

        private Uri RechercherUneSerie(string text)
        {
            return new Uri(string.Format("http://api.betaseries.com/shows/search.json?title={0}&key={1}&token={2}", text, Key, BetaSerieData.Token));
        }

        private Uri EpisodeUri(string serieUrl, int saison, int episode)
        {
            return
                new Uri(
                    string.Format(
                        "http://api.betaseries.com/shows/episodes/{0}.json?season={1}&episode={2}&key={3}", serieUrl, saison, episode, Key));
        }

        private Uri MarquerEpisodeCommeVuUri(Episode prochainEpisode)
        {
            return
                new Uri(
                    string.Format(
                        "http://api.betaseries.com/members/watched/{0}.json?season={1}&episode={2}&key={3}&token={4}", prochainEpisode.ShowUrl, prochainEpisode.NumeroSaison, prochainEpisode.NumeroEpisode, Key, BetaSerieData.Token));
        }

        private Uri Deconnection()
        {
            return
                new Uri(
                    string.Format(
                        "http://api.betaseries.com/members/destroy.json?key={0}&token={1}", Key, BetaSerieData.Token));
        }

        private Uri SousTitre(Episode episode)
        {
            return
                new Uri(
                    string.Format(
                        "http://api.betaseries.com/subtitles/show/{0}.json?season={1}&episode={2}&key={3}&token={4}", episode.ShowUrl, episode.NumeroSaison, episode.NumeroEpisode, Key, BetaSerieData.Token));
        }

        private Uri Episodes(string serieUrl, int numeroSaison)
        {
            return
                           new Uri(
                               string.Format(
                                   "http://api.betaseries.com/shows/episodes/{0}.json?season={1}&key={2}&token={3}", serieUrl, numeroSaison, Key, BetaSerieData.Token));
        }

        public async Task<string> AllSeries()
        {
            return await RecupererJSon(AfficherToutesLesSeries);
        }

        public async Task<string> Login(string login, string md5)
        {
            return await RecupererJSon(Authentifier(login, md5));
        }

        private async Task<string> RecupererJSon(Uri uri)
        {
            HttpResponseMessage response = await Client.GetAsync(uri);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> RecupererMesInformations()
        {
            return await RecupererJSon(MesInformations);
        }

        public async Task<string> RecupererMesEpisodesRestantARegarder(bool uniquementLesPremiers)
        {
            return await RecupererJSon(MesEpisodesRestantARegarder(uniquementLesPremiers));
        }

        public async Task<string> RecupererUneFicheSerie(string serieName)
        {
            return await RecupererJSon(RecupererLaSerie(serieName));
        }

        public async Task<string> RecupererMonPlanning()
        {
            return await RecupererJSon(AfficherTousLesEpisodesDiffusés);
        }

        public async Task<string> Rechercher(string text)
        {
            return await RecupererJSon(RechercherUneSerie(text));
        }

        public async Task<string> AjouterUneSeries(string url)
        {
            return await RecupererJSon(Ajouter(url));
        }

        public async Task<string> SupprimerUneSeries(string url)
        {
            return await RecupererJSon(Supprimer(url));
        }

        public async Task<string> ArchiverUneSerie(string url)
        {
            return await RecupererJSon(Archiver(url));
        }

        public async Task<string> DesarchiverUneSerie(string url)
        {
            return await RecupererJSon(Desarchiver(url));
        }

        public async Task<string> Inscription(string nomUtilisateur, string email, string motDePasse)
        {
            return await RecupererJSon(Inscrire(nomUtilisateur, email, motDePasse));
        }

        public async Task<string> Episode(string serieUrl, int saison, int episode)
        {
            return await RecupererJSon(EpisodeUri(serieUrl,saison, episode));
        }

        public async Task<string> MarquerUnEpisodeCommeVu(Episode prochainEpisode)
        {
            return await RecupererJSon(MarquerEpisodeCommeVuUri(prochainEpisode));
        }

        public async Task<string> SeDeconnecter()
        {
            return await RecupererJSon(Deconnection());
        }


        public async Task<string> ProchainEpisodePourLaSerie(string serieUrl)
        {
            return await RecupererJSon(ProchainEpisodeUri(serieUrl));
        }

        public async Task<string> RecupererLesSousTitre(Data.Episode episode)
        {
            return await RecupererJSon(SousTitre(episode));
        }

        public async Task<string> RecupererLesEpisodes(string urlSerie, int numerosaison)
        {
            return await RecupererJSon(Episodes(urlSerie, numerosaison));
        }
    }
}