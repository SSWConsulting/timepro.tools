# Instructions - Deployment

TimePro Tools is a local CLI and MCP host. There is no hosted application, database, or cloud infrastructure to deploy from this repository.

Deployment means packaging the `tp` .NET global tool and installing it where it will run.

## Release Package

1. Update the package version in `src/SSW.TimePro.Cli/SSW.TimePro.Cli.csproj`.
2. Run the test suite:

```bash
dotnet test tests/SSW.TimePro.Cli.Tests/
dotnet test tests/SSW.TimePro.Cli.Integration/
```

3. Create the release package:

```bash
dotnet pack src/SSW.TimePro.Cli/ -c Release -o artifacts/nupkg
```

4. Install or update from the generated package source:

```bash
dotnet tool uninstall -g SSW.TimePro.Cli
dotnet tool install -g --add-source artifacts/nupkg SSW.TimePro.Cli
```

## Distribution

Current supported distribution path:

- Local or internal package installation via `dotnet tool install -g --add-source`.

Future distribution options:

- Publish the package to an internal NuGet feed.
- TODO: Publish the package to NuGet as a .NET tool. See [GitHub issue #4](https://github.com/SSWConsulting/timepro.tools/issues/4).
- Add a GitHub Actions release workflow after the package source and signing/release rules are agreed.

## Configuration After Install

Each user or automation host configures its own tenant:

```bash
tp login --tenant ssw
```

Headless automation should use environment-provided secrets and non-interactive setup. Do not commit tenant config files or API keys.

## Verification

After installation:

```bash
tp --help
tp tenant info --json
tp ts get --week
```

Run staging E2E scripts only where staging credentials are available:

```bash
./scripts/e2e/run-all.sh
```
