---
description: "Runtime conventions for errors, logs, settings and CLIs"
paths:
  - "**/*.cs"
---

# Runtime Conventions

One set of rules for errors, logs, settings and CLIs holds in every language, so logs and errors
from different repos read and search together. This is how they look here.

## Errors

- Bad input throws `ArgumentException`; any other failure throws a subclass of one
  `<Component>Exception` base per assembly (.NET's name for `<Component>Error`). Messages are
  lowercase, with no trailing punctuation, and name what failed (`claim code expired`).

## Logging

- Library code never writes to stdout or stderr and adds no log handlers. Where it logs, it takes an
  `ILogger`.
- Log through `ILogger` with a `snake_case` event as the message template and named fields
  (`"device_paired {device_id}"`), never a sentence with values interpolated.
- Field keys are `snake_case` in every language, so one query reads every service. The shared names
  are `team_id`, `device_id`, `request_id`, `trace_id`, `duration_ms`, and `error` for the error,
  never `e` or `err`.

## Settings

- Options are an immutable `<Name>Options` record (.NET's name for `<Component>Config`) with
  production defaults, and units go in names (`TimeoutMs`, `MaxBodyBytes`).
- Env vars it reads for its own settings start with `QUALITHM_`, because it runs on a builder's
  machine or device. Standard names (`PORT`, `LOG_LEVEL`) stay unprefixed, and credentials keep
  their own naming rule.
