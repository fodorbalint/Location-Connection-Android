using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
//**using Android.Support.Design.Widget;
using Android.Support.V4.App;
//**using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Android.Text;
using Android.Text.Style;
using Android.Graphics;
using Android.Views.Animations;
using System.Net.Http;
using AndroidX.Core.Content;
using AndroidX.Core.App;

namespace LocationConnection
{
	/* ----- Registration / Profile Edit ----- */
	public class RegisterCommonMethods//<T> where T:ProfilePage
	{
		ProfilePage context;
		//private WebClient client;
		//**Snackbar snack;

		public RegisterCommonMethods(ProfilePage context)
		{
			this.context = context;

			context.CheckUsername.Click += CheckUsername_Click;
			context.Images.Click += Images_Click;

			context.Description.Touch += Description_Touch;

			context.UseLocationSwitch.Click += UseLocationSwitch_Click;
			context.LocationShareAll.Click += LocationShareAll_Click;
			context.LocationShareLike.Click += LocationShareLike_Click;
			context.LocationShareMatch.Click += LocationShareMatch_Click;
			context.LocationShareFriend.Click += LocationShareFriend_Click;
			context.LocationShareNone.Click += LocationShareNone_Click;

			context.DistanceShareAll.Click += DistanceShareAll_Click;
			context.DistanceShareLike.Click += DistanceShareLike_Click;
			context.DistanceShareMatch.Click += DistanceShareMatch_Click;
			context.DistanceShareFriend.Click += DistanceShareFriend_Click;
			context.DistanceShareNone.Click += DistanceShareNone_Click;

			context.ImageEditorCancel.Click += ImageEditorCancel_Click;
			context.ImageEditorOK.Click += ImageEditorOK_Click;

			/*client = new WebClient();
			client.UploadProgressChanged += Client_UploadProgressChanged;
			client.UploadFileCompleted += Client_UploadFileCompleted;
			client.Headers.Add("Content-Type", "image/jpeg");*/
		}

		public async void CheckUsername_Click(object sender, System.EventArgs e)
		{
			if (context.Username.Text.Trim() == "")
			{
				context.Username.RequestFocus();
				context.c.Snack(Resource.String.UsernameEmpty);
				return;
			}
			if (context.Username.Text.Trim() == Session.Username)
			{
				context.c.Snack(Resource.String.UsernameSame);
				return;
			}

			context.CheckUsername.Enabled = false;

			string responseString = await context.c.MakeRequest("action=usercheck&Username=" + context.Username.Text.Trim());
			if (responseString == "OK")
			{
				context.c.Snack(Resource.String.UsernameAvailable);
			}
			else if (responseString.Substring(0, 6) == "ERROR_")
			{
				context.c.Snack(context.Resources.GetIdentifier(responseString.Substring(6), "string", context.PackageName));
			}
			else
			{
				context.c.ReportError(responseString);
			}

			context.CheckUsername.Enabled = true;
		}

