using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace June.Core {

	public abstract partial class IBaseModel<TRecord> where TRecord : class {

		protected TRecord _Record;

		public abstract object this[string key] {
			get;
			set;
		}

		public IBaseModel(TRecord doc) {
			UpdateDoc(doc);
		}

		/// <summary>
		/// Updates the document.
		/// </summary>
		/// <param name="updatedDoc">Updated document.</param>
		public virtual void UpdateDoc(TRecord updatedDoc) {
			this._Record = updatedDoc;
		}

		/// <summary>
		/// Gets the int.
		/// </summary>
		/// <returns>The int.</returns>
		/// <param name="key">Key.</param>
		public abstract int GetInt(string key);

		/// <summary>
		/// Gets the long.
		/// </summary>
		/// <returns>The long.</returns>
		/// <param name="key">Key.</param>
		public abstract long GetLong(string key);

		/// <summary>
		/// Gets the float.
		/// </summary>
		/// <returns>The float.</returns>
		/// <param name="key">Key.</param>
		public abstract float GetFloat(string key);

		/// <summary>
		/// Gets the double.
		/// </summary>
		/// <returns>The double.</returns>
		/// <param name="key">Key.</param>
		public abstract double GetDouble(string key);

		/// <summary>
		/// Gets the bool.
		/// </summary>
		/// <returns><c>true</c>, if bool was gotten, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public abstract bool GetBool(string key);

		/// <summary>
		/// Gets the string.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="key">Key.</param>
		public abstract string GetString(string key);

		/// <summary>
		/// Gets as string.
		/// </summary>
		/// <returns>The as string.</returns>
		/// <param name="key">Key.</param>
		public abstract string GetAsString(string key);

		/// <summary>
		/// Gets the string list.
		/// </summary>
		/// <returns>The string list.</returns>
		/// <param name="key">Key.</param>
		public abstract List<string> GetStringList(string key);

		/// <summary>
		/// Get the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public abstract T Get<T>(string key);

		/// <summary>
		/// Gets the enum.
		/// </summary>
		/// <returns>The enum.</returns>
		/// <param name="key">Key.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual T GetEnum<T>(string key) {
			return (T)System.Enum.Parse(typeof(T), GetString(key));
		}

		/// <summary>
		/// Gets a base model object.
		/// </summary>
		/// <returns>The model.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual T GetModel<T>(string key, Func<TRecord, T> converter) {
			return converter(Get<TRecord>(key));
		}

		/// <summary>
		/// Gets the model list from array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="T">The final type parameter.</typeparam>
		/// <typeparam name="JType">The json type parameter.</typeparam>
		//public abstract List<T> GetModelListFromArray<T, JType>(string key, Func<JType, T> converter) where JType : class;

		/// <summary>
		/// Set the specified key and value.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public abstract void Set(string key, object value);

		/// <summary>
		/// Gets the raw.
		/// </summary>
		/// <returns>The raw.</returns>
		public virtual TRecord GetRaw() {
			return _Record;
		}
	}

	/// <summary>
	/// IBaseModel extended.
	/// </summary>
	public abstract partial class IBaseModelExt<TRecord, TRecordArray> : IBaseModel<TRecord> where TRecord : class where TRecordArray : IEnumerable {
	
		/// <summary>
		/// Initializes a new instance of the <see cref="June.Core.IBaseModelExt`2"/> class.
		/// </summary>
		/// <param name="doc">Document.</param>
		public IBaseModelExt(TRecord doc) : base(doc) { }

		/// <summary>
		/// Gets the model list from records array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Record to Model Converter.</param>
		/// <typeparam name="TResult">Result type parameter.</typeparam>
		/// <typeparam name="TRecord">Record type parameter.</typeparam>
		/// <typeparam name="TRecordArray">Record Array type parameter.</typeparam>
		protected virtual
		List<TResult> GetModelListFromArray<TResult, TObj, TObjArray>(string key, Func<TObj, TResult> converter) where TObj : class where TObjArray : IEnumerable {
			List<TResult> values = null;
			var array = Get<TObjArray>(key);
			if(null != array && null != converter) {
				values = new List<TResult>();
				foreach(var item in array) {
					if(null != item && item is TObj) {
						values.Add(converter(item as TObj));
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
		/// <typeparam name="TResult">Result type parameter.</typeparam>
		/// <typeparam name="TRecord">Record type parameter.</typeparam>
		protected virtual
		List<TResult> GetModelListFromArray<TResult, TObj>(string key, Func<TObj, TResult> converter) where TObj : class {
			return GetModelListFromArray<TResult, TObj, TRecordArray>(key, converter);
		}

		/// <summary>
		/// Gets the model list from json array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="T">Result type parameter.</typeparam>
		public virtual List<TResult> GetModelListFromArray<TResult>(string key, Func<TRecord, TResult> converter) {
			return GetModelListFromArray<TResult, TRecord>(key, converter);
		}
	}
}