using UnityEngine;
using System.Collections;
using June.Core;
using System.Collections.Generic;

/// <summary>
/// Test config document.
/// </summary>
public class TestConfigDoc : BaseModel {
	
	/// <summary>
	/// Gets the integer field.
	/// </summary>
	/// <value>The integer field.</value>
	public int IntegerField {
		get {
			return GetInt(Schema.TestConfigDoc.IntegerField);
		}
	}

	/// <summary>
	/// Gets the string field.
	/// </summary>
	/// <value>The string field.</value>
	public string StringField {
		get {
			return GetString(Schema.TestConfigDoc.StringField);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TestConfigDoc"/> class.
	/// </summary>
	/// <param name="doc">Document.</param>
	public TestConfigDoc(IDictionary<string, object> doc) : base(doc) { }

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="TestConfigDoc"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="TestConfigDoc"/>.</returns>
	public override string ToString() {
		return string.Format("[TestConfigDoc: IntegerField={0}, StringField={1}]", IntegerField, StringField);
	}
}
