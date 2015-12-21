using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Linq.Expressions;

public class ObjectTestBench : EditorWindow {

    private const string NO_TYPES_FOUND = "No types found in assembly";
    private const string NO_STATIC_METHODS = "No static methods found";

    private static GUIStyle _STYLE_TYPE = null;
    private static GUIStyle STYLE_TYPE {
        get {
            if (null == _STYLE_TYPE) {
                _STYLE_TYPE = new GUIStyle(EditorStyles.popup) {
                    stretchWidth = true,
                    //border = new RectOffset(6, 6, 6, 6),
                    //margin = new RectOffset(4, 4, -1, -1),
                    //padding = new RectOffset(10, 6, 6, 6)
                };
            }
            return _STYLE_TYPE;
        }
    }

    private static readonly Type[] TYPES_TO_CONSIDER = { 
		
	};

    private static bool _IS_FILTERED = false;
    private static bool _FULL_METHOD_NAME = false;

    public static ObjectTestBench Instance {
        get;
        private set;
    }

    private int _TypeIndex = 0;
    private int _MethodIndex = 0;
    public object Result = null;
    private object[] paramValues = null;
    object temp;

    /// <summary>
    /// Open object browser window.
    /// </summary>
    [MenuItem("June/Object Test Bench")]
    public static void OpenObjectTestBench() {
        var window = EditorWindow.GetWindow(typeof(ObjectTestBench));
        Instance = (ObjectTestBench)window;
        window.Show();
    }

    /// <summary>
    /// Gets the assembly.
    /// </summary>
    /// <returns>The assembly.</returns>
    public static Assembly GetAssembly() {
        return typeof(June.Util).Assembly;
    }

    /// <summary>
    /// Should the type be considered.
    /// </summary>
    /// <returns><c>true</c>, if consider type was shoulded, <c>false</c> otherwise.</returns>
    /// <param name="type">Type.</param>
    public static bool ShouldConsiderType(Type type) {
        if (true == _IS_FILTERED && null != TYPES_TO_CONSIDER) {
            return TYPES_TO_CONSIDER.Contains(type);
        }
        return null != type ? type.IsClass : false;
    }

    /// <summary>
    /// Gets the types.
    /// </summary>
    /// <returns>The types.</returns>
    /// <param name="assembly">Assembly.</param>
    public static Type[] GetTypes(Assembly assembly) {
        return null != assembly
                ? assembly.GetTypes().Where(ShouldConsiderType).ToArray()
                : null;
    }

    /// <summary>
    /// Gets the methods.
    /// </summary>
    /// <returns>The methods.</returns>
    /// <param name="type">Type.</param>
    public static MethodInfo[] GetMethods(Type type) {
        return null != type
                ? type.GetMethods().Where(m => m.IsPublic && m.IsStatic).ToArray()
                : null;
    }

    public static string GetParameterTypeName(ParameterInfo parameter) {
        if (parameter.ParameterType.IsGenericType) {
            return string.Format("{0}<{1}>",
                parameter.ParameterType.Name,
                    string.Join(", ",
                        parameter.ParameterType.GetGenericArguments().Select(t => GetTypeName(t)).ToArray()));
        }
        return parameter.ParameterType.Name;
    }

    public static string GetTypeName(Type type) {
        if (type.IsGenericType) {
            return type.ToString();
        }

        return type.Name;
    }

    /// <summary>
    /// Gets the method string.
    /// </summary>
    /// <returns>The method string.</returns>
    /// <param name="method">Method.</param>
    public static string GetMethodStr(MethodInfo method) {
        if (null == method) {
            return "... select method ...";
        }

        string parametersStr = string.Empty;
        var parameters = GetParameters(method);
        if (null != parameters && parameters.Length > 0) {
            parametersStr = string.Join(", ",
                parameters.Select(
                    p => string.Format("{0} {1}", GetParameterTypeName(p), p.Name)).ToArray());
        }
        return string.Format("{0} {1} ({2})",
                             method.ReturnType.Name,
                             method.Name,
                             parametersStr);
    }

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    /// <returns>The parameters.</returns>
    /// <param name="method">Method.</param>
    public static ParameterInfo[] GetParameters(MethodInfo method) {
        return null != method
            ? method.GetParameters()
                : null;
    }

    void Awake() {
        Instance = this;
    }

    /// <summary>
    /// Raises the GU event.
    /// </summary>
    public void OnGUI() {

        var assembly = GetAssembly();

        // --------------
        // POPULATE TYPES
        // --------------
        var types = GetTypes(assembly);
        string[] typeNames = null;
        if (null == types || types.Length == 0) {
            typeNames = new string[] { NO_TYPES_FOUND };
            _TypeIndex = 0;
        }
        else {
            typeNames = types.Select(t => t.FullName).ToArray();
        }

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Type", GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
            _TypeIndex = EditorGUILayout.Popup(_TypeIndex, typeNames, STYLE_TYPE);
            _IS_FILTERED = EditorGUILayout.ToggleLeft("Filter Types", _IS_FILTERED, GUILayout.MaxWidth(140f), GUILayout.ExpandWidth(false));

        } EditorGUILayout.EndHorizontal();

        // -----------------
        // SET SELECTED TYPE
        // -----------------
        Type selectedType = null;
        if (null != types && _TypeIndex >= 0 && _TypeIndex < types.Length) {
            selectedType = types[_TypeIndex];
            //Debug.Log("Selected - " + selectedType.Name);
            //Debug.Log("Selected Methods - " + selectedType.GetMethods().Where(m => m.IsPublic && m.IsStatic).Count());
        }

