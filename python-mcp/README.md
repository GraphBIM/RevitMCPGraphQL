# Revit GraphQL MCP Server

This folder contains a FastMCP server (`revit_mcp_server.py`) that proxies the local Revit GraphQL endpoint exposed by the Revit add-in.

## Environment
Set `REVIT_GRAPHQL_URL` if your GraphQL server is not `http://localhost:5000/graphql`.
Optionally set `REVIT_GRAPHQL_TIMEOUT` (seconds, default 10).

## Install & Run (manual)
```powershell
cd python-mcp
python -m pip install -r requirements.txt
python revit_mcp_server.py
```
The server communicates over stdio (MCP compatible).

## Convenience Launcher (Windows)
Use the batch file:
```powershell
python-mcp\run_revit_mcp_server.bat
```
It auto-installs dependencies then launches the server.

## VS Code / MCP Client Integration
Preferred (simplest) executable path on Windows:
```
${workspaceFolder}/python-mcp/revit_mcp_server.cmd
```
This wrapper installs dependencies (idempotent) and launches the stdio MCP server.

Alternative (explicit python):
```
command: python
args: ["${workspaceFolder}/python-mcp/revit_mcp_server.py"]
cwd: ${workspaceFolder}/python-mcp
```
Ensure you do NOT set the command to just `revit_mcp_server.py` on Windows; that triggers a "program not found" spawn error because `.py` isn’t directly executable by all hosts.

## Tools Exposed
- health
- categories
- elements (optional category_name argument)
- rooms
- set_element_parameters_batch
- raw (arbitrary GraphQL)
- server_info

## Troubleshooting
- Error `program not found` (or exit code 2): Point your MCP client to `revit_mcp_server.cmd` (recommended) or use `python revit_mcp_server.py`. Avoid specifying only the `.py` filename as the command on Windows.
- Error `fastmcp package not installed`: Dependencies failed to install. Re-run `python -m pip install -r requirements.txt` (check proxy / SSL if it persists).
- Network errors: Verify the Revit add-in GraphQL server is running and reachable at `REVIT_GRAPHQL_URL` (try opening it in a browser or `Invoke-WebRequest`).
- Hanging / no responses: Confirm the client uses stdio transport (no TCP by default). Use the `server_info` tool to inspect configuration.
- Need verbose HTTP diagnostics: Temporarily set the env var `REVIT_MCP_DEBUG=1` (then add prints or logging inside `_post`).
