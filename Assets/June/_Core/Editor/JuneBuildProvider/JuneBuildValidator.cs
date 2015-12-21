using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[ExecuteInEditMode]
public class JuneBuildValidator {

	/// <summary>
	/// Halt on Failure
	/// </summary>
	private const bool HALT_ON_FAILURE = true;

	/// <summary>
	/// Validator methods should conform to this signature.
	/// </summary>
	public delegate bool Validator(string assetFolder, Assembly assembly);

	/// <summary>
	/// Gets the asset path.
	/// </summary>
	/// <value>The asset path.</value>
	internal static string AssetPath {
		get {
			return Application.dataPath;
		}
	}

	/// <summary>
	/// Gets the validation assembly.
	/// </summary>
	/// <value>The validation assembly.</value>
	internal static Assembly ValidationAssembly {
		get {
			return Assembly.GetExecutingAssembly();
		}
	}

	private static object[] _ValidationDelegateParameters = new object[] { AssetPath, ValidationAssembly };
	/// <summary>
	/// Gets the validation delegate parameters.
	/// </summary>
	/// <value>The validation delegate parameters.</value>
	private static object[] ValidationDelegateParameters {
		get {
			return _ValidationDelegateParameters;
		}
	}

	/// <summary>
	/// Gets the build targets.
	/// </summary>
	/// <value>The build targets.</value>
	public static JuneBuildTargets BuildTargets {
		get {
			switch(UnityEditor.EditorUserBuildSettings.activeBuildTarget) {
				case UnityEditor.BuildTarget.Android:
					return JuneBuildTargets.Android;					
				case UnityEditor.BuildTarget.iOS:
					return JuneBuildTargets.iPhone;
				default:
					return JuneBuildTargets.All;
			}
		}
	}

	/// <summary>
	/// Gets the unity build target group.
	/// </summary>
	/// <value>The unity build target group.</value>
	public static BuildTargetGroup UnityBuildTargetGroup {
		get {
			BuildTargetGroup unityBuildTargetGroup = BuildTargetGroup.Unknown;
			switch(UnityEditor.EditorUserBuildSettings.activeBuildTarget) {
				case UnityEditor.BuildTarget.Android:
					unityBuildTargetGroup = BuildTargetGroup.Android;
					break;
				case UnityEditor.BuildTarget.iOS:
					unityBuildTargetGroup = BuildTargetGroup.iOS;
					break;
			}
			return unityBuildTargetGroup;
		}
	}

	private static List<JuneBuildValidationItem> _ValidationItems = null;
	/// <summary>
	/// Gets the validation items.
	/// </summary>
	/// <value>The validation items.</value>
	public static List<JuneBuildValidationItem> ValidationItems {
		get {
			if(null == _ValidationItems) {
				_ValidationItems = GetAllJuneValidationItems(ValidationAssembly, BuildTargets);
				_ValidationItems.Sort(
					(JuneBuildValidationItem a, JuneBuildValidationItem b) => {
						return b.CanValidate.CompareTo(a.CanValidate);
					});
			}
			return _ValidationItems;
		}
	}

	/// <summary>
	/// Gets the active validation items.
	/// </summary>
	/// <value>The active validation items.</value>
	public static List<JuneBuildValidationItem> ActiveValidationItems {
		get {
			return ValidationItems.Where(v => v.CanValidate).ToList();
		}
	}

	/// <summary>
	/// Gets the validations enabled.
	/// </summary>
	/// <value>The validations enabled.</value>
	public static int ValidationsEnabled {
		get {
			return null != ValidationItems ? ValidationItems.Count(i => i.CanValidate) : 0;
		}
	}

	/// <summary>
	/// Gets the validations passed.
	/// </summary>
	/// <value>The validations passed.</value>
	public static int ValidationsPassed {
		get {
			return null != ValidationItems ? ValidationItems.Count(i => i.Status) : 0;
		}
	}

	/// <summary>
	/// Validate this instance.
	/// </summary>
	public static bool Validate() {
		bool validationStatus = true;

		Log (string.Format("Searching Validators for Target: {0}, Symbols: {1}", 
			    BuildTargets.ToString().ToUpper(),
		        PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityBuildTargetGroup)));

		// Loop through all validate methods and print results=
		if(null != ValidationItems) {
			Log (string.Format("Found {0} Validations", ValidationItems.Count));

			foreach(var item in ValidationItems) {
				if(item.CanValidate) {
					validationStatus &= item.Validate(ValidationDelegateParameters);
					Log (string.Format("Executing `{0}` ... {1}", item.Name, validationStatus.ToString().ToUpper()));
					if(HALT_ON_FAILURE && false == validationStatus) {
						Log ("STOPPING VALIDATIONS, PLEASE CHECK VALUES !!!");
						return validationStatus;
					}
				}
			}
		}

