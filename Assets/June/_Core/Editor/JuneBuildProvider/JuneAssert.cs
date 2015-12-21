using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;

[ExecuteInEditMode]
public class JuneAssert {
	
	/// <summary>
	/// Check if file contains the specified content.
	/// This method reads one line at a time.
	/// If check required is multi-line, please use another method.
	/// </summary>
	/// <returns><c>true</c>, if content was present, <c>false</c> otherwise.</returns>
	/// <param name="filePath">File path.</param>
	/// <param name="content">Content.</param>
	public static bool FileContains(string filePath, IEnumerable<string> contents) {
		return FileContains(filePath, contents.ToArray());
	}

	/// <summary>
	/// Check if file contains the specified content.
	/// This method reads one line at a time.
	/// If check required is multi-line, please use another method.
	/// </summary>
	/// <returns><c>true</c>, if content was present, <c>false</c> otherwise.</returns>
	/// <param name="filePath">File path.</param>
	/// <param name="content">Content.</param>
	public static bool FileContains(string filePath, params string[] contents) {
		if(false == string.IsNullOrEmpty(filePath) && File.Exists(filePath) && null != contents && contents.Length > 0) {
			bool[] found = new bool[contents.Length];
			using(StreamReader sr = new StreamReader(filePath)) {
				while(false == sr.EndOfStream) {
					string line = sr.ReadLine();
					for(int i=0; i<contents.Length; i++) {
						found[i] |= line.Contains(contents[i]);
					}
				}
			}
			return found.All(f => f);
		}

		return false;
	}

	/// <summary>
	/// Constants the field equals.
	/// </summary>
	/// <returns><c>true</c>, if field equals was constanted, <c>false</c> otherwise.</returns>
	/// <param name="assembly">Assembly.</param>
	/// <param name="typeName">Type name.</param>
	/// <param name="constName">Const name.</param>
	/// <param name="value">Value.</param>
	public static bool ConstantFieldEquals(Assembly assembly, string typeName, string constName, object value) {
		if(null != assembly) {
			var type = assembly.GetType(typeName);
			return ConstantFieldEquals(type, constName, value);
		}
		return false;
	}

	/// <summary>
	/// Constants the field equals.
	/// </summary>
	/// <returns><c>true</c>, if field equals was constanted, <c>false</c> otherwise.</returns>
	/// <param name="type">Type.</param>
	/// <param name="constName">Const name.</param>
	/// <param name="value">Value.</param>
	public static bool ConstantFieldEquals(Type type, string constName, object value) {
		if(null != type) {
			var constant = type.GetFields()
								.FirstOrDefault(fi => {
									return fi.Name == constName; 
								});
			if(null != constant) {
				bool status = constant.GetValue(null) == value;
				if(false == status) {
					Log ("ASSERT FAILURE {0}.{1} != {2}", type.Name, constName, value);
				}
				return status;
			}
		}
		return false;
	}

	/// <summary>
	/// Asserts if both the objects are equal
	/// </summary>
	/// <param name="obj1">Obj1.</param>
	/// <param name="obj2">Obj2.</param>
	public static bool Equals(object obj1, object obj2) {
		return obj1.Equals(obj2);
	}

	/// <summary>
	/// Log the specified messageFormat and parameters.
	/// </summary>
	/// <param name="messageFormat">Message format.</param>
	/// <param name="parameters">Parameters.</param>
	public static void Log(string messageFormat, params object[] parameters) {
		JuneBuildValidator.Log(messageFormat, parameters);
	}
}