		public void Images_Click(object sender, System.EventArgs e)
		{
			/*if (!(snack is null))
			{
				snack.Dismiss();
				snack = null;
			}*/
			
			if (context.uploadedImages.Count < Constants.MaxNumPictures)
			{
				if (!context.imagesUploading && !context.imagesDeleting)
				{
					context.ImagesProgressText.Text = "";
					context.ImagesProgress.Progress = 0;

					if (ContextCompat.CheckSelfPermission(context, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
					{
						if (AndroidX.Core.App.ActivityCompat.ShouldShowRequestPermissionRationale(context, Manifest.Permission.ReadExternalStorage)) //shows when the user has once denied the permission, and now requesting it again.
						{
							var requiredPermissions = new String[] { Manifest.Permission.ReadExternalStorage };

                            context.c.SnackIndefAction(context.res.GetString(Resource.String.StorageRationale), new Action<View>(delegate (View obj) { AndroidX.Core.App.ActivityCompat.RequestPermissions(context, requiredPermissions, 1); }));
                        }
						else
						{
							AndroidX.Core.App.ActivityCompat.RequestPermissions(context, new String[] { Manifest.Permission.ReadExternalStorage }, 1);
						}
					}
					else
					{
						SelectImage();
					}
				}
				else
				{
					if (context.imagesUploading)
					{
						context.c.Snack(Resource.String.ImagesUploading);
					}
					else
					{
						context.c.Snack(Resource.String.ImagesDeleting);
					}
				}
			}
			else
			{
				context.c.SnackStr(context.res.GetString(Resource.String.MaxNumImages) + " " + Constants.MaxNumPictures + ".");
			}
		}

		public void SelectImage()
		{
			context.c.Log("Opening file selector");	
			context.Images.Enabled = false;
			Intent i = new Intent();
			i.SetType("image/*");
			i.SetAction(Intent.ActionGetContent);
			context.StartActivityForResult(Intent.CreateChooser(i, "Select a picture"), 1);
		}
		public void ImageEditorCancel_Click(object sender, EventArgs e)
		{
			CloseEditor();
		}

		public async void ImageEditorOK_Click(object sender, EventArgs e)
		{
			try
			{
				//device rotation needs to be handled
				if (context.ImageEditor.IsOutOfFrameX() || context.ImageEditor.IsOutOfFrameY())
				{
					await context.c.Alert(context.res.GetString(Resource.String.ImageEditorAlert));
					return;
				}

				context.c.Log("ImageEditorOK not out of frame");
				float w = context.ImageEditor.bm.Width;
				float h = context.ImageEditor.bm.Height;

				float x = ((context.ImageEditor.intrinsicWidth - context.ImageEditorFrameBorder.Width / context.ImageEditor.scaleFactor) / 2 - context.ImageEditor.xDist) * w / context.ImageEditor.intrinsicWidth;
				float y = ((context.ImageEditor.intrinsicHeight - context.ImageEditorFrameBorder.Height / context.ImageEditor.scaleFactor) / 2 - context.ImageEditor.yDist) * h / context.ImageEditor.intrinsicHeight;
				float cropW = context.ImageEditorFrameBorder.Width / context.ImageEditor.scaleFactor * w / context.ImageEditor.intrinsicWidth;
				float cropH = cropW;

				context.c.Log("ImageEditorOK_Click w " + w + " h " + h + " intrinsicWidth " + context.ImageEditor.intrinsicWidth + " intrinsicHeight " + context.ImageEditor.intrinsicHeight + " x " + x + " y " + y + " cropW " + cropW + " cropH " + cropH + " totalX " + (x + cropW) + " totalY " + (y + cropH));

				Bitmap bm = Bitmap.CreateBitmap(context.ImageEditor.bm, (int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(cropW), (int)Math.Round(cropH));

				context.c.Log("Bitmap created");

				string fileName = System.IO.Path.Combine(CommonMethods.cacheFolder, "image.jpg");
				try
				{
					FileStream stream = new FileStream(fileName, FileMode.Create);
					bm.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
					stream.Close();
				}
				catch (Exception ex)
				{
					context.c.ReportError(context.res.GetString(Resource.String.CropImageError) + " " + ex.Message);
					return;
				}

				context.c.Log("Cropped image created, closing editor");

				CloseEditor();

				//FileInfo fi = new FileInfo(fileName);
				//context.c.CW(fileName + " Image size: " + fi.Length + " " + ext);

				UploadFile(fileName, RegisterActivity.regsessionid); //works for profile edit too
			}
			catch (Exception ex)
			{
				context.c.ReportError(context.res.GetString(Resource.String.CropImageError) + " " + ex.Message);
			}
		}

		public void CloseEditor()
		{
			context.c.Log("CloseEditor");

			ProfilePage.selectedFile = null;
			context.ImageEditorFrame.Visibility = ViewStates.Invisible;
			context.ImageEditor.Visibility = ViewStates.Invisible;
			context.ImageEditorFrameBorder.Visibility = ViewStates.Invisible;
			context.ImageEditorControls.Visibility = ViewStates.Invisible;
			context.TopSeparator.Visibility = ViewStates.Invisible;

			if (context.ImageEditor.bm != null)
			{
				context.ImageEditor.bm.Recycle();
				context.ImageEditor.bm = null;
			}
		}

		public async void UploadFile(string fileName, string regsessionid) //use Task<int> for return value
		{
			ProfilePage.selectedFileStr = null;
			context.imagesUploading = true;
			string currentText = context.ImagesProgressText.Text;
			StartAnim();

			try
			{
				try
				{
					string url;
					if (context.c.IsLoggedIn())
					{
						url = Constants.HostName + "?action=uploadtouser&ID=" + Session.ID + "&SessionID=" + Session.SessionID;
					}
					else
					{
						url = (regsessionid == "") ? Constants.HostName + "?action=uploadtotemp" : Constants.HostName + "?action=uploadtotemp&regsessionid=" + regsessionid;
					}

					MultipartFormDataContent form = new MultipartFormDataContent();
					form.Add(new StreamContent(new MemoryStream(File.ReadAllBytes(fileName))), "file", System.IO.Path.GetFileName(fileName));

					context.c.CW("UploadFile 1" + fileName);

					var response = await CommonMethods.client.PostAsync(url, form);

					context.c.CW("UploadFile 2");

					string responseString = response.Content.ReadAsStringAsync().Result;

					if (responseString.Substring(0, 2) == "OK")
					{
						responseString = responseString.Substring(3);

						string[] arr = responseString.Split(";");
						string imgName = arr[0];
						context.uploadedImages.Add(imgName);
						if (context.c.IsLoggedIn())
						{
							Session.Pictures = context.uploadedImages.ToArray();
						}
						else
						{
							RegisterActivity.regsessionid = arr[1];
							if (!File.Exists(BaseActivity.regSessionFile))
							{
								File.WriteAllText(BaseActivity.regSessionFile, RegisterActivity.regsessionid);
							}
							context.SaveRegData();
						}
						context.ImagesUploaded.AddPicture(imgName, context.uploadedImages.Count - 1);
						context.ImagesUploaded.RefitImagesContainer();
					}
					else if (responseString.Substring(0, 6) == "ERROR_")
					{
						context.c.SnackIndef(context.Resources.GetIdentifier(responseString.Substring(6), "string", context.PackageName));
					}
					else
					{
						context.c.ReportError(responseString);
					}

					context.ImagesProgress.Progress = 0;
					if (context.uploadedImages.Count > 1)
					{
						context.ImagesProgressText.Text = context.res.GetString(Resource.String.ImagesRearrange);
					}
					else
					{
						context.ImagesProgressText.Text = "";
					}

					//client.UploadFileTaskAsync(url, fileName);
				}
				catch (WebException ex)
				{
					context.ImagesProgressText.Text = currentText;
					//Client_UploadFileCompleted is called too which resets the views
					if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.InternalServerError)
					{
						await context.c.ErrorAlert(context.res.GetString(Resource.String.OutOfMemory));
					}
					else
					{
						context.c.ReportErrorSilent("Upload image error: " + ((HttpWebResponse)ex.Response).StatusCode + " " + ex.Message + System.Environment.NewLine + ex.StackTrace);
					}
				}
			}
			catch (Exception ex) //The operation was canceled. Image size limit is between 4.3 and 4.9 MB
			{
				context.ImagesProgressText.Text = currentText;
				
				if (ex is System.OperationCanceledException)
				{
					await context.c.ErrorAlert(context.res.GetString(Resource.String.ImageTooLarge));
					context.c.ReportErrorSilent(ex.Message);
				}
				else
				{
					context.c.ReportError(ex.Message);
				}				
			}

			context.imagesUploading = false;
			StopAnim();
		}

		/*private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage == 0)
			{
				context.ImagesProgressText.Text = context.res.GetString(Resource.String.ImagesProgressText);
			}
			else
			{
				context.ImagesProgressText.Text = context.res.GetString(Resource.String.ImagesProgressTextPercent) + " " + e.ProgressPercentage + "%";
			}
			context.ImagesProgress.Progress = e.ProgressPercentage;
		}

		private void Client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
		{
			context.imagesUploading = false;
			StopAnim();

			try
			{
				string responseString = System.Text.Encoding.UTF8.GetString(e.Result);
				if (responseString.Substring(0, 2) == "OK")
				{
					responseString = responseString.Substring(3);

					string[] arr = responseString.Split(";");
					string imgName = arr[0];
					context.uploadedImages.Add(imgName);
					if (context.c.IsLoggedIn())
					{
						Session.Pictures = context.uploadedImages.ToArray();
					}
					else
					{
						RegisterActivity.regsessionid = arr[1];
						if (!File.Exists(BaseActivity.regSessionFile))
						{
							File.WriteAllText(BaseActivity.regSessionFile, RegisterActivity.regsessionid);
						}
						context.SaveRegData();
					}
					context.ImagesUploaded.AddPicture(imgName, context.uploadedImages.Count - 1);
					context.ImagesUploaded.RefitImagesContainer();
				}
				else if (responseString.Substring(0, 6) == "ERROR_")
				{
					context.c.SnackIndef(context.Resources.GetIdentifier(responseString.Substring(6), "string", context.PackageName));
				}
				else
				{
					context.c.ReportError(responseString);
				}
				
				context.ImagesProgress.Progress = 0;
				if (context.uploadedImages.Count > 1)
				{
					context.ImagesProgressText.Text = context.res.GetString(Resource.String.ImagesRearrange);
				}
				else
				{
					context.ImagesProgressText.Text = "";
				}
			}
			catch (Exception ex)
			{
				context.ImagesProgressText.Text = "";

				if (!(ex.InnerException is WebException))
				{
					context.c.ReportErrorSilent(ex.Message + " --- " + ex.InnerException + " --- " + System.Environment.NewLine + ex.StackTrace);
				}
			}
		}*/

		public void StartAnim()
		{
			Animation anim = Android.Views.Animations.AnimationUtils.LoadAnimation(context, Resource.Animation.rotate);
			context.LoaderCircle.Visibility = ViewStates.Visible;
			context.LoaderCircle.StartAnimation(anim);
			context.ImagesProgressText.Text = context.res.GetString(Resource.String.ImagesProgressText);
		}

		public void StopAnim()
        {
            context.LoaderCircle.Visibility = ViewStates.Invisible;
			context.LoaderCircle.ClearAnimation();
        }

		private void Description_Touch(object sender, View.TouchEventArgs e)
		{
			if (context.Description.HasFocus)
			{
				context.MainScroll.RequestDisallowInterceptTouchEvent(true);
			}
			e.Handled = false;
		}

		public void UseLocationSwitch_Click(object sender, EventArgs e)
		{
			if (context.UseLocationSwitch.Checked)
			{
				if (!context.c.IsLocationEnabled())
				{
					context.UseLocationSwitch.Checked = false;
					if (AndroidX.Core.App.ActivityCompat.ShouldShowRequestPermissionRationale(context, Manifest.Permission.AccessFineLocation)) //shows when the user has once denied the permission, and now requesting it again.
					{
						var requiredPermissions = new String[] { Manifest.Permission.AccessFineLocation };

						context.c.SnackIndefAction(context.res.GetString(Resource.String.LocationRationale), new Action<View>(delegate (View obj) { AndroidX.Core.App.ActivityCompat.RequestPermissions(context, requiredPermissions, 2); }));
                    }
					else
					{
						AndroidX.Core.App.ActivityCompat.RequestPermissions(context, new String[] { Manifest.Permission.AccessFineLocation }, 2);
					}
				}
				else
				{
					EnableLocationSwitches(true);
				}
			}
			else
			{
				EnableLocationSwitches(false);
			}
		}

		public void EnableLocationSwitches(bool val)
		{
			context.LocationShareAll.Enabled = val;
			context.LocationShareLike.Enabled = val;
			context.LocationShareMatch.Enabled = val;
			context.LocationShareFriend.Enabled = val;
			context.LocationShareNone.Enabled = val;

			context.DistanceShareAll.Enabled = val;
			context.DistanceShareLike.Enabled = val;
			context.DistanceShareMatch.Enabled = val;
			context.DistanceShareFriend.Enabled = val;
			context.DistanceShareNone.Enabled = val;
		}

		public void LocationShareAll_Click(object sender, EventArgs e)
		{
			if (context.LocationShareAll.Checked)
			{
				context.LocationShareLike.Checked = true;
				context.LocationShareMatch.Checked = true;
				context.LocationShareFriend.Checked = true;
				context.LocationShareNone.Checked = false;
			}
		}

		public void LocationShareLike_Click(object sender, EventArgs e)
		{
			if (context.LocationShareLike.Checked)
			{
				context.LocationShareMatch.Checked = true;
				context.LocationShareFriend.Checked = true;
				context.LocationShareNone.Checked = false;
			}
			else
			{
				context.LocationShareAll.Checked = false;
			}
		}

		public void LocationShareMatch_Click(object sender, EventArgs e)
		{
			if (context.LocationShareMatch.Checked)
			{
				context.LocationShareFriend.Checked = true;
				context.LocationShareNone.Checked = false;
			}
			else
			{
				context.LocationShareAll.Checked = false;
				context.LocationShareLike.Checked = false;
			}
		}

		public void LocationShareFriend_Click(object sender, EventArgs e)
		{
			if (context.LocationShareFriend.Checked)
			{
				context.LocationShareNone.Checked = false;
			}
			else
			{
				context.LocationShareAll.Checked = false;
				context.LocationShareLike.Checked = false;
				context.LocationShareMatch.Checked = false;
				context.LocationShareNone.Checked = true;
			}
		}

		public void LocationShareNone_Click(object sender, EventArgs e)
		{
			if (context.LocationShareNone.Checked)
			{
				context.LocationShareAll.Checked = false;
				context.LocationShareLike.Checked = false;
				context.LocationShareMatch.Checked = false;
				context.LocationShareFriend.Checked = false;
			}
			else
			{
				context.LocationShareFriend.Checked = true;
			}
		}

		public void DistanceShareAll_Click(object sender, EventArgs e)
		{
			if (context.DistanceShareAll.Checked)
			{
				context.DistanceShareLike.Checked = true;
				context.DistanceShareMatch.Checked = true;
				context.DistanceShareFriend.Checked = true;
				context.DistanceShareNone.Checked = false;
			}
		}

		public void DistanceShareLike_Click(object sender, EventArgs e)
		{
			if (context.DistanceShareLike.Checked)
			{
				context.DistanceShareMatch.Checked = true;
				context.DistanceShareFriend.Checked = true;
				context.DistanceShareNone.Checked = false;
			}
			else
			{
				context.DistanceShareAll.Checked = false;
			}
		}

		public void DistanceShareMatch_Click(object sender, EventArgs e)
		{
			if (context.DistanceShareMatch.Checked)
			{
				context.DistanceShareFriend.Checked = true;
				context.DistanceShareNone.Checked = false;
			}
			else
			{
				context.DistanceShareAll.Checked = false;
				context.DistanceShareLike.Checked = false;
			}
		}

		public void DistanceShareFriend_Click(object sender, EventArgs e)
		{
			if (context.DistanceShareFriend.Checked)
			{
				context.DistanceShareNone.Checked = false;
			}
			else
			{
				context.DistanceShareAll.Checked = false;
				context.DistanceShareLike.Checked = false;
				context.DistanceShareMatch.Checked = false;
				context.DistanceShareNone.Checked = true;
			}
		}

		public void DistanceShareNone_Click(object sender, EventArgs e)
		{
			if (context.DistanceShareNone.Checked)
			{
				context.DistanceShareAll.Checked = false;
				context.DistanceShareLike.Checked = false;
				context.DistanceShareMatch.Checked = false;
				context.DistanceShareFriend.Checked = false;
			}
			else
			{
				context.DistanceShareFriend.Checked = true;
			}
		}

		public int GetLocationShareLevel()
		{
			if (context.LocationShareAll.Checked)
			{
				return 4;
			}
			else if (context.LocationShareLike.Checked)
			{
				return 3;
			}
			else if (context.LocationShareMatch.Checked)
			{
				return 2;
			}
			else if (context.LocationShareFriend.Checked)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		public int GetDistanceShareLevel()
		{
			if (context.DistanceShareAll.Checked)
			{
				return 4;
			}
			else if (context.DistanceShareLike.Checked)
			{
				return 3;
			}
			else if (context.DistanceShareMatch.Checked)
			{
				return 2;
			}
			else if (context.DistanceShareFriend.Checked)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}
		public void SetLocationShareLevel(int level)
		{
			switch (level)
			{
				case 0:
					context.LocationShareNone.Checked = true;
					context.LocationShareFriend.Checked = false;
					context.LocationShareMatch.Checked = false;
					context.LocationShareLike.Checked = false;
					context.LocationShareAll.Checked = false;
					break;
				case 1:
					context.LocationShareNone.Checked = false;
					context.LocationShareFriend.Checked = true;
					context.LocationShareMatch.Checked = false;
					context.LocationShareLike.Checked = false;
					context.LocationShareAll.Checked = false;
					break;
				case 2:
					context.LocationShareNone.Checked = false;
					context.LocationShareFriend.Checked = true;
					context.LocationShareMatch.Checked = true;
					context.LocationShareLike.Checked = false;
					context.LocationShareAll.Checked = false;
					break;
				case 3:
					context.LocationShareNone.Checked = false;
					context.LocationShareFriend.Checked = true;
					context.LocationShareMatch.Checked = true;
					context.LocationShareLike.Checked = true;
					context.LocationShareAll.Checked = false;
					break;
				case 4:
					context.LocationShareNone.Checked = false;
					context.LocationShareFriend.Checked = true;
					context.LocationShareMatch.Checked = true;
					context.LocationShareLike.Checked = true;
					context.LocationShareAll.Checked = true;
					break;
			}
		}

		public void SetDistanceShareLevel(int level)
		{
			switch (level)
			{
				case 0:
					context.DistanceShareNone.Checked = true;
					context.DistanceShareFriend.Checked = false;
					context.DistanceShareMatch.Checked = false;
					context.DistanceShareLike.Checked = false;
					context.DistanceShareAll.Checked = false;
					break;
				case 1:
					context.DistanceShareNone.Checked = false;
					context.DistanceShareFriend.Checked = true;
					context.DistanceShareMatch.Checked = false;
					context.DistanceShareLike.Checked = false;
					context.DistanceShareAll.Checked = false;
					break;
				case 2:
					context.DistanceShareNone.Checked = false;
					context.DistanceShareFriend.Checked = true;
					context.DistanceShareMatch.Checked = true;
					context.DistanceShareLike.Checked = false;
					context.DistanceShareAll.Checked = false;
					break;
				case 3:
					context.DistanceShareNone.Checked = false;
					context.DistanceShareFriend.Checked = true;
					context.DistanceShareMatch.Checked = true;
					context.DistanceShareLike.Checked = true;
					context.DistanceShareAll.Checked = false;
					break;
				case 4:
					context.DistanceShareNone.Checked = false;
					context.DistanceShareFriend.Checked = true;
					context.DistanceShareMatch.Checked = true;
					context.DistanceShareLike.Checked = true;
					context.DistanceShareAll.Checked = true;
					break;
			}
		}
	}
}
 