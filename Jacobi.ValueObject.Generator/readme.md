# ValueObject Generator


## AnalizerReleases.(Un)Shipped.md

https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md


## TODO

- [ ] Serialization System.Text.Json + Newtonsoft.Json (JsonConvertor) - Can these classes be nested private?
- [ ] AspnetCore (TypeConvertor)/EFCore (ValueConvertor) support? (serialization?)
- [ ] FindMethod parameter (type) checking (both generators)
- [ ] Multi: Do we need to detect if property-types are structs (not primitives) and do ref-struct passing (avoids copying)? Can we do both?
- [ ] Multi: check properties accessor list for mutable 'set'. TBD: 'init'?
- [ ] 

