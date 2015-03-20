using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetaSeriesW8.Data;
using BetaSeriesW8.DataModel;
using BetaSeriesW8.Service.TileContent;
using Windows.UI.Notifications;

namespace BetaSeriesW8.Service
{
    public static class ServicesTiles
    {
        public static async Task MettreAJourLesTilesEpisodes()
        {
            if (BetaSerieData.EstConnecte)
            {
                Cache.FaireExpirer(Cache.Episodes);
                var mesEpisodesAregarder = await ServicesBetaSeries.RecupererMesEpisodesRestantARegarder();
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                if(mesEpisodesAregarder != null && mesEpisodesAregarder.Count == 0)
                    TileUpdateManager.CreateTileUpdaterForApplication().Clear();

                foreach (var episode in mesEpisodesAregarder.OrderByDescending(x => x.Date))
                {
                    ITileWideImageAndText02 tileContent = TileContentFactory.CreateTileWideImageAndText02();
                    tileContent.TextCaption1.Text = episode.ShowName;
                    tileContent.TextCaption2.Text = episode.NumeroComplet;

                    tileContent.Image.Src = episode.Banniere.AbsoluteUri;

                    tileContent.RequireSquareContent = false;
                    TileUpdateManager.CreateTileUpdaterForApplication().Update(tileContent.CreateNotification());
                }
            }
            else TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }
    }
}
