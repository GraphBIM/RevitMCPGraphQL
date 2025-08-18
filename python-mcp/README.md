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
If your MCP client expects an executable path, point it to:
```
python
```
with args:
```
path\to\revit_mcp_server.py
```
Or directly to the batch file.

## Tools Exposed
- health
- categories
- elements (optional category_name argument)
- rooms
- set_element_parameters_batch
- raw (arbitrary GraphQL)
- server_info

## Troubleshooting
- Error `program not found`: Ensure the launcher references `python` plus the script, not just the script name (Windows does not treat `.py` as executable in all contexts).
- Error `fastmcp package not installed`: Run `pip install -r requirements.txt`.
- Network errors: Verify the Revit add-in GraphQL server is running and reachable at `REVIT_GRAPHQL_URL`.
