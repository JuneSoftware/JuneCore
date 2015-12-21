using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

/// <summary>
/// June editor utils.
/// </summary>
using UnityEditor;
using System.IO;


public class JuneEditorUtils {

	/// <summary>
	/// Finds the references.
	/// </summary>
	/// <returns>The references.</returns>
	/// <param name="text">Text.</param>
	public static List<KeyValuePair<FileInfo, int>> FindReferences(string text) {
		List<KeyValuePair<FileInfo, int>> references = new List<KeyValuePair<FileInfo, int>>();
		string output = FindInFiles(text);
		if(!string.IsNullOrEmpty(output)) {
			string[] lines = output.Split('\n');
			foreach(var line in lines) {
				if(!string.IsNullOrEmpty(line)) {
					string[] parts = line.Split(':');
					if(null != parts && parts.Length >= 2) {
						references.Add(
							new KeyValuePair<FileInfo, int>(new FileInfo(parts[0]), int.Parse(parts[1])));
					}
				}
			}
		}
		return references;
	}

	/// <summary>
	/// Finds the references.
	/// </summary>
	/// <param name="text">Text.</param>
	public static string FindInFiles(string text) {
		return ExecuteCommand("grep", " -R -n '" + text + "' .");
	}

	/// <summary>
	/// Executes the command.
	/// Uses the Application.dataPath as location in which the command will be executed
	/// </summary>
	/// <returns>The command.</returns>
	/// <param name="command">Command.</param>
	/// <param name="arguments">Arguments.</param>
	public static string ExecuteCommand(string command, string arguments) {
		return ExecuteCommand(command, arguments, Application.dataPath);
	}

	/// <summary>
	/// Executes the command.
	/// </summary>
	/// <returns>The command.</returns>
	/// <param name="command">Command.</param>
	/// <param name="arguments">Arguments.</param>
	/// <param name="dataPath">Data path.</param>
	public static string ExecuteCommand(string command, string arguments, string dataPath) {
		ProcessStartInfo pInfo = new ProcessStartInfo(command, arguments);
		pInfo.WorkingDirectory = dataPath;
		pInfo.RedirectStandardOutput = true;
		pInfo.UseShellExecute = false;
		using(var process = Process.Start(pInfo)) {
			process.Start();
			process.WaitForExit();
			return process.StandardOutput.ReadToEnd();
		}
	}

}
