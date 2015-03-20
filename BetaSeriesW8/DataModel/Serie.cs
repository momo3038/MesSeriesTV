using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BetaSeriesW8.Common;
using BetaSeriesW8.Service;
using Windows.Foundation.Metadata;

namespace BetaSeriesW8.Data
{
    [WebHostHidden]
    public class Serie : BindableBase
    {

        private Uri _background;
        private string _backgroundUri;
        private Uri _banniere;
        private string _banniereString;
        private string _chaineTV;
        private string _description;
        private string _dureeMoyenne;
        private int _episodesNonVus;
        private bool _estArchive;
        private bool _estComplete;
        private bool _estDansMesSeries;
        private bool _imageEstChargee;
        private IList<Uri> _images;
        private int _nombreTotalEpisodes;
        private int _nombreTotalSaisons;
        private bool _possedeDesEpisodesNonVus;
        private Episode _prochainEpisode;
        private bool _prochainEpisodeComplet;
        private string _statut;
        private string _titre = string.Empty;
        private int _tvdbId;
        private string _url = string.Empty;
        private double _note;
        private bool _isLoading = true;
        private List<BanniereTvDB> _bannieres;
        private Uri _fond;
        private string _fondString;
        private string _posterString;
        private bool _fondEnCoursDeTelechargement;
        private bool _doitRecupererLesBannieres;

        public Serie()
        {
        }

        public Serie(string titre, string description, string url, int idTvDb = 0)
        {
            _description = description;
            //BanniereString = ServicesBetaSeries.RecupererUneBanniere(url).AbsoluteUri;
            //BanniereString = ServicesTvDb.RecupererUneBanniere(idTvDb);
            _titre = titre;
            _tvdbId = idTvDb;
            _url = url;
        }


        public Serie(string titre, string url)
        {
            _titre = titre;
            _url = url;
        }

