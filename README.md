# JuneCore

This repository contains the core classes used across all our projects

The `IBaseModel` & `IBaseModelExt` interfaces are the basis of our Models. Each app extends these interfaces and specifies the exact `TRecord` & `TRecordArray` implementation for that app.

Since we use **JSON** extensively (configs/api/database/etc) throughout our apps we are going to assume **SimpleJson** as our Serialiser/Deserialiser.

SimpleJson deserialises JSON into `IDictionary<string, object>` and `List<object>` objects in C#.

![June.Core Class Diagram](https://raw.githubusercontent.com/JuneSoftware/JuneCore/master/images/June.Core.ClassDiagram.png)

```csharp
IBaseModel<TRecord> 
	where TRecord : class
	
		/\
		||
	
IBaseModelExt<TRecord, TRecordArray> : IBaseModel<TRecord>
	where TRecord : class
	where TRecordArray : class, IEnumerable

		/\
		||

BaseModel : IBaseModelExt<IDictionary<string, object>, List<object>>

```


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