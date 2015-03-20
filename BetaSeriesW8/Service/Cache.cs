using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetaSeriesW8.Common;
using BetaSeriesW8.Data;
using BetaSeriesW8.DataModel;
using Windows.Storage;
using System.Collections.ObjectModel;

namespace BetaSeriesW8.Service
{
    public class Cache
    {
        public static async Task<bool> Existe<T>()
        {
            var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
            return await objectStorageHelper.Exist();
        }

        public static async Task<bool> Existe<T>(string key)
        {
            try
            {
                var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
                return await objectStorageHelper.Exist(key);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static async Task<bool> Ajouter<T>(T series)
        {
            try
            {
                var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
                await objectStorageHelper.SaveAsync(series);
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static async Task<bool> Ajouter<T>(T series, string key)
        {
            try
            {
                var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
                await objectStorageHelper.SaveAsync(series, key);
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static async Task<T> Recuperer<T>()
        {
            try
            {
                var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
                return await objectStorageHelper.LoadAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static async Task<T> Recuperer<T>(string key)
        {
            try
            {
                var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
                return await objectStorageHelper.LoadAsync(key);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static async Task<bool> Supprimer<T>(string name)
        {
            var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
            await objectStorageHelper.DeleteAsync(name);
            return true;
        }

        public static async Task<bool> Supprimer<T>()
        {
            var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
            await objectStorageHelper.DeleteAsync();
            return true;
        }

        public static bool EstExpire(string element)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(element + "_estExpire_" + BetaSerieData.Login))
            {
                var estExpire = (bool)ApplicationData.Current.LocalSettings.Values[element + "_estExpire_" + BetaSerieData.Login];

                if (estExpire)
                    return true;

                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(element + "_dateMiseAJour_" +BetaSerieData.Login))
                {
                    var miseAJour = (string)ApplicationData.Current.LocalSettings.Values[element + "_dateMiseAJour_" + BetaSerieData.Login];
                    var dateMiseAJour = DateTime.Parse(miseAJour);
                    return (DateTime.Now - dateMiseAJour).TotalHours > 1;
                }
            }
            return true;
        }

        public static void AEteMisAJour(string element)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(element + "_estExpire_" + BetaSerieData.Login))
            {
                ApplicationData.Current.LocalSettings.Values[element + "_estExpire_" + BetaSerieData.Login] = false;
                ApplicationData.Current.LocalSettings.Values[element + "_dateMiseAJour_" + BetaSerieData.Login] = DateTime.Now.ToString();
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values.Add(new KeyValuePair<string, object>(element + "_estExpire_" + BetaSerieData.Login, false));
                ApplicationData.Current.LocalSettings.Values.Add(new KeyValuePair<string, object>(element + "_dateMiseAJour_" + BetaSerieData.Login, DateTime.Now.ToString()));
            }
        }

        public static string Token
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Token"))
                    return ApplicationData.Current.LocalSettings.Values["Token"] as string;
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    ApplicationData.Current.LocalSettings.Values["Token"] = value;
            }
        }

        public const string Series = "Series";
        public const string Utilisateur = "Utilisateur";
        public const string Episodes = "Episodes";

        public static void FaireExpirer(string element)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(element + "_estExpire_" + BetaSerieData.Login))
            {
                ApplicationData.Current.LocalSettings.Values[element + "_estExpire_" + BetaSerieData.Login] = true;
            }
            else
                ApplicationData.Current.LocalSettings.Values.Add(new KeyValuePair<string, object>(
                                                                     element + "_estExpire_" + BetaSerieData.Login, true));
        }

        public static async Task AjouterUneSerie(Serie serie)
        {
            var cacheSerie = await Recuperer<List<string>>();
            cacheSerie.Add(serie.Url);
            Ajouter(cacheSerie);
        }

        public static async void SupprimerTout()
        {
            var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
            foreach (var file in files)
            {
                file.DeleteAsync();
            }
            ServiceToast.Afficher("Le cache à été supprimé");
        }

        public static async void SupprimerUneSerie(Serie serie)
        {
            var cacheSerie = await Recuperer<List<string>>();
            var liste = cacheSerie.ToList();
            liste.RemoveAll(x => x == serie.Url);
            Ajouter(new List<string>(liste));
            Ajouter(serie, serie.Url);
        }

        public static async void ArchiverUneSerie(Serie serie)
        {
            serie.EstDansMesSeries = true;
            serie.EstArchive = true;
            var cacheSerie = await Recuperer<Serie>(serie.Url);
            cacheSerie.EstDansMesSeries = true;
            cacheSerie.EstArchive = true;
            Ajouter(cacheSerie, serie.Url);
        }

        public static async void DesarchiverUneSerie(Serie serie)
        {
            serie.EstDansMesSeries = true;
            serie.EstArchive = false;
            var cacheSerie = await Recuperer<Serie>(serie.Url);
            cacheSerie.EstDansMesSeries = true;
            cacheSerie.EstArchive = false;
            Ajouter(cacheSerie, serie.Url);
        }
    }
}