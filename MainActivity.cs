﻿/* When server throws error, we get message: object reference not set to an instance of object, line 59 */
// improve back button action: user needs to press it twice

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
//**using Android.Support.Design.Widget;
//**using Android.Support.V4.Content;
//**using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Firebase.Iid;

namespace LocationConnection
{
    [Activity(MainLauncher = false, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : BaseActivity
    {
		public new ConstraintLayout MainLayout;
		public EditText LoginEmail, LoginPassword, ResetEmail;
		Button LoginDone, ResetPassword, ResetSendButton, RegisterButton, ListButton;
		TextView ResetEmailText;

		InputMethodManager imm;
		int checkFormMessage;

		protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
				base.OnCreate(savedInstanceState);

				if (Settings.DisplaySize == 1 || Settings.DisplaySize is null)
				{
					SetContentView(Resource.Layout.activity_main_normal);
				}
				else
				{
					SetContentView(Resource.Layout.activity_main_small);
				}

				MainLayout = FindViewById<ConstraintLayout>(Resource.Id.MainLayout);
				LoginEmail = FindViewById<EditText>(Resource.Id.LoginEmail);
				LoginPassword = FindViewById<EditText>(Resource.Id.LoginPassword);
				LoginDone = FindViewById<Button>(Resource.Id.LoginDone);
				ResetPassword = FindViewById<Button>(Resource.Id.ResetPassword);
				ResetEmailText = FindViewById<TextView>(Resource.Id.ResetEmailText);
				ResetEmail = FindViewById<EditText>(Resource.Id.ResetEmail);
				ResetSendButton = FindViewById<Button>(Resource.Id.ResetSendButton);
				RegisterButton = FindViewById<Button>(Resource.Id.RegisterButton);
				ListButton = FindViewById<Button>(Resource.Id.ListButton);

				c.view = MainLayout;
				imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
				Window.SetSoftInputMode(SoftInput.AdjustPan);

				LoginPassword.KeyPress += LoginPassword_KeyPress;
				LoginDone.Click += LoginDone_Click;
				ResetPassword.Click += ResetPassword_Click;
				ResetEmail.KeyPress += ResetEmail_KeyPress;
				ResetSendButton.Click += ResetSendButton_Click;
				RegisterButton.Click += RegisterButton_Click;
				ListButton.Click += ListButton_Click;
			}
            catch (Exception ex)
            {
				c.ReportErrorSilent(ex.Message + System.Environment.NewLine + ex.StackTrace);
			}
        }

		protected override void OnResume()
		{
			try { 
				base.OnResume();

				if (!ListActivity.initialized) { return; }

				MainLayout.RequestFocus();
				
				/*if (!(snack is null) && snack.IsShown)
				{
					snack.Dismiss();
				}*/

				if (!(IntentData.error is null)) {
					c.ErrorAlert(IntentData.error + System.Environment.NewLine + System.Environment.NewLine + Resources.GetString(Resource.String.ErrorNotificationSent));
					IntentData.error = null;
					return;
				}
				if (IntentData.logout)
				{
					c.Log("Logout");
					IntentData.logout = false;

					if (!string.IsNullOrEmpty(locationUpdatesTo))
					{
						EndLocationShare();
						locationUpdatesTo = null;
					}
					locationUpdatesFrom = null;
					locationUpdatesFromData = null;

					var latitude = Session.Latitude;
					var longitude = Session.Longitude;
					var locationTime = Session.LocationTime;

					bool restart = false;
					if (c.IsLocationEnabled() && Settings.InAppLocationRate != currentLocationRate)
					{
						restart = true;
					}

					c.ClearCurrentUser();

					Session.Latitude = latitude;
					Session.Longitude = longitude;
					Session.LocationTime = locationTime;

					if (restart)
					{
						RestartLocationUpdates();
					}

					if (IntentData.authError)
					{
						IntentData.authError = false;
						c.SnackIndef(Resource.String.LoggedOut);
					}
					LoginEmail.Text = "";
					LoginPassword.Text = "";
				}
				if (!(ListActivity.listProfiles is null))
				{
					ListActivity.listProfiles.Clear();
					ListActivity.totalResultCount = null;
				}
				Session.LastDataRefresh = null;
			}
            catch (Exception ex)
            {
				c.ReportErrorSilent(ex.Message + System.Environment.NewLine + ex.StackTrace);
			}
		}

		private void LoginPassword_KeyPress(object sender, View.KeyEventArgs e)
		{
			e.Handled = false;
			if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
			{
				LoginDone_Click(null, null);
			}
		}

		private async void LoginDone_Click(object sender, EventArgs e)
		{
			imm.HideSoftInputFromWindow(MainLayout.WindowToken, 0);
			if (CheckFields())
			{
				LoginDone.Enabled = false;

				string url = "action=login&User=" + c.UrlEncode(LoginEmail.Text.Trim()) + "&Password=" + c.UrlEncode(LoginPassword.Text.Trim());

				if (File.Exists(firebaseTokenFile)) //sends the token whether it was already sent from this device or not
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
					if (File.Exists(regSaveFile))
					{
						File.Delete(regSaveFile);
					}
					if (File.Exists(firebaseTokenFile))
					{
						File.WriteAllText(tokenUptoDateFile, "True");
					}

					c.LoadCurrentUser(responseString);

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
				LoginDone.Enabled = true;
			}
			else
			{
				c.Snack(checkFormMessage);
			}
		}

		private bool CheckFields()
		{
			if (LoginEmail.Text.Trim() == "")
			{
				checkFormMessage = Resource.String.LoginEmailEmpty;
				LoginEmail.RequestFocus();
				return false;
			}
			if (LoginPassword.Text.Trim().Length < 6)
			{
				checkFormMessage = Resource.String.LoginPasswordShort;
				LoginPassword.RequestFocus();
				return false;
			}
			return true;
		}

		private void ResetPassword_Click(object sender, EventArgs e)
		{
			if (ResetEmailText.Visibility == ViewStates.Gone)
			{
				ResetEmailText.Visibility = ViewStates.Visible;
				ResetEmail.Visibility = ViewStates.Visible;
				ResetSendButton.Visibility = ViewStates.Visible;
			}
			else
			{
				ResetEmailText.Visibility = ViewStates.Gone;
				ResetEmail.Visibility = ViewStates.Gone;
				ResetSendButton.Visibility = ViewStates.Gone;
			}
		}

		private void ResetEmail_KeyPress(object sender, View.KeyEventArgs e)
		{
			e.Handled = false;
			if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
			{
				ResetSendButton_Click(null, null);
			}
		}

		private async void ResetSendButton_Click(object sender, EventArgs e)
		{
			imm.HideSoftInputFromWindow(MainLayout.WindowToken, 0);
			if (CheckFieldsReset())
			{
				ResetSendButton.Enabled = false;

				string url = "action=resetpassword&Email=" + c.UrlEncode(ResetEmail.Text.Trim());

				string responseString = await c.MakeRequest(url);
				if (responseString.Substring(0, 2) == "OK")
				{
					await c.Alert(res.GetString(Resource.String.ResetEmailSent));
					ResetEmail.Text = "";
					MainLayout.RequestFocus();
					ResetEmailText.Visibility = ViewStates.Gone;
					ResetEmail.Visibility = ViewStates.Gone;
					ResetSendButton.Visibility = ViewStates.Gone;
				}
				else
				{
					c.ReportError(responseString);
				}
				ResetSendButton.Enabled = true;
			}
			else
			{
				c.Snack(checkFormMessage);
			}
		}

		private bool CheckFieldsReset()
		{
			if (ResetEmail.Text.Trim() == "")
			{
				checkFormMessage = Resource.String.LoginEmailEmpty;
				ResetEmail.RequestFocus();
				return false;
			}
			return true;
		}

		private void RegisterButton_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(RegisterActivity));
			i.SetFlags(ActivityFlags.ReorderToFront);
			StartActivity(i);
        }

        private void ListButton_Click(object sender, EventArgs e)
        {
			Intent i = new Intent(this, typeof(ListActivity));
			i.SetFlags(ActivityFlags.ReorderToFront);
			StartActivity(i);
        }
    }
}