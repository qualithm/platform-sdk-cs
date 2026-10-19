---
description: "Conventions for runnable example projects"
paths:
  - "examples/**"
---

# Example Conventions

## File Structure

1. Each example is its own console project, `examples/<Name>/<Name>.csproj`, referencing only the
   library it shows (`src/<Name>`, or `src/<Name>.Core` in an app). It joins the solution the checks
   build (`<Name>.slnx`, and `portable.slnf` in an app).
2. `Program.cs` uses top-level statements and opens with one comment line:
   `// Example: <title and brief description.>`
3. Imports: `System.*` first, then packages, then this repo's namespaces.
4. Show the happy path first, then the error case the API documents, printed with
   `Console.WriteLine($"error: {e.Message}")`.

## README

`examples/README.md` lists every example in an `| Example | Description |` table and shows how to
run one: `dotnet run --project examples/<Name>`.
