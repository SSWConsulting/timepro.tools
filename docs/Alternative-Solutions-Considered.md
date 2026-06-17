# Alternative Solutions Considered

## Decision

Use a CLI-first architecture with MCP support.

The CLI is the primary interface because it works well for humans, shell scripts, local automation, and agentic coding tools such as Claude Code and Codex. MCP is a secondary interface for more closed agents that need structured tool calls instead of direct shell access.

## Options

| Option | Strengths | Weaknesses | Decision |
|--------|-----------|------------|----------|
| CLI-first | Works for humans, scripts, CI, launchd, and agents with shell access. Easy to compose with files, Git, GitHub, Azure DevOps, and local repo context. Keeps execution local. | Requires a terminal-capable environment. UX is command-oriented rather than visual. | Chosen as the primary interface. |
| MCP-first | Good for agents that expose MCP tools and prefer structured calls. Can hide command syntax from the agent. | Less useful for humans and shell automation. More constrained when the agent also needs files, Git, GitHub, or Azure DevOps context. | Supported through `tp mcp`, not primary. |
| Website | Familiar visual UI and easier for non-terminal users. Could provide guided forms. | Duplicates the existing TimePro web app, adds hosting/auth/security overhead, and is weaker for local agent automation. | Not chosen. |

## Rationale

The core use case is agentic and automation-heavy. Agents often need to inspect code, read local files, query Git history, inspect GitHub issues and PRs, and then create or validate TimePro entries. A CLI fits that workflow because it composes naturally with the rest of the developer environment.

MCP remains useful for closed agents or chat surfaces where direct shell execution is unavailable or undesirable. The MCP host should reuse the same infrastructure as the CLI so behavior stays consistent.

A website can be reconsidered only if there is a distinct user group that cannot use the TimePro web app or the CLI and has a workflow that justifies the extra hosting and security surface.
