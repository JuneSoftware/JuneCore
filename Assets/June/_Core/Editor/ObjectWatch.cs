using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Linq.Expressions;

public class ObjectWatch : EditorWindow {

	private const string NO_TYPES_FOUND = "No types found in assembly";
	private const string NO_STATIC_VARIABLES = "No static fields or properties found";

	private const float CLOSE_BUTTON_WIDTH = 40f;

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
	
	private static bool _FULL_FIELD_NAME = false;
	private static bool _IS_FILTERED = false;

	private static List<GUIClassField> _WatchObjects = new List<GUIClassField>();

	public static ObjectWatch Instance {
		get;
		private set;
	}

	private int _TypeIndex = 0;
	private int _FieldIndex = 0;
	public object Result = null;
	private Vector2 _ScrollPosition;

	[MenuItem("June/Object Watch")]
	public static void OpenObjectWatch() {
		var window = EditorWindow.GetWindow(typeof(ObjectWatch));
		Instance = (ObjectWatch)window;
		window.Show();
	}

	#region Relection Methods

	private static Assembly _Assembly;
	/// <summary>
	/// Gets the assembly.
	/// </summary>
	/// <returns>The assembly.</returns>
	public static Assembly GetAssembly() {
		if(null == _Assembly) {
			_Assembly = typeof(GameConfig).Assembly;
		}
		return _Assembly;
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
	/// Gets the fields.
	/// </summary>
	/// <returns>The fields.</returns>
	/// <param name="type">Type.</param>
	public static FieldInfo[] GetFields(Type type) {
		return null != type 
			? type.GetFields().Where(f => f.IsStatic || f.IsLiteral).ToArray()
            : null;
	}

	/// <summary>
	/// Gets the properties.
	/// </summary>
	/// <returns>The properties.</returns>
	/// <param name="type">Type.</param>
	public static PropertyInfo[] GetProperties(Type type) {
		return null != type 
			? type.GetProperties().Where(p => p.CanRead).ToArray() 
			: null;
	}

	/// <summary>
	/// Gets the fields and properties.
	/// </summary>
	/// <returns>The fields and properties.</returns>
	/// <param name="type">Type.</param>
	public static List<GUIClassField> GetFieldsAndProperties(Type type, object instance = null) {
		List<GUIClassField> gFields = new List<GUIClassField>();

		var fields = GetFields(type);
		if(null != fields && fields.Length > 0) {
			gFields.AddRange(GUIClassField.GetClassFields(type, fields, instance));
		}

		var properties = GetProperties(type);
		if(null != properties && properties.Length > 0) {
			gFields.AddRange(GUIClassField.GetClassFields(type, properties, instance));
		}

		return gFields;
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
		
		using(var gTypeSelection = new JuneHorizontalSection()) {
			EditorGUILayout.LabelField("Type", GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
			_TypeIndex = EditorGUILayout.Popup(_TypeIndex, typeNames, STYLE_TYPE);
			_IS_FILTERED = EditorGUILayout.ToggleLeft("Filter Types", _IS_FILTERED, GUILayout.MaxWidth(140f), GUILayout.ExpandWidth(false));
			
		}
		
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
		// POPULATE SELECTED TYPE FIELDS
		// ------------------------------
		var fields = GetFieldsAndProperties(selectedType);
		string[] fieldNames = null;
		if (null == fields || fields.Count == 0) {
			fieldNames = new string[] { NO_STATIC_VARIABLES };
			_FieldIndex = 0;
		}
		else {
			try {
				fieldNames = fields.Select(f => _FULL_FIELD_NAME ? f.FullName : f.Name).ToArray();
			}
			catch(Exception ex) {
				Debug.Log(ex);
			}
		}
		
		using(var gVarSelection = new JuneHorizontalSection()) {
			EditorGUILayout.LabelField("Variables", GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
			_FieldIndex = EditorGUILayout.Popup(_FieldIndex, fieldNames, GUILayout.ExpandWidth(true));
			_FULL_FIELD_NAME = EditorGUILayout.ToggleLeft("Full Variable Signature", _FULL_FIELD_NAME, GUILayout.MaxWidth(140f), GUILayout.ExpandWidth(false));
		}
		
		if (null == fields || _FieldIndex >= fields.Count) {
			_FieldIndex = 0;
		}
		
		// -------------------
		// SET SELECTED FIELD
		// -------------------
		GUIClassField selectedField = null;
		if (null != fields && _FieldIndex >= 0 && _FieldIndex < fields.Count) {
			selectedField = fields[_FieldIndex];
		}

		using(var gAddObject = new JuneHorizontalSection()) {
			if(GUILayout.Button("Add")) {
				if(null != selectedField && false == _WatchObjects.Contains(selectedField)) {
					_WatchObjects.Add(selectedField);
				}
			}
		}
		
		EditorGUILayout.Separator();

		var normalColor = GUI.color;
		GUI.color = Color.yellow;
		using(var gHeader = new JuneHorizontalSection()) {
			EditorGUILayout.LabelField("Name");
			EditorGUILayout.LabelField("Value");
			EditorGUILayout.LabelField("Type");
			if(GUILayout.Button("Refresh", GUILayout.Width(CLOSE_BUTTON_WIDTH + 15))) {
				_WatchObjects.ForEach(o => o.Refresh());
			}
			if(GUILayout.Button("Clear", GUILayout.Width(CLOSE_BUTTON_WIDTH))) {
				_WatchObjects.Clear();
			}
		}
		GUI.color = normalColor;

		EditorGUILayout.Separator();

		if(null != _WatchObjects && _WatchObjects.Count > 0) {
			using(var gScrollView = new JuneScrollView(ref _ScrollPosition)) {
				try {
					for(int i=0 ;i<_WatchObjects.Count; i++) {
						var item = _WatchObjects[i];
						using(var gObj = new JuneVerticalSection()) {
							item.RenderField();
						}
					}
				}
				catch(Exception ex) {
					Debug.Log(ex);
				}
			}
		}

		// -------------------------
		// DISPLAY METHOD PARAMETERS
		// -------------------------
		/*
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
		*/
	}
	
	#endregion

	#region GUI Field
	
	public class GUIClassField {

		Type _Class;

		FieldInfo _Field;
		PropertyInfo _Property;
		System.Object _Instance = null;
		System.Object _Value = null;
		string _Name = "#ERROR - Untitled";

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				if(null != _Field) {
					return _Field.DeclaringType.Name + "." + _Field.Name;
				}
				else if(null != _Property) {
					return _Property.Name;
				}
				return _Name;
			}
		}

		/// <summary>
		/// Gets the full name.
		/// </summary>
		/// <value>The full name.</value>
		public string FullName {
			get {
				if(null != _Field) {
					return GetFullFieldName();
				}
				else if(null != _Property) {
					return GetFullPropertyName();
				}
				return _Name;
			}
		}

		private Type _FieldType = null;
		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>The type.</value>
		public Type FieldType {
			get {
				if(null == _FieldType) {
					if(null != _Field) {
						_FieldType = _Field.GetType();

						//Ignore MonoField, this usually signifies a constant, use it's FieldType instead.
						if(null != _FieldType && _FieldType.Name == "MonoField") {
							_FieldType = _Field.FieldType;
						}
					}
					else if(null != _Property) {
						_FieldType = _Property.GetGetMethod().ReturnType;
					}
					else if(null != _Value) {
						_FieldType = _Value.GetType();
					}
					else {
						_FieldType = typeof(object);
					}
				}
				return _FieldType;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is object.
		/// </summary>
		/// <value><c>true</c> if this instance is object; otherwise, <c>false</c>.</value>
		public bool IsObject {
			get {
				if(null != FieldType) {
					return FieldType.IsClass && FieldType != typeof(string);
				}
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is static.
		/// </summary>
		/// <value><c>true</c> if this instance is static; otherwise, <c>false</c>.</value>
		public bool IsStatic {
			get {
				if(null != _Field) {
					return _Field.IsStatic;
				}
				if(null != _Property) {
					var getMethod = _Property.GetGetMethod();
					return null != getMethod ? getMethod.IsStatic : true;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is collection.
		/// </summary>
		/// <value><c>true</c> if this instance is collection; otherwise, <c>false</c>.</value>
		public bool IsCollection {
			get {
				return FieldType.IsArray
					|| typeof(ICollection).IsAssignableFrom(FieldType);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is dictionary.
		/// </summary>
		/// <value><c>true</c> if this instance is dictionary; otherwise, <c>false</c>.</value>
		public bool IsDictionary {
			get {
				return typeof(IDictionary).IsAssignableFrom(FieldType);
			}
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>The value.</value>
		public object Value {
			get {
				if(null != _Field) {
					return _Field.GetValue(_Instance);
				}
				else if(null != _Property && _Property.GetIndexParameters().Length <= 0) {
					return _Property.GetGetMethod().Invoke(_Instance, null);
				}
				else if(null != _Property && _Property.GetIndexParameters().Length > 0) {
					return "#IndexParameters: " + _Property.GetIndexParameters().Length;
				}
				return _Value;
			}
		}

		/// <summary>
		/// Gets the value as string.
		/// </summary>
		/// <value>The value as string.</value>
		public string ValueAsStr {
			get {
				var obj = Value;
				return (null == obj) ? "<null>" : obj.ToString();
			}
		}

		private GUIContent _UIContent;
		/// <summary>
		/// Gets the content of the user interface.
		/// </summary>
		/// <value>The content of the user interface.</value>
		public GUIContent UIContent {
			get {
				if(null == _UIContent) {
					_UIContent = new GUIContent(ValueAsStr, ValueAsStr);
				}
				return _UIContent;
			}
		}

		public List<GUIClassField> _Children;
		/// <summary>
		/// Sets the children.
		/// </summary>
		/// <value>The children.</value>
		public List<GUIClassField> Children {
			get {
				if (null == _Children && true == IsObject && false == IsCollection) {
					_Children = ObjectWatch.GetFieldsAndProperties(FieldType, Value);
				}
				if(null == _Children && true == IsObject && true == IsDictionary) {
					object obj = Value;
					if(null != obj) {
						var dictionary = (IDictionary)obj;
						if(null != dictionary) {
							_Children = new List<GUIClassField>();
							foreach(var key in dictionary.Keys) {
								_Children.Add(new GUIClassField(FieldType, Value, dictionary[key], key.ToString()));
							}
						}
					}
				}
				else if (null == _Children && true == IsObject && true == IsCollection) {
					object obj = Value;
					if(null != obj) {
						var enumerable = (IEnumerable)obj;
						_Children = new List<GUIClassField>();
						int index = 0;
						foreach(var item in enumerable) {
							Debug.Log("[CHILD OBJ] " + item.GetType().ToString());
							_Children.Add(new GUIClassField(FieldType, Value, item, index.ToString()));
							index++;
						}
					}
				}
				return _Children;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectWatch+GUIClassField"/> class.
		/// </summary>
		private GUIClassField(Type type, System.Object instance, System.Object value, string name) {
			_Class = type;
			_Instance = instance;
			_Name = name;
			_Value = value;
			_Field = null;
			_Property = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectWatch+GUIClassField"/> class.
		/// </summary>
		/// <param name="field">Field.</param>
		public GUIClassField(Type type, FieldInfo field, object instance) : this(type, instance, null, null) {
			_Field = field;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectWatch+GUIClassField"/> class.
		/// </summary>
		/// <param name="property">Property.</param>
		public GUIClassField(Type type, PropertyInfo property, object instance) : this(type, instance, null, null) {
			_Property = property;
		}

		/// <summary>
		/// Refresh this instance.
		/// </summary>
		public void Refresh() {
			_FieldType = null;
			_UIContent = null;
			_Children = null;
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="obj">Object.</param>
		public void SetValue(object obj) {
			if(null != _Field) {
				_Field.SetValue(_Instance, obj);
			}
			else if(null != _Property) {
				_Property.SetValue(_Instance, obj, null);
			}
		}

		/// <summary>
		/// Gets the full name of the field.
		/// </summary>
		/// <returns>The full field name.</returns>
		public string GetFullFieldName() {
			string fullName = string.Empty;
			if(null != _Field && null != FieldType) {
				fullName = string.Format("{0} {1} {2}", _Field.IsPublic ? "public" : "private", FieldType.Name, Name);
			}
			return fullName;
		}

		/// <summary>
		/// Gets the full name of the property.
		/// </summary>
		/// <returns>The full property name.</returns>
		public string GetFullPropertyName() {
			string fullName = string.Empty;
			if(null != _Property && null != FieldType) {
				fullName = string.Format("{0} {1} {{ }}", FieldType.Name, Name);
			}
			return fullName;
		}

		private bool _Foldout = false;
		/// <summary>
		/// Renders the field.
		/// </summary>
		public void RenderField(bool renderCloseButton = true) {
			float windowWidth = (null != Instance) ? Instance.position.width : 800f;
			windowWidth -= CLOSE_BUTTON_WIDTH;
			float nameWidth = windowWidth * 0.2f;
			float valueWidth = windowWidth * 0.6f;
			float typeWidth = windowWidth * 0.2f;

			try {
				using(var gItemRender = new JuneHorizontalSection()) {
					if(this.IsObject) {
						//var style = new GUIStyle(EditorStyles.foldout);
						//style.fixedWidth = nameWidth;
						_Foldout = EditorGUILayout.Foldout(_Foldout, this.Name);
					}
					else {
						EditorGUILayout.LabelField(this.Name, GUILayout.Width(nameWidth));
					}

					EditorGUILayout.LabelField(UIContent, GUILayout.Width(valueWidth));
					EditorGUILayout.LabelField(this.FieldType.Name, GUILayout.Width(typeWidth));
					if(renderCloseButton) {
						if(GUILayout.Button("X", GUILayout.Width(CLOSE_BUTTON_WIDTH))) {
							_WatchObjects.Remove(this);
						}
					}
				}

				if(this.IsObject && _Foldout) {
					RenderChildren();
				}
			}
			catch(Exception ex) {
				Debug.Log("[ObjectWATCH] ERROR: " + Name + "\n" + ex.ToString());
				//Debug.Log(ex);
			}
		}

		/// <summary>
		/// Renders the children.
		/// </summary>
		private void RenderChildren() {
			using(var gVert = new JuneVerticalSection())
			using(var gIndent = new JuneIndentLevel()) {
				foreach(var child in Children) {
					child.RenderField(renderCloseButton: false);
				}
			}
		}

		#region Static Helper Methods

		/// <summary>
		/// Gets from field info.
		/// </summary>
		/// <returns>The from field info.</returns>
		/// <param name="field">Field.</param>
		public static GUIClassField GetFromFieldInfo(Type type, FieldInfo field, object instance) {
			return new GUIClassField(type, field, instance);
		}

		/// <summary>
		/// Gets the class fields.
		/// </summary>
		/// <returns>The class fields.</returns>
		/// <param name="fields">Fields.</param>
		public static GUIClassField[] GetClassFields(Type type, FieldInfo[] fields, object instance) {
			return Array.ConvertAll<FieldInfo, GUIClassField>(fields, f => GetFromFieldInfo(type, f, instance));
		}

		/// <summary>
		/// Gets from property info.
		/// </summary>
		/// <returns>The from property info.</returns>
		/// <param name="property">Property.</param>
		public static GUIClassField GetFromPropertyInfo(Type type, PropertyInfo property, object instance) {
			return new GUIClassField(type, property, instance);
		}

		/// <summary>
		/// Gets the class fields.
		/// </summary>
		/// <returns>The class fields.</returns>
		/// <param name="properties">Properties.</param>
		public static GUIClassField[] GetClassFields(Type type, PropertyInfo[] properties, object instance) {
			return Array.ConvertAll<PropertyInfo, GUIClassField>(properties, p => GetFromPropertyInfo(type, p, instance));
		}
		#endregion
	}

	#endregion
}


