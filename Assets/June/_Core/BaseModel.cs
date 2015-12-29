
// NOTE: Enable the line below to use IBaseModelExt as base class, you can change the record type by modifying IRecord
#define ENABLE_GENERIC_BASE_MODEL

// NOTE: This defines the type of Record object.
// In this project our DeSerializer is `SimpleJson` and that returns an IDictionary
using IRecord = System.Collections.Generic.IDictionary<string, object>;

// NOTE: This defines the type of array/list/collection object that holds records.
// In this project our DeSerializer is `SimpleJson` and that returns an List<object>
using IRecordArray = System.Collections.Generic.List<object>;

using System;
using System.Collections;
using System.Collections.Generic;

namespace June.Core {

	#region BASE MODEL
	#if ENABLE_GENERIC_BASE_MODEL
	public partial class BaseModel : IBaseModelExt<IRecord, IRecordArray> {
	#else
	public partial class BaseModel {
	#endif

		#if !ENABLE_GENERIC_BASE_MODEL
		protected IRecord _Record;
		#endif

		/// <summary>
		/// Gets or sets the <see cref="BaseModel"/> with the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		object this[string key] {
			get {
				return _Record[key];
			}
			set {
				_Record[key] = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseModel"/> class.
		/// </summary>
		/// <param name="doc">The document.</param>
		public BaseModel(IRecord doc) 
		#if ENABLE_GENERIC_BASE_MODEL
			: base(doc) { }
		#else
			{ UpdateDoc(doc); }
		#endif

		/// <summary>
		/// Updates the document.
		/// </summary>
		/// <param name="updatedDoc">Updated document.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		void UpdateDoc(IRecord updatedDoc) {
			this._Record = updatedDoc;
		}

		/// <summary>
		/// Gets the int.
		/// </summary>
		/// <returns>The int.</returns>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		int GetInt(string key) {
			int result = 0;
			if(_Record.ContainsKey(key)) {
				result = int.Parse(_Record[key].ToString());
			}
			return result;
		}

		/// <summary>
		/// Gets the long.
		/// </summary>
		/// <returns>The long.</returns>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		long GetLong(string key) {
			long result = 0;
			if(_Record.ContainsKey(key)) {
				result = long.Parse(_Record[key].ToString());
			}
			return result;
		}

