#!/usr/bin/env bash
# Built-in NuGet vulnerability audit for this .NET solution.
#
# Missing dotnet does not fail the run; real vulnerability findings do.
set -uo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$root" || exit 1

if ! command -v dotnet >/dev/null 2>&1; then
    echo "skip - dotnet is not installed"
    exit 0
fi

report="$(mktemp)"
trap 'rm -f "$report"' EXIT

if ! dotnet package list --include-transitive --vulnerable --format json --output-version 1 >"$report"; then
    echo "warning - NuGet vulnerability audit did not complete"
    echo "Run dotnet restore, then retry scripts/security/nuget-audit.sh."
    exit 0
fi

if command -v python3 >/dev/null 2>&1; then
    python3 - "$report" <<'PY'
import json
import sys

with open(sys.argv[1], encoding="utf-8") as f:
    data = json.load(f)

findings = []
for project in data.get("projects", []):
    project_path = project.get("path", "<unknown project>")
    for framework in project.get("frameworks", []):
        framework_name = framework.get("framework", "<unknown framework>")
        packages = []
        packages.extend(framework.get("topLevelPackages", []))
        packages.extend(framework.get("transitivePackages", []))
        for package in packages:
            for vulnerability in package.get("vulnerabilities") or []:
                findings.append(
                    (
                        project_path,
                        framework_name,
                        package.get("id", "<unknown package>"),
                        package.get("resolvedVersion") or package.get("requestedVersion") or "<unknown version>",
                        vulnerability.get("severity", "<unknown severity>"),
                        vulnerability.get("advisoryurl", "<no advisory URL>"),
                    )
                )

if not findings:
    print("ok - no vulnerable NuGet packages reported by current sources")
    sys.exit(0)

print("findings - vulnerable NuGet package(s) reported:")
for project_path, framework_name, package_id, version, severity, advisory_url in findings:
    print(f"- {project_path} [{framework_name}] {package_id} {version}: {severity} {advisory_url}")
sys.exit(1)
PY
    exit $?
fi

if grep -q '"vulnerabilities"' "$report"; then
    echo "findings - vulnerable NuGet package(s) reported:"
    grep -n '"id"\|"resolvedVersion"\|"severity"\|"advisoryurl"' "$report"
    exit 1
fi

echo "ok - no vulnerable NuGet packages reported by current sources"
