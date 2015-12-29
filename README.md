# JuneCore

This framework was born out of need to abstract our data formats while providing plain old C# objects to the game developer without any dependence on Unity3d or a full featured Serialization/Deserialization framework *(e.g. System.Xml.Serialization)*.

A game typically deals with data from various sources _(Configs/API/etc)_, there is a need to provide a standard way of defining these models and the methods of accessing the data.

This framework tries to abstract the data format from the developer while giving him a consistent interface to query the data and create the models.

This repository contains the core classes used across all our projects.

The `IBaseModel` interfaces is the basis of our Models. Each app extends these interfaces and specifies the exact `TRecord` & `TRecordArray` implementation for that app.

Our apps use **JSON** extensively (configs/api/database/etc), and we use **SimpleJson** as our Serialiser/Deserialiser.

SimpleJson deserialises JSON into `IDictionary<string, object>` and `List<object>` objects in C#.

The `IBaseModel` interface takes two types `TRecord` and `TKey`. The `TRecord` parameter defines the type of object holding the data while the `TKey` object defines the type of object used to fetch individual values.

This is a detailed class diagram with :
[June.Core Class Diagram](https://raw.githubusercontent.com/JuneSoftware/JuneCore/master/images/June.Core.ClassDiagram.png)

Below is a basic overview of the hierarchy for `BaseModel`.

```csharp
IBaseModel<TRecord, TKey> 
	where TRecord : class
	
		/\
		||
	
IBaseModelExt<TRecord, TRecordArray> : IBaseModel<TRecord, string>
	where TRecord : class
	where TRecordArray : class, IEnumerable

		/\
		||

BaseModel : IBaseModelExt<IDictionary<string, object>, List<object>>

```
Basic overview of hierarchy for `BaseList`

```csharp
IBaseList<TRecordArray, TRecord, TModel> : IList<TModel>
	where TModel : IBaseModel<TRecord>
	where TRecord : class
	where TRecordArray : class, IEnumerable

		/\
		||

BaseList<T> : IBaseList<IRecordArray, IRecord, T> 
	where T : BaseModel
	
```
The framework also contains a `BaseConfig` interface which lets the developers define the configuration manager with only a few lines of code.

Below is an example of a config file:

```json
{
	"sections":[
		{
			"id":1,
			"name":"section1",
			"doc":{
				"int":1,
				"str":"str1"
			}
		},
		{
			"id":2,
			"name":"section2",
			"doc":{
				"int":2,
				"str":"str2"
			}
		},
		{
			"id":3,
			"name":"section3",
			"doc":{
				"int":3,
				"str":"str3"
			}
		}
	]
}
```

The model which defines each individual item in the config will look like:

```csharp
// Model which represents document defined in the `sections` key
public class TestConfig : BaseModel {

	public int Id {
		get {
			return GetInt(Schema.TestConfig.Id);
		}
	}

	public string Name {
		get {
			return GetString(Schema.TestConfig.Name);
		}
	}

	private TestConfigDoc _Doc;
	public TestConfigDoc Doc {
		get {
			if(null == _Doc) {
				_Doc = GetModel(Schema.TestConfig.Doc, doc => new TestConfigDoc(doc));
			}
			return _Doc;
		}
	}

	public TestConfig(IDictionary<string, object> doc) : base(doc) { }
}

// Model which represents the document in the key `doc`
public class TestConfigDoc : BaseModel {
	
	public int IntegerField {
		get {
			return GetInt(Schema.TestConfigDoc.IntegerField);
		}
	}

	public string StringField {
		get {
			return GetString(Schema.TestConfigDoc.StringField);
		}
	}

	public TestConfigDoc(IDictionary<string, object> doc) : base(doc) { }
}
```

The code below implements `TestConfigManager` class which reads the above JSON config and converts them into C# objects. The `BaseConfig` creates a singleton and provides an `Instance` property which can be used to access the individual objects.

```csharp
// The first parameter in BaseConfig defines the return type of the static instance property, 
// The second parameter defines the type of deserialised object
public class TestConfigManager : BaseConfig<TestConfigManager, TestConfig> {

	// Gets the name of the resource.
	public override string ResourceName {
		get {
			return "TestConfig";
		}
	}

	// Gets the root key.
	public override string RootKey {
		get {
			return "sections";
		}
	}

	// Gets the item converter.
	public override Converter<IDictionary<string, object>, TestConfig> ItemConverter {
		get {
			return doc => new TestConfig(doc);
		}
	}
}

// This class now be accessed via `TestConfigManager.Instance` property e.g.

Debug.Log(TestConfigManager.Instance.Items[0].Id)	// This will print '1' to the console

Debug.Log(TestConfigManager.Instance.Items[1].Name)	// This will print 'section2' to the console

```

