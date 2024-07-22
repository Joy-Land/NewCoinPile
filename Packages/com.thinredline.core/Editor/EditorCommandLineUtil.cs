using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThinRL.Core.Editor
{
    static public class EditorCommandLineUtil
    {
        private static string ConvertBase642UTF8(string s)
        {
            string ss = "";
            try
            {
                byte[] bs = System.Convert.FromBase64String(s);
                ss = System.Text.Encoding.GetEncoding("utf-8").GetString(bs);
            }
            catch (Exception)
            {
                ss = s;
            }
            return ss;
        }        
        public static string[] UnityCommandLine_ExecuteMethod_ParseParamsFromArgs(string[] args)
        {
            List<string> ps = new List<string>();

            if (args == null) return ps.ToArray();            

            bool inParamRange = false;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (string.IsNullOrEmpty(arg)) continue;

                if (inParamRange)
                {
                    if (arg[0] == '-') break;//到这里结束
                    ps.Add(arg);
                }
                else
                {
                    if (arg == "-executeMethod")
                    {
                        inParamRange = true;//从这里开始
                        i++;//跳过方法名，这次循环相当于连续i++两次。
                    }
                }
            }

            return ps.ToArray();
        }
        public class JenkinsBaseUnityJobParam
        {
            public string branch = "";
            public string platform = "";
        }
        public static T UnityCommandLine_ExecuteMethod_ParseFromArgs<T>(string[] args) where T : class
        {
            string[] ps = UnityCommandLine_ExecuteMethod_ParseParamsFromArgs(args);
            if (ps.Length <= 0) return null;

            string json = ConvertBase642UTF8(ps[0]);
            if (string.IsNullOrEmpty(json)) return null;
            
            return JsonUtility.FromJson<T>(json);
        }
    }
}