		/// <summary>
		/// Gets the float.
		/// </summary>
		/// <returns>The float.</returns>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		float GetFloat(string key) {
			float result = 0.0f;
			if(_Record.ContainsKey(key)) {
				result = float.Parse(_Record[key].ToString());
			}
			return result;
		}

		/// <summary>
		/// Gets the double.
		/// </summary>
		/// <returns>The double.</returns>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		double GetDouble(string key) {
			double result = 0.0d;
			if(_Record.ContainsKey(key)) {
				result = double.Parse(_Record[key].ToString());
			}
			return result;
		}

		/// <summary>
		/// Gets the bool.
		/// </summary>
		/// <returns><c>true</c>, if bool was gotten, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		bool GetBool(string key) {
			bool result = false;
			if(_Record.ContainsKey(key)) {
				result = (bool)_Record[key];
			}
			return result;
		}

		/// <summary>
		/// Gets the string.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		string GetString(string key) {
			return Get<string>(key);
		}

		/// <summary>
		/// Gets as string.
		/// </summary>
		/// <returns>The as string.</returns>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		string GetAsString(string key) {
			string value = string.Empty;
			if(_Record.ContainsKey(key)) {
				value = _Record[key].ToString();
			}
			return value;
		}

		/// <summary>
		/// Gets the string list.
		/// </summary>
		/// <returns>The string list.</returns>
		/// <param name="key">Key.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		List<string> GetStringList(string key) {
			List<object> objects = Get<List<object>> (key);
			List<string> strings = new List<string>();

			if(null != objects) {
				foreach(object o in objects) {
					if(null != o && o is String) {
						strings.Add((string)o);
					}
				}
			}

			return strings;
		}

		/// <summary>
		/// Get the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		T Get<T>(string key) {
			T result = default(T);
			if(_Record.ContainsKey(key) && _Record[key] is T) {
				result = (T)_Record[key];
			}
			return result;
		}

		/// <summary>
		/// Gets the enum.
		/// </summary>
		/// <returns>The enum.</returns>
		/// <param name="key">Key.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		T GetEnum<T>(string key) {
			return (T)Enum.Parse(typeof(T), GetString(key));
		}

		/// <summary>
		/// Gets a base model object.
		/// </summary>
		/// <returns>The model.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		T GetModel<T>(string key, Converter<IRecord, T> converter) {
			return converter(Get<IRecord>(key));
		}

		#if !ENABLE_GENERIC_BASE_MODEL
		/// <summary>
		/// Gets the model list from array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="TResult">The result type parameter.</typeparam>
		/// <typeparam name="JSONType">The JSON object type parameter.</typeparam>
		/// <typeparam name="JSONArrayType">The JSON array type parameter.</typeparam>
		public
		List<TResult> GetModelListFromArray<TResult, TJson, TJsonArray>(string key, Func<TJson, TResult> converter) where TJson : class where TJsonArray : IEnumerable {
			List<TResult> values = null;
			var array = Get<TJsonArray>(key);
			if(null != array && null != converter) {
				values = new List<TResult>();
				foreach(var item in array) {
					if(null != item && item is TJson) {
						values.Add(converter(item as TJson));
					}
				}
			}
			return values;
		}

		/// <summary>
		/// Gets the model list from array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="T">The final type parameter.</typeparam>
		/// <typeparam name="JType">The json type parameter.</typeparam>
		public
		List<TResult> GetModelListFromArray<TResult, TJson>(string key, Func<TJson, TResult> converter) where TJson : class {
			return GetModelListFromArray<TResult, TJson, IRecordArray>(key, converter);
		}

		/// <summary>
		/// Gets the model list from json array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> GetModelListFromArray<T>(string key, Func<IRecord, T> converter) {
			return GetModelListFromArray<T, IRecord>(key, converter);
		}
		#endif

		/// <summary>
		/// Set the specified key and value.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		void Set(string key, object value) {
			if(null != _Record) {
				if(_Record.ContainsKey(key)) {
					_Record[key] = value;
				}
				else {
					_Record.Add(key, value);
				}
			}
		}

		/// <summary>
		/// Gets the raw.
		/// </summary>
		/// <returns>The raw.</returns>
		public
		#if ENABLE_GENERIC_BASE_MODEL
		override 
		#endif
		IRecord GetRaw() {
			return _Record;
		}
	}
	#endregion	

	#region BASE LIST
	public partial class BaseList<T> : IBaseList<IRecordArray, IRecord, T> where T : BaseModel {
		
		#region implemented abstract members of IBaseList

		/// <summary>
		/// Insert the specified index and record.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="record">Record.</param>
		protected override void _Insert(int index, IRecord record) {
			_RawRecords.Insert(index, record);
		}

		/// <summary>
		/// Removes at index.
		/// </summary>
		/// <param name="index">Index.</param>
		protected override void _RemoveAt(int index) {
			_RawRecords.RemoveAt(index);
		}

		/// <summary>
		/// Add the specified record.
		/// </summary>
		/// <param name="record">Record.</param>
		protected override void _Add(IRecord record) {
			_RawRecords.Add(record);
		}

		/// <summary>
		/// Remove the specified record.
		/// </summary>
		/// <param name="record">Record.</param>
		protected override void _Remove(IRecord record) {
			_RawRecords.Remove(record);
		}

		/// <summary>
		/// Clear this instance.
		/// </summary>
		protected override void _Clear() {
			_RawRecords.Clear();
		}

		#endregion

		
		#region Ctors
		/// <summary>
		/// Initializes a new instance of the <see cref="June.Core.BaseList`1"/> class.
		/// </summary>
		/// <param name="records">Records.</param>
		public BaseList(IRecordArray records, Converter<IRecord, T> ctor) : base(records, ctor) { }

		#endregion

	}
	#endregion
}