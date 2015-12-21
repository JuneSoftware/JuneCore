using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// June build summary.
/// </summary>
public class JuneBuildSummary {

	/// <summary>
	/// The TITLE.
	/// </summary>
	public const string TITLE = "Ninjump Dash";

	/// <summary>
	/// The GENERATORS.
	/// </summary>
	public static List<Action<StringBuilder>> GENERATORS = new List<Action<StringBuilder>>() {
		JuneBuildSummary.WriteHeader,
		JuneBuildSummary.WriteBuildOptions,
		JuneBuildSummary.WriteGameConfig,
		JuneBuildSummary.WriteFooter
	};

	/// <summary>
	/// Generates the build report.
	/// </summary>
	/// <param name="path">Path.</param>
	public static void GenerateBuildReport(string path, bool openFile) {
		StringBuilder html = new StringBuilder();

		foreach(var gen in GENERATORS) {
			gen(html);
		}

		File.WriteAllText(path, html.ToString());

		if(openFile) {
			System.Diagnostics.Process.Start("open", path);
		}
	}

	#region Helper Methods
	/// <summary>
	/// Writes the section header.
	/// </summary>
	/// <param name="builder">Builder.</param>
	/// <param name="header">Header.</param>
	public static void WriteSectionHeader(StringBuilder builder, string header) {
		builder.AppendFormat("<h3>{0}</h3>", header);
	}

	/// <summary>
	/// Writes the table start.
	/// </summary>
	/// <param name="builder">Builder.</param>
	public static void WriteTableStart(StringBuilder builder) {
		builder.Append("<table>");
	}

	/// <summary>
	/// Writes the table end.
	/// </summary>
	/// <param name="builder">Builder.</param>
	public static void WriteTableEnd(StringBuilder builder) {
		builder.Append("</table>");
	}

	/// <summary>
	/// Writes the table row.
	/// </summary>
	/// <param name="builder">Builder.</param>
	/// <param name="columns">Columns.</param>
	public static void WriteTableRow(StringBuilder builder, params string[] columns) {
		if(null != columns && columns.Length > 0) {
			builder.Append("<tr>");
			foreach(var col in columns) {
				builder.AppendFormat("<td>{0}</td>", col);
			}
			builder.Append("</tr>");
		}
	}

	#endregion

	/// <summary>
	/// Writes the header.
	/// </summary>
	/// <param name="builder">Builder.</param>
	private static void WriteHeader(StringBuilder builder) {
		builder.AppendFormat("<html><head><title>{0}</title></head><body>", TITLE);
		builder.AppendFormat("<h1>{0}</h1>", TITLE);
	}

	/// <summary>
	/// Writes the footer.
	/// </summary>
	/// <param name="builder">Builder.</param>
	private static void WriteFooter(StringBuilder builder) {
		builder.Append("</body></html>");
	}

	/// <summary>
	/// Writes the build options.
	/// </summary>
	/// <param name="builder">Builder.</param>
	private static void WriteBuildOptions(StringBuilder builder) {
		WriteSectionHeader(builder, "Build Options");

		WriteTableStart(builder);

		WriteTableRow(builder, "Bundle Identifier", PlayerSettings.bundleIdentifier);
		WriteTableRow(builder, "Bundle Version", PlayerSettings.bundleVersion);

		#if UNITY_ANDROID
		WriteTableRow(builder, "Bundle Version Code", PlayerSettings.Android.bundleVersionCode.ToString());
		WriteTableRow(builder, "Minimum API Level", PlayerSettings.Android.minSdkVersion.ToString());
		#endif

		WriteTableEnd(builder);
	}

	/// <summary>
	/// Writes the game config.
	/// </summary>
	/// <param name="builder">Builder.</param>
	private static void WriteGameConfig(StringBuilder builder) {
		WriteSectionHeader(builder, "Game Config");
		
		WriteTableStart(builder);


		WriteTableEnd(builder);
	}
}
