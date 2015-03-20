using System.Collections.Generic;
using System.Collections.ObjectModel;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;

namespace BetaSeriesW8
{
    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class GroupeDeSerie : BindableBase
    {
        private readonly ObservableCollection<Serie> _items = new ObservableCollection<Serie>();
        private string _titreGroupe = string.Empty;
        private bool _estVisible;

        public GroupeDeSerie(string titreGroupe, IList<Serie> series)
        {
            _titreGroupe = titreGroupe;
            _items = new ObservableCollection<Serie>(series);
        }

        public string TitreGroupe
        {
            get { return _titreGroupe; }
            set { SetProperty(ref _titreGroupe, value); }
        }


        public bool EstVisible
        {
            get { return _estVisible; }
            set { SetProperty(ref _estVisible, value); }
        }

        public ObservableCollection<Serie> Series
        {
            get { return _items; }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class GroupeEpisode : BindableBase
    {
        private ObservableCollection<Episode> _items = new ObservableCollection<Episode>();
        private string _titreGroupe = string.Empty;

        public GroupeEpisode(string titreGroupe, IList<Episode> episodes)
        {
            _titreGroupe = titreGroupe;
            _items = new ObservableCollection<Episode>(episodes);
        }

        public string TitreGroupe
        {
            get { return _titreGroupe; }
            set { SetProperty(ref _titreGroupe, value); }
        }

        public ObservableCollection<Episode> Episodes
        {
            get { return _items; }
            set { _items = value; }
        }
    }
}