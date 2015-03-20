using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using BetaSeriesW8.Common;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.Storage;

namespace BetaSeriesW8.Service
{
    [WebHostHidden]
    public class BanniereTvDB : BindableBase
    {
        public string Serie;
        private string _localPath;
        private bool _fondEnCoursDeTelechargement;

        public BanniereTvDB Clone()
        {
            return new BanniereTvDB()
                       {
                           Path = this.Path,
                           Rating = this.Rating,
                           Serie = this.Serie,
                           Type = this.Type
                       };
        }
        protected BanniereTvDB()
        {
            
        }

        public BanniereTvDB(string serie)
        {
            Serie = serie;
        }

        public string LocalPath
        {
           get { return _localPath; }
            set { SetProperty(ref _localPath, value); }
        }

        public string Path { get; set; }
        public string Rating { get; set; }

        public double RatingD
        {
            get { 
                float result;
                if (float.TryParse(Rating, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                    return result;
                return result;
            }
        }

        public string Type { get; set; }

        public string UniqueName
        {
            get
            {
                var name = Path.Split('/').Last();
                name = name.Replace(".jpg", string.Empty);
                return Serie + "_" + Type + "_" + name;
            }
        }

        public bool FondEnCoursDeTelechargement
        {
            get { return _fondEnCoursDeTelechargement; }
            set { SetProperty(ref _fondEnCoursDeTelechargement, value); }
        }

        public async Task<Uri> Recuperer()
        {
            var image = await BitMap.GetLocalSavedImageAsync(UniqueName, ".jpg");
            if (image == null)
                return await BitMap.GetLocalImageAsync(Path, UniqueName);
            return image;
        }
    }

    public static class ServicesTvDb
    {
        public static async Task<IList<BanniereTvDB>> RecupererLesBannieres(int id, string url)
        {
            try
            {
                var result = new TvDbAPI();
                XmlDocument resultJson = await result.Background(id);

                if (resultJson == null)
                    return null;

                var nodes = resultJson.SelectNodes("Banners/Banner");

                var banns = new List<BanniereTvDB>();

                foreach (var node in nodes)
                {
                    if (node.ChildNodes.ElementAt(0) != null)
                    {
                        var banniere = new BanniereTvDB(url);

                        if (node.ChildNodes.ElementAtOrDefault(3) != null)
                            banniere.Path = new Uri("http://www.thetvdb.com/banners/" + node.ChildNodes[3].InnerText).AbsoluteUri;

                        if (node.ChildNodes.ElementAtOrDefault(5) != null)
                            banniere.Type = node.ChildNodes[5].InnerText;

                        if (node.ChildNodes.ElementAtOrDefault(13) != null)
                            banniere.Rating = node.ChildNodes[13].InnerText;

                        banns.Add(banniere);
                    }
                }

                return banns;
            }
            catch (Exception)
            {
                    
                throw;
            }
        }

        public static async Task<Uri> RecupererUneBanniere(int id, int? increment)
        {
            var result = new TvDbAPI();
            XmlDocument resultJson = await result.Background(id);

            if (resultJson == null)
                return null;

            var nodes = resultJson.SelectNodes("Banners/Banner");

            foreach (var node in nodes)
            {
                if (node.ChildNodes.ElementAt(0) != null)
                {
                    if (node.ChildNodes.ElementAtOrDefault(3) != null)
                        return new Uri("http://www.thetvdb.com/banners/posters/" + node.ChildNodes[3].InnerText);

                    if (node.ChildNodes.ElementAtOrDefault(5) != null)
                        return new Uri("http://www.thetvdb.com/banners/posters/" + node.ChildNodes[5].InnerText);

                    if (node.ChildNodes.ElementAtOrDefault(13) != null)
                        return new Uri("http://www.thetvdb.com/banners/posters/" + node.ChildNodes[5].InnerText);
                }
            }
            //var banners = resultJson.GetElementsByTagName("Banner").Where(x => x.ChildNodes.);


            return null;
        }

        public static async Task<Uri> RecupererUneImage(int id)
        {
            var result = new TvDbAPI();
            XmlDocument resultJson = await result.Background(id);

            if (resultJson == null)
                return null;
            var banners = resultJson.GetElementsByTagName("Banner");

            if (banners.ElementAtOrDefault(0) != null)
            {
                IXmlNode banner = banners[0];
                if (banner.ChildNodes.ElementAtOrDefault(3) != null)
                    return new Uri("http://www.thetvdb.com/banners/" + banners[0].ChildNodes[3].InnerText);
            }
            return null;
        }

        public static async Task<IList<Uri>> RecupererLesImagesHD(int tvdbId, int? max = null)
        {
            var result = new TvDbAPI();
            XmlDocument resultJson = await result.Background(tvdbId);

            if (resultJson == null)
                return null;
            XmlNodeList banners = resultJson.GetElementsByTagName("Banner");

            IList<Uri> images = new List<Uri>();

            int index = 0;

            foreach (IXmlNode banner in banners)
            {
                if (max.HasValue && index == max.Value)
                    return images;

                if (banner.ChildNodes.ElementAtOrDefault(3) != null
                    && banner.ChildNodes.ElementAtOrDefault(5) != null
                    && banner.ChildNodes[5].InnerText == "fanart")
                {
                    //if (banner.ChildNodes.ElementAtOrDefault(7) != null)
                    //{
                    //    banner.ChildNodes[7].InnerText
                    //}

                    images.Add(new Uri("http://www.thetvdb.com/banners/" + banner.ChildNodes[3].InnerText));
                    index++;
                }
            }
            return images;
        }
    }
}