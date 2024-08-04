using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LocationConnection
{
	public class SlowAES
	{
		private byte[] _key;
		private byte[] _iv;

		public SlowAES(byte[] key, byte[] iv)
		{
			_key = key;
			_iv = iv;
		}

		public byte[] Encrypt(byte[] plaintext)
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = _key;
				aes.IV = _iv;
				aes.Padding = PaddingMode.Zeros; // Set the padding mode

				using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
				{
					return PerformCryptography(plaintext, encryptor);
				}
			}
		}

		public byte[] Decrypt(byte[] ciphertext)
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = _key;
				aes.IV = _iv;
				aes.Padding = PaddingMode.Zeros; // Set the padding mode

				using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
				{
					return PerformCryptography(ciphertext, decryptor);
				}
			}
		}

		private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
				{
					cryptoStream.Write(data, 0, data.Length);
					cryptoStream.FlushFinalBlock();
					return memoryStream.ToArray();
				}
			}
		}
	}
}