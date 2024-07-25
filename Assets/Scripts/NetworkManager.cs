using Joyland.GamePlay;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ThinRL.Core;
using ThinRL.Core.Tools;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using WeChatWASM;

namespace Joyland.GamePlay
{
    [Preserve]
    public class ServerResult<T>
    {
        /// <summary>
        /// 服务器返回code
        /// </summary>
        public int code;

        /// <summary>
        /// 服务器返回数据
        /// </summary>
        public T data;

        /// <summary>
        /// 服务器返回错误信息
        /// </summary>
        public string errorInfo;
    }

    [Preserve]
    /// <summary>
    /// 返回数据包装
    /// </summary>
    /// <typeparam name="T">服务器返回的业务数据结构</typeparam>
    public class HttpReturn<T>
    {
        [Preserve]
        public class ErrInfo
        {
            public string errMessage;
            public long errResponseCode;
        }

        public bool isSuccessed = false;
        public ErrInfo errInfo = new ErrInfo (){ errMessage = "no error", errResponseCode = 200 };
        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public ServerResult<T> resData;
    }

    public class NetworkManager : Singleton<NetworkManager>
    {
        private string m_BaseUrl = "";
        private string m_HeaderCookie = "";
        public string HeaderCookie
        {
            get { return m_HeaderCookie; }
        }
        public void Initialize(string baseUrl)
        {
            m_BaseUrl = baseUrl;

            m_HeaderCookie = PlayerPrefsManager.GetUserString(GamePlayerPrefsKey.UserCookie, "");
            if (m_HeaderCookie == null)
            {
                m_HeaderCookie = "";
            }

        }

        public void CacheCookieSync(string headerCookie)
        {
            if (headerCookie == null || headerCookie == "")
            {
                return;
            }
            var splitRes = headerCookie.Split(";");
            if (splitRes != null && splitRes.Length > 0)
            {
                m_HeaderCookie = splitRes[0];
                PlayerPrefsManager.SetUserString(GamePlayerPrefsKey.UserCookie, m_HeaderCookie);
            }
            else
            {
                console.error("cookie数据异常");
            }
        }


        private IEnumerator BaseRequest<T>(string method, string url, object data, Action<HttpReturn<T>> onComplete)
        {

            var httpRes = new HttpReturn<T>();
            byte[] array = null;
            if(data == null)
            {
                data = default(object);
            }
            var jsonStr = JsonConvert.SerializeObject(data);
            array = Encoding.UTF8.GetBytes(jsonStr);
            UnityWebRequest req = new UnityWebRequest(url, method);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.uploadHandler = new UploadHandlerRaw(array);
            req.timeout = 10;
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Cache-Control", "no-cache");
            if (J.Minigame.CurrentPlatform != JPlantform.Editor)
            {
                req.SetRequestHeader("cookie", HeaderCookie);
            }
            else
            {
                req.SetRequestHeader("Access-Control-Allow-Credentials", "true");
                req.SetRequestHeader("Access-Control-Expose-Headers", "Content-Length, Content-Encoding");
                req.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time, Content-Type");
                req.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                req.SetRequestHeader("Access-Control-Allow-Origin", "*");
            }

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {

                console.info("POST 请求成功！");
                // 处理响应结果
                string responseText = req.downloadHandler.text;
                httpRes.resData = JsonConvert.DeserializeObject<ServerResult<T>>(responseText);
                httpRes.isSuccessed = req.responseCode == 200;
                httpRes.headers = req.GetResponseHeaders();
                if(httpRes.isSuccessed)
                {
                    onComplete?.Invoke(httpRes);
                }
                else
                {
                    httpRes.errInfo.errMessage = $"http错误码：{req.responseCode}";
                    httpRes.errInfo.errResponseCode = req.responseCode;
                    onComplete?.Invoke(httpRes);
                }
                //Debug.Log("POST 请求成功！");
                //Debug.Log($"响应内容：{responseText}");
            }
            else
            {
                httpRes.resData = null;
                httpRes.isSuccessed = false;
                httpRes.errInfo.errMessage = req.error;
                httpRes.errInfo.errResponseCode = req.responseCode;
                console.error($"请求失败：{req.error}");
                onComplete?.Invoke(httpRes);
            }

        }

        public void GetReqWithFullURL<T>(string fullURL, object data, Action<HttpReturn<T>> onComplete)
        {
            CoroutineManager.Instance.StartCoroutine(BaseRequest<T>(UnityWebRequest.kHttpVerbGET, fullURL, data, onComplete));
        }
        public void GetReq<T>(string url, object data, Action<HttpReturn<T>> onComplete)
        {
            CoroutineManager.Instance.StartCoroutine(BaseRequest<T>(UnityWebRequest.kHttpVerbGET, m_BaseUrl+ url, data, onComplete));
        }

        public void PostReqWithFullURL<T>(string fullURL, object data, Action<HttpReturn<T>> onComplete)
        {
            CoroutineManager.Instance.StartCoroutine(BaseRequest<T>(UnityWebRequest.kHttpVerbPOST, fullURL, data, onComplete));
        }
        public void PostReq<T>(string url, object data, Action<HttpReturn<T>> onComplete)
        {
            CoroutineManager.Instance.StartCoroutine(BaseRequest<T>(UnityWebRequest.kHttpVerbPOST, m_BaseUrl + url, data, onComplete));
        }

    }
}

