# C# Code Guidelines

`dotnet format`, the .NET analyzers and `TreatWarningsAsErrors` enforce formatting, naming and most
idioms. Fix what they flag; suppress a rule only in `.editorconfig`, with a comment saying why. This
file covers what they can't.

## Layout

- `src/<Name>` is one `net10.0` class library, packed for NuGet. It's the only packable project.
  `<Name>` is the package ID and the assembly name: never set `<PackageId>` apart from it, or
  coverlet finds no module to measure and the coverage gate fails.
- Tests live in `tests/<Name>.Tests` (xUnit v3 on the Microsoft Testing Platform), one test class
  per type. Examples live in `examples/`, each its own console project. Both set `IsPackable` false.
- `<Name>.slnx` names every project. Package versions live only in `Directory.Packages.props`;
  shared build settings and package metadata only in `Directory.Build.props`. The release version is
  `<Version>` there, never in a `.csproj`.

## API

- The public surface is the package's contract: keep it small, `sealed` by default, and options as
  immutable records. Anything not meant for consumers is `internal`.
- A breaking change to a public type or member is a major version bump.

## Errors and docs

- Throw `ArgumentException` (and its subclasses) for bad input, with lowercase messages and no
  trailing punctuation; use the `ThrowIf*` helpers. Catch only what you can handle.
- Nullable reference types are on: no `!` except in tests that check null handling.
- Every public type and member carries `///` docs, brief, with `<exception>` where it throws. They
  ship in the package and docfx builds the API reference from them, so a broken `cref` fails the
  build.

## When code changes

A behaviour change carries tests that cover its new lines (`dx coverage patch` checks them after the
coverage step). A package change updates `Directory.Packages.props` and every affected
`packages.lock.json` in the same commit.

## Generated files

`.github/workflows/ci.yaml` is generated from `dx/ci-templates/`; change the template and run
`dx ci sync` from dx, never edit it here. `CI Required` and `PR Title` are required.
