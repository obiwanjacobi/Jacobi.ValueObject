# Value Object Source Generator

A C# Roslyn source generator that auto-generates immutable, strongly-typed Value Object implementations from simple struct declarations.

- Automatically generates code from partial struct or partial record struct declarations decorated with [ValueObject] or [MultiValueObject] attributes
- Creates single-value wrappers (e.g., ProductId wrapping a Guid) and multi-value objects (structs with multiple properties)
- Ensures immutability and value-based equality comparison by default
- Supports optional features like parsing, implicit conversions, deconstruction, and custom validation
- Supports Json serialization, both System.Text.Json and Newtonsoft.Json
- Reduces boilerplate code while maintaining strong typing and compile-time safety
- Licensed under LGPL v2.1

The goal of this project is to provide a library that is simple to use and good (enough) for most cases.

## Value Object

Add the `Jacobi.ValueObject` (NuGet) package to your project to get started.

Read more on how to use the Value Object generator in this [readme](./Jacobi.ValueObject/readme.md).

## Solution

- Jacobi.ValueObject contains the `ValueObject` code attribute and related types.
- Jacobi.ValueObject.Generator contains the Roslyn Source Code Generator that creates the Value Object implementation.
- Jacobi.ValueObject.Tests contains test code to excercise the various states and conditions for the Value Object implementation.
