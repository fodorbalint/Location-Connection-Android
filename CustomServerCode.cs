using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocationConnection
{
	public class CustomServerCode
	{
		public static List<byte> ToNumbers(string hexString)
		{
			List<byte> result = new List<byte>();

			for (int i = 0; i < hexString.Length; i += 2)
			{
				string hexByte = hexString.Substring(i, 2);
				byte value = Convert.ToByte(hexByte, 16);
				result.Add(value);
			}

			return result;
		}

		public static string ToHex(params byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();

			foreach (byte b in bytes)
			{
				sb.Append(b.ToString("x2"));
			}

			return sb.ToString();
		}

		public static string MainFunction(string astr, string bstr, string cstr)
		{
			List<byte> a = ToNumbers(astr);
			List<byte> b = ToNumbers(bstr);
			List<byte> c = ToNumbers(cstr);

			SlowAES slowAES = new SlowAES(a.ToArray(), b.ToArray());
			byte[] decrypted = slowAES.Decrypt(c.ToArray());
			return ToHex(decrypted);
		}
	}
}