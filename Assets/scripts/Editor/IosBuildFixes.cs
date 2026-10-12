#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

// Runs automatically every time you build for iOS.
public static class IosBuildFixes
{
    // Adds the text for Apple's "Allow this app to track you?" popup.
    // Without it, the app crashes when it asks for tracking permission.
    [PostProcessBuild(1)]
    public static void AddTrackingDescription(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;
        string plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("NSUserTrackingUsageDescription",
            "Your data will be used to show you more relevant ads.");
        plist.WriteToFile(plistPath);
    }

    // Adds the Podfile fix (iOS 15.0 for all pods), so you don't have to paste it by hand.
    // Runs after the Podfile is written and before "pod install".
    [PostProcessBuild(45)]
    public static void FixPodDeploymentTarget(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;
        string podfile = Path.Combine(path, "Podfile");
        if (!File.Exists(podfile)) return;
        string text = File.ReadAllText(podfile);
        if (text.Contains("post_install")) return;
        text += "\npost_install do |installer|\n" +
                "  installer.pods_project.targets.each do |t|\n" +
                "    t.build_configurations.each do |config|\n" +
                "      config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '15.0'\n" +
                "    end\n" +
                "  end\n" +
                "end\n";
        File.WriteAllText(podfile, text);
    }
}
#endif
