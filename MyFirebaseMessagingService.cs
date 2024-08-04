using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Messaging;

namespace LocationConnection
{
	[Service(Exported = false)]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class MyFirebaseMessagingService : FirebaseMessagingService
	{
		public override void OnMessageReceived(RemoteMessage message)
		{
            System.Diagnostics.Debug.WriteLine(DateTime.UtcNow.ToString(@"yyyy-MM-dd HH\:mm\:ss.fff") + "---------- OnMessageReceived ----------" + System.Environment.NewLine);
			Intent intent = new Intent("balintfodor.locationconnection.ChatReceiver");
			intent.PutExtra("fromuser", message.Data["fromuser"]);
			intent.PutExtra("touser", message.Data["touser"]);
			intent.PutExtra("type", message.Data["type"]);
			intent.PutExtra("meta", message.Data["meta"]);
			intent.PutExtra("inapp", (int.Parse(message.Data["inapp"]) == 0) ? false : true);

			if (!(message.GetNotification() is null)) {
				intent.PutExtra("title", message.GetNotification().Title);
				intent.PutExtra("body", message.GetNotification().Body);
			}
			else if (message.Data.ContainsKey("title"))
			{
				intent.PutExtra("title", message.Data["title"]);
				intent.PutExtra("body", message.Data["body"]);
			}

			SendBroadcast(intent);
		}

		public override async void OnNewToken(string p0)
		{
            System.Diagnostics.Debug.WriteLine(DateTime.UtcNow.ToString(@"yyyy-MM-dd HH\:mm\:ss.fff") + "---------- OnNewToken ----------" + System.Environment.NewLine);

            base.OnNewToken(p0);

			File.WriteAllText(BaseActivity.firebaseTokenFile, p0);
			File.WriteAllText(BaseActivity.tokenUptoDateFile, "False");

			CommonMethods c = new CommonMethods(null);
			if (c.IsLoggedIn()) //might never be true
			{
				string responseString = await c.MakeRequest("action=updatetoken&ID=" + Session.ID + "&SessionID=" + Session.SessionID + "&token=" + p0);
				if (responseString == "OK")
				{
					File.WriteAllText(BaseActivity.tokenUptoDateFile, "True");
				}
				else
				{
					c.LogError("Error sending token: ID=" + Session.ID + "&SessionID=" + Session.SessionID + "&token=" + p0);
				}
			}
		}
	}
}