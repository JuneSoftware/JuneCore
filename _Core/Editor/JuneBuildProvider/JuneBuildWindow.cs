using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class JuneBuildWindow : EditorWindow {
	
	private const string RELEASE_SYMBOL = "RELEASE";
	
	private const string BUILD_SYMBOL_SEPARATOR = ";";

	private Vector2 _ScrollPosition;
	private bool _BuildOptionFoldout;
	private bool _ValidationFoldout;
	public static bool GenerateBuildReport;

#if UNITY_ANDROID
	private const BuildTargetGroup BUILD_TARGET = BuildTargetGroup.Android;
#elif UNITY_IPHONE
	private const BuildTargetGroup BUILD_TARGET = BuildTargetGroup.iPhone;
#else
	private const BuildTargetGroup BUILD_TARGET = BuildTargetGroup.Standalone;
#endif

	private Dictionary<string, bool> SYMBOLS = new Dictionary<string, bool>() {
		{ RELEASE_SYMBOL, 
			#if RELEASE
			true
			#else
			false
			#endif
		},
		{ "INAPP_ENABLED",
			#if INAPP_ENABLED
			true
			#else
			false
			#endif
		},
		{ "NGUI", 
			#if NGUI 
			true 
			#else
			false
			#endif
		},
		{ "TK2D", 
			#if TK2D
			true 
			#else
			false
			#endif
		},
		{ "SPINE_TK2D", 
			#if SPINE_TK2D
			true 
			#else
			false
			#endif
		},
		{ "HTTP_CONSOLE",
			#if HTTP_CONSOLE
			true
			#else
			false
			#endif
		}
	};
	
	/// <summary>
	/// Gets the BUILD SYMBOLS
	/// </summary>
	/// <value>The BUILD SYMBOLS.</value>
	private string BUILD_SYMBOLS {
		get {
			string symbols = string.Empty;

			foreach(var kv in SYMBOLS) {
				if(!string.IsNullOrEmpty(symbols)) {
					symbols += BUILD_SYMBOL_SEPARATOR;
				}
				if(kv.Value) {
					symbols += kv.Key;
				}
			}

			return symbols;
		}
	}

	[MenuItem("June/Build Settings %&s")]
	static void CustomBuildWindow() {
		GetWindow<JuneBuildWindow>("JuneBuildWindow");
	}

	private GUIStyle richTextStyle = new GUIStyle() { richText = true };

	//private const string _KeyValueFormat = "<size=12><color=WHITE>{0}</color>: <color=YELLOW>{1}</color></size>";
	private static string _KeyValueFormat = new RichText()
												.SizeColor(12, RichText.COLOR_WHITE, "{0}: ")
												.SizeColor(12, RichText.COLOR_YELLOW, "{1}")
												.ToString();

	private void PrintKeyValue(string key, string value) {
		GUILayout.Label(string.Format(_KeyValueFormat, key, value), richTextStyle);
	}

	private void OnGUI() {
		using(var gGroup = new JuneDisabledGroup(EditorApplication.isCompiling)) {
			using(var gSettings = new JuneHorizontalSection()) {
				// Player Settings
				using(var gSettingsLeft = new JuneVerticalSection(GUILayout.ExpandWidth(true))) {
					PrintKeyValue(" Current Build Platform", BUILD_TARGET.ToString());
					PrintKeyValue(" Bundle Identifier", PlayerSettings.bundleIdentifier);
					PrintKeyValue(" Bundle Version", PlayerSettings.bundleVersion);
					#if UNITY_ANDROID
					PrintKeyValue(" Bundle Version Code", PlayerSettings.Android.bundleVersionCode.ToString());
					#endif
				}

				// Build Options
				using(var gBuidOptionsVertical = new JuneVerticalSection(GUILayout.ExpandWidth(true))) {
					string compileStatus = EditorApplication.isCompiling ? "Compiling, Please Wait ..." : "Ready";

					GUILayout.Label(
						RichText.Create()
								.SizeColor(12, RichText.COLOR_WHITE, "Status: ")
								.SizeColor(12, EditorApplication.isCompiling ? "RED" : "LIME", compileStatus)
								.EndAllGetString(), richTextStyle);

					#if UNITY_ANDROID
					string keyAliasPass = "<null>";
					if(!string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass)) {
						keyAliasPass = PlayerSettings.Android.keyaliasPass[0].ToString();
						for(int i=1; i<PlayerSettings.Android.keyaliasPass.Length-1; i++) {
							keyAliasPass += "*";
						}
						keyAliasPass += PlayerSettings.Android.keyaliasPass[PlayerSettings.Android.keyaliasPass.Length-1].ToString();
					}
					string keyAliasStr = string.Format("{0} / {1}", PlayerSettings.Android.keyaliasName, keyAliasPass);
					PrintKeyValue("Keystore Alias", keyAliasStr);
					#endif

					_BuildOptionFoldout = EditorGUILayout.Foldout(_BuildOptionFoldout, "Build Options");
					if(_BuildOptionFoldout) {
						using(var gBuildOptionsSymbols = new JuneHorizontalSection()) {
							var keys = SYMBOLS.Keys.ToList();
							foreach(var k in keys) {
								bool original = SYMBOLS[k];
								if(original) {
									GUI.contentColor = Color.green;
								}
								SYMBOLS[k] = GUILayout.Toggle(original, k);
								GUI.contentColor = Color.white;
								if(original != SYMBOLS[k]) {
									PlayerSettings.SetScriptingDefineSymbolsForGroup(BUILD_TARGET, BUILD_SYMBOLS);
								}
							}
						}
					}
				}
			}

			GUILayout.Space(10f);

			using(var gButtons = new JuneHorizontalSection()) {
				if(GUILayout.Button("Validate")) {
					JuneBuildProcessor.Validate();
				}

				if(GUILayout.Button("Validate Build")) {
					JuneBuildProcessor.CustomBuildOnly();
				}

				if(GUILayout.Button("Validate Build Run")) {
					JuneBuildProcessor.CustomBuildAndRun();
				}
			}

			using(var gReportButton = new JuneHorizontalSection()) {
				GenerateBuildReport = GUILayout.Toggle(GenerateBuildReport, "Generate Build Report");

				if(GUILayout.Button("Player Settings")) {
					EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
				}				
			}
		}

		GUILayout.Space(10);

		string validationLabel = string.Format("VALIDATIONS [{0}/{1}]", JuneBuildValidator.ValidationsPassed, JuneBuildValidator.ValidationsEnabled);
		_ValidationFoldout = EditorGUILayout.Foldout(_ValidationFoldout, validationLabel); 
		if(_ValidationFoldout) {
			using(var scroll = new JuneScrollView(ref this._ScrollPosition)) {
				if (null == JuneBuildValidator.ValidationItems || 0 == JuneBuildValidator.ValidationItems.Count) {
					GUILayout.Label ("No validation methods for current project", EditorStyles.miniLabel);
				}
				else {
					foreach (var item in JuneBuildValidator.ValidationItems) {
						using(var gValidationItems = new JuneHorizontalSection(GUILayout.MinHeight (18))) {

							string name = item.Name;
							bool status = item.Status;

							if(false == item.CanValidate) {
								GUI.color = Color.white;
								status = false;
								name = string.Format("{0} [Enabled:{1} Targets:{2}]", name, item.Enabled, item.Targets);
							}
							else if (false == item.Status) {
								GUI.color = Color.yellow;
								status = false;
							} 
							else if(true == item.Status) {
								GUI.color = Color.green;
								status = true;
							}

							bool oldStatus = status;
							status = GUILayout.Toggle(status, name);
							if(status != oldStatus) {
								item.OpenEditor();
							}
						}
						GUI.color = Color.white;
					}
				}
			}
		}
	}
	
}
