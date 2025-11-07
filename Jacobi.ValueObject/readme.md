# Value Object

## Usage

> Add a (package) reference to `Jacobi.ValueObject`.

```csharp
using Jacobi.ValueObject;
```

Two syntax variations:

```csharp
[ValueObject<Guid>]
public partial record struct ProductId;

[ValueObject(typeof(Guid))]
public partial record struct ProductId;
```

> We call the `Guid` the 'datatype' and the `ProductId` the 'ValueObject'.

Minimal effort:

```csharp
[ValueObject<Guid>]
public partial record struct ProductId;

...

var prodId = new ProductId(Guid.NewGuid());
```

Using Options to manage what code (support) is generated.

```csharp
[ValueObject<Guid>(Options = ValueObjectOptions.Parsable)]
public partial record struct ProductId;

...

var prodId = ProductId.Parse("<guid>", null); // no format provider
```

Implement validation by providing a `static bool IsValid(<datatype> value)` method.
You determine the accessibility (`public`, `internal`, `private`).

```csharp
[ValueObject<Guid>]
public partial record struct ProductId
{
    public static bool IsValid(Guid id) => id != Guid.Empty;
}

...

var prodId = new ProductId(Guid.Empty);   // <- will throw
```


## Options

| Option | Description |
| -- | -- |
| Constructor | Makes the value-constructor public. This option is default if none are specified. |
| ImpicitFrom | Adds an implicit assignment operator that allows assigning the `<datatype>` value to a new instance of the ValueObject. Additionally an implementation for the `IEquatable<datatype>` interface will also be generated. |
| ImplicitAs | Adds an implicit assignment operator that allows assigning the ValueObject to a `<datatype>` variable. Additionally an implementation for the `IEquatable<datatype>` interface will also be generated. |
| ExplicitFrom | Adds a static factory method `From` that construct a new ValueObject instance from a specified `<datatype>` value. |
| ToString | Overrides the `record struct` dotnet implementation to return the `ValueObject.Value` as string.
| Comparable | Implements the `IComparable<ValueObject>` interface to compare between ValueObject instances. If ImplicitFrom and/or ImplictAs options are also active, an implementation for `IComparable<datatype>` is also generated. |
| Parsable | Implements the `IParsable<ValueObject>` and `ISpanParsable<ValueObject>` interfaces to provide `Parse` and `TryParse` methods. Note that this option cannot be used in combination with a `<datatype>` of string (`System.String`).

As an alternative there is also an option to declare the interfaces explicitly and forgo specifying options.

The folowing interfaces are supported:

| Interface | Description |
| -- | -- |
| `IEquatable<datatype>` | Implements the `IEquatable<T>` interface for the datatype, as you get with the implicit-options. |
| `IComparable<ValueObject>` | Implements the `IComparable<T>` interface for the ValueObject, as if the `Comparable` option was specified. |
| `IComparable<datatype>` | Implements the `IComparable<T>` interface for the datatype, as if the `Comparable` option was specified  together with one of the implicit-options. |
| `IParsable<ValueObject>` | Implements the `IParsable<T>` interface for the ValueObject (but not `ISpanParsable<T>`), as if the `Parsable` option was specified. |
| `ISpanParsable<ValueObject>` | Implements the `ISpanParsable<T>` interface for the ValueObject (including `IParsable<T>`), as if the `Parsable` option was specified. |

## Methods

Implement a `static bool IsValid(<datatype> value)` method in your ValueObject and it will be detected and used when constructing new instances.

```csharp
[ValueObject<Guid>]
public partial record struct ProductId
{
    // public, internal or private - you decide
    public static bool IsValid(Guid id) => id != Guid.Empty;
}
```

Declare a `public static partial bool From(<datatype> value);` partial method (no implementation) in your ValueObject and it will be detected and implemented similar to specifying the `ExplicitFrom` option.

```csharp
[ValueObject<Guid>]
public partial record struct ProductId
{
    public static partial ProductId From(Guid id);
}
```

## Exceptions

The `Jacobi.ValueObject.ValueObjectException` is throw in these circumstances.

- The default (parameterless) constructor of the ValueObject is called.
- The `Value` property is accessed while the instance of the ValueObject was not correctly initialized.
- If the ValueObject implements the `IsValid` static method and the value fails the test.


## Project File

To see the generated source files for the value objects, add to your `.csproj` project file:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

## Compiler Errors

| Code | Error |
| -- | -- |
| VO001 | You have declare a ValueObject in the global namespace. It is mandatory to declare your ValueObjects inside a namespace. |
| VO002 | You did `[ValueObject(null)]` - It cannot work without a datatype. |
| VO003 | You used the Parsable option on a ValueObject with the `string`/`Systsem.String` datatype. |


Compiler errors caused by you not following the rules :-)

- Do not specify a default constructor. So do NOT do this: `public partial record struct ProductId()`
- Do not use the `ToString` option and also implement a `string ToString()` override in your ValueObject.

## Unsupported

- Json Serialization (System.Text.Json or Newtonsoft.Json)
- AspNet (TypeConvertor)
- EFcore (ValueConvertor)

For now, you have to write these yourself.
