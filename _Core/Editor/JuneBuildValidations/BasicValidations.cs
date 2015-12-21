using System;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BasicValidations {

	private const string ANDROID_MANIFEST = "Plugins/Android/AndroidManifest.xml";

	#region Application Settings

//	[JuneBuildValidationAttribute]
//	public static bool VerifyFacebook(string assetPath, Assembly assembly) {
//		return JuneAssert.ConstantFieldEquals(typeof(FacebookSDK), "FACEBOOK_APP_ID", GameConfig.FacebookAppId) 
//			&& JuneAssert.FileContains(Path.Combine(assetPath, ANDROID_MANIFEST),
//				string.Format(
//					@"<meta-data android:name=""com.facebook.sdk.ApplicationId"" android:value=""\ {0}""/>", 
//					GameConfig.FacebookAppId));
//	}
	
	[JuneBuildValidationAttribute]
	public static bool VerifyBundleIdentifier(string assetPath, Assembly assembly) {
		return UnityEditor.PlayerSettings.bundleIdentifier == "";
	}

	[JuneBuildValidationAttribute]
	public static bool VerifyBundleVersion(string assetPath, Assembly assembly) {
		return UnityEditor.PlayerSettings.bundleVersion == "";
	}


	#endregion
}
