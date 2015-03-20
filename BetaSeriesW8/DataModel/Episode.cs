using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using BetaSeriesW8.Common;
using BetaSeriesW8.DataModel;
using BetaSeriesW8.Service;
using Windows.Foundation.Metadata;

namespace BetaSeriesW8.Data
{

    [WebHostHidden]
    public class Episode : BindableBase, IResizable
    {
        private string _showName;
        private string _titre;
        private string _description;
        private DateTime _date;
        private int _numeroEpisode;
        private int _numeroSaison;
        private string _showUrl;
        private string _numeroComplet;
        private Uri _image;
        private bool _estComplet;

        public string TitrePlanning
        {
            get { return string.Format("{0} - {1}", Jour, ShowName); }
        }

        public string SousTitrePlanning
        {
            get { return string.Format("{0} - {1}", NumeroComplet, Titre); }
        }

        public Uri Banniere
        {
            get { return ServicesBetaSeries.RecupererUneBanniere(_showUrl); }
        }

        public bool EstComplet
        {
            get { return _estComplet; }
            set { SetProperty(ref _estComplet, value); }
        }

        public string ShowName
        {
            get { return _showName; }
            set { SetProperty(ref _showName, value); }
        }

        public string Jour
        {
            get
            {
                switch (Date.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        return "Lundi";
                    case DayOfWeek.Tuesday:
                        return "Mardi";
                    case DayOfWeek.Wednesday:
                        return "Mercredi";
                    case DayOfWeek.Thursday:
                        return "Jeudi";
                    case DayOfWeek.Friday:
                        return "Vendredi";
                    case DayOfWeek.Saturday:
                        return "Samedi";
                    case DayOfWeek.Sunday:
                        return "Dimanche";
                    default:
                        return string.Empty;
                }
            }
        }

        public string Titre
        {
            get { return _titre; }
            set { SetProperty(ref _titre, value); }
        }
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value); }
        }
        public int NumeroEpisode
        {
            get { return _numeroEpisode; }
            set { SetProperty(ref _numeroEpisode, value); }
        }
        public int NumeroSaison
        {
            get
            {
                return _numeroSaison;
            }
            set { SetProperty(ref _numeroSaison, value); }
        }
        public string ShowUrl
        {
            get { return _showUrl; }
            set { SetProperty(ref _showUrl, value); }
        }
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        [XmlIgnore]
        public Uri Image
        {
            get { return _image; }
            set { SetProperty(ref _image, value); }
        }
        public string NumeroComplet
        {
            get
            {
                if(string.IsNullOrEmpty(_numeroComplet))
                {
                    var saison = (NumeroSaison.ToString().Length == 1) ? "S0" + NumeroSaison : "S"+NumeroSaison.ToString();
                    var episode = (NumeroEpisode.ToString().Length == 1) ? "E0" + NumeroEpisode : "E" + NumeroEpisode.ToString();
                    return saison + episode;
                }
                return _numeroComplet;
            }
            set { SetProperty(ref _numeroComplet, value); }
        }

        public string ResumeComplet { get { return string.Format("Episode {0} Diffusé le {3}", NumeroComplet, NumeroSaison, Titre, Date.Date.ToString("dd/MM/yyyy")); } }

        public Episode()
        {
            Column = 2;
            Row = 2;
        }

        public Episode(string titre, DateTime date, int saison, int episode, string showUrl, string showName, string image = null)
        {
            _showName = showName;
            _titre = titre;
            _date = date;
            _numeroEpisode = episode;
            _numeroSaison = saison;
            _showUrl = showUrl;
            if (!string.IsNullOrEmpty(image))
                _image = new Uri(image);
        }

        public string ResumeTop
        {
            get { return string.Format("{0} {1}", ShowName, NumeroComplet); }
        }

        public string DateS
        {
            get { return Date.ToString("dd/MM/yyyy"); }
        }

        public string Resume { get { return string.Format("Episode {0} / Saison {1} - {2} - Sorti le {3}", NumeroEpisode, NumeroSaison, Titre, Date.Date.ToString("dd/MM/yyyy")); } }
        public string Resume2 { get { return string.Format("Episode {0} - {1}", NumeroEpisode, Titre); } }

        public int Column { get; set; }
        public int Row { get; set; }
    }
}