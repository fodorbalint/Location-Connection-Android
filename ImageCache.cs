using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LocationConnection
{
    public class ImageCache
    {
        BaseActivity context;
        public static List<string> imagesInProgress = new List<string>(); //using list is unreliable, when I add 103 to 1|4|7|104|6, it becames 1|4|7|104|6|6. Sometimes an empty stirng is present. Adding volatile did not help. 
        //public static Dictionary<ImageView, string> imageViewToLoadLater = new Dictionary<ImageView, string>();
        private static readonly object lockObj = new object();

        public ImageCache(BaseActivity context)
        {
            this.context = context;
        }

        public async Task<Bitmap> LoadBitmap(string userID, string picture) { //used for the map only

            string saveName = userID + "_" + Constants.SmallImageSize.ToString() + "_" + picture;

            if (Exists(saveName)) //images when loaded from the cache does not always appear 
            {
                await Task.Delay(100); //without delay, not all pictures appear. A minimum of 70 ms required, 60 ms can fail.
                return Load(saveName);

            }
            else
            {
                string url;
                url = Constants.HostName + Constants.UploadFolder + "/" + userID + "/" + Constants.SmallImageSize.ToString() + "/" + picture;

                byte[] bytes = null;

                var task = CommonMethods.GetImageDataFromUrlAsync(url);
                CancellationTokenSource cts = new CancellationTokenSource();

                if (await Task.WhenAny(task, Task.Delay(Constants.RequestTimeout, cts.Token)) == task)
                {
                    cts.Cancel();
                    bytes = await task;
                }

                if (bytes != null)
                {
                    Save(saveName, bytes);
                    return BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
                }
                else
                {
                    return BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.noimage);
                }
            }
        }

        public async Task LoadImage(ImageView imageView, string userID, string picture, bool isLarge = false, bool temp = false)
        {
            try
            {
                string subFolder;

                if (isLarge)
                {
                    subFolder = Constants.LargeImageSize.ToString();
                }
                else
                {
                    subFolder = Constants.SmallImageSize.ToString();
                }

                string saveName = userID + "_" + subFolder + "_" + picture;

                //For the same user, LoadImage is called multiple times shortly after each other. We should only consider the first call.

				if (Exists(saveName))
                {
					if (context is ProfileViewActivity && (((ProfileViewActivity)context).currentID.ToString() != userID || ((ProfileViewActivity)context).cancelImageLoading))
                    {
                        return;
                    }

                    if (imageView != null)
                    {
                        context.RunOnUiThread(() =>
                        {
                            imageView.SetImageBitmap(Load(saveName)); //takes 3-4 ms

                            if (context is ChatListActivity) //only necessary because in the list the pictures appear on the wrong place for a moment
                            {
                                imageView.Alpha = 0;
                                imageView.Animate().Alpha(1).SetDuration(context.tweenTime).Start();
                            }
                        });
                    }
                    
                }
                else
                {
                    if (context is ProfileViewActivity && (((ProfileViewActivity)context).currentID.ToString() != userID || ((ProfileViewActivity)context).cancelImageLoading))
                    {
                        return;
                    }

                    if (imageView != null)
                    {
                        context.RunOnUiThread(() => {
                            imageView.SetImageResource(Resource.Drawable.color_loadingimage_light_dfdfdf);
                        });
                    }

                    string url;
                    if (!temp)
                    {
                        url = Constants.HostName + Constants.UploadFolder + "/" + userID + "/" + subFolder + "/" + picture;
                    }
                    else
                    {
                        url = Constants.HostName + Constants.TempUploadFolder + "/" + userID + "/" + subFolder + "/" + picture;
                    }

                    if (imageView != null)
                    {
                        //the same image may be requested with only 4 ms difference and added twice to the request queue, therefore locking is needed
                        lock (lockObj)
                        {
                            if (imagesInProgress.IndexOf(saveName) != -1)
                            {
                                return;
                            }
                            else
                            {
                                imagesInProgress.Add(saveName);
                            }
                        }
                    }

					byte[] bytes = null;

                    var task = CommonMethods.GetImageDataFromUrlAsync(url);
                    CancellationTokenSource cts = new CancellationTokenSource();

                    if (await Task.WhenAny(task, Task.Delay(Constants.RequestTimeout, cts.Token)) == task)
                    {
                        cts.Cancel();
                        bytes = await task;
                    }

                    /*if (imageView == null)
                    {
                        context.c.CW("Image Loaded " + userID);
                    }*/

                    if (bytes != null)
                    {
                        Save(saveName, bytes);
                        Bitmap bmp = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);

                        if (context is ProfileViewActivity && (((ProfileViewActivity)context).currentID.ToString() != userID || ((ProfileViewActivity)context).cancelImageLoading))
                        {
                            return;
                        }
                        if (imageView != null)
                        {
                            context.RunOnUiThread(() =>
                            {
                                imageView.SetImageBitmap(bmp);
                            });
                        }
                    }
                    else
                    {
                        if (context is ProfileViewActivity && (((ProfileViewActivity)context).currentID.ToString() != userID || ((ProfileViewActivity)context).cancelImageLoading))
                        {
                            return;
                        }
                        
                        if (imageView != null)
                        {
                            context.RunOnUiThread(() =>
                            {
                                if (isLarge)
                                {
                                    imageView.SetImageResource(Resource.Drawable.noimage_hd);
                                }
                                else
                                {
                                    imageView.SetImageResource(Resource.Drawable.noimage);
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.c.ReportErrorSilent("ImageCache error at userID " + userID + ": " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }

        private void Save(string imageName, byte[] data)
        {
            
            string fileName = System.IO.Path.Combine(CommonMethods.cacheFolder, imageName);
            try
            {
                using FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
                fs.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                try
                {
                    context.c.Log("Error saving image: " + ex.Message);
                }
                catch
                {
                }
            }
        }

        private Bitmap Load(string imageName)
        {
            string fileName = System.IO.Path.Combine(CommonMethods.cacheFolder, imageName);;
            return BitmapFactory.DecodeFile(fileName);
        }

        public bool Exists(string imageName)
        {
            string fileName = System.IO.Path.Combine(CommonMethods.cacheFolder, imageName);
            return File.Exists(fileName);
        }
    }
}