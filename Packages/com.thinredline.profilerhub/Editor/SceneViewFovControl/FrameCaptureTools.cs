using System;
using System.Reflection;
using UnityEditorInternal;

#if !UNITY_EDITOR
#error This script must be placed under "Editor/" directory.
#endif

namespace ThinRL.ProfilerHub.Editor.SceneViewFovControl
{
    static class FrameCaptureTools
    {
        public static UnityEditor.EditorWindow gameView;
        public static bool IsLoaded
        {
            get
            {
#if !UNITY_EDITOR
            return false;
#endif
                return RenderDoc.IsLoaded();
            }
        }

        public static bool IsInstalled
        {
            get
            {
#if !UNITY_EDITOR
            return false;
#endif
                return RenderDoc.IsInstalled();
            }
        }

        public static bool IsSupported
        {
            get
            {
#if !UNITY_EDITOR
            return false;
#endif
                return RenderDoc.IsSupported();
            }
        }
        public static void LoadRenderDoc()
        {
            if (IsInstalled && IsLoaded == false)
            {
                Assembly asm = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
                // Assembly asm = Assembly.LoadFile(@"D:\Unity\Unity2018.4.7f1\Editor\Data\Managed\UnityEditor.dll");
                System.Type shaderUtilType = asm.GetType("UnityEditor.ShaderUtil");
                MethodInfo method = shaderUtilType.GetMethod("RequestLoadRenderDoc", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                method.Invoke(null, new System.Object[] { });
                RenderDoc.Load();
            }
        }

        //反射HostView测试，貌似无法拿到HostView实例。。
        public static void GetHost()
        {

            System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
            //子类
            Type type = assembly.GetType("UnityEditor.GameView");
            //基类
            Type typeGUIView = assembly.GetType("UnityEditor.GUIView");

            Type hostView = assembly.GetType("UnityEditor.HostView");
            object obj = Activator.CreateInstance(hostView);
            //获得私有字段
            FieldInfo fi = type.GetField("m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);
            //FieldInfo[] fis = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            //Console.WriteLine(fis.Length);
            //foreach (var fieldInfo in fis)
            //{
            //    Console.WriteLine(fieldInfo.FieldType + ":" + fieldInfo.Name + ":" + fieldInfo.GetValue(obj));
            //}

            //gameView = UnityEditor.EditorWindow.GetWindow(typeGUIView);
            var method = fi.FieldType.BaseType.GetMethod("CaptureRenderDocScene", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //Convert.ChangeType(fi,)
            //Convert.ChangeType(fi, typeGUIView);

            method.Invoke(hostView, new System.Object[] { });
            //UnityEngine.Debug.Log("fzy aaa:" + typeGUIView.Name + "  " + fi.Name + "  " + fi.FieldType.BaseType + " " + method.Name + "  " + obj + "   ");
        }

        public static void BeginCapture()
        {
            if (IsLoaded == false) throw new Exception("Render Not Initialized");
            System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
            Type gameViewType = assembly.GetType("UnityEditor.GameView");
            gameView = UnityEditor.EditorWindow.GetWindow(gameViewType);
            RenderDoc.BeginCaptureRenderDoc(gameView);
            var repaintImm = gameViewType.GetMethod("RepaintImmediately", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            repaintImm.Invoke(gameView, new System.Object[] { });
            //UnityEngine.Debug.Log("fzy Reflect Method:" + repaintImm);
        }
        public static void EndCapture()
        {
            if (IsLoaded == false) throw new Exception("Render Not Initialized");
            RenderDoc.EndCaptureRenderDoc(gameView);
        }

        public static void CaptureGameScene()
        {
            if (IsLoaded == false) LoadRenderDoc();
            BeginCapture();
            EndCapture();
        }
    }
}
