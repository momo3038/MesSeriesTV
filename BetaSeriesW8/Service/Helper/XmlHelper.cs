using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace BetaSeriesW8.Service
{
    internal class XmlHelper
    {
        public static async Task<XmlDocument> RecupererXml(Uri uri)
        {
            try
            {
                return await XmlDocument.LoadFromUriAsync(uri);
            }
            catch (Exception)
            {
                return new XmlDocument();
            }

        }
    }
}