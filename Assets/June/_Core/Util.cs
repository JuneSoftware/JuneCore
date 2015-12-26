#define INTERNET_CHECK_USING_PING

using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using Logger = June.DummyLogger;

namespace June {

	/// <summary>
	/// Util.
	/// </summary>
	public static partial class Util {

		/// <summary>
		/// JSON encoder/decoder
		/// </summary>
		public readonly static Func<string, string> JSON_ENCODER_DECODER = str => str;

		/// <summary>
		/// Job object which is checking for internet connection
		/// </summary>
		private static Job _IsCheckingForInternet;

		/// <summary>
		/// Gets the current UTC timestamp.
		/// </summary>
		/// <value>
		/// The current UTC timestamp.
		/// </value>
		public static int CurrentUTCTimestamp {
			get {
				return (int)ToUnixTimestamp(DateTime.UtcNow);
			}
		}

		private static bool _IsInternetReachable = true;
		/// <summary>
		/// Gets a value indicating is internet reachable.
		/// </summary>
		/// <value><c>true</c> if is internet reachable; otherwise, <c>false</c>.</value>
		public static bool IsInternetReachable {
			get {
				return _IsInternetReachable;
			}
			set {
				_IsInternetReachable = value;
			}
		}

		/// <summary>
		/// Gets the time zone.
		/// </summary>
		/// <returns>The time zone.</returns>
		public static double GetTimeZone() {
			var date = DateTime.Now;
			return Math.Round((date - date.ToUniversalTime()).TotalHours, 1, MidpointRounding.AwayFromZero);
		}

		/// <summary>
		/// Serializes the json document to file.
		/// </summary>
		/// <returns><c>true</c>, if json document to file was serialized, <c>false</c> otherwise.</returns>
		/// <param name="filePath">File path.</param>
		/// <param name="jsonDoc">Json document.</param>
		public static bool SerializeJsonDocToFile(string filePath, IDictionary<string, object> jsonDoc) {
			return SerializeJsonDocToFile(filePath, jsonDoc, JSON_ENCODER_DECODER);
		}

		/// <summary>
		/// Serializes the json document to file.
		/// </summary>
		/// <returns><c>true</c>, if json document to file was serialized, <c>false</c> otherwise.</returns>
		/// <param name="filePath">File path.</param>
		/// <param name="encodeMethod">Encode method.</param>
		public static bool SerializeJsonDocToFile(string filePath, IDictionary<string, object> jsonDoc, Func<string, string> encodeMethod) {
			bool result = false;
			try {
				if (null != jsonDoc) {
					string resultStr = SimpleJson.SimpleJson.SerializeObject(jsonDoc);
					if (!string.IsNullOrEmpty(resultStr)) {
						if (null != encodeMethod) {
							resultStr = encodeMethod(resultStr);
						}

						if (!string.IsNullOrEmpty(resultStr)) {
							File.WriteAllText(filePath, resultStr);
							result = true;
						}
					}
				}
			}
			catch (Exception ex) {
				Logger.Log("[Util] Error in persisting file: " + ex.Message);
				result = false;
			}
			return result;
		}

		/// <summary>
		/// DeSerialize json document from file.
		/// </summary>
		/// <returns>The serialize json document from file.</returns>
		/// <param name="filePath">File path.</param>
		public static IDictionary<string, object> DeSerializeJsonDocFromFile(string filePath) {
			return DeSerializeJsonDocFromFile(filePath, JSON_ENCODER_DECODER);
		}

		/// <summary>
		/// DeSerialize json document from file.
		/// </summary>
		/// <returns>The serialize json document from file.</returns>
		/// <param name="filePath">File path.</param>
		public static IDictionary<string, object> DeSerializeJsonDocFromFile(string filePath, Func<string, string> decodeMethod) {
			IDictionary<string, object> result = null;
			if (File.Exists(filePath)) {
				string fileData = File.ReadAllText(filePath);
				if (null != decodeMethod && !string.IsNullOrEmpty(fileData)) {
					fileData = decodeMethod(fileData);
				}
				if (!string.IsNullOrEmpty(fileData)) {
					result = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject(fileData);
				}
			}
			return result;
		}

