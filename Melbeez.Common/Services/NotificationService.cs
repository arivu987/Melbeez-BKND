using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Net;
using System.Text;

namespace Melbeez.Common.Services
{
    public class NotificationService
    {
        public readonly IConfiguration Configuration;

        private static readonly HttpClient httpClient = new HttpClient();

        public NotificationService()
        {

        }
        public static async Task<string> Send(List<string> DeviceToken, string Title, string Desc, string Type, string RefId, string FromId, string FirebaseAccount = null, string FireBaseRequestUrl = null)
        {
            string result = string.Empty;
            bool HasMore = true;
            int offset = 0, limit = 1000;
            if (DeviceToken.Count > 0)
            {
                while (HasMore)
                {
                    var newList = DeviceToken.Skip(offset).Take(limit).ToList();

                    if (newList.Count() < limit)
                    {
                        HasMore = false;
                    }
                    offset += limit;
                    result = await SendNotificationFromFirebaseCloud(Title, Desc, Type, RefId, FromId, newList, FirebaseAccount, FireBaseRequestUrl);
                }
            }
            return result;
        }

        public static async Task<string> SendNotificationFromFirebaseCloud(
        string title, string desc, string type, string refId, string fromId,
        List<string> deviceTokens, string FirebaseAccount, string firebaseRequestUrl)
        {
            try
            {
                // 🔹 Step 1: Validate Inputs
                title = title ?? "Default Title";
                desc = desc ?? "Default Description";
                type = type ?? "general";
                refId = refId ?? "0";
                fromId = fromId ?? "system";

                if (deviceTokens == null || !deviceTokens.Any())
                {
                    throw new Exception("Device token list is empty or null.");
                }

                if (string.IsNullOrEmpty(FirebaseAccount) || !File.Exists(FirebaseAccount))
                {
                    throw new Exception("Firebase account JSON file not found.");
                }

                // 🔹 Step 2: Get Firebase Access Token
                string accessToken = await GetAccessToken(FirebaseAccount);
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new Exception("Failed to retrieve Firebase Access Token.");
                }

                string result = "";

                using (HttpClient httpClient = new HttpClient())
                {
                    deviceTokens = deviceTokens.Where(t => t.Contains(":")).ToList(); // Filter invalid tokens

                    foreach (var token in deviceTokens)
                    {
                        var payload = new
                        {
                            message = new
                            {
                                token = token,
                                notification = new
                                {
                                    title,
                                    body = desc
                                },
                                android = new
                                {
                                    priority = "high"
                                },
                                apns = new
                                {
                                    headers = new
                                    {
                                        apns_priority = "10"
                                    }
                                }
                            }
                        };

                        Console.WriteLine($"Payload before serialization: {payload}");

                        string postBody = JsonConvert.SerializeObject(payload, Formatting.Indented,
                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                        var request = new HttpRequestMessage(HttpMethod.Post, firebaseRequestUrl)
                        {
                            Headers = { { "Authorization", $"Bearer {accessToken}" } },
                            Content = new StringContent(postBody, Encoding.UTF8, "application/json")
                        };

                        Console.WriteLine($"Sending notification to: {token}");
                        Console.WriteLine($"Payload: {postBody}");

                        HttpResponseMessage response = await httpClient.SendAsync(request);
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Firebase Error: {response.StatusCode} - {responseContent}");
                            result += $"Error: {response.StatusCode} - {responseContent}\n";
                        }
                        else
                        {
                            result += responseContent + "\n";
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Firebase notification: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }



        // 🔹 Generate Firebase OAuth 2.0 Access Token
        private static async Task<string> GetAccessToken(string FirebaseAccount)
        {
            try
            {
                GoogleCredential credential = GoogleCredential.FromFile(FirebaseAccount)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

                var tokenResponse = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                return tokenResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating access token: {ex.Message}");
                return null;
            }
        }
    }
}
