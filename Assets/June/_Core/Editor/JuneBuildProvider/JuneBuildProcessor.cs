using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class JuneBuildProcessor : MonoBehaviour {

	static void Log(string message) {
		Debug.Log(string.Format("[{0:hh:mm:ss}] <CUSTOM_BUILD> {1}", System.DateTime.Now, message));
	}
	
	[MenuItem ("Build/Validate %&v", false, 1)]
	public static bool Validate() {
		Log("********** STARTING VALIDATIONS **********");
		return JuneBuildValidator.Validate();
	}

	[MenuItem ("Build/Validate and Build %&b", false, 2)]
	public static void CustomBuildOnly() {
		if(Validate()) {
			CustomBuild(false);
		}
	}

	[MenuItem ("Build/Validate, Build and Run %&r", false, 3)]
	public static void CustomBuildAndRun() {
		if(Validate()) {
			CustomBuild(true);
		}
	}

	static void CustomBuild(bool runbuild) {
		Log("********** STARTING BUILD **********");
		BuildOptions customBuildOptions = BuildOptions.None;
		if (runbuild) {
			//set EditorUserBuildSettings.architectureFlags enum flags set buildrun bit on
			if ((customBuildOptions & BuildOptions.AutoRunPlayer) != BuildOptions.AutoRunPlayer) {
				customBuildOptions = customBuildOptions | BuildOptions.AutoRunPlayer;
			}
		} 
		else {
			//set EditorUserBuildSettings.architectureFlags enum flags set showfolder bit on
			if ((customBuildOptions & BuildOptions.ShowBuiltPlayer) != BuildOptions.ShowBuiltPlayer) {
				customBuildOptions = customBuildOptions | BuildOptions.ShowBuiltPlayer;
			}
		}
		Log("OPTIONS " + customBuildOptions);
		BuildTarget mycustombuildtarget = EditorUserBuildSettings.activeBuildTarget;
		string extn="";
		switch (EditorUserBuildSettings.selectedBuildTargetGroup) {
			case BuildTargetGroup.Standalone:
				switch (EditorUserBuildSettings.selectedStandaloneTarget) {
					//case BuildTarget.DashboardWidget:
					//	mycustombuildtarget = BuildTarget.DashboardWidget;
					//	extn="wdgt";
					//	break;
					case BuildTarget.StandaloneWindows:
						mycustombuildtarget = BuildTarget.StandaloneWindows;
						extn="exe";
						break;
					//case BuildTarget.StandaloneWindows64:
					//        mycustombuildtarget=BuildTarget.StandaloneWindows64;
					//        extn="exe";
					//        break;
					case BuildTarget.StandaloneOSXUniversal:
						mycustombuildtarget = BuildTarget.StandaloneOSXUniversal;
						extn="app";
						break;
					//case BuildTarget.StandaloneOSXPPC:
					//	mycustombuildtarget = BuildTarget.StandaloneOSXPPC;
					//	extn="app";
					//	break;
					case BuildTarget. StandaloneOSXIntel:
						mycustombuildtarget = BuildTarget.StandaloneOSXIntel;
						extn="app";
						break;
				}        
				break;
			case BuildTargetGroup.WebPlayer:
				if (EditorUserBuildSettings.webPlayerStreamed) {
					mycustombuildtarget = BuildTarget.WebPlayerStreamed;
					extn="unity3d";
				} else {
					mycustombuildtarget = BuildTarget.WebPlayer;
					extn="unity3d";
				}
				break;
			//case BuildTargetGroup.Wii:
			//	mycustombuildtarget = BuildTarget.Wii;
			//	//extn="???"
			//	break;
			case BuildTargetGroup.iOS:
				mycustombuildtarget = BuildTarget.iOS;
				extn="xcode";
				break;
			case BuildTargetGroup.PS3:
				mycustombuildtarget = BuildTarget.PS3;
				//extn="???"
				break;
			case BuildTargetGroup.XBOX360:
				mycustombuildtarget = BuildTarget.XBOX360;
				//extn="???"
				break;
			case BuildTargetGroup.Android:
				mycustombuildtarget = BuildTarget.Android;
				extn="apk";
				break;
			//case BuildTargetGroup.Broadcom:
			//	mycustombuildtarget = BuildTarget.StandaloneBroadcom;
			//	//extn="???"
			//	break;
			case BuildTargetGroup.GLESEmu:
				mycustombuildtarget = BuildTarget.StandaloneGLESEmu;
				//extn="???"
				break;
		}

		Log("TARGET: " + mycustombuildtarget.ToString().ToUpper() + " Extn: " + extn.ToUpper());

		string savepath = EditorUtility.SaveFilePanel("Build "+ mycustombuildtarget,
							EditorUserBuildSettings.GetBuildLocation(mycustombuildtarget), "", extn);

		Log("SAVE PATH: " + savepath);

		if(savepath.Length != 0) {
			string dir = System.IO.Path.GetDirectoryName(savepath); //get build directory
			string[] scenes = new string[EditorBuildSettings.scenes.Length];
			for(int i=0; i<EditorBuildSettings.scenes.Length; ++i) {
				scenes[i] = EditorBuildSettings.scenes[i].path.ToString();
			};
			Log("SCENES: " + scenes.Length + "\n" + string.Join("\n", scenes));
			BuildPipeline.BuildPlayer(scenes, savepath, mycustombuildtarget, customBuildOptions);
			EditorUserBuildSettings.SetBuildLocation(mycustombuildtarget, dir); //store new location for this type of build
		}  
		Log("********** BUILD COMPLETED **********");

		if(JuneBuildWindow.GenerateBuildReport) {
			string dir = System.IO.Path.GetDirectoryName(savepath); //get build directory
			string buildReportPath = System.IO.Path.Combine(dir, "BuildReport.html");
			JuneBuildSummary.GenerateBuildReport(buildReportPath, true);
		}

		if(runbuild) {
			Log("********** DEPLOYING APPLICATION TO DEVICE **********");
		}
	}

	#if !UNITY_PRO_LICENSE
	[MenuItem ("Build/Pre Process, for Free License", false, 1)]
	static void PreProcess() {
		PreProcess(string.Empty);
	}
	#endif
	
	static void PreProcess(string BuildPathDir) {
		//Your pre-process here, copy Assets/Folders to Directory, edit html pages etc
		//or you could execute a shell, cgi-perl or bat command by doing something like below:-
		//Application.OpenURL("yourfile.bat "+BuildPathDir); //etc
		Log("PreProcess");
	}

	#if !UNITY_PRO_LICENSE
	[MenuItem ("Build/Post Process, for Free License", false, 2)]
	static void PostProcess() {
		PostProcess(string.Empty);
	}
	#endif

	static void PostProcess(string BuildPathDir) {
		//Your post-process here, copy Assets/Folders to Directory, edit html pages etc
		//*warning, if set to run after build, these commands may execute after the built application is run.
		//Application.OpenURL("yourfile.bat "+BuildPathDir); //etc
		Log("PostProcess");
	}

}
