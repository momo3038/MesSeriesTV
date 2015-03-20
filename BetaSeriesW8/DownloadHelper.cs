using System;
using System.Linq;
using System.Threading.Tasks;
using BetaSeriesW8.Data;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace BetaSeriesW8
{
    public static class DownloadHelper
    {
        public static async Task<DownloadOperation> Telecharger(SousTitre sousTitre, StorageFolder folder, bool uniquementTelechargerEtEnregistrerOuSRT)
        {
            StorageFile destinationFile;
            try
            {
                if ((folder != null && uniquementTelechargerEtEnregistrerOuSRT) || sousTitre.Fichier.Split('.').Last() == "srt")
                    destinationFile = await folder.CreateFileAsync(sousTitre.Fichier, CreationCollisionOption.GenerateUniqueName);
                else destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(sousTitre.Fichier, CreationCollisionOption.GenerateUniqueName);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception("Impossible d'enregistrer le sous-titre à cet emplacement. Accés non autorisé");
            }
            catch (Exception ex)
            {
                throw new Exception("Une erreur est survenue lors de la création du fichier sous-titre");
            }

            try
            {
                var downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(new Uri(sousTitre.Url), destinationFile);
                await download.StartAsync();
   
                return download;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}