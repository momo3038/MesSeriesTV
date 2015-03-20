using BetaSeriesW8.Data;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace BetaSeriesW8.ViewModel
{
    public class ViewModelFicheSerie
    {
        public Serie Serie { get; set; }

        public Brush CouleurStatut
        {
            get
            {
                switch (Serie.Statut)
                {
                    case "Série en cours":
                        return new SolidColorBrush(Color.FromArgb(255, 10, 109, 5));
                    case "Série terminée":
                        return new SolidColorBrush(Color.FromArgb(255, 204,0,0));
                    case "Série en pause":
                        return new SolidColorBrush(Color.FromArgb(255, 209, 58, 0));
                    case "Statut de la Série  inconnu":
                        return new SolidColorBrush(Color.FromArgb(255, 139, 79, 23));
                    default:
                        return new SolidColorBrush(Colors.Olive);
                }
            }
        }
    }
}