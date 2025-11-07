# ValueObject Generator


## AnalizerReleases.(Un)Shipped.md

https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md


## TODO

- [ ] Detect user constructor/methods?
- [ ] Serialization System.Text.Json + Newtonsoft.Json (JsonConvertor) - Can these classes be nested private?
- [ ] AspnetCore (TypeConvertor)/EFCore (ValueConvertor) support? (serialization?)
- [ ] Work on both plain `struct`s as well as `record struct`s (structs can delcare `IEquatable<ValueObject>`)
- [ ] If IsValid is present also generate a `static bool Try(<datatype> value, out <ValueObject> valObj)`

