using System.IO;
using UnityEngine;

#if UNITY_IOS


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;

#endif

namespace ThinRL.Core.Editor{
    public class CorePBXEditor
    {
        //该属性是在build完成后，被调用的callback
        [PostProcessBuildAttribute(0)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            // BuildTarget需为iOS
            if (buildTarget != BuildTarget.iOS)
                return;

            // 初始化
            var projectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxProject = new PBXProject();            
            pbxProject.ReadFromFile(projectPath);
            string targetGuid = pbxProject.GetUnityFrameworkTargetGuid(); // pbxProject.TargetGuidByName("Unity-iPhone");


            // 关闭Bitcode
            pbxProject.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            // 开启 OC exception
            pbxProject.SetBuildProperty(targetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
            // 开启 modules
            pbxProject.SetBuildProperty(targetGuid, "CLANG_ENABLE_MODULES", "YES");
            
            // 添加framwrok
            pbxProject.AddFrameworkToProject(targetGuid, "MobileCoreServices.framework", false);
            pbxProject.AddFrameworkToProject(targetGuid, "Security.framework", false);
            pbxProject.AddFrameworkToProject(targetGuid, "MessageUI.framework", false);
            pbxProject.AddFrameworkToProject(targetGuid, "CoreData.framework", false);
            pbxProject.AddFrameworkToProject(targetGuid, "UserNotifications.framework", false);
            pbxProject.AddFrameworkToProject(targetGuid, "ReplayKit.framework", false);
            pbxProject.AddFrameworkToProject(targetGuid, "AdSupport.framework", false);
            pbxProject.AddFrameworkToProject(targetGuid, "AppTrackingTransparency.framework", false);
            // 添加SystemCapabilities,
#if UNITY_2017_2_OR_NEWER
            pbxProject.AddCapability(targetGuid, PBXCapabilityType.PushNotifications);// available for unity 2017.1.0+
#endif
            //添加lib
            AddLibToProject(pbxProject, targetGuid, "libxml2.tbd");


            // 修改Info.plist文件
            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.CreateDict("ITSAppUsesNonExemptEncryption");
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            plist.root.CreateDict("NSCameraUsageDescription");
            plist.root.SetString("NSCameraUsageDescription", "Need your consent, APP can access the camera");
            plist.root.CreateDict("NSMicrophoneUsageDescription");
            plist.root.SetString("NSMicrophoneUsageDescription", "Need your consent, APP can access the microphone");
            plist.root.CreateDict("NSPhotoLibraryUsageDescription");
            plist.root.SetString("NSPhotoLibraryUsageDescription", "Need your consent, APP can access the Photo Library");
            plist.root.CreateDict("NSLocationWhenInUseUsageDescription");
            plist.root.SetString("NSLocationWhenInUseUsageDescription", "Need your consent, App can access the Location");            
            plist.root.CreateDict("User Interface Style");
            plist.root.SetString("User Interface Style", "Light");

            if (plist.root["NSAppTransportSecurity"] == null) {
                plist.root.CreateDict ("NSAppTransportSecurity");
            }
            plist.root["NSAppTransportSecurity"].AsDict ().SetBoolean ("NSAllowsArbitraryLoads", true);
            plist.root["NSAppTransportSecurity"].AsDict ().SetBoolean ("NSAllowsArbitraryLoadsInWebContent", false);



            // ATT提示
            plist.root.CreateDict("NSUserTrackingUsageDescription");
            plist.root.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");

            // 应用修改
            plist.WriteToFile(plistPath);
            File.WriteAllText(projectPath, pbxProject.WriteToString());


            var projectPath2 = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var project = new PBXProject();
            project.ReadFromString(System.IO.File.ReadAllText(projectPath2));
#if UNITY_2019_3_OR_NEWER
            var manager = new ProjectCapabilityManager(projectPath2, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());            
            manager.AddPushNotifications(true);            
            manager.WriteToFile();
#else
            var manager = new ProjectCapabilityManager(projectPath2, "Entitlements.entitlements", PBXProject.GetUnityTargetName());            
            manager.AddPushNotifications(true);            
            manager.WriteToFile();
#endif

        }

        //添加lib方法
        static void AddLibToProject(PBXProject inst, string targetGuid, string lib)
        {
            string fileGuid = inst.AddFile("usr/lib/" + lib, "Frameworks/" + lib, PBXSourceTree.Sdk);
            inst.AddFileToBuild(targetGuid, fileGuid);
        }
    }
}
#endif