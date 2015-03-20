using BetaSeriesW8.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Foundation.Metadata;

namespace BetaSeriesW8.Data
{
    /// <summary>
    ///     string title: title of the episode
    //int season: number of the season of the episode
    //int episode: number of the episode
    //string language: language of the subtitle
    //enum source: source of the subtitle (addic7ed, seriessub, soustitres, tvsubtitles, usub)
    //file file: name of the file of the subtitle
    //url url: url to the subtitle
    //int quality: quality of the subtitle (1 the worst, 5 the best)
    /// </summary>
    [WebHostHidden]
    public class SousTitre : BindableBase
    {
        public string Langue { get; set; }
        public string Source { get; set; }
        public string Fichier { get; set; }
        public string Url { get; set; }
        public int Qualite { get; set; }

        public string Episode { get; set; }

        public string Saison { get; set; }
    }
}
