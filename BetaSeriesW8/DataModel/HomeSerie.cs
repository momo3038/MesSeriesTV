using System;
using System.Xml.Serialization;
using Windows.Foundation.Metadata;

namespace BetaSeriesW8.DataModel
{
    [WebHostHidden]
    public class HomeSerie
    {
        [XmlIgnore]
        public Uri Image { get; set; }
        public string Titre { get; set; }
        public string Description { get; set; }
        public string SerieUrl { get; set; }

        public string ImageString
        {
            get { return Image.AbsoluteUri; }
        }
    }
}