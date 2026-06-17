# Business

## Purpose

TimePro Tools gives developers, consultants, and AI agents a fast local interface for SSW TimePro. It supports timesheet entry, review, reporting, leave, project lookup, accounting reads, and daily scrum generation from the terminal.

## Problem

TimePro work is often spread across manual UI actions, local repo context, GitHub activity, calendar bookings, and repeated reporting tasks. Humans and AI agents need a reliable way to combine those inputs without copying sensitive data into prompts or rebuilding the same workflow each day.

## Goals

- Make common TimePro tasks fast from a terminal.
- Give AI coding agents a scriptable, auditable command surface.
- Support closed agents through MCP where direct shell access is not available.
- Keep credentials local and avoid exposing API keys in command output.
- Keep examples and fixtures sanitized with Northwind placeholder data.
- Support automation workflows such as timesheet checks, daily scrum generation, and read-only accounting summaries.

## Statement of Intent

This project is CLI-first. The CLI is the primary interface for humans, scripts, and agents that can run shell commands. MCP is provided as a compatibility layer for agents that prefer or require structured tool calls.

The tool should remain local, composable, and automation-friendly. It should not become a replacement TimePro website.

## Users

- Developers and consultants entering or checking timesheets.
- AI agents such as Claude Code, Codex, and Copilot working in a local repository.
- Scripted automation running checks or summaries.
- Accounting/admin users running read-only reports through approved commands.

## Out of Scope

- Replacing the TimePro web application.
- Hosting a new backend or database.
