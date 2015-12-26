
//#define USE_GENERIC_LIST_FOR_ITEMS

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace June.Core {
	
	/// <summary>
	/// Base config.
	/// </summary>
	public abstract class BaseConfig<T, U> : BaseModel where U : BaseModel where T : BaseConfig<T, U>, new() {

		private static object _padLock_ = new object();

		#region Singleton
		private static T _Instance;
		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static T Instance {
			get {
				if(null == _Instance) {
					lock(_padLock_) {
						if(null == _Instance) {
							_Instance = new T();
							_Instance.Load();
						}
					}
				}
				return _Instance;
			}
		}
		#endregion

		#if USE_GENERIC_LIST_FOR_ITEMS
		protected List<U> _Items = null
		#else
		protected BaseList<U> _Items = null;
		#endif
		/// <summary>
		/// Gets or sets the items.
		/// </summary>
		/// <value>The items.</value>
		public virtual BaseList<U> Items {
			get {
				if(null == _Items) {
					lock(_padLock_) {
						if(null == _Items) {
							LoadItems();
						}
					}
				}
				return _Items;
			}
		}

		/// <summary>
		/// Gets the name of the resource.
		/// </summary>
		/// <value>The name of the resource.</value>
		public abstract string ResourceName { 
			get;
		}

		/// <summary>
		/// Gets the root key.
		/// </summary>
		/// <value>The root key.</value>
		public abstract string RootKey {
			get;
		}

		/// <summary>
		/// Gets the deserialize func.
		/// </summary>
		/// <value>The deserialize func.</value>
		public virtual Converter<string, IDictionary<string, object>> DeserializeFunc {
			get {
				return Util.DeSerializeJsonDocFromCacheOrResource;
			}
		}

		/// <summary>
		/// Gets the item converter.
		/// </summary>
		/// <returns>The item converter.</returns>
		/// <typeparam name="U">The 1st type parameter.</typeparam>
		public abstract Converter<IDictionary<string, object>, U> ItemConverter {
			get;
		}

		/// <summary>
		/// Clear this instance.
		/// </summary>
		public virtual void Clear() {
			_Record = null;
			_Items = null;
			_Instance = null;
		}

		/// <summary>
		/// Refresh this instance.
		/// </summary>
		public virtual void Refresh() {
			Clear();
			Load ();
		}

		/// <summary>
		/// PreLoad handler.
		/// </summary>
		protected virtual void PreLoad() { }

		/// <summary>
		/// Load this instance.
		/// </summary>
		protected virtual void Load() {
			PreLoad();
			UpdateDoc(DeserializeFunc(ResourceName));
			LoadItems();
			PostLoad();
		}

		/// <summary>
		/// PostLoad handler.
		/// </summary>
		protected virtual void PostLoad() { }

		/// <summary>
		/// Loads the items.
		/// </summary>
		protected virtual void LoadItems() {
			var rawItems = null != RootKey && null != _Record ? Get<SimpleJson.JsonArray>(RootKey) : null;
			if(null != rawItems) {
				_Items = new BaseList<U>(rawItems, ItemConverter);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseConfig`2"/> class.
		/// </summary>
		public BaseConfig() : this(null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseConfig`2"/> class.
		/// </summary>
		/// <param name="doc">Document.</param>
		public BaseConfig(IDictionary<string, object> doc) : base(doc) { }

		/// <summary>
		/// Add the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void Add(IDictionary<string, object> item) {
			if(null == _Items) {
				LoadItems();
			}

			if(null != _Items) {
				var rawItems = null != RootKey ? Get<SimpleJson.JsonArray>(RootKey) : null;
				if(null != rawItems) {
					rawItems.Add(item);
					_Items.Add(ItemConverter(item));
				}
			}
		}

		/// <summary>
		/// Remove the specified item.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void Remove(U item) {
			if(null == _Items) {
				LoadItems();
			}

			if(null != _Items) {
				var rawItems = null != RootKey ? Get<SimpleJson.JsonArray>(RootKey) : null;
				if(null != rawItems && null != item) {
					int index = _Items.IndexOf(item);
					if(-1 != index) {
						rawItems.RemoveAt(index);
						_Items.RemoveAt(index);
					}
				}
			}
		}

		//#if UNITY_EDITOR	
		/// <summary>
		/// Save this instance.
		/// </summary>
		//public abstract void Save();
		//#endif
	}
}