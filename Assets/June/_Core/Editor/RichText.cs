/// RichText string formatter for Unity.
/// 
/// Version : 0.1	[22-OCT-2014]
/// Author  : Gaurang Sinha

using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Rich text formatter.
/// </summary>
public class RichText {

	private const string TAG_BOLD 		= "b";
	private const string TAG_ITALICS 	= "i";
	private const string TAG_SIZE 		= "size";
	private const string TAG_COLOR 		= "color";

	public const string COLOR_AQUA		= "#00FFFFFF";
	public const string COLOR_BLACK 	= "#000000FF";
	public const string COLOR_BLUE 		= "#0000FFFF";
	public const string COLOR_BROWN 	= "#A52A2AFF";
	public const string COLOR_CYAN 		= "#00FFFFFF";
	public const string COLOR_DARKBLUE 	= "#0000A0FF";
	public const string COLOR_FUCHSIA 	= "#FF00FFFF";
	public const string COLOR_GREEN 	= "#008000FF";
	public const string COLOR_GREY 		= "#808080FF";
	public const string COLOR_LIGHTBLUE = "#ADD8E6FF";
	public const string COLOR_LIME 		= "#00FF00FF";
	public const string COLOR_MAGENTA 	= "#FF00FFFF";
	public const string COLOR_MAROON 	= "#800000FF";
	public const string COLOR_NAVY 		= "#000080FF";
	public const string COLOR_OLIVE 	= "#808000FF";
	public const string COLOR_ORANGE 	= "#FFA500FF";
	public const string COLOR_PURPLE 	= "#800080FF";
	public const string COLOR_RED 		= "#FF0000FF";
	public const string COLOR_SILVER 	= "#C0C0C0FF";
	public const string COLOR_TEAL 		= "#008080FF";
	public const string COLOR_WHITE 	= "#FFFFFFFF";
	public const string COLOR_YELLOW 	= "#ffff00ff";

	private StringBuilder _RichTextStr = null;

