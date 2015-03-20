using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace BetaSeriesW8.Service
{
    internal class TvDbAPI
    {
        private const string KEY = "40E1F1234011DADC";

        private static Uri BackgroundImageUri(int tvdbId)
        {
            return new Uri(string.Format("http://www.thetvdb.com/api/{0}/series/{1}/banners.xml", KEY, tvdbId));
        }


        public async Task<XmlDocument> Background(int tbDbId)
        {
            return await XmlHelper.RecupererXml(BackgroundImageUri(tbDbId));
        }
    }
}