        public string Statut
        {
            get { return _statut; }
            set
            {
                switch (value)
                {
                    case "Continuing":
                        SetProperty(ref _statut, "Série en cours");
                        break;
                    case "Ended":
                        SetProperty(ref _statut, "Série terminée");
                        break;
                    case "On Hiatus":
                        SetProperty(ref _statut, "Série en pause");
                        break;
                    case "Other":
                        SetProperty(ref _statut, "Statut de la Série  inconnu");
                        break;
                    default:
                        SetProperty(ref _statut, value);
                        break;
                }
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public bool EstComplete
        {
            get { return _estComplete; }
            set { SetProperty(ref _estComplete, value); }
        }

        public bool ImageEstChargee
        {
            get { return _imageEstChargee; }
            set { SetProperty(ref _imageEstChargee, value); }
        }

        public bool EstDansMesSeries
        {
            get { return _estDansMesSeries; }
            set { SetProperty(ref _estDansMesSeries, value); }
        }

        public Episode ProchainEpisode
        {
            get { return _prochainEpisode; }
            set { SetProperty(ref _prochainEpisode, value); }
        }

        public string ResumeProchainEpisode
        {
            get
            {
                if (ProchainEpisode != null)
                    return ProchainEpisode.Resume;
                return null;
            }
        }

        public string Titre
        {
            get { return _titre; }
            set { SetProperty(ref _titre, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        [XmlIgnore]
        public Uri Background
        {
            get { return _background; }
            set
            {
                SetProperty(ref _background, value);
                if (_background != null)
                    BackgroundUri = _background.AbsoluteUri;
            }
        }

        public string BackgroundUri
        {
            get { return _backgroundUri; }
            set { SetProperty(ref _backgroundUri, value); }
        }

        public string BanniereString
        {
            get { return _banniereString; }
            set
            {
                SetProperty(ref _banniereString, value);
                Banniere = new Uri(_banniereString);
            }
        }

        [XmlIgnore]
        public Uri Banniere
        {
            get { return _banniere; }
            set { SetProperty(ref _banniere, value); }
        }

        public string Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        public bool EstArchive
        {
            get { return _estArchive; }
            set { SetProperty(ref _estArchive, value); }
        }

        public bool PossedeDesEpisodesNonVus
        {
            get { return _possedeDesEpisodesNonVus; }
            set { SetProperty(ref _possedeDesEpisodesNonVus, value); }
        }

        public int EpisodesNonVus
        {
            get { return _episodesNonVus; }
            set
            {
                SetProperty(ref _episodesNonVus, value);
                OnPropertyChanged("PossedeDesEpisodesNonVus");
            }
        }

        public int NombreTotalEpisodes
        {
            get { return _nombreTotalEpisodes; }
            set { SetProperty(ref _nombreTotalEpisodes, value); }
        }

        public int NombreTotalSaisons
        {
            get { return _nombreTotalSaisons; }
            set { SetProperty(ref _nombreTotalSaisons, value); }
        }

        public string ChaineTV
        {
            get { return _chaineTV; }
            set { SetProperty(ref _chaineTV, value); }
        }

        public string DureeMoyenne
        {
            get { return _dureeMoyenne; }
            set { SetProperty(ref _dureeMoyenne, value); }
        }

        public bool ProchainEpisodeComplet
        {
            get { return _prochainEpisodeComplet; }
            set { SetProperty(ref _prochainEpisodeComplet, value); }
        }

        public int TvdbId
        {
            get { return _tvdbId; }
            set
            {
                SetProperty(ref _tvdbId, value);

            }
        }

        [XmlIgnore]
        public IList<Uri> Images
        {
            get { return _images; }
            set { SetProperty(ref _images, value); }
        }

        public double Note
        {
            get { return _note; }
            set { SetProperty(ref _note, value); }
        }

        [XmlIgnore]
        public List<BanniereTvDB> BannieresFanArt
        {
            get
            {
                if (Bannieres != null)
                    return Bannieres.ToList().Where(x => x.Type == "fanart").OrderByDescending(x => x.Rating).ToList();
                return new List<BanniereTvDB>();
            }
        }

        [XmlIgnore]
        public List<BanniereTvDB> BannieresPoster
        {
            get
            {
                if (Bannieres != null)
                    return Bannieres.ToList().Where(x => x.Type == "poster").OrderByDescending(x => x.Rating).ToList();
                return new List<BanniereTvDB>();
            }
        }

        [XmlIgnore]
        public List<BanniereTvDB> BannieresPosterMiniature
        {
            get
            {
                var bann = new List<BanniereTvDB>();
                if (BannieresPoster == null)
                    return new List<BanniereTvDB>();

                foreach (var banniereTvDb in BannieresPoster)
                {
                    var miniature = banniereTvDb.Clone();
                    miniature.Path = miniature.Path.Replace("posters", "_cache/posters");
                    bann.Add(miniature);
                }

                return bann;
            }
        }

        public List<BanniereTvDB> Bannieres
        {
            get { return _bannieres; }
            set { SetProperty(ref _bannieres, value); }
        }

        [XmlIgnore]
        public Uri Fond
        {
            get { return _fond; }
            set { SetProperty(ref _fond, value); }
        }

        public string FondString
        {
            get { return _fondString; }
            set { SetProperty(ref _fondString, value); }
        }

        public string PosterString
        {
            get { return _posterString; }
            set { SetProperty(ref _posterString, value);
            }
        }


        public Uri BanniereBs
        {
            get
            {
                    return ServicesBetaSeries.RecupererUneBanniere(_url);

            }
        }

        public bool FondEnCoursDeTelechargement
        {
            get { return _fondEnCoursDeTelechargement; }
            set { SetProperty(ref _fondEnCoursDeTelechargement, value); }
        }

        public bool DoitRecupererLesBannieres
        {
            get { return _doitRecupererLesBannieres; }
            set { SetProperty(ref _doitRecupererLesBannieres, value); }
        }

        public async Task<Uri> LoadBackgroundImageUri()
        {
            try
            {
                Uri image = await ServicesTvDb.RecupererUneImage(_tvdbId);
                ImageEstChargee = true;
                return image;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async void LoadBackgroundImage()
        {
            try
            {
                Background = await ServicesTvDb.RecupererUneImage(_tvdbId);
                ImageEstChargee = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Copier(Serie serieT)
        {
            _background = serieT.Background;
            _backgroundUri = serieT.BackgroundUri;
            _banniere = serieT.Banniere;
            _banniereString = serieT.BanniereString;
            _chaineTV = serieT.ChaineTV;
            _description = serieT.Description;
            _dureeMoyenne = serieT.DureeMoyenne;
            _episodesNonVus = serieT.EpisodesNonVus;
            _estArchive = serieT.EstArchive;
            _estComplete = serieT.EstComplete;
            _estDansMesSeries = serieT.EstDansMesSeries;
            _imageEstChargee = serieT.ImageEstChargee;
            _images = serieT.Images;
            _nombreTotalEpisodes = serieT.NombreTotalEpisodes;
            _nombreTotalSaisons = serieT.NombreTotalSaisons;
            _possedeDesEpisodesNonVus = serieT.PossedeDesEpisodesNonVus;
            _prochainEpisode = serieT.ProchainEpisode;
            _prochainEpisodeComplet = serieT.ProchainEpisodeComplet;
            _statut = serieT.Statut;
            _titre = serieT.Titre;
            _tvdbId = serieT.TvdbId;
            _url = serieT.Url;
            _note = serieT.Note;
            _isLoading = serieT.IsLoading;
            _bannieres = serieT.Bannieres;
            _fond = serieT.Fond;
            _fondString = serieT.FondString;
            _posterString = serieT.PosterString;
            _fondEnCoursDeTelechargement = serieT.FondEnCoursDeTelechargement;
            _doitRecupererLesBannieres = serieT.DoitRecupererLesBannieres;
        }
    }
}