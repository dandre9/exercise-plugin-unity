using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

public class MyPluginPostProcessBuild
{
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Get root
            PlistElementDict rootDict = plist.root;

            // background location useage key (new in iOS 8)
            rootDict.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", "Uses background location");

            // background modes
            // var buildKey = "UIBackgroundModes";
            // rootDict.CreateArray(buildKey).AddString("location");
            // rootDict.CreateArray(buildKey).AddString("fetch");

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            var projPath = buildPath + "/Unity-Iphone.xcodeproj/project.pbxproj";
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var targetGuid = proj.TargetGuidByName(PBXProject.GetUnityTestTargetName());


            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");



            proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Plugins/iOS/UnityIosPlugin/Source/UnityPlugin-Bridging-Header.h");

            proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "UnityIosPlugin-Swift.h");

            // string guid = proj.GetUnityMainTargetGuid();
            // proj.AddCapability(guid, PBXCapabilityType.BackgroundModes);

            proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks $(PROJECT_DIR)/lib/$(CONFIGURATION) $(inherited)");
            proj.AddBuildProperty(targetGuid, "FRAMERWORK_SEARCH_PATHS",
                "$(inherited) $(PROJECT_DIR) $(PROJECT_DIR)/Frameworks");
            proj.AddBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            proj.AddBuildProperty(targetGuid, "DYLIB_INSTALL_NAME_BASE", "@rpath");
            proj.AddBuildProperty(targetGuid, "LD_DYLIB_INSTALL_NAME",
                "@executable_path/../Frameworks/$(EXECUTABLE_PATH)");
            proj.AddBuildProperty(targetGuid, "DEFINES_MODULE", "YES");
            proj.AddBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");
            proj.AddBuildProperty(targetGuid, "COREML_CODEGEN_LANGUAGE", "Swift");

            proj.WriteToFile(projPath);

            // var projCapability = new ProjectCapabilityManager(projPath, "Unity-iPhone/mmk.entitlements", null, proj.GetUnityMainTargetGuid());
            // projCapability.AddBackgroundModes(BackgroundModesOptions.LocationUpdates);
        }
    }
}