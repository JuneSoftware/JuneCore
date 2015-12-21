using System;
using UnityEditor;
using UnityEngine;
using System.Collections;

/// <summary>
/// June vertical section.
/// </summary>
public class JuneVerticalSection : GenericSection {
	
	public JuneVerticalSection(params GUILayoutOption[] options) 
		: base(
			begin: () => EditorGUILayout.BeginVertical(options),
			end:   () => EditorGUILayout.EndVertical()) { }
}

/// <summary>
/// June horizontal section.
/// </summary>
public class JuneHorizontalSection : GenericSection {
	
	public JuneHorizontalSection(params GUILayoutOption[] options) 
		: base(
			begin: () => EditorGUILayout.BeginHorizontal(options),
			end:   () => EditorGUILayout.EndHorizontal()) { }
}

/// <summary>
/// June disabled group.
/// </summary>
public class JuneDisabledGroup : GenericSection {

	public JuneDisabledGroup(bool isDisabled) 
		: base(
			begin: () => EditorGUI.BeginDisabledGroup(isDisabled),
			end:   () => EditorGUI.EndDisabledGroup()) { }
}

/// <summary>
/// June indent level.
/// </summary>
public class JuneIndentLevel : GenericSection {

	public JuneIndentLevel() : this(1) { }

	public JuneIndentLevel(int level) 
		: base(
			begin: () => { EditorGUI.indentLevel += level; },
			end:   () => { EditorGUI.indentLevel -= level; }) { }
}

/// <summary>
/// June scroll view.
/// </summary>
public class JuneScrollView : GenericSection {
	
	public JuneScrollView(ref Vector2 position, params GUILayoutOption[] options) 
		: base(
			begin: null, // not providing a begin action as we need the `ref Vector2` to be updated
			end: () => EditorGUILayout.EndScrollView(), 
		beginImmediate: false) { 
		Begin(ref position, options);
	}
	
	public void Begin(ref Vector2 pos, params GUILayoutOption[] options) {
		pos = EditorGUILayout.BeginScrollView(pos, options);
	}
}

/// <summary>
/// Generic section.
/// </summary>
public class GenericSection : IDisposable {

	protected Action _begin;
	protected Action _end;

	/// <summary>
	/// Initializes a new instance of the <see cref="GenericSection"/> class.
	/// </summary>
	/// <param name="begin">Begin.</param>
	/// <param name="end">End.</param>
	public GenericSection(Action begin, Action end) : this(begin, end, true) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="GenericSection"/> class.
	/// </summary>
	/// <param name="begin">Begin.</param>
	/// <param name="end">End.</param>
	/// <param name="beginImmediate">If set to <c>true</c> begin immediate.</param>
	public GenericSection(Action begin, Action end, bool beginImmediate) {
		_begin = begin;
		_end = end;
		if(null != _begin && beginImmediate) {
			_begin();
		}
	}

	/// <summary>
	/// Releases all resource used by the <see cref="GenericSection"/> object.
	/// </summary>
	/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="GenericSection"/>. The <see cref="Dispose"/>
	/// method leaves the <see cref="GenericSection"/> in an unusable state. After calling <see cref="Dispose"/>, you must
	/// release all references to the <see cref="GenericSection"/> so the garbage collector can reclaim the memory that the
	/// <see cref="GenericSection"/> was occupying.</remarks>
	public void Dispose() {
		if(null != _end) {
			_end();
		}
	}
}