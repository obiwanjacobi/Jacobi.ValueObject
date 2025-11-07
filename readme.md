# Value Object Source Generator

This package contains a Roslyn Source Generator that creates Value Object implementations from `partial record struct` declarations.

The goal of this project is to provide a library that is simple to use and good (enough) for most cases.

## Value Object

Read more on how to use the Value Object generator in this [readme](./Jacobi.ValueObject/readme.md).

## Solution

- Jacobi.ValueObject contains the `ValueObject` code attribute and related types.
- Jacobi.ValueObject.Generator contains the Roslyn Source Code Generator that creates the Value Object implementation.
- Jacobi.ValueObject.Tests contains test code to excercise the various states and conditions for the Value Object implementation.