		if(true == validationStatus) {
			Log ("All Validations PASSED");
		}

		return validationStatus;
	}

	/// <summary>
	/// Gets all june validation items.
	/// </summary>
	/// <returns>The all june validation items.</returns>
	/// <param name="assembly">Assembly.</param>
	/// <param name="buildTarget">Build target.</param>
	private static List<JuneBuildValidationItem> GetAllJuneValidationItems(Assembly assembly, JuneBuildTargets buildTarget) {
		return GetAllEnabledJuneBuildValidationMethods(assembly, buildTarget)
				.FindAll(VerifyMethodSignature)
				.ConvertAll(m => new JuneBuildValidationItem(m))
				.ToList();
	}

	/// <summary>
	/// Gets all enabled june build validation methods.
	/// </summary>
	/// <returns>The all enabled june build validation methods.</returns>
	/// <param name="assembly">Assembly.</param>
	private static List<MethodInfo> GetAllEnabledJuneBuildValidationMethods(Assembly assembly, JuneBuildTargets buildTarget) {
		if(null == assembly)
			return null;

		return assembly.GetTypes()
			.SelectMany(t => t.GetMethods())
			.Where(m => { 
				var attr = m.GetCustomAttributes(typeof(JuneBuildValidationAttribute), false);
				if(null != attr && 1 == attr.Length && attr[0] is JuneBuildValidationAttribute) {
					// Filter methods that are enabled
					//var buildAttr = (JuneBuildValidationAttribute)attr[0];
					//return buildAttr.Enable && ((buildAttr.Targets & buildTarget) > 0);
						return true;
				}
				return false;
			})
			.ToList();
	}

	private const Type _ParameterType = null;
	private const Type _ReturnType = null;
	/// <summary>
	/// Verifies the method signature.
	/// </summary>
	/// <returns><c>true</c>, if method signature was verifyed, <c>false</c> otherwise.</returns>
	/// <param name="method">Method.</param>
	private static bool VerifyMethodSignature(MethodInfo method) {
		var validatorMethod = typeof(Validator).GetMethod("Invoke");
		return CompareMethodSignature(validatorMethod, method);
	}

	/// <summary>
	/// Compares the method signature.
	/// </summary>
	/// <returns><c>true</c>, if method signature was compared, <c>false</c> otherwise.</returns>
	/// <param name="method1">Method1.</param>
	/// <param name="method2">Method2.</param>
	private static bool CompareMethodSignature(MethodInfo method1, MethodInfo method2) {
		if(null == method1 || null == method2)
			return false;

		string[] params1 = method1.GetParameters().Select(p => p.ParameterType.Name).ToArray();
		string[] params2 = method2.GetParameters().Select(p => p.ParameterType.Name).ToArray();

		if(null != params1 && null != params2 && params1.Length == params2.Length) {
			for(int i=0; i<params1.Length; i++) {
				if(params1[i] != params2[i]) {
					return false;
				}
			}
		}

		return method1.ReturnType.Name == method2.ReturnType.Name;
	}

	/// <summary>
	/// Log the specified messageFormat and parameters.
	/// </summary>
	/// <param name="messageFormat">Message format.</param>
	/// <param name="parameters">Parameters.</param>
	public static void Log(string messageFormat, params object[] parameters) {
		Log (string.Format(messageFormat, parameters));
	}

	/// <summary>
	/// Log the specified message.
	/// </summary>
	/// <param name="message">Message.</param>
	public static void Log(string message) {
		string log = string.Format("[JuneBuildValidator] {0}", message);
		Debug.Log(log);
	}

	/// <summary>
	/// Logs the error.
	/// </summary>
	/// <param name="messageFormat">Message format.</param>
	/// <param name="parameters">Parameters.</param>
	public static void LogError(string messageFormat, params object[] parameters) {
		LogError (string.Format(messageFormat, parameters));
	}

	/// <summary>
	/// Logs the error.
	/// </summary>
	/// <param name="message">Message.</param>
	public static void LogError(string message) {
		string log = string.Format("[JuneBuildValidator] ERROR: {0}", message);
		Debug.LogError(log);
	}
}

/// <summary>
/// June build validation attribute.
/// </summary>
[ExecuteInEditMode]
public class JuneBuildValidationAttribute : Attribute {

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="JuneBuildValidationAttribute"/> is enabled.
	/// </summary>
	/// <value><c>true</c> if enable; otherwise, <c>false</c>.</value>
	public bool Enable { get; set; }

