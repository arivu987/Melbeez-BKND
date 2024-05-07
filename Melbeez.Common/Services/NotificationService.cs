using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Melbeez.Common.Services
{
    public class NotificationService
    {
        public readonly IConfiguration Configuration;
        public NotificationService()
        {

        }
        public static string Send(List<string> DeviceToken, string Title, string Desc, string Type, string RefId, string FromId, int? Badge, string FireBaseKey = null, string FireBaseRequestUrl = null)
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
                    result = SendNotificationFromFirebaseCloud(Title, Desc, Type, RefId, FromId, Badge, DeviceToken, FireBaseKey, FireBaseRequestUrl);
                }
            }
            return result;
        }

        protected static string SendNotificationFromFirebaseCloud(string Title, string desc, string type, string refId, string fromId, int? Badge, List<string> DeviceToken, string key, string FireBaseRequestUrl)
        {
            string result = string.Empty;
            WebRequest tRequest = WebRequest.Create(FireBaseRequestUrl);
            tRequest.Method = "post";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", key));
            tRequest.ContentType = "application/json";

            var payload = new
            {
                registration_ids = DeviceToken,
                priority = "high",
                content_available = true,
                notification = new
                {
                    title = Title,
                    body = desc,
                    type,
                    refId,
                    fromId,
                    badge = Badge
                },
                data = new
                {
                    title = Title,
                    body = desc,
                    type,
                    refId,
                    fromId,
                    badge = Badge
                }
            };
            string postbody = JsonConvert.SerializeObject(payload).ToString();

            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
            tRequest.ContentLength = byteArray.Length;
            using (Stream dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                result = tReader.ReadToEnd();
                            }
                    }
                }
            }
            return result;
        }
    }
}
