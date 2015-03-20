using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BetaSeriesW8.Service;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BetaSeriesW8.Common
{
    public enum StorageType
    {
        Roaming, Local, Temporary
    }

    public class ObjectStorageHelper<T>
    {
        private ApplicationData appData = Windows.Storage.ApplicationData.Current;
        private XmlSerializer serializer;
        private StorageType storageType;

        private string FileName(T Obj)
        {
            return String.Format("{0}_{1}.xml", Obj.GetType().FullName, BetaSerieData.Login);
        }

        private string FileName(T Obj, string key)
        {
            return String.Format("{0}_{1}_{2}.xml", Obj.GetType().FullName, key, BetaSerieData.Login);
        }

        public ObjectStorageHelper(StorageType StorageType)
        {
            serializer = new XmlSerializer(typeof(T));
            storageType = StorageType;
        }

        public async Task<bool> DeleteAsync(string key)
        {
            try
            {
                StorageFolder folder = GetFolder(storageType);

                var file = await GetFileIfExistsAsync(folder, FileName(Activator.CreateInstance<T>(), key));
                if (file != null)
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        public async Task<bool> DeleteAsync()
        {
            try
            {
                StorageFolder folder = GetFolder(storageType);

                var file = await GetFileIfExistsAsync(folder, FileName(Activator.CreateInstance<T>()));
                if (file != null)
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        public async Task<bool> SaveAsync(T Obj)
        {
            try
            {
                if (Obj != null)
                {
                    StorageFile file = null;
                    StorageFolder folder = GetFolder(storageType);
                    file = await folder.CreateFileAsync(FileName(Obj), CreationCollisionOption.ReplaceExisting);

                    using (IRandomAccessStream writeStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        Stream outStream = Task.Run(() => writeStream.AsStreamForWrite()).Result;
                        serializer.Serialize(outStream, Obj);
                        await outStream.FlushAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        public async Task<bool> SaveAsync(T Obj, string key)
        {
            try
            {
                if (Obj != null)
                {
                    StorageFile file = null;
                    StorageFolder folder = GetFolder(storageType);
                    file = await folder.CreateFileAsync(FileName(Obj, key), CreationCollisionOption.ReplaceExisting);

                    using (IRandomAccessStream writeStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        Stream outStream = Task.Run(() => writeStream.AsStreamForWrite()).Result;
                        serializer.Serialize(outStream, Obj);
                        await outStream.FlushAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }

        public async Task<T> LoadAsync()
        {
            try
            {
                StorageFile file = null;
                StorageFolder folder = GetFolder(storageType);
                file = await folder.GetFileAsync(FileName(Activator.CreateInstance<T>()));
                using (IRandomAccessStream readStream = await file.OpenReadAsync())
                {
                    Stream inStream = Task.Run(() => readStream.AsStreamForRead()).Result;
                    var resu = (T)serializer.Deserialize(inStream);
                    await inStream.FlushAsync();
                    return resu;
                }
            }
            catch (FileNotFoundException)
            {
                //file not existing is perfectly valid so simply return the default 
                return default(T);
                //Interesting thread here: How to detect if a file exists (http://social.msdn.microsoft.com/Forums/en-US/winappswithcsharp/thread/1eb71a80-c59c-4146-aeb6-fefd69f4b4bb)
                //throw;
            }
            catch (Exception)
            {
                //Unable to load contents of file
                throw;
            }
        }


        public async Task<T> LoadAsync(string key)
        {
            try
            {
                StorageFile file = null;
                StorageFolder folder = GetFolder(storageType);
                file = await folder.GetFileAsync(FileName(Activator.CreateInstance<T>(), key));
                using (IRandomAccessStream readStream = await file.OpenReadAsync())
                {
                    Stream inStream = Task.Run(() => readStream.AsStreamForRead()).Result;
                    var resu = (T)serializer.Deserialize(inStream);
                    await inStream.FlushAsync();
                    return resu;
                }
            }
            catch (FileNotFoundException)
            {
                //file not existing is perfectly valid so simply return the default 
                return default(T);
                //Interesting thread here: How to detect if a file exists (http://social.msdn.microsoft.com/Forums/en-US/winappswithcsharp/thread/1eb71a80-c59c-4146-aeb6-fefd69f4b4bb)
                //throw;
            }
            catch (Exception)
            {
                //Unable to load contents of file
                throw;
            }
        }

        private StorageFolder GetFolder(StorageType storageType)
        {
            StorageFolder folder;
            switch (storageType)
            {
                case StorageType.Roaming:
                    folder = appData.RoamingFolder;
                    break;
                case StorageType.Local:
                    folder = appData.LocalFolder;
                    break;
                case StorageType.Temporary:
                    folder = appData.TemporaryFolder;
                    break;
                default:
                    throw new Exception(String.Format("Unknown StorageType: {0}", storageType));
            }
            return folder;
        }

        public static async Task<StorageFile> GetFileIfExistsAsync(StorageFolder folder, string fileName)
        {
            try
            {
                return await folder.GetFileAsync(fileName);

            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> Exist()
        {
            StorageFolder folder = GetFolder(storageType);
            var file = await GetFileIfExistsAsync(folder, FileName(Activator.CreateInstance<T>()));
            return file != null;
        }

        public async Task<bool> Exist(string key)
        {
            StorageFolder folder = GetFolder(storageType);
            var file = await GetFileIfExistsAsync(folder, FileName(Activator.CreateInstance<T>(), key));
            return file != null;
        }
    }
}