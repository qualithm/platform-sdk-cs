---
description: "Exact pre-commit commands for the dotnet-lib CI archetype, kept in sync with ci.yaml"
---

# Pre-commit Checks

This repo's `ci.yaml` is generated from `dx/ci-templates/dotnet-lib.yaml` via `dx ci sync` (check
for drift with `dx ci drift`). Run these before committing so CI passes on the first try:

```bash
dotnet restore --locked-mode                     # fails when a packages.lock.json is stale; run `dotnet restore` to update it
dotnet format --verify-no-changes --no-restore   # fix with `dotnet format`
dotnet build --no-restore -c Release
python3 .claude/checks/ci-jobs.py --skip-ci-only coverage
python3 .claude/checks/ci-jobs.py pack
python3 .claude/checks/ci-jobs.py docs
```

They run on macOS, Linux or Windows with the SDK `global.json` pins, against the repo's one `.slnx`.
The `ci-jobs.py` steps run CI's jobs as written:

- `coverage` runs the tests and the 80% line gate, minus the Codecov upload.
- `pack` builds the `.nupkg` and its `.snupkg` symbols into `nupkg/`, as the release does.
- `docs` restores the pinned docfx (`.config/dotnet-tools.json`) and builds the API reference from
  the XML doc comments into `docs/_site/`, failing on a warning. CI runs it on PRs into `main` only.

## Outside the gate

The gate measures the library in `src/`. Tests and `examples/` aren't measured.
