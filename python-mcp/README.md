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

### Query Tools (All support pagination with offset/limit)
**Model & Project Info:**
- `health` - Server health check
- `model_health` - Model statistics (element count, warnings, errors)
- `document` - Document info (title, path, is family)
- `project_info` - Project metadata (name, number, address, author)
- `project_location` - Location data (latitude, longitude, elevation)
- `units` - Unit specifications and symbols

**Elements:**
- `elements` - Get elements with optional category filter
- `element_types` - Get element types (with optional category)
- `elements_by_id` - Get specific elements by ID list
- `elements_by_category` - Get elements by BuiltInCategory enum
- `elements_in_bounding_box` - Get elements within spatial bounds
- `element_relationship` - Get element dependencies and host relationships

**Spatial & Organization:**
- `rooms` - Get room elements with area data
- `levels` - Get building levels with elevation
- `coordinates` - Convert between coordinate systems

**Views & Documentation:**
- `views` - Get views (with type and template filters)
- `sheets` - Get drawing sheets
- `schedules` - Get schedule views
- `grids` - Get grid elements

**Families & Types:**
- `families` - Get families (with optional category)
- `family_types` - Get family symbols/types
- `family_instances` - Get placed family instances

**Categories & Materials:**
- `categories` - Get all categories
- `materials` - Get materials library

**Project Management:**
- `worksets` - Get worksets (for workshared models)
- `phases` - Get project phases
- `design_options` - Get design option sets
- `links` - Get linked Revit models
- `warnings` - Get model warnings

**Current Context:**
- `active_view_and_selection` - Get active view and selected elements

### Mutation Tools
**Modify Elements:**
- `set_element_parameter` - Set single parameter on element
- `set_element_parameters_batch` - Batch set parameters on multiple elements
- `set_element_type` - Change element type

**Transform Elements:**
- `move_elements` - Translate elements by vector
- `rotate_elements` - Rotate elements around axis
- `delete_elements` - Delete elements from model

**Create Elements:**
- `create_family_instance` - Place new family instance
- `duplicate_view` - Duplicate view (with/without detailing)
- `create_sheet` - Create new sheet

**Import/Export:**
- `export_schedules_to_excel` - Export schedules to Excel file
- `import_schedule_from_excel` - Import schedule data from Excel

### Utility Tools
- `raw` - Execute arbitrary GraphQL query/mutation
- `set_server` - Change GraphQL endpoint URL at runtime
- `server_info` - Get server configuration and available tools

**Pagination:** All collection tools default to limit=100, max=1000 items per request.

## Troubleshooting
- Error `program not found` (or exit code 2): Point your MCP client to `revit_mcp_server.cmd` (recommended) or use `python revit_mcp_server.py`. Avoid specifying only the `.py` filename as the command on Windows.
- Error `fastmcp package not installed`: Dependencies failed to install. Re-run `python -m pip install -r requirements.txt` (check proxy / SSL if it persists).
- Network errors: Verify the Revit add-in GraphQL server is running and reachable at `REVIT_GRAPHQL_URL` (try opening it in a browser or `Invoke-WebRequest`).
- Hanging / no responses: Confirm the client uses stdio transport (no TCP by default). Use the `server_info` tool to inspect configuration.
- Need verbose HTTP diagnostics: Temporarily set the env var `REVIT_MCP_DEBUG=1` (then add prints or logging inside `_post`).
