using System;
using System.Collections.Generic;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using Windows.Foundation.Metadata;

namespace BetaSeriesW8.DataModel
{
    [WebHostHidden]
    public class Utilisateur : BindableBase
    {
        private int _nombreDEpisodesRegardes;
        private int _nombreDeBadgesGagnes;
        private int _nombreDeSaisonsRegardes;
        private int _nombreDeSerieSuivies;
        private int _nombreEpisodeARegarder;
        private string _progressionGenerale;
        private List<Serie> _series;
        private DateTime _tempsPasseARegarderDesSeries;
        private DateTime _tempsRestantARegarderDesSeries;

        public int NombreDeSerieSuivies
        {
            get { return _nombreDeSerieSuivies; }
            set { SetProperty(ref _nombreDeSerieSuivies, value); }
        }

        public int NombreDeSaisonsRegardes
        {
            get { return _nombreDeSaisonsRegardes; }
            set { SetProperty(ref _nombreDeSaisonsRegardes, value); }
        }

        public int NombreDEpisodesRegardes
        {
            get { return _nombreDEpisodesRegardes; }
            set { SetProperty(ref _nombreDEpisodesRegardes, value); }
        }

        public string PogressionGenerale
        {
            get { return _progressionGenerale; }
            set { SetProperty(ref _progressionGenerale, value); }
        }

        public int NombreEpisodeARegarder
        {
            get { return _nombreEpisodeARegarder; }
            set { SetProperty(ref _nombreEpisodeARegarder, value); }
        }

        public DateTime TempsPasseARegarderDesSeries
        {
            get { return _tempsPasseARegarderDesSeries; }
            set { SetProperty(ref _tempsPasseARegarderDesSeries, value); }
        }

        public DateTime TempsRestantARegarderDesSeries
        {
            get { return _tempsRestantARegarderDesSeries; }
            set { SetProperty(ref _tempsRestantARegarderDesSeries, value); }
        }

        public int NombreDeBadgesGagnes
        {
            get { return _nombreDeBadgesGagnes; }
            set { SetProperty(ref _nombreDeBadgesGagnes, value); }
        }

        public List<Serie> Series
        {
            get { return _series; }
            set { SetProperty(ref _series, value); }
        }

        public DateTime DateMiseAJour { get; set; }

        public bool EstExpireDepuisPlusDUnHeure
        {
            get
            {
                var intervalle = DateTime.Now - DateMiseAJour;
                return intervalle.TotalHours > 1;
            }
        }
    }
}