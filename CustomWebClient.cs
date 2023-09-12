using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;

namespace LocationConnection
{
    class CustomWebClient
    {
        const string TestCookieSign = "__test";
        public static string TestCookie = "";
        public static HttpClientHandler handler = new();
        

        public async static Task<string> Get(string url)
        {
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            string data = "";

            if (TestCookie != "")
            {
                CookieContainer cookieContainer = new();
                cookieContainer.Add(new Cookie(TestCookieSign, TestCookie) { Domain = new Uri(url).Host });
                handler.CookieContainer = cookieContainer;

                HttpClient client = new(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(Constants.RequestTimeout)
                };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Other");

                using var httpResponse = await client.GetAsync(url);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    data = await httpResponse.Content.ReadAsStringAsync();
                    httpResponse.Dispose();
                }

                if (data.Contains(TestCookieSign + "="))
			    {
				    TestCookie = "";
				    throw new Exception("TestCookie");
			    }

			    return data;
            }
            else
            {
                HttpClient client = new(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(Constants.RequestTimeout)                    
                };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Other");

                var httpResponse = await client.GetAsync(url);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    data = await httpResponse.Content.ReadAsStringAsync();
                    httpResponse.Dispose();
                }

                int start = data.IndexOf("a=toNumbers") + 13;
				//in case the server sends the reponse immediately without requiring the test cookie
                if (start == -1)
                {
                    return data;
                }
                int end = data.IndexOf("\"", start);
                string a = data.Substring(start, end - start);

				start = data.IndexOf("b=toNumbers") + 13;
				end = data.IndexOf("\"", start);
				string b = data.Substring(start, end - start);

				start = data.IndexOf("c=toNumbers") + 13;
				end = data.IndexOf("\"", start);
				string c = data.Substring(start, end - start);

                TestCookie = CustomServerCode.MainFunction(a, b, c);

                CookieContainer cookieContainer = new();
                cookieContainer.Add(new Cookie(TestCookieSign, TestCookie) { Domain = new Uri(url).Host });
                handler.CookieContainer = cookieContainer;

                client = new(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(Constants.RequestTimeout)
                };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Other");

                httpResponse = await client.GetAsync(url);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    data = await httpResponse.Content.ReadAsStringAsync();
                    httpResponse.Dispose();
                }

                if (data.Contains(TestCookieSign + "="))
                {
					TestCookie = "";
                    throw new Exception("TestCookie");
                }
                else //cookie acquired
                {
                    CommonMethods.handler.CookieContainer = cookieContainer;
                    CommonMethods.handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                    CommonMethods.client = new HttpClient(CommonMethods.handler) { Timeout = TimeSpan.FromMilliseconds(Constants.RequestTimeout) };
                    CommonMethods.client.DefaultRequestHeaders.UserAgent.ParseAdd("Other");
                }

				return data;
            }
        }

        public async static Task<string> Post(string url, string postData)
        {
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            string data = "";

            if (TestCookie != "")
            {
                CookieContainer cookieContainer = new();
                cookieContainer.Add(new Cookie(TestCookieSign, TestCookie) { Domain = new Uri(url).Host });
                handler.CookieContainer = cookieContainer;

                HttpClient client = new(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(Constants.RequestTimeout)
                };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Other");

                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
                var byteContent = new ByteArrayContent(byteArray);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                using var httpResponse = await client.PostAsync(url, byteContent);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    data = await httpResponse.Content.ReadAsStringAsync();
                    httpResponse.Dispose();
                }

                if (data.Contains(TestCookieSign + "="))
                {
                    TestCookie = "";
                    throw new Exception("TestCookie");
                }

                return data;
            }
            else
            {
                HttpClient client = new(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(Constants.RequestTimeout)
                };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Other");

                var httpResponse = await client.GetAsync(url);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    data = await httpResponse.Content.ReadAsStringAsync();
                    httpResponse.Dispose();
                }

                int start = data.IndexOf("a=toNumbers") + 13;
                //in case the server sends the reponse immediately without requiring the test cookie
                if (start == -1)
                {
                    return data;
                }
                int end = data.IndexOf("\"", start);
                string a = data.Substring(start, end - start);

                start = data.IndexOf("b=toNumbers") + 13;
                end = data.IndexOf("\"", start);
                string b = data.Substring(start, end - start);

                start = data.IndexOf("c=toNumbers") + 13;
                end = data.IndexOf("\"", start);
                string c = data.Substring(start, end - start);

                TestCookie = CustomServerCode.MainFunction(a, b, c);

                CookieContainer cookieContainer = new();
                cookieContainer.Add(new Cookie(TestCookieSign, TestCookie) { Domain = new Uri(url).Host });
                handler.CookieContainer = cookieContainer;

                client = new(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(Constants.RequestTimeout)
                };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Other");

                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
                var byteContent = new ByteArrayContent(byteArray);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                httpResponse = await client.PostAsync(url, byteContent);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    data = await httpResponse.Content.ReadAsStringAsync();
                    httpResponse.Dispose();
                }

                if (data.Contains(TestCookieSign + "="))
                {
                    TestCookie = "";
                    throw new Exception("TestCookie");
                }
                else //cookie acquired
                {
                    CommonMethods.handler.CookieContainer = cookieContainer;
                    CommonMethods.handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                    CommonMethods.client = new HttpClient(CommonMethods.handler) { Timeout = TimeSpan.FromMilliseconds(Constants.RequestTimeout) };
                    CommonMethods.client.DefaultRequestHeaders.UserAgent.ParseAdd("Other");
                }

                return data;
            }
        }
    }
}