		/// <summary>
		/// Reads the text from resource.
		/// </summary>
		/// <returns>
		/// The text from resource.
		/// </returns>
		/// <param name='resourceName'>
		/// Resource name.
		/// </param>
		public static string ReadTextFromResource(string resourceName) {
			TextAsset data = (TextAsset)Resources.Load(resourceName, typeof(TextAsset));
			return null != data ? data.text : null;
		}

		/// <summary>
		/// Deserialize json document from resource.
		/// </summary>
		/// <returns>The serialize json document from resource.</returns>
		/// <param name="resourceName">Resource name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static IDictionary<string, object> DeSerializeJsonDocFromResource(string resourceName) {
			IDictionary<string, object> result = null;
			string text = ReadTextFromResource(resourceName);
			if (null != text) {
				object obj;
				if (SimpleJson.SimpleJson.TryDeserializeObject(text, out obj)) {
					result = (IDictionary<string, object>)obj;
				}
			}
			return result;
		}

		/// <summary>
		/// Deserialize json document from cache or resource.
		/// </summary>
		/// <returns>The deserialized json document from cache or resource.</returns>
		/// <param name="resourceName">Resource name.</param>
		public static IDictionary<string, object> DeSerializeJsonDocFromCacheOrResource(string resourceName) {
			IDictionary<string, object> result = null;
			string filePath = Path.Combine(Application.persistentDataPath, resourceName);
			if (File.Exists(filePath)) {
				Logger.Log("[DE-SERIALIZE] OPENING FILE :" + filePath);
				result = DeSerializeJsonDocFromFile(filePath, null);
			}
			if (null == result) {
				Logger.Log("[DE-SERIALIZE] OPENING RESOURCE :" + resourceName);
				result = DeSerializeJsonDocFromResource(resourceName);
			}
			return result;
		}

		/// <summary>
		/// Convert to unix timestamp.
		/// </summary>
		/// <returns>
		/// The unix timestamp.
		/// </returns>
		/// <param name='dt'>
		/// Date time object
		/// </param>
		public static long ToUnixTimestamp(System.DateTime dt) {
			DateTime unixRef = new DateTime(1970, 1, 1, 0, 0, 0);
			return (dt.Ticks - unixRef.Ticks) / 10000000;
		}

		/// <summary>
		/// Convert from the unix timestamp.
		/// </summary>
		/// <returns>
		/// The date time object.
		/// </returns>
		/// <param name='timestamp'>
		/// Timestamp.
		/// </param>
		public static DateTime FromUnixTimestamp(long timestamp) {
			DateTime unixRef = new DateTime(1970, 1, 1, 0, 0, 0);
			return unixRef.AddSeconds(timestamp);
		}

		/// <summary>
		/// DeSerializes the JSON string.
		/// </summary>
		/// <returns>
		/// Dictionary object.
		/// </returns>
		/// <param name='jsonStr'>
		/// Json string.
		/// </param>
		public static IDictionary<string, object> DeSerializeJSON(string jsonStr) {
			return (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject(jsonStr);
		}

		/// <summary>
		/// SHA1 encode.
		/// </summary>
		/// <param name="plainText">The plain text.</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string SHA1Encode(string plainText) {
			var inputBytes = ASCIIEncoding.ASCII.GetBytes(plainText);
			using(var sha1 = new SHA1Managed()) {
				var hashBytes = sha1.ComputeHash(inputBytes);
				StringBuilder hashValue = new StringBuilder();
				Array.ForEach<byte>(hashBytes, b => hashValue.Append(b.ToString("x2")));
				return hashValue.ToString();
			}
		}

