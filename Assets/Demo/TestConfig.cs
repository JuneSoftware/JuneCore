using UnityEngine;
using System.Collections;
using June.Core;
using System.Collections.Generic;

/// <summary>
/// Test config section.
/// </summary>
public class TestConfig : BaseModel {

	/// <summary>
	/// Gets the identifier.
	/// </summary>
	/// <value>The identifier.</value>
	public int Id {
		get {
			return GetInt(Schema.TestConfig.Id);
		}
	}

	/// <summary>
	/// Gets the name.
	/// </summary>
	/// <value>The name.</value>
	public string Name {
		get {
			return GetString(Schema.TestConfig.Name);
		}
	}

	private TestConfigDoc _Doc;
	/// <summary>
	/// Gets the sub document.
	/// </summary>
	/// <value>The document.</value>
	public TestConfigDoc Doc {
		get {
			if(null == _Doc) {
				_Doc = GetModel(Schema.TestConfig.Doc, doc => new TestConfigDoc(doc));
			}
			return _Doc;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TestConfig"/> class.
	/// </summary>
	/// <param name="doc">Document.</param>
	public TestConfig(IDictionary<string, object> doc) : base(doc) { }

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="TestConfig"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="TestConfig"/>.</returns>
	public override string ToString() {
		return string.Format("[TestConfig: Id={0}, Name={1}, Doc={2}]", Id, Name, Doc);
	}
}