        // ------------------------------
        // POPULATE SELECTED TYPE METHODS
        // ------------------------------
        var methods = GetMethods(selectedType);
        string[] methodNames = null;
        if (null == methods || methods.Length == 0) {
            methodNames = new string[] { NO_STATIC_METHODS };
            _MethodIndex = 0;
        }
        else {
            methodNames = methods.Select(m => (_FULL_METHOD_NAME ? GetMethodStr(m) : m.Name)).ToArray();
        }

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Method", GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
            _MethodIndex = EditorGUILayout.Popup(_MethodIndex, methodNames, GUILayout.ExpandWidth(true));
            _FULL_METHOD_NAME = EditorGUILayout.ToggleLeft("Full Method Signature", _FULL_METHOD_NAME, GUILayout.MaxWidth(140f), GUILayout.ExpandWidth(false));
        } EditorGUILayout.EndHorizontal();

        if (null == methods || _MethodIndex >= methods.Length) {
            _MethodIndex = 0;
        }

        // -------------------
        // SET SELECTED METHOD
        // -------------------
        MethodInfo selectedMethod = null;
        if (null != methods && _MethodIndex >= 0 && _MethodIndex < methods.Length) {
            selectedMethod = methods[_MethodIndex];
        }

        EditorGUILayout.Separator();

        // -------------------------
        // DISPLAY METHOD PARAMETERS
        // -------------------------
        var parameters = GetParameters(selectedMethod);
        EditorGUILayout.BeginVertical();
        {

            EditorGUILayout.HelpBox(GetMethodStr(selectedMethod), MessageType.None);

            EditorGUILayout.Separator();

            if (null != parameters && parameters.Length > 0) {

                if (null == paramValues || paramValues.Length != parameters.Length) {
                    paramValues = new object[parameters.Length];
                }

                for (int i = 0; i < parameters.Length; i++) {
                    EditorGUILayout.BeginHorizontal();
                    {
                        PopulateField(ref paramValues[i], parameters[i]);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else {
                EditorGUILayout.LabelField("No parameters.");
            }

            if (GUILayout.Button("Invoke")) {
                try {
                    //if(null == Dispatcher.Instance) {
                    Dispatcher.Initialize();
                    //}

                    Result = selectedMethod.Invoke(null, paramValues);
                    //Result = "Invoked";
                    //EditorCoroutine.start(Util.ExecuteGetCommand("http://yahoo.com", www => {
                    //	Debug.Log("[CALLBACK] " + www.ToString());
                    //	EditorUtility.DisplayDialog("Callback", www.text, "ok");
                    //}));
                }
                catch (Exception ex) {
                    Result = ex.ToString();
                }
            }
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Result");
            EditorGUILayout.TextArea((null == Result) ? "<NULL>" : Result.ToString(), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
        EditorGUILayout.EndVertical();
    }



    public static void PopulateField(ref object destination, ParameterInfo parameter) {
        string pType = parameter.ParameterType.Name;
        string pName = string.Format("{0} ({1})", parameter.Name, pType);

        if (pType.ToLower().Contains("action`")) {
            var dynamicMethod = GetDynamicMethodForParameters(parameter.ParameterType.GetGenericArguments());
            destination = dynamicMethod.CreateDelegate(parameter.ParameterType);
            GUI.color = Color.magenta;
            EditorGUILayout.LabelField(string.Format("{0} : {1}", parameter.Name, dynamicMethod.ToString()));
            GUI.color = Color.white;
            return;
        }

        switch (pType.ToLower()) {
            case "int32":
                destination = EditorGUILayout.IntField(pName, (destination is int) ? (int)destination : 0);
                break;
            case "single":
                destination = EditorGUILayout.FloatField(pName, (destination is float) ? (float)destination : 0f);
                break;
            case "double":
                destination = double.Parse(EditorGUILayout.TextField(pName, (destination is double) ? ((double)destination).ToString() : "0.0"));
                break;
            case "boolean":
                if (null == destination) {
                    destination = false;
                }
                destination = EditorGUILayout.Toggle(pName, (bool)destination); //bool.Parse(EditorGUILayout.TextField(pName, (destination is bool) ? ((bool)destination).ToString() : "false"));
                break;
            case "string":
                destination = EditorGUILayout.TextField(pName, (null != destination) ? destination.ToString() : "");
                break;
            default:
                destination = parameter.DefaultValue;
                break;
        }

    }

    /// <summary>
    /// Gets the dynamic method for parameters specified.
    /// </summary>
    /// <returns>The dynamic method for parameters.</returns>
    /// <param name="parameters">Parameters.</param>
    public static DynamicMethod GetDynamicMethodForParameters(Type[] parameters) {

        MethodInfo printMethod = typeof(Debug).GetMethod("Log", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null);

        //Create a dynamic method
        DynamicMethod method = new DynamicMethod("auto_callback", null, parameters);
        //Intermediate language runtime generator
        ILGenerator il = method.GetILGenerator();
        if (null != parameters && parameters.Length > 0) {
            for (int j = 0; j < parameters.Length; j++) {

                //Load parameter using index (i.e. 'j')
                il.Emit(OpCodes.Ldarg, j);

                //If parameter is value type, box it (i.e. convert it to 'object')
                if (parameters[j].IsValueType) {
                    il.Emit(OpCodes.Box, parameters[j]);
                }

                //Call print method to print object
                il.Emit(OpCodes.Call, printMethod);
            }
        }
        else {
            il.Emit(OpCodes.Ldstr, "Action was invoked.");
            il.Emit(OpCodes.Call, printMethod);
        }

        //Return from method
        il.Emit(OpCodes.Ret);

        return method;
    }

}
