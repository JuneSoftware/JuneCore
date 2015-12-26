using UnityEngine;
using System.Collections;
using June.Core;

/// <summary>
/// Test config manager.
/// </summary>
public class TestConfigManager : BaseConfig<TestConfigManager, TestConfig> {
	
	#region implemented abstract members of BaseConfig

	/// <summary>
	/// Gets the name of the resource.
	/// </summary>
	/// <value>The name of the resource.</value>
	public override string ResourceName {
		get {
			return "TestConfig";
		}
	}

	/// <summary>
	/// Gets the root key.
	/// </summary>
	/// <value>The root key.</value>
	public override string RootKey {
		get {
			return "sections";
		}
	}

	/// <summary>
	/// Gets the item converter.
	/// </summary>
	/// <returns>The item converter.</returns>
	/// <typeparam name="U">The 1st type parameter.</typeparam>
	/// <value>The item converter.</value>
	public override System.Converter<System.Collections.Generic.IDictionary<string, object>, TestConfig> ItemConverter {
		get {
			return doc => new TestConfig(doc);
		}
	}

	#endregion


}
