using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

//using LuaInterface;

using UnityEngine;

// using System.Security.Cryptography;
namespace ThinRL.Core.Tools
{
    public static partial class Utils
    {
//        static readonly System.Security.Cryptography.MD5 _md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
//        /// <summary>
//        /// 生成文件MD5码
//        /// </summary>
//        public static string GenerateMD5File(string filePath)
//        {
//            try
//            {
//                FileStream fs = File.OpenRead(filePath);
//                byte[] retVal = _md5.ComputeHash(fs);
//                fs.Flush();
//                fs.Close();
//                fs.Dispose();
//                StringBuilder sb = new StringBuilder();
//                for (int i = 0; i < retVal.Length; i++)
//                {
//                    sb.Append(retVal[i].ToString("x2"));
//                }
//                return sb.ToString();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("GenerateMD5File() fail, error:" + ex.Message);
//            }
//        }

//        /// <summary>
//        /// 解析key：value格式文件
//        /// </summary>
//        public static void ParseDictionary(string content, Dictionary<string, string> dic)
//        {
//            if (content == null || content.Length == 0)
//                return;
//            string[] lines = content.Split(new char[] { '\r', '\n' });
//            foreach (string line in lines)
//            {
//                if (string.IsNullOrEmpty(line))
//                    continue;
//                string[] infos = line.Split(':');
//                if (dic.ContainsKey(infos[0]) == false)
//                {
//                    //Debug.Log("infos[0]1:" + infos[0]);
//                    dic.Add(infos[0], infos[1]);
//                }
//                else
//                {
//                    Debug.LogError("infos[0]:" + infos[0]);
//                }
//            }
//        }

//        public static string SaveDictionary(Dictionary<string, string> dic)
//        {
//            var sb = new StringBuilder();
//            if (dic == null || dic.Count == 0)
//                return string.Empty;
//            foreach (var item in dic)
//            {
//                sb.AppendLine($"{item.Key}:{item.Value}");
//            }
//            return sb.ToString();
//        }

//        /// <summary>
//        /// 获取当前文件夹的所有文件
//        /// </summary>
//        public static List<string> GetCurAllFiles(string path, List<string> fileList, params string[] filterExts)
//        {
//            string[] files = Directory.GetFiles(path);
//            foreach (string file in files)
//            {
//                string ext = Path.GetExtension(file);
//                bool flag = true;
//                for (int i = 0; i < filterExts.Length; i++)
//                {
//                    if (ext == filterExts[i])
//                    {
//                        flag = false;
//                    }
//                }
//                if (flag)
//                {
//                    fileList.Add(file);
//                }
//            }
//            return fileList;
//        }

//        /// <summary>
//        /// 获取所有文件名
//        /// </summary>
//        public static List<string> GetAllFiles(string path, List<string> fileList, params string[] filterExts)
//        {
//            string[] files = Directory.GetFiles(path);
//            foreach (string file in files)
//            {
//                string ext = Path.GetExtension(file);
//                bool flag = true;
//                for (int i = 0; i < filterExts.Length; i++)
//                {
//                    if (ext == filterExts[i])
//                    {
//                        flag = false;
//                    }
//                }
//                if (flag)
//                {
//                    fileList.Add(file);
//                }
//            }

//            string[] directorys = Directory.GetDirectories(path);
//            foreach (string directory in directorys)
//            {
//                GetAllFiles(directory, fileList, filterExts);
//            }

//            return fileList;
//        }

//        /// <summary>
//        /// 拷贝所有文件
//        /// </summary>
//        public static void CopyAllFiles(string sourcePath, string destPath)
//        {
//            string[] files = Directory.GetFiles(sourcePath);
//            foreach (string file in files)
//            {
//                string fileName = Path.GetFileName(file);
//                string destFile = Path.Combine(destPath, fileName);
//                File.Copy(file, destFile);
//            }

//            string[] directorys = Directory.GetDirectories(sourcePath);
//            foreach (string directory in directorys)
//            {
//                CopyAllFiles(directory, destPath);
//            }
//        }

//        /// <summary>
//        /// 拷贝文件夹（附带子文件夹）
//        /// </summary>
//        public static void CopyFolder(string sourcePath, string destPath, ref int index)
//        {
//            if (!Directory.Exists(destPath))
//                Directory.CreateDirectory(destPath);

//            string[] files = Directory.GetFiles(sourcePath);
//            foreach (string file in files)
//            {
//                string fileName = Path.GetFileName(file);
//                string destFile = Path.Combine(destPath, fileName);
//                File.Copy(file, destFile);
//                index++;
//            }

//            string[] directorys = Directory.GetDirectories(sourcePath);
//            foreach (string directory in directorys)
//            {
//                string diretoryName = Path.GetFileName(directory);
//                string destDirectory = Path.Combine(destPath, diretoryName);
//                CopyFolder(directory, destDirectory, ref index);
//            }
//        }

//        /// <summary>
//        /// copy完整的文件夹
//        /// </summary>
//        public static void CopyFullFolder(string sPath, string dPath)
//        {
//            if (!Directory.Exists(dPath))
//            {
//                Directory.CreateDirectory(dPath);
//            }

//            string[] files = Directory.GetFiles(sPath);
//            for (int i = 0; i < files.Length; i++)
//            {
//                string sFile = files[i];
//                string fileName = Path.GetFileName(sFile);
//                string dFile = Path.Combine(dPath, fileName);
//                File.Copy(sFile, dFile);
//            }

//            string[] directorys = Directory.GetDirectories(sPath);
//            for (int i = 0; i < directorys.Length; i++)
//            {
//                string sDirectory = directorys[i];
//                string directoryName = Path.GetFileName(sDirectory);
//                string dDirectory = Path.Combine(dPath, directoryName);
//                CopyFullFolder(sDirectory, dDirectory);
//            }
//        }

//        /// <summary>
//        /// copy完整的文件夹（另一种实现）
//        /// </summary>
//        public static void CopyFullFolder1(string sPath, string dPath)
//        {
//            if (!Directory.Exists(sPath))
//            {
//                return;
//            }
//            int length = sPath.Length;
//            if (sPath[length - 1] == '\\' || sPath[length - 1] == '/')
//            {
//                length--;
//            }

//            string[] sFiles = Directory.GetFiles(sPath, "", SearchOption.AllDirectories);
//            for (int i = 0; i < sFiles.Length; i++)
//            {
//                string sFile = sFiles[i];
//                string rFile = sFile.Remove(0, length);
//                string dFile = dPath + "/" + rFile;
//                string dDirectory = Path.GetDirectoryName(dFile);
//                Directory.CreateDirectory(dDirectory);
//                File.Copy(sFile, dFile);
//            }
//        }


//        /// <summary>
//        /// Base64加密，采用utf8编码方式加密
//        /// </summary>
//        /// <param name="source">待加密的明文</param>
//        /// <returns>加密后的字符串</returns>
//        public static string Base64Encode(string source)
//        {
//            return Base64Encode(Encoding.UTF8, source);
//        }

//        /// <summary>
//        /// Base64加密
//        /// </summary>
//        /// <param name="encodeType">加密采用的编码方式</param>
//        /// <param name="source">待加密的明文</param>
//        /// <returns></returns>
//        public static string Base64Encode(Encoding encodeType, string source)
//        {
//            string encode = string.Empty;
//            byte[] bytes = encodeType.GetBytes(source);
//            try
//            {
//                encode = Convert.ToBase64String(bytes);
//            }
//            catch
//            {
//                encode = source;
//            }
//            return encode;
//        }

//        private static DateTime _datetime1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
//        /// 获取当前时间戳
//        /// </summary>
//        /// <param name="bflag">为真时获取10位时间戳,为假时获取13位时间戳.</param>
//        /// <returns></returns>
//        public static long GetTimeStamp(bool bflag = true)
//        {
//            TimeSpan ts = DateTime.UtcNow - _datetime1970;
//            long ret;
//            if (bflag)
//                ret = Convert.ToInt64(ts.TotalSeconds);
//            else
//                ret = Convert.ToInt64(ts.TotalMilliseconds);
//            return ret;
//        }

//        public class TimeFormat
//        {
//            public int day;
//            public int hour;
//            public int min;
//            public int sec;
//        }

//        public static TimeFormat GetTimeFormat(int totalSec)
//        {
//            if (totalSec < 0)
//                return null;
//            TimeFormat obj = new TimeFormat();
//            obj.day = totalSec / 86400;
//            obj.hour = (totalSec % 86400) / 3600;
//            obj.min = (totalSec % 3600) / 60;
//            obj.sec = totalSec % 60;
//            return obj;
//        }

//        public static Color StringToColor(string colorStr)
//        {
//            if (string.IsNullOrEmpty(colorStr))
//            {
//                return new Color();
//            }
//            if (colorStr[0] == '#')
//            {
//                colorStr = colorStr.Substring(1);
//            }
//            int colorInt = 0;
//            try
//            {
//                colorInt = int.Parse(colorStr, System.Globalization.NumberStyles.AllowHexSpecifier);
//            }
//            catch (Exception ex)
//            {
//                // DebugHelper.LogError(ex.Message);
//            }
//            return IntToColor(colorInt);
//        }

//        //[NoToLuaAttribute]
//        public static Color IntToColor(int colorInt)
//        {
//            float basenum = 255;

//            int b = 0xFF & colorInt;
//            int g = 0xFF00 & colorInt;
//            g >>= 8;
//            int r = 0xFF0000 & colorInt;
//            r >>= 16;
//            return new Color((float)r / basenum, (float)g / basenum, (float)b / basenum, 1);

//        }

//        public static void DownloadLocalData(string url, System.Action<string> callBack)
//        {
//            using (FileStream fsRead = new FileStream(url, FileMode.Open))
//            {
//                int fsLen = (int)fsRead.Length;
//                byte[] heByte = new byte[fsLen];
//                int r = fsRead.Read(heByte, 0, heByte.Length);
//                string content = System.Text.Encoding.UTF8.GetString(heByte);
//                fsRead.Close();
//                if (callBack != null)
//                {
//                    callBack(content);
//                }
//            }
//        }

//        public static void ReadTextFile(string fileName, System.Action<string> callBack)
//        {
//            if (string.IsNullOrEmpty(fileName) || callBack == null) return;
//            string fullPath = Application.persistentDataPath + Path.DirectorySeparatorChar + fileName;
//            try
//            {
//                DownloadLocalData(fullPath, callBack);
//            }
//            catch (Exception e)
//            {
//                //  DebugHelper.Log(e.Message);  
//                if (callBack != null)
//                {
//                    callBack("");
//                }
//            }
//        }

//        public static void WriteTextFile(string fileName, string content)
//        {
//            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(content)) return;
//            string fullPath = Application.persistentDataPath + Path.DirectorySeparatorChar + fileName;
//            // byte[] bytes = Encryption(content);
//            byte[] bytes = UTF8Encoding.UTF8.GetBytes(content);
//            try
//            {
//                using (FileStream fs = new FileStream(fullPath, FileMode.Create))
//                {
//                    fs.Write(bytes, 0, bytes.Length);
//                    fs.Flush();
//                    fs.Close();
//                    fs.Dispose();
//                }
//            }
//            catch (Exception e)
//            {
//                //  DebugHelper.Log(e.Message);  
//            }
//        }

//        public static void Invoke(Action<int> callBack, int param)
//        {
//            callBack.Invoke(param);
//        }

//        /// <summary>
//        /// 获取安卓平台上键盘的高度
//        /// </summary>
//        /// <returns></returns>
//        private static int GetAndroidKeyboardHeight()
//        {
//            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//            {
//                AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").
//                    Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
//                using (AndroidJavaObject Pt = new AndroidJavaObject("android.graphics.Point"))
//                {
//                    UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getSize", Pt);

//                    using (AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect"))
//                    {
//                        View.Call("getWindowVisibleDisplayFrame", Rct);
//                        int h = Pt.Get<int>("y");
//                        int v = Rct.Call<int>("height");
//                        float r = (float)(h - v) / (float)h;
//                        int result = (int)(r * Screen.height);
//                        return result;
//                    }
//                }
//            }
//        }

//        public static int GetAndroidPixelScreenHeight()
//        {
//            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//            {
//                AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").
//                    Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
//                using (AndroidJavaObject Pt = new AndroidJavaObject("android.graphics.Point"))
//                {
//                    UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getSize", Pt);
//                    int h = Pt.Get<int>("y");
//                    return h;
//                }
//            }
//        }

//        /// <summary>
//        /// 获取iOS平台上键盘的高度
//        /// </summary>
//        /// <returns></returns>
//        private static float GetIOSKeyboardHeight()
//        {
//#if UNITY_IOS
//        return TouchScreenKeyboard.area.height;
//#else
//            return 0.0f;
//#endif
//        }

//        /// <summary>
//        /// 获取键盘高度
//        /// </summary>
//        /// <returns></returns>
//        public static float GetKeyboardHeight()
//        {
//            /*
//            if(Application.isEditor){
//                return 0.4f*Screen.height;
//            }
//            else if(SystemManager.getPlatform() == SystemManager.iOS){
//                return GetIOSKeyboardHeight();
//            }
//            else if(SystemManager.getPlatform() == SystemManager.Android){
//                return GetAndroidKeyboardHeight();
//            } 
//            */
//            return 0f;
//        }

//        public static void updateAndroidAutoExitConfig(bool auto, bool autoHigher, int duration)
//        {
//#if UNITY_ANDROID && !UNITY_EDITOR
//        AndroidJavaClass jc2 = new AndroidJavaClass("com.jykplugin.notification.JobSchedulerTool");
//        jc2.CallStatic("autoExitApp", auto);
//        jc2.CallStatic("autoExitAppHigher", autoHigher);
//        jc2.CallStatic("setDurationTime", duration);
//#endif
//        }

//#if UNITY_ANDROID && !UNITY_EDITOR
//    public static AndroidJavaObject dic2Map(Dictionary<string, string> dictionary) {
//        if(dictionary == null) {
//            return null;
//        } 
//        AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
//        foreach(KeyValuePair<string, string> pair in dictionary)
//        {
//            map.Call<string>("put", pair.Key, pair.Value);
//        }
//        return map;
//    }
//#endif

//        public static void CreateOrOpenFile(string path, string info)
//        {          //路径、文件名、写入内容
//            FileInfo fi = new FileInfo(path);
//            StreamWriter sw;
//            // StreamWriter sw = new StreamWriter(path+ name, false);

//            //File.Create(path + name);

//            //sw.WriteLine("");
//            //删除文件，重写文件的内容
//            fi.Delete();
//            if (!fi.Exists)
//            {
//                //File.Create(path+name);
//                //创建文件
//                sw = fi.CreateText();
//            }
//            else
//            {
//                sw = fi.AppendText();
//            }
//            //在 文件原有内容上 ，写入文件
//            sw.WriteLine(info);
//            sw.Close();
//            sw.Dispose();
//            //DebugHelper.Log("CreateOrOpenFile, file = " + path);
//        }

//        public static void ClearOrCreateDirectory(string path)
//        {
//            if (Directory.Exists(path) == false)
//            {
//                Directory.CreateDirectory(path);
//            }
//            else
//            {
//                var files = Directory.GetFiles(path);
//                var directories = Directory.GetDirectories(path);
//                foreach (var file in files)
//                {
//                    File.Delete(file);
//                }

//                foreach (var directory in directories)
//                {
//                    Directory.Delete(directory, true);
//                }
//            }
//        }
    }
}