		/// <summary>
		/// Executes the post command.
		/// </summary>
		/// <returns>
		/// The post command.
		/// </returns>
		/// <param name='url'>
		/// URL.
		/// </param>
		/// <param name='data'>
		/// Data.
		/// </param>
		/// <param name='responseCode'>
		/// Response code.
		/// </param>
		public static IEnumerator ExecutePostCommand(string url, string data, Action<WWW> callback) {
			var form = new WWWForm();
			form.headers ["Content-Type"] = "application/json";
			Logger.Log(string.Format("[HTTP POST] Request {0} {1}", url, data));
		
			using (WWW www = new WWW(url, Encoding.UTF8.GetBytes(data), form.headers)) {
				yield return www;
				Logger.Log (string.Format("[HTTP POST] Response {0}", (!string.IsNullOrEmpty (www.error)) ? (" Error - " + www.error) : www.text));
				if (www.isDone && null != callback) {
					callback(www);
				}		
			}
		}

		/// <summary>
		/// Executes the post command with basic auth.
		/// </summary>
		/// <returns>The post command with basic auth.</returns>
		/// <param name="url">URL.</param>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="data">Data.</param>
		/// <param name="callback">Callback.</param>
		public static IEnumerator ExecutePostCommandWithBasicAuth(string url, string username, string password, string data, Action<WWW> callback) {
			var form = new WWWForm();
			var headers = form.headers;
		
			headers ["Content-Type"] = "application/json";
			headers ["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));
		
			Logger.Log(string.Format("[HTTP POST] Request {0} {1}:{2} {3}", url, username, password, data));

			using (WWW www = new WWW(url, Encoding.UTF8.GetBytes(data), headers)) {
				yield return www;
				Logger.Log (string.Format("[HTTP POST] Response {0}", (!string.IsNullOrEmpty (www.error)) ? (" Error - " + www.error) : www.text));
				if (www.isDone && null != callback) {
					callback(www);
				}		
			}
		}

		/// <summary>
		/// Executes the get command.
		/// </summary>
		/// <returns>
		/// The get command.
		/// </returns>
		/// <param name='url'>
		/// URL.
		/// </param>
		/// <param name='callback'>
		/// Callback.
		/// </param>
		public static IEnumerator ExecuteGetCommand(string url, Action<WWW> callback) {
			Logger.Log(string.Format("[HTTP GET] Request {0}", url));
			
			if (!Util.IsInternetReachable) {
				if (null != callback) {
					callback(null);
				}
				yield break;
			}

			using (WWW www = new WWW(url)) {
	
				yield return www;
				//Logger.Log (string.Format("[HTTP GET] Response {0} {1}", requestNo, (!string.IsNullOrEmpty (www.error)) ? (" Error - " + www.error) : www.text));
				if (www.isDone && null != callback) {
					callback(www);
				}
			}
		}

		#if !UNITY_WEBGL
		/// <summary>
		/// Executes the ping command.
		/// </summary>
		/// <returns>The ping command.</returns>
		/// <param name="address">Address.</param>
		/// <param name="timeOut">Time out.</param>
		/// <param name="callback">Callback.</param>
		public static IEnumerator ExecutePingCommand (string address, float timeOut, Action<float> callback) {
			Ping ping = new Ping (address);
			float timeDiff = 0.1f;
			float time = 0.0f;
			while (!ping.isDone) {
				yield return new WaitForSeconds (timeDiff);
				time += timeDiff;
				if (time >= timeOut) {
						break;
				}
			}
			Logger.Log("PING:" + time.ToString("0.0") + " TIMEOUT:" + timeOut + " IsDone:" + ping.isDone + " Time:" + ping.time);
			if (null != callback) {
					callback (time);
			}
		}
		#endif

		/// <summary>
		/// Converts to string.
		/// </summary>
		/// <returns>The to string.</returns>
		/// <param name="d">D.</param>
		public static string ConvertToString(Dictionary<string, string> d) {
			// Build up each line one-by-one and then trim the end
			StringBuilder builder = new StringBuilder();
			foreach (KeyValuePair<string, string> pair in d) {
				builder.Append(pair.Key).Append(":").Append(pair.Value).Append(',');
			}
			// Remove the final delimiter and convert to string
			return builder.Remove(builder.Length-1, 1).ToString();
		}

		/// <summary>
		/// Tos the title case.
		/// </summary>
		/// <returns>The title case.</returns>
		/// <param name="word">Word.</param>
		public static string ToTitleCase(string word) {
			if (word == null)
				return null;
		
			if (word.Length > 1)
				return char.ToUpper(word [0]) + word.Substring(1);
		
			return word.ToUpper();
		}

		#if !UNITY_WEBGL
		/// <summary>
		/// Starts the internet reachability check.
		/// </summary>
		public static void StartInternetReachabilityCheck () {
			if (null == _IsCheckingForInternet || _IsCheckingForInternet.IsPaused) {
				if(null != _IsCheckingForInternet)
					_IsCheckingForInternet.Kill();
				_IsCheckingForInternet = Job.Create (TestInternetConnection ());
			}
		}

		/// <summary>
		/// Tests the internet connection.
		/// </summary>
		/// <returns>The internet connection.</returns>
		public static IEnumerator TestInternetConnection () {
			long startTime = 0;
			long timeTaken = 0;
			float maxTime = 5.0F;
			float secondsToWait = 10F;
				
			#if INTERNET_CHECK_USING_PING
				while (true) {
					yield return Job.CreateAsCoroutine (
						ExecutePingCommand ("74.125.224.72", maxTime,
					    	time => {
								Logger.Log ("RECEIVED PING - " + time);
								if (time < maxTime) {
										IsInternetReachable = true;
										secondsToWait = 20F;
								} else {
										IsInternetReachable = false;
										secondsToWait = 5F;
								}
						}));
					yield return new WaitForSeconds (secondsToWait);
				}
			#else
				while (true) {
					startTime = DateTime.Now.Ticks;
					//Util.Logger.Log("START TICK - " + startTime);
					yield return Coroutiner.StartCoroutine(
						Util.ExecuteGetCommand("http://www.google.com",
							www => {
								timeTaken = DateTime.Now.Ticks - startTime;
								//Util.Logger.Log("END TICK - " + DateTime.Now.Ticks);
								//Util.Logger.Log("TIME TAKEN - " + timeTaken);
								if(!string.IsNullOrEmpty(www.error)) {
									//Util.Logger.Log("ERROR - " + www.error);
									IsInternetReachable = false;
									secondsToWait = 5;
								}
								else {
									IsInternetReachable = true;
									secondsToWait = 20;
								}
							}));
					yield return new WaitForSeconds(secondsToWait);
				}
			#endif
		}
		#endif
			
		/// <summary>
		/// Converts to generic dictionary.
		/// </summary>
		/// <returns>The to generic dictionary.</returns>
		/// <param name="obj">Object.</param>
		public static IDictionary<string, object> ConvertToGenericDictionary(IDictionary obj) {
			IDictionary<string, object> result = new Dictionary<string, object>(); 
			foreach (var key in obj.Keys) {
				string keyStr = key.ToString();
				if (!obj.Contains(key)) {
					result.Add(keyStr, obj [key]);
				}
			}
			return result;
		}

		/// <summary>
		/// Medians the specified list.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static int Median(int[] list) {
			int result = 0;
			Array.Sort(list);
			var listSize = list.Length;

			int midIndex = listSize / 2;
			if (0 == listSize % 2) {    // Even number of elements            
				result = ((list [midIndex - 1] + list [midIndex]) / 2);
			}
			else {                      // Odd number of elements
				result = list [midIndex];
			}

			return result;
		}

		/// <summary>
		/// Determines if is number the specified obj.
		/// </summary>
		/// <returns><c>true</c> if is number the specified obj; otherwise, <c>false</c>.</returns>
		/// <param name="obj">Object.</param>
		public static bool IsNumber(object obj) {
			if (null == obj)
				return false;

			if (obj is sbyte) return true;
			if (obj is byte) return true;
			if (obj is short) return true;
			if (obj is ushort) return true;
			if (obj is int) return true;
			if (obj is uint) return true;
			if (obj is long) return true;
			if (obj is ulong) return true;
			if (obj is float) return true;
			if (obj is double) return true;
			if (obj is decimal) return true;

			return false;
		}

	}
}