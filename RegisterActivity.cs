/* remove non-letter char
 * acters from form, @ from username
 * image filename must not contain ; and |
 * uploading the same image twice will result in missing image when we delete one.
 * loading circle while images are loading
 * sending email for account confirmation
 * cron to remove temp picture directories
 * cron to calculate response rate, update on answering a message
*/

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
//**using Android.Support.Design.Animation;
//**using Android.Support.Design.Widget;
using Android.Support.V4.App;
//**using Android.Support.V4.Content;
//**using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace LocationConnection
{
	[Activity(MainLauncher = false, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
	public class RegisterActivity : ProfilePage
    {
        ConstraintLayout MainLayout1;
        Spinner Sex;
        public EditText Password, ConfirmPassword;
		Button Register, Reset, Cancel;
		EditText EulaText;

		//RegisterCommonMethods<RegisterActivity> rc;
		public BaseAdapter adapter;
		
		public static string regsessionid; //for use in UploadedListAdapter

        int checkFormMessage;
		private bool registerCompleted;
		private int spinnerItem;
		private int spinnerItemDropdown;
		private bool eulaLoaded;

		protected override void OnCreate(Bundle savedInstanceState)
        {
			try
			{
				base.OnCreate(savedInstanceState);



				//Test reset

				/*ListActivity.initialized = false;
				Type type = typeof(Settings);
				FieldInfo[] fieldInfo = type.GetFields();
				foreach (FieldInfo field in fieldInfo)
				{
					field.SetValue(null, null);
				}*/



				if (!ListActivity.initialized) { //Huawei Y6 after selecting image. OnCreate is called, with static variables from other activities being cleared out.
					//Honor 20 Pro does not call OnCreate, but will call ListActvity after onResume ends anyway. ProfileEditActivity will call ProfileViewActivity, which is gets the Not initialized error, and be therefore redirected to ListActivity.
					c.Log("RegisterActivity not initialized, restoring");
					ListActivity.initialized = true;
					GetScreenMetrics(true);
					c.LoadSettings(false);
					if (!(savedInstanceState is null))
					{
						imageEditorFrameBorderWidth = savedInstanceState.GetInt("imageEditorFrameBorderWidth");
					}
				}

				if (Settings.DisplaySize == 1)
				{
					SetContentView(Resource.Layout.activity_register_normal);
					spinnerItem = Resource.Layout.spinner_item_normal;
					spinnerItemDropdown = Resource.Layout.spinner_item_dropdown_normal;
				}
				else
				{
					SetContentView(Resource.Layout.activity_register_small);
					spinnerItem = Resource.Layout.spinner_item_small;
					spinnerItemDropdown = Resource.Layout.spinner_item_dropdown_small;
				}

				//ProfilePage start

				MainScroll = FindViewById<TouchScrollView>(Resource.Id.MainScroll);
                MainLayout = FindViewById<ConstraintLayout>(Resource.Id.MainLayout);
                MainLayout1 = FindViewById<ConstraintLayout>(Resource.Id.MainLayout1);
				Email = FindViewById<EditText>(Resource.Id.Email);
				Username = FindViewById<EditText>(Resource.Id.Username);
				CheckUsername = FindViewById<Button>(Resource.Id.CheckUsername);
				Name = FindViewById<EditText>(Resource.Id.Name);
				ImagesUploaded = FindViewById<ImageFrameLayout>(Resource.Id.ImagesUploaded);
				Images = FindViewById<Button>(Resource.Id.Images);
				ImagesProgressText = FindViewById<TextView>(Resource.Id.ImagesProgressText);
				LoaderCircle = FindViewById<ImageView>(Resource.Id.LoaderCircle);
				ImagesProgress = FindViewById<ProgressBar>(Resource.Id.ImagesProgress);
				Description = FindViewById<EditText>(Resource.Id.Description);

				UseLocationSwitch = FindViewById<Switch>(Resource.Id.UseLocationSwitch);
				LocationShareAll = FindViewById<Switch>(Resource.Id.LocationShareAll);
				LocationShareLike = FindViewById<Switch>(Resource.Id.LocationShareLike);
				LocationShareMatch = FindViewById<Switch>(Resource.Id.LocationShareMatch);
				LocationShareFriend = FindViewById<Switch>(Resource.Id.LocationShareFriend);
				LocationShareNone = FindViewById<Switch>(Resource.Id.LocationShareNone);

				DistanceShareAll = FindViewById<Switch>(Resource.Id.DistanceShareAll);
				DistanceShareLike = FindViewById<Switch>(Resource.Id.DistanceShareLike);
				DistanceShareMatch = FindViewById<Switch>(Resource.Id.DistanceShareMatch);
				DistanceShareFriend = FindViewById<Switch>(Resource.Id.DistanceShareFriend);
				DistanceShareNone = FindViewById<Switch>(Resource.Id.DistanceShareNone);

				ImageEditorFrame = FindViewById<View>(Resource.Id.ImageEditorFrame);
				ImageEditorFrameBorder = FindViewById<View>(Resource.Id.ImageEditorFrameBorder);
				ImageEditor = FindViewById<ScaleImageView>(Resource.Id.ImageEditor);
				ImageEditorControls = FindViewById<LinearLayout>(Resource.Id.ImageEditorControls);
				ImageEditorCancel = FindViewById<ImageButton>(Resource.Id.ImageEditorCancel);
				ImageEditorOK = FindViewById<ImageButton>(Resource.Id.ImageEditorOK);
				TopSeparator = FindViewById<View>(Resource.Id.TopSeparator);

				//ProfilePage end

				Sex = FindViewById<Spinner>(Resource.Id.Sex);

				Password = FindViewById<EditText>(Resource.Id.Password);
				ConfirmPassword = FindViewById<EditText>(Resource.Id.ConfirmPassword);

				EulaText = FindViewById<EditText>(Resource.Id.EulaText);

				Register = FindViewById<Button>(Resource.Id.Register);
				Reset = FindViewById<Button>(Resource.Id.Reset);
				Cancel = FindViewById<Button>(Resource.Id.Cancel);

				ImagesUploaded.numColumns = 5; //it does not get passed in the layout file
				ImagesUploaded.tileSpacing = 2;
				ImagesProgress.Progress = 0;
				c.view = MainLayout;
				rc = new RegisterCommonMethods(this);
				imm = (InputMethodManager)GetSystemService(InputMethodService);

				var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.SexEntries, spinnerItem);
				adapter.SetDropDownViewResource(spinnerItemDropdown);
				Sex.Adapter = adapter;

				if (!File.Exists(regSessionFile))
				{
					regsessionid = "";
				}
				else
				{
					regsessionid = File.ReadAllText(regSessionFile);
				}

				eulaLoaded = false;

				EulaText.Touch += EulaText_Touch;

				Register.Click += Register_Click;
				Reset.Click += Reset_Click;
				Cancel.Click += Cancel_Click;
			}
			catch (Exception ex)
			{
				c.ReportErrorSilent(ex.Message + System.Environment.NewLine + ex.StackTrace);
			}
		}

		protected async override void OnResume() //will be called after opening the file selector and permission results
		{
			try
			{
				base.OnResume();

				if (!ListActivity.initialized) { return; }

				GetScreenMetrics(false);
				ImagesUploaded.SetTileSize();
				MainLayout1.RequestFocus();
				Images.Enabled = true;

				if (!(ListActivity.listProfiles is null))
				{
					ListActivity.listProfiles.Clear();
					ListActivity.totalResultCount = null;
				}
				Session.LastDataRefresh = null;
				Session.LocationTime = null;

				registerCompleted = false;

				if (File.Exists(regSaveFile))
				{
					string[] arr = File.ReadAllLines(regSaveFile);
					Sex.SetSelection(int.Parse(arr[0]));
					Email.Text = arr[1];
					Password.Text = arr[2];
					ConfirmPassword.Text = arr[3];
					Username.Text = arr[4];
					Name.Text = arr[5];
					if (arr[6] != "") //it would give one element
					{
						string[] images = arr[6].Split("|");
						uploadedImages = new List<string>(images);
					}
					else
					{
						uploadedImages = new List<string>();
					}

					if (uploadedImages.Count > 1)
					{
						ImagesProgressText.Text = res.GetString(Resource.String.ImagesRearrange);
					}
					else
					{
						ImagesProgressText.Text = "";
					}

					ImagesUploaded.RemoveAllViews();
					ImagesUploaded.drawOrder = new List<int>();

					ImageCache.imagesInProgress = new List<string>();

					ImagesUploaded.AddShadow();
					int i = 0;
					foreach (string image in uploadedImages)
					{
						ImagesUploaded.AddPicture(image, i);
						i++;
					}
					ImagesUploaded.RefitImagesContainer();

					if (imagesUploading)
					{
						rc.StartAnim();
					}

					Description.Text = arr[7].Replace("[newline]", "\n");

					UseLocationSwitch.Checked = bool.Parse(arr[8]);
					rc.EnableLocationSwitches(UseLocationSwitch.Checked);
					rc.SetLocationShareLevel(byte.Parse(arr[9]));
					rc.SetDistanceShareLevel(byte.Parse(arr[10]));
				}
				else //in case we are stepping back from a successful registration
				{
					ResetForm();
				}

				if (!eulaLoaded)
				{
					string responseString = await c.MakeRequest("action=eula&ios=0"); //deleting images from server
					if (responseString.Substring(0, 2) == "OK")
					{
						eulaLoaded = true;
						if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
						{
							EulaText.TextFormatted = Html.FromHtml(responseString.Substring(3), FromHtmlOptions.ModeLegacy);
						}
						else {
#pragma warning disable CS0618 // Type or member is obsolete
							EulaText.TextFormatted = Html.FromHtml(responseString.Substring(3));
#pragma warning restore CS0618 // Type or member is obsolete
						}
					}
					else
					{
						c.ReportError(responseString);
					}
				}

				if (selectedFile != null)
				{
					OnResumeEnd();
				}

				c.Log("OnResume end");
			}
			catch (Exception ex)
			{
				c.ReportErrorSilent(ex.Message + System.Environment.NewLine + ex.StackTrace);
			}
		}

		protected override void OnPause()
		{
			base.OnPause();

			if (!registerCompleted)
			{
				SaveRegData();
			}
		}

		protected override void OnSaveInstanceState(Bundle outState) //called after OnPause
		{
			base.OnSaveInstanceState(outState);

			outState.PutInt("imageEditorFrameBorderWidth", imageEditorFrameBorderWidth);
		}

		public override void SaveRegData()
		{
			File.WriteAllText(regSaveFile, Sex.SelectedItemId + "\n" + Email.Text.Trim() + "\n" + Password.Text.Trim() + "\n" + ConfirmPassword.Text.Trim()
					+ "\n" + Username.Text.Trim() + "\n" + Name.Text.Trim()
					+ "\n" + string.Join("|", uploadedImages) + "\n" + Description.Text.Trim().Replace("\n", "[newline]")
					+ "\n" + UseLocationSwitch.Checked + "\n" + rc.GetLocationShareLevel() + "\n" + rc.GetDistanceShareLevel());
		}

		private void ResetForm()
		{
			if (File.Exists(regSaveFile))
			{
				File.Delete(regSaveFile);
			}
			
			Sex.SetSelection(0);
			Email.Text = "";
			Password.Text = "";
			ConfirmPassword.Text = "";
			Username.Text = "";
			Name.Text = "";
			Description.Text = "";
			uploadedImages = new List<string>();
			ImagesUploaded.RemoveAllViews(); //shadow is removed too, but after selecting an image, it is added in OnResume.
			ImagesUploaded.RefitImagesContainer();

			Description.Text = "";

			ImagesProgressText.Text = "";
			ImagesProgress.Progress = 0;

			UseLocationSwitch.Checked = false;

			LocationShareAll.Checked = false;
			LocationShareLike.Checked = false;
			LocationShareMatch.Checked = false;
			LocationShareFriend.Checked = false;
			LocationShareNone.Checked = true;

			DistanceShareAll.Checked = false;
			DistanceShareLike.Checked = false;
			DistanceShareMatch.Checked = false;
			DistanceShareFriend.Checked = false;
			DistanceShareNone.Checked = true;

			rc.EnableLocationSwitches(false);

            System.Timers.Timer t = new System.Timers.Timer(); //when removing uploaded images, form does not scroll up
			t.Interval = 1;
			t.Elapsed += T_Elapsed0;
			t.Start();
		}

		private void T_Elapsed0(object sender, ElapsedEventArgs e)
		{
			((System.Timers.Timer)sender).Stop();
			this.RunOnUiThread(() => {
				MainScroll.SmoothScrollTo(0, 0);
			});
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
		{
			if (requestCode == 1) //Storage
			{
				if ((grantResults.Length == 1) && (grantResults[0] == Permission.Granted))
				{
					rc.SelectImage();
				}
				else
				{
					c.Snack(Resource.String.StorageNotGranted);
				}
			}
			else if (requestCode == 2) //Location
			{
				if ((grantResults.Length == 1) && (grantResults[0] == Permission.Granted))
				{
                    System.Timers.Timer t = new System.Timers.Timer();
					t.Interval = 1;
					t.Elapsed += T_Elapsed;
					t.Start();
				}
				else
				{
					c.Snack(Resource.String.LocationNotGranted);
				}
			}
			else
			{
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			}
		}

		private void T_Elapsed(object sender, ElapsedEventArgs e)
		{
			((System.Timers.Timer)sender).Stop();
			this.RunOnUiThread(() => {
				UseLocationSwitch.Checked = true;
				rc.EnableLocationSwitches(true);
			});
		}

		private void EulaText_Touch(object sender, View.TouchEventArgs e)
		{
			MainScroll.RequestDisallowInterceptTouchEvent(true);
			e.Handled = false;
			base.OnTouchEvent(e.Event);
		}

		private async void Register_Click(object sender, System.EventArgs e)
        {            
            if (CheckFields())
            {
				imm.HideSoftInputFromWindow(Email.WindowToken, 0);
				MainLayout1.RequestFocus();

				Register.Enabled = false;

				int locationShare = 0;
				int distanceShare = 0;

				if (UseLocationSwitch.Checked)
				{
					locationShare = rc.GetLocationShareLevel();
					distanceShare = rc.GetDistanceShareLevel();
				}

				string url = "action=register&Sex=" + (Sex.SelectedItemId - 1) + "&Email=" + c.UrlEncode(Email.Text.Trim()) + "&Password=" + c.UrlEncode(Password.Text.Trim())
					+ "&Username=" + c.UrlEncode(Username.Text.Trim()) + "&Name=" + c.UrlEncode(Name.Text.Trim())
					+ "&Pictures=" + c.UrlEncode(string.Join("|", uploadedImages)) + "&Description=" + c.UrlEncode(Description.Text) + "&UseLocation=" + UseLocationSwitch.Checked
					+ "&LocationShare=" + locationShare + "&DistanceShare=" + distanceShare + "&regsessionid=" + regsessionid;

				if (File.Exists(firebaseTokenFile)) //sends the token whether it was sent from this device or not
				{
					url += "&token=" + File.ReadAllText(firebaseTokenFile);
				}
				string responseString = await c.MakeRequest(url);
				if (responseString.Substring(0, 2) == "OK")
				{
					if (File.Exists(regSessionFile))
					{
						File.Delete(regSessionFile);
					}
					regsessionid = "";
					if (File.Exists(regSaveFile))
					{
						File.Delete(regSaveFile);
					}
					registerCompleted = true; //to prevent OnPause from saving form data.
					
					if (File.Exists(firebaseTokenFile))
					{
						File.WriteAllText(tokenUptoDateFile, "True");
					}

					c.LoadCurrentUser(responseString);

					Register.Enabled = true;

					Intent i = new Intent(this, typeof(ListActivity));
					i.SetFlags(ActivityFlags.ReorderToFront);
					StartActivity(i);					
				}
				else if (responseString.Substring(0, 6) == "ERROR_")
				{
					c.Snack(Resources.GetIdentifier(responseString.Substring(6), "string", PackageName));
				}
				else
                {
					c.ReportError(responseString);
				}
				Register.Enabled = true;
			}
            else
            {
                c.Snack(checkFormMessage);
            }
        }

        private bool CheckFields()
        {
			if (Sex.SelectedItemId == 0)
			{
				checkFormMessage = Resource.String.SexEmpty;
				Sex.RequestFocus();
				return false;
			}
			if (Email.Text.Trim() == "")
            {
                checkFormMessage = Resource.String.EmailEmpty;
                Email.RequestFocus();
                return false;
            }
			int lastDotPos = Email.Text.LastIndexOf(".");
			if (lastDotPos < Email.Text.Length - 5)
			{
				checkFormMessage = Resource.String.EmailWrong;
				return false;
			}
			//If the extension is long, the regex will freeze the app.
			Regex regex = new Regex(Constants.EmailFormat);
            if (!regex.IsMatch(Email.Text))
            {
                checkFormMessage = Resource.String.EmailWrong;
                Email.RequestFocus();
                return false;
            }
            if (Password.Text.Trim().Length < 6)
            {
                checkFormMessage = Resource.String.PasswordShort;
                Password.RequestFocus();
                return false;
            }
            if (Password.Text.Trim() != ConfirmPassword.Text.Trim()) {
                checkFormMessage = Resource.String.ConfirmPasswordNoMatch;
                ConfirmPassword.RequestFocus();
                return false;
            }
            if (Username.Text.Trim() == "")
            {
                checkFormMessage = Resource.String.UsernameEmpty;
                Username.RequestFocus();
                return false;
            }
			if (Username.Text.Trim().Substring(Username.Text.Trim().Length - 1) == "\\")
			{
				checkFormMessage = Resource.String.UsernameBackslash;
				Username.RequestFocus();
				return false;
			}
			if (Name.Text.Trim() == "")
            {
                checkFormMessage = Resource.String.NameEmpty;
                Name.RequestFocus();
                return false;
            }
			if (Name.Text.Trim().Substring(Name.Text.Trim().Length - 1) == "\\")
			{
				checkFormMessage = Resource.String.NameBackslash;
				Name.RequestFocus();
				return false;
			}
			if (uploadedImages.Count == 0)
            {
                checkFormMessage = Resource.String.ImagesEmpty;
                Images.RequestFocus();
                return false;
            }
            if (Description.Text.Trim() == "")
            {
                checkFormMessage = Resource.String.DescriptionEmpty;
                Description.RequestFocus();
                return false;
            }
			if (Description.Text.Substring(Description.Text.Length - 1) == "\\")
			{
				checkFormMessage = Resource.String.DescriptionBackslash;
				Description.RequestFocus();
				return false;
			}
			return true;            
        }

		private async void Reset_Click(object sender, EventArgs e)
		{
			imm.HideSoftInputFromWindow(Email.WindowToken, 0);
			MainLayout1.RequestFocus();

			Reset.Enabled = false;

			if (regsessionid != "")
			{
				string responseString = await c.MakeRequest("action=deletetemp&imageName=&regsessionid=" + regsessionid); //deleting images from server
				if (responseString == "OK" || responseString == "INVALID_TOKEN")
				{
					if (File.Exists(regSessionFile))
					{
						File.Delete(regSessionFile);
					}
					regsessionid = "";
					
					ResetForm();
				}
				else
				{
					c.ReportError(responseString);
				}
			}
			else
			{
				ResetForm();
			}
			Reset.Enabled = true;
		}

		private void Cancel_Click(object sender, System.EventArgs e)
        {
			OnBackPressed();
        }

		public override void OnBackPressed() //Permission Denial error would be shown otherwise next time I open this activity
		{
			if (ImageEditor.Visibility == ViewStates.Visible)
			{
				rc.CloseEditor();
			}
			else
			{
				base.OnBackPressed();
			}
		}
	}
}
 