	/// <summary>
	/// Gets or sets the targets. Default is ALL
	/// </summary>
	/// <value>The targets.</value>
	public JuneBuildTargets Targets { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="JuneBuildValidationAttribute"/> class.
	/// </summary>
	public JuneBuildValidationAttribute() : this(true, JuneBuildTargets.All) { }

	public JuneBuildValidationAttribute(bool enabled, JuneBuildTargets targets) {
		this.Enable = enabled;
		this.Targets = targets;
	}
}

/// <summary>
/// June build targets.
/// </summary>
[Flags]
public enum JuneBuildTargets {
	iPhone = (1 << 0),
	Android = (1 << 1),
	Amazon = (1 << 2),
	AndroidAmazon = (Android | Amazon),
	All = iPhone | Android | Amazon
}

public class JuneBuildValidationItem {
	/// <summary>
	/// Gets the name.
	/// </summary>
	/// <value>The name.</value>
	public string Name {
		get {
			return null != Method
				? string.Format("{0}.{1}", this.Method.DeclaringType.Name, this.Method.Name)
				: "UNKNOWN";
		}
	}

	/// <summary>
	/// The method.
	/// </summary>
	public MethodInfo Method { 
		get; 
		private set; 
	}

	private JuneBuildValidationAttribute _MethodAttribute = null;
	/// <summary>
	/// Gets the method attribute.
	/// </summary>
	/// <value>The method attribute.</value>
	private JuneBuildValidationAttribute MethodAttribute {
		get {
			if(null == _MethodAttribute && null != Method) {
				var attr = Method.GetCustomAttributes(typeof(JuneBuildValidationAttribute), false);
				if(null != attr && 1 == attr.Length && attr[0] is JuneBuildValidationAttribute) {
					_MethodAttribute = (JuneBuildValidationAttribute)attr[0];
				}
			}
			return _MethodAttribute;
		}
	}

	/// <summary>
	/// The status.
	/// </summary>
	public bool Status { get; private set; }

	/// <summary>
	/// Gets a value indicating whether this instance is validating.
	/// </summary>
	/// <value><c>true</c> if this instance is validating; otherwise, <c>false</c>.</value>
	public bool IsValidating { get; private set; }

	/// <summary>
	/// Gets a value indicating whether this <see cref="JuneBuildValidationItem"/> is enabled.
	/// </summary>
	/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
	public bool Enabled { 
		get {
			if(null != MethodAttribute) {
				return MethodAttribute.Enable;
			}
			return true;
		}
	}

	/// <summary>
	/// Gets the targets.
	/// </summary>
	/// <value>The targets.</value>
	public JuneBuildTargets Targets {
		get {
			if(null != MethodAttribute) {
				return MethodAttribute.Targets;
			}
			return JuneBuildTargets.All;
		}
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="JuneBuildValidationItem"/> valid build target.
	/// </summary>
	/// <value><c>true</c> if valid build target; otherwise, <c>false</c>.</value>
	public bool ValidBuildTarget {
		get {
			return (Targets & JuneBuildValidator.BuildTargets) > 0;
		}
	}

	/// <summary>
	/// Gets a value indicating whether this instance can validate.
	/// </summary>
	/// <value><c>true</c> if this instance can validate; otherwise, <c>false</c>.</value>
	public bool CanValidate {
		get {
			return Enabled && ValidBuildTarget;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JuneBuildValidationItem"/> class.
	/// </summary>
	public JuneBuildValidationItem() {
		this.Status = false;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JuneBuildValidationItem"/> class.
	/// </summary>
	/// <param name="method">Method.</param>
	public JuneBuildValidationItem(MethodInfo method) : this() {
		this.Method = method;
	}

	/// <summary>
	/// Validate the specified parameters.
	/// </summary>
	/// <param name="parameters">Parameters.</param>
	public bool Validate(object[] parameters) {
		this.IsValidating = true;
		if(null != this.Method && this.CanValidate) {
			this.Status = (bool)this.Method.Invoke(null, parameters);
		}
		this.IsValidating = false;
		return this.Status;
	}

	/// <summary>
	/// Opens the code.
	/// </summary>
	public void OpenEditor() {
		UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(JuneBuildValidator.AssetPath + "Editor/JuneBuildValidations/BasicValidations.cs", 28);
	}

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="JuneBuildValidationItem"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="JuneBuildValidationItem"/>.</returns>
	public override string ToString () {
		return string.Format ("{0} - {1}", Name, Status);
	}
}