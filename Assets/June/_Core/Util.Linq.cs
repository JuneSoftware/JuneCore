using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace June {
	/// <summary>
	/// Util LINQ methods.
	/// </summary>
	public partial class Util {

		/// <summary>
		/// Select the specified list and selector.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="selector">Selector.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <typeparam name="V">The 2nd type parameter.</typeparam>
		public static List<V> Select<T, V>(IEnumerable<T> list, Func<T, V> selector) {
			List<V> values = new List<V>();
			foreach (T item in list) {
				values.Add(selector(item));
			}
			return values;
		}

		/// <summary>
		/// Filters the list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <param name="comparator">The comparator.</param>
		/// <returns></returns>
		public static List<T> FilterList<T>(IEnumerable<T> list, Predicate<T> predicate) {
			List<T> filteredList = new List<T>();
			if (null != list) {
				foreach (T item in list) {
					if (predicate(item)) {
						filteredList.Add(item);
					}
				}
			}
			return filteredList;
		}

		/// <summary>
		/// Flattens hierarchy
		/// </summary>
		/// <returns>
		/// The many.
		/// </returns>
		/// <param name='source'>
		/// Source.
		/// </param>
		/// <param name='selector'>
		/// Selector.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		/// <typeparam name='TResult'>
		/// The 2nd type parameter.
		/// </typeparam>
		public static List<TResult> SelectMany<T, TResult>(IEnumerable<T> source, Func<T, IEnumerable<TResult>> selector) {
			List<TResult> list = new List<TResult>();
			foreach (T item in source) {
				list.AddRange(selector(item));
			}
			return list;
		}

		/// <summary>
		/// Firsts the or default.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <param name="comparator">The comparator.</param>
		/// <returns></returns>
		public static T FirstOrDefault<T>(IEnumerable<T> list, Predicate<T> predicate) {
			foreach (T item in list) {
				if (predicate(item)) {
					return item;
				}
			}
			return default(T);
		}

		/// <summary>
		/// Find if any element in the list matches the predicate specified
		/// </summary>
		/// <param name="items">Items.</param>
		/// <param name="comparator">Comparator.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool Any<T>(IEnumerable<T> items, Predicate<T> predicate) {
			if (null == items)
				return false;

			foreach (T item in items) {
				if (predicate(item)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Uniques the list.
		/// </summary>
		/// <returns>
		/// The list.
		/// </returns>
		/// <param name='list'>
		/// List.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static List<T> UniqueList<T>(List<T> list) {
			List<T> uniqueList = new List<T>();
			foreach (T item in list) {
				if (!uniqueList.Contains(item))
					uniqueList.Add(item);
			}
			return uniqueList;
		}

		/// <summary>
		/// Contains the specified data and value.
		/// </summary>
		/// <param name='data'>
		/// If set to <c>true</c> data.
		/// </param>
		/// <param name='value'>
		/// If set to <c>true</c> value.
		/// </param>
		public static bool Contains(string[] data, string value) {
			if (null != data) {
				foreach (string s in data) {
					if (s == value)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Indexs the of.
		/// </summary>
		/// <returns>The of.</returns>
		/// <param name="items">Items.</param>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static int IndexOf<T>(List<T> items, Predicate<T> predicate) {
			int index = -1;
			if (null != items && items.Count > 0) {
				for (int i = 0; i < items.Count; i++) {
					if (predicate(items [i])) {
						return i;
					}
				}
			}
			return index;
		}

		/// <summary>
		/// Adds the element to array.
		/// </summary>
		/// <param name='array'>
		/// Array.
		/// </param>
		/// <param name='newItem'>
		/// New item.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static void AddElementToArray<T>(ref T[] array, T newItem) {
			Array.Resize(ref array, array.Length + 1);
			array [array.Length - 1] = newItem;
			//Logger.Log("AddElementToArray" + newItem);
		}

		/// <summary>
		/// Shuffle the specified list.
		/// </summary>
		/// <param name='list'>
		/// List.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static List<T> Shuffle<T>(List<T> list) {
			System.Random rng = new System.Random(DateTime.Now.Millisecond);
			List<T> shuffleList = new List<T>(list);
			int n = list.Count;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1);
				T value = shuffleList [k];
				shuffleList [k] = shuffleList [n];
				shuffleList [n] = value;  
			}
			return shuffleList;
		}

		/// <summary>
		/// Shuffle the specified list.
		/// </summary>
		/// <param name='list'>
		/// List.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static T[] Shuffle<T>(T[] list) {
			return Shuffle(new List<T>(list)).ToArray();
		}

		/// <summary>
		/// Fors the each.
		/// </summary>
		/// <param name='list'>
		/// List.
		/// </param>
		/// <param name='action'>
		/// Action.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static void ForEach<T>(IEnumerable<T> list, Action<T> action) {
			foreach (T item in list) {
				action(item);
			}
		}

		/// <summary>
		/// Count the specified items in the list using the comparator.
		/// </summary>
		/// <param name='list'>
		/// List.
		/// </param>
		/// <param name='action'>
		/// Action.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		public static int Count<T>(IEnumerable<T> list, Predicate<T> predicate) {
			return FilterList<T>(list, predicate).Count;
		}

		/// <summary>
		/// All the specified list and predicate.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool All<T>(IEnumerable<T> list, Predicate<T> predicate) {
			if (null != list) {
				foreach (T item in list) {
					if (false == predicate(item)) {
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Gets the element having the Max value of the selected field.
		/// </summary>
		/// <returns>The by.</returns>
		/// <param name="source">Source.</param>
		/// <param name="selector">Selector.</param>
		/// <typeparam name="TSource">The 1st type parameter.</typeparam>
		/// <typeparam name="TKey">The 2nd type parameter.</typeparam>
		public static TSource MaxBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> selector) {
			return MaxBy(source, selector, Comparer<TKey>.Default);
		}

		/// <summary>
		/// Gets the element having the Max value of the selected field.
		/// </summary>
		/// <returns>The by.</returns>
		/// <param name="source">Source.</param>
		/// <param name="selector">Selector.</param>
		/// <param name="comparer">Comparer.</param>
		/// <typeparam name="TSource">The 1st type parameter.</typeparam>
		/// <typeparam name="TKey">The 2nd type parameter.</typeparam>
		public static TSource MaxBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer) {
			if (source == null)
				throw new ArgumentNullException("source");
			if (selector == null)
				throw new ArgumentNullException("selector");
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			using (var sourceIterator = source.GetEnumerator()) {
				if (!sourceIterator.MoveNext()) {
					throw new InvalidOperationException("Sequence contains no elements");
				}
				var max = sourceIterator.Current;
				var maxKey = selector(max);
				while (sourceIterator.MoveNext()) {
					var candidate = sourceIterator.Current;
					var candidateProjected = selector(candidate);
					if (comparer.Compare(candidateProjected, maxKey) > 0) {
						max = candidate;
						maxKey = candidateProjected;
					}
				}
				return max;
			}
		}


		/// <summary>
		/// Contains the specified list and value.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="value">Value.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool Contains<T>(IEnumerable<T> list, T value) {
			foreach (T item in list) {
				if (item.Equals(value)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the comparison method for ascending order.
		/// </summary>
		/// <returns>The comparison ascending.</returns>
		/// <param name="fieldSelector">Field selector.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <typeparam name="F">The 2nd type parameter.</typeparam>
		public static Comparison<TObj> GetComparisonAscending<TObj, TField>(Func<TObj, TField> fieldSelector) where TField : IComparable {
			return (o1, o2) => {
				return fieldSelector(o1).CompareTo(fieldSelector(o2));
			};
		}

		/// <summary>
		/// Gets the comparison method for descending order.
		/// </summary>
		/// <returns>The comparison descending.</returns>
		/// <param name="fieldSelector">Field selector.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		/// <typeparam name="F">The 2nd type parameter.</typeparam>
		public static Comparison<TObj> GetComparisonDescending<TObj, TField>(Func<TObj, TField> fieldSelector) where TField : IComparable {
			return (o1, o2) => {
				return fieldSelector(o2).CompareTo(fieldSelector(o1));
			};
		}

	}
}