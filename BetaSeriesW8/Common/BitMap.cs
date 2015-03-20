using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BetaSeriesW8.Service;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace BetaSeriesW8.Common
{
    public class BitMap
    {
        public static async Task<Uri> GetLocalImageAsync(string internetUri, string uniqueName)
        {
            if (string.IsNullOrEmpty(internetUri))
            {
                return null;
            }

            var desiredName = string.Format("{0}.jpg", uniqueName);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(desiredName, CreationCollisionOption.ReplaceExisting);

            DownloadOperation download = BetaSerieData.Downloader.CreateDownload(new Uri(internetUri), file);
            var d = await download.StartAsync().AsTask();
            if (d.GetResponseInformation().StatusCode == 200)
                return new Uri(string.Format("ms-appdata:///local/{0}.jpg", uniqueName), UriKind.Absolute);
            return null;
            //using (var response = await HttpWebRequest.CreateHttp(internetUri).GetResponseAsync())
            //{
            //    using (var stream = response.GetResponseStream())
            //    {
            //        using (var filestream = await file.OpenStreamForWriteAsync())
            //        {
            //            await stream.CopyToAsync(filestream);
            //            await stream.FlushAsync();
            //            stream.Dispose();
            //        }

            //        //using (var fs = await file.OpenAsync(FileAccessMode.ReadWrite))
            //        //{
            //        //    var outStream = fs.GetOutputStreamAt(0);
            //        //    var dataWriter = new Windows.Storage.Streams.DataWriter(outStream);
            //        //    await dataWriter.StoreAsync();
            //        //    dataWriter.DetachStream();
            //        //    await outStream.FlushAsync();
            //        //    outStream.Dispose(); 
            //        //    fs.Dispose();
            //        //    return new Uri(string.Format("ms-appdata:///local/{0}.jpg", uniqueName), UriKind.Absolute);
            //        //}

            //    }
            //}

        }

        public static async Task<Uri> GetLocalSavedImageAsync(string uniqueName, string extension)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(uniqueName + extension);
                return new Uri(string.Format("ms-appdata:///local/{0}.jpg", uniqueName), UriKind.Absolute);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