	private Stack<string> _UsedTags = null;

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="RichText"/> class.
	/// </summary>
	/// <param name="initialText">Initial text.</param>
	public RichText(string initialText) {
		this._RichTextStr = new StringBuilder(initialText);
		this._UsedTags = new Stack<string>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RichText"/> class.
	/// </summary>
	public RichText() : this(string.Empty) { }
	#endregion

	#region Private Helper Methods
	/// <summary>
	/// Starts the tag.
	/// </summary>
	/// <returns>The tag.</returns>
	/// <param name="tag">Tag.</param>
	private RichText StartTag(string tag) {
		this._UsedTags.Push(tag);
		this._RichTextStr.AppendFormat("<{0}>", tag);
		return this;
	}

	/// <summary>
	/// Starts the tag.
	/// </summary>
	/// <returns>The tag.</returns>
	/// <param name="tag">Tag.</param>
	/// <param name="value">Value.</param>
	private RichText StartTag(string tag, string value) {
		this._UsedTags.Push(tag);
		this._RichTextStr.AppendFormat("<{0}={1}>", tag, value);
		return this;
	}

	/// <summary>
	/// Ends the tag.
	/// </summary>
	/// <returns>The tag.</returns>
	/// <param name="tag">Tag.</param>
	private RichText EndTag(string tag) {
		this._RichTextStr.AppendFormat("</{0}>", tag);
		return this;
	}
	#endregion

	#region Static Functions

	/// <summary>
	/// Create the specified text.
	/// </summary>
	/// <param name="text">Text.</param>
	public static RichText Create(string text = "") {
		return new RichText(text);
	}

	/// <summary>
	/// Bold the specified text.
	/// </summary>
	/// <param name="text">Text.</param>
	public static RichText BoldText(string text) {
		return new RichText().Bold(text);
	}

	/// <summary>
	/// Italics the specified text.
	/// </summary>
	/// <param name="text">Text.</param>
	public static RichText ItalicsText(string text) {
		return new RichText().Italics(text);
	}

	/// <summary>
	/// Size the specified size and text.
	/// </summary>
	/// <param name="size">Size.</param>
	/// <param name="text">Text.</param>
	public static RichText SizeText(int size, string text) {
		return new RichText().Size(size, text);
	}

	/// <summary>
	/// Color the specified color and text.
	/// </summary>
	/// <param name="color">Color.</param>
	/// <param name="text">Text.</param>
	public RichText ColorText(string color, string text) {
		return new RichText().Color(color, text);
	}

	#endregion

	/// <summary>
	/// Append the specified text.
	/// </summary>
	/// <param name="text">Text.</param>
	public RichText Append(string text) {
		this._RichTextStr.Append(text);
		return this;
	}

	/// <summary>
	/// Appends the line.
	/// </summary>
	/// <returns>The line.</returns>
	/// <param name="text">Text.</param>
	public RichText AppendLine(string text) {
		this._RichTextStr.AppendLine(text);
		return this;
	}

	/// <summary>
	/// Appends the format.
	/// </summary>
	/// <returns>The format.</returns>
	/// <param name="text">Text.</param>
	/// <param name="args">Arguments.</param>
	public RichText AppendFormat(string text, params object[] args) {
		this._RichTextStr.AppendFormat(text, args);
		return this;
	}

	/// <summary>
	/// Ends the tag.
	/// </summary>
	/// <returns>The tag.</returns>
	public RichText End() {
		return this.EndTag(_UsedTags.Pop());
	}

	/// <summary>
	/// Ends all.
	/// </summary>
	/// <returns>The all.</returns>
	public RichText EndAll() {
		while(null != this._UsedTags && this._UsedTags.Count > 0) {
			this.End();
		}
		return this;
	}

	/// <summary>
	/// Ends all get string.
	/// </summary>
	/// <returns>The all get string.</returns>
	public string EndAllGetString() {
		return this.EndAll().ToString();
	}

	/// <summary>
	/// Starts the bold tag.
	/// </summary>
	/// <returns>The bold.</returns>
	public RichText Bold() {
		return this.StartTag(TAG_BOLD);
	}

	/// <summary>
	/// Bold the specified text.
	/// </summary>
	/// <param name="text">Text.</param>
	public RichText Bold(string text) {
		return this.Bold().Append(text).End();
	}

	/// <summary>
	/// Starts the italics.
	/// </summary>
	/// <returns>The italics.</returns>
	public RichText Italics() {
		return this.StartTag(TAG_ITALICS);
	}

	/// <summary>
	/// Italics the specified text.
	/// </summary>
	/// <param name="text">Text.</param>
	public RichText Italics(string text) {
		return this.Italics().Append(text).End();
	}

	/// <summary>
	/// Starts the color.
	/// </summary>
	/// <returns>The color.</returns>
	/// <param name="color">Color.</param>
	public RichText Color(string color) {
		return this.StartTag(TAG_COLOR, color);
	}

	/// <summary>
	/// Color the specified color and text.
	/// </summary>
	/// <param name="color">Color.</param>
	/// <param name="text">Text.</param>
	public RichText Color(string color, string text) {
		return this.Color(color).Append(text).End();
	}

	/// <summary>
	/// Starts the size.
	/// </summary>
	/// <returns>The size.</returns>
	/// <param name="size">Size.</param>
	public RichText Size(int size) {
		return this.StartTag(TAG_SIZE, size.ToString());
	}
	
	/// <summary>
	/// Size the specified size and text.
	/// </summary>
	/// <param name="size">Size.</param>
	/// <param name="text">Text.</param>
	public RichText Size(int size, string text) {
		return this.Size(size).Append(text).End();
	}

	/// <summary>
	/// Sizes the color.
	/// </summary>
	/// <returns>The color.</returns>
	/// <param name="size">Size.</param>
	/// <param name="color">Color.</param>
	/// <param name="text">Text.</param>
	public RichText SizeColor(int size, string color, string text) {
		return this.Size(size).Color(color, text).End();
	}

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="RichText"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="RichText"/>.</returns>
	public override string ToString () {
		return _RichTextStr.ToString();
	}
}
