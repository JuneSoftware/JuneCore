using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace June.Core {

	/// <summary>
	/// IBaseModel interface
	/// </summary>
	public partial interface IBaseModel<TRecord, TKey> where TRecord : class {

		void UpdateDoc(TRecord updateDoc);

		int GetInt(TKey key);

		long GetLong(TKey key);

		float GetFloat(TKey key);

		double GetDouble(TKey key);

		bool GetBool(TKey key);

		string GetString(TKey key);

		string GetAsString(TKey key);

		List<string> GetStringList(TKey key);

		T Get<T>(TKey key);

		T GetEnum<T>(TKey key);

		T GetModel<T>(TKey key, Converter<TRecord, T> converter);

		void Set(TKey key, object value);

		TRecord GetRaw();
	}

	/// <summary>
	/// IBaseModel where the key type is string.
	/// Since we mostly use JSON as our data exchange format, they key will always be of type string.
	/// 
	/// </summary>
	public abstract partial class IBaseModel<TRecord> : IBaseModel<TRecord, string>
		where TRecord : class {

		protected TRecord _Record;

		/// <summary>
		/// Gets or sets the <see cref="June.Core.IBaseModel`1"/> with the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		public abstract object this[string key] {
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="June.Core.IBaseModel`1"/> class.
		/// </summary>
		/// <param name="doc">Document.</param>
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
		public virtual T GetModel<T>(string key, Converter<TRecord, T> converter) {
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
	public abstract partial class IBaseModelExt<TRecord, TRecordArray> : IBaseModel<TRecord> 
		where TRecord : class 
		where TRecordArray : class, IEnumerable {
	
		/// <summary>
		/// Initializes a new instance of the <see cref="June.Core.IBaseModelExt`2"/> class.
		/// </summary>
		/// <param name="doc">Document.</param>
		public IBaseModelExt(TRecord doc) : base(doc) { }

		/// <summary>
		/// Gets the model list from array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="TResult">The 1st type parameter.</typeparam>
		/// <typeparam name="TTRecord">The 2nd type parameter.</typeparam>
		/// <typeparam name="TTRecordArray">The 3rd type parameter.</typeparam>
		protected virtual
		List<TResult> GetModelListFromArray<TResult, TTRecord, TTRecordArray>(string key, Converter<TTRecord, TResult> converter)
			where TTRecord : class 
			where TTRecordArray : class, IEnumerable
			where TResult : IBaseModel<TTRecord> {

			return IBaseList<TTRecordArray, TTRecord, TResult>
				.GetModelListFromArray(Get<TTRecordArray>(key), converter);
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
		List<TResult> GetModelListFromArray<TResult, TTRecord>(string key, Converter<TTRecord, TResult> converter) 
			where TTRecord : class 
			where TResult : IBaseModel<TTRecord> {
		
			return GetModelListFromArray<TResult, TTRecord, TRecordArray>(key, converter);
		}

		/// <summary>
		/// Gets the model list from array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="key">Key.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="TResult">The 1st type parameter.</typeparam>
		protected virtual
		List<TResult> GetModelListFromArray<TResult>(string key, Converter<TRecord, TResult> converter) 
			where TResult : IBaseModel<TRecord> {
		
			return GetModelListFromArray<TResult, TRecord>(key, converter);	
		}
	}

	/// <summary>
	/// Generic base list.
	/// </summary>
	public abstract partial class IBaseList<TRecordArray, TRecord, TModel> : IList<TModel>
		where TModel : IBaseModel<TRecord>
		where TRecord : class
		where TRecordArray : class, IEnumerable {
	
		/// <summary>
		/// The raw records.
		/// </summary>
		protected TRecordArray _RawRecords;

		/// <summary>
		/// The model records.
		/// </summary>
		protected List<TModel> _Records;

		/// <summary>
		/// The conveter `record to model`.
		/// </summary>
		Converter<TRecord, TModel> _Conveter_RecordToModel;

		/// <summary>
		/// The conveter `model to record`.
		/// </summary>
		Converter<TModel, TRecord> _Conveter_ModelToRecord;

		#region IList implementation
		/// <summary>
		/// Indexs the of.
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="item">Item.</param>
		public int IndexOf (TModel item) {
			return _Records.IndexOf(item);
		}

		/// <summary>
		/// Insert the specified index and item.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="item">Item.</param>
		public void Insert (int index, TModel item) {
			_Records.Insert(index, item);
			if(null != _Conveter_ModelToRecord) {
				_Insert(index, _Conveter_ModelToRecord(item));
			}
		}

		/// <summary>
		/// Removes at index.
		/// </summary>
		/// <param name="index">Index.</param>
		public void RemoveAt (int index) {
			_Records.RemoveAt(index);
			if(null != _Conveter_ModelToRecord) {
				_RemoveAt(index);
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="June.Core.BaseList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		public TModel this [int index] {
			get {
				return (TModel)_Records[index];
			}
			set {
				_Records[index] = value;
			}
		}
		#endregion

		#region ICollection implementation
		/// <summary>
		/// Add the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public void Add (TModel item) {
			_Records.Add(item);
			if(null != _Conveter_ModelToRecord) {
				_Add(_Conveter_ModelToRecord(item));
			}
		}

		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			_Records.Clear();
			_Clear();
		}

		/// <summary>
		/// Contains the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public bool Contains (TModel item) {
			return _Records.Contains(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">Array.</param>
		/// <param name="arrayIndex">Array index.</param>
		public void CopyTo (TModel[] array, int arrayIndex) {
			if(null == array)
				throw new ArgumentNullException();

			if(0 > arrayIndex)
				throw new ArgumentOutOfRangeException();

			if(arrayIndex + _Records.Count >= array.Length)
				throw new ArgumentException("Index is greater than or equal to array length");
			
			for(int i=arrayIndex; i<_Records.Count + arrayIndex; i++) {
				array[i] = _Records[i-arrayIndex];
			}
		}

		/// <summary>
		/// Remove the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public bool Remove (TModel item) {
			if(null != _Conveter_ModelToRecord) {
				_Remove(_Conveter_ModelToRecord(item));
			}
			return _Records.Remove(item);
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		public int Count {
			get {
				return _Records.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<TModel> GetEnumerator () {
			foreach(var item in _Records) {
				yield return (TModel)item;
			}
		}

		/// <summary>
		/// Gets the raw enumerator.
		/// </summary>
		/// <returns>The raw enumerator.</returns>
		public IEnumerator<TRecord> GetRawEnumerator() {
			foreach(var item in _RawRecords) {
				if(item is TRecord) {
					yield return item as TRecord;
				}
			}
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator () {
			return this.GetEnumerator();
		}
		#endregion

		#region Abstract Memebers

		/// <summary>
		/// Insert raw record at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="record">Record.</param>
		protected abstract void _Insert(int index, TRecord record);

		/// <summary>
		/// Removes raw record at index.
		/// </summary>
		/// <param name="index">Index.</param>
		protected abstract void _RemoveAt(int index);

		/// <summary>
		/// Add the specified raw record.
		/// </summary>
		/// <param name="record">Record.</param>
		protected abstract void _Add(TRecord record);

		/// <summary>
		/// Remove the specified raw record.
		/// </summary>
		/// <param name="record">Record.</param>
		protected abstract void _Remove(TRecord record);

		/// <summary>
		/// Clear this instances raw records.
		/// </summary>
		protected abstract void _Clear();

		/// <summary>
		/// Gets the raw record collection.
		/// </summary>
		/// <returns>The raw.</returns>
		protected virtual TRecordArray GetRaw() {
			return _RawRecords;
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="June.Core.IBaseList`3"/> class.
		/// </summary>
		/// <param name="array">Array.</param>
		/// <param name="recordToModel">Record to model.</param>
		/// <param name="modelToRecord">Model to record.</param>
		public IBaseList(TRecordArray array, Converter<TRecord, TModel> recordToModel, Converter<TModel, TRecord> modelToRecord) {
			this._RawRecords = array;
			this._Conveter_RecordToModel = recordToModel;
			this._Conveter_ModelToRecord = modelToRecord;
			ReLoadModels();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="June.Core.IBaseList`3"/> class.
		/// </summary>
		/// <param name="array">Array.</param>
		/// <param name="recordToModel">Record to model.</param>
		public IBaseList(TRecordArray array, Converter<TRecord, TModel> recordToModel) 
			: this(array, recordToModel, (TModel model) => model.GetRaw()) { }

		/// <summary>
		/// Loads the models.
		/// </summary>
		protected void ReLoadModels() {
			_Records = GetModelListFromArray<TModel, TRecord, TRecordArray>(this._RawRecords, this._Conveter_RecordToModel);
		}

		#region List Helper Methods
		/// <summary>
		/// Converts all.
		/// </summary>
		/// <returns>The all.</returns>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> ConvertAll<T>(Converter<TModel, T> converter) {
			return _Records.ConvertAll(converter);
		}
		#endregion

		/// <summary>
		/// Gets the model list from array.
		/// </summary>
		/// <returns>The model list from array.</returns>
		/// <param name="array">Array.</param>
		/// <param name="converter">Converter.</param>
		/// <typeparam name="TModel">The 1st type parameter.</typeparam>
		/// <typeparam name="TObj">The 2nd type parameter.</typeparam>
		/// <typeparam name="TObjArray">The 3rd type parameter.</typeparam>
		public static List<TResult> GetModelListFromArray<TResult, TObj, TObjArray>(TObjArray array, Converter<TObj, TResult> converter) 
			where TResult : IBaseModel<TObj>
			where TObj : class
			where TObjArray : class, IEnumerable {

			List<TResult> values = null;
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
	}
}