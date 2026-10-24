# Examples

Each example reads the API token from `QUALITHM_API_TOKEN`. Create one in the Qualithm app, on the
team page under **API tokens**.

| Example                     | Description                                                            |
| --------------------------- | ---------------------------------------------------------------------- |
| [ListDevices](ListDevices/) | List the devices in the token's team, and report a `PlatformException` |

## Running Examples

```bash
QUALITHM_API_TOKEN=<your-api-token> dotnet run --project examples/ListDevices
```
