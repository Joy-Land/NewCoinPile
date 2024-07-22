using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System.Net;
using System;
using System.Security.Cryptography;

namespace ThinRL.Core
{
    public class DingTalkNotifier
    {

        /// <summary>
        /// 向dingding发送http请求信息
        /// </summary>
        /// <param name="customInfo"></param>
        public static void NotifyDingtalk(string customInfo, string dingTalkGroupUrl)
        {
            // 是用https不报验证错
            // https://forums.xamarin.com/discussion/10405/the-authentication-or-decryption-has-failed-in-the-web-request
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

            //改掉影响格式的字符
            customInfo = customInfo.Replace("\\", "/");
            customInfo = customInfo.Replace("\"", "'");

            //发送信息，内容固定为文本格式
            var message = "{ \"msgtype\": \"text\", \"text\": {\"content\": \"" + customInfo + "\"}}";
            Debug.LogWarning(message);
            HttpWebRequest request = HttpWebRequest.Create(dingTalkGroupUrl) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            Encoding encoding = Encoding.GetEncoding("UTF-8");
            byte[] data = encoding.GetBytes(message);
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            //返回部分可选
            var httpWebResponse = request.GetResponse() as HttpWebResponse;
            if ((int)httpWebResponse.StatusCode != 200)
            {
                StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream());
                Debug.Log(sr.ReadToEnd());
            }
        }

        public static void NotifyDingtalkWithSign(string customInfo, string dingTalkGroupUrl, string secretKey, string atUsers = "")
        {
            // 是用https不报验证错
            // https://forums.xamarin.com/discussion/10405/the-authentication-or-decryption-has-failed-in-the-web-request
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

            //改掉影响格式的字符
            customInfo = customInfo.Replace("\\", "/");
            customInfo = customInfo.Replace("\"", "'");

            //创建UTF-8编码器
            Encoding encoding = Encoding.GetEncoding("UTF-8");

            //对secret编码
            secretKey = encoding.GetString(encoding.GetBytes(secretKey.ToString()));

            //获取时间戳并编码
            var nowTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).Ticks / 10000;
            string timestamp = encoding.GetString(encoding.GetBytes(nowTime.ToString()));

            //合成校验信息
            string signString = timestamp + "\n" + secretKey;

            //创建HMACSHA256编码器，对校验串加密
            var hmacsha256 = new HMACSHA256(encoding.GetBytes(secretKey));
            string hashmsg = Convert.ToBase64String(hmacsha256.ComputeHash(encoding.GetBytes(signString)));
            
            string urlmsg = UnityWebRequest.EscapeURL(hashmsg);
            urlmsg = encoding.GetString(encoding.GetBytes(urlmsg.ToString()));

            //合成目的URL
            string targetUrl = dingTalkGroupUrl + "&timestamp=" + timestamp + "&sign=" + urlmsg;

            targetUrl = encoding.GetString(encoding.GetBytes(targetUrl.ToString()));

            //发送POST请求
            var message = "{" + atUsers + "\"msgtype\": \"text\", \"text\": {\"content\": \"" + customInfo + "\"}}";
            Debug.LogWarning(message);
            HttpWebRequest request = HttpWebRequest.Create(targetUrl) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            
            byte[] data = encoding.GetBytes(message);
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            //回收报文
            var httpWebResponse = request.GetResponse() as HttpWebResponse;
            StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream());
            if ((int)httpWebResponse.StatusCode == 200)
            {
                Debug.Log("POST Success " + sr.ReadToEnd());
            }
            else{
                Debug.Log("POST Fail " + sr.ReadToEnd());
            }
        }

    }


}