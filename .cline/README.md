# Cline MCP Configuration

This directory contains MCP (Model Context Protocol) server configuration for Cline.

## Configuration File

**`mcp.json`** - Defines MCP servers available to Cline

## Current Configuration

### revit-graphql Server

A local MCP server that provides access to Revit BIM data through GraphQL.

**Transport:** STDIO (local process)  
**Command:** `python revit_mcp_server.py`  
**Location:** `c:\Users\chuon\RiderProjects\RevitMCPGraphQL\python-mcp\`

#### Environment Variables
- `REVIT_GRAPHQL_URL`: GraphQL endpoint (default: http://localhost:5000/graphql)
- `REVIT_GRAPHQL_TIMEOUT`: Request timeout in seconds (default: 10)

#### Auto-Approved Tools
Safe read-only tools are auto-approved for faster workflow:
- health, model_health, document, project_info
- categories, elements, rooms, levels, views
- materials, families, sheets
- active_view_and_selection, server_info

#### All Available Tools
**Query Tools (40+ total):**
- Model & project information
- Elements and element types
- Spatial data (rooms, levels, coordinates)
- Views, sheets, schedules
- Families and instances
- Materials, categories
- Worksets, phases, design options
- Warnings and relationships

**Mutation Tools:**
- Modify, move, rotate, delete elements
- Create family instances, views, sheets
- Import/export schedules

See [RevitMCPGraphQL python-mcp README](c:\Users\chuon\RiderProjects\RevitMCPGraphQL\python-mcp\README.md) for complete tool documentation.

## Usage

1. **Start Revit** with the RevitMCPGraphQL add-in loaded
2. **Open Cline** in VS Code
3. Click the **MCP Servers** icon in Cline's toolbar
4. Verify the "revit-graphql" server is enabled
5. Ask Cline to query Revit data (e.g., "Show me all walls in the model")

## Troubleshooting

**Server won't connect:**
- Ensure Revit is running with the GraphQL add-in active
- Verify the GraphQL endpoint is accessible at http://localhost:5000/graphql
- Check Python and dependencies are installed: `pip install -r requirements.txt`

**Missing tools:**
- Restart the MCP server from Cline's MCP Servers panel
- Check server logs for errors

**Authentication errors:**
- The Revit GraphQL server runs locally and doesn't require authentication
- Verify the REVIT_GRAPHQL_URL environment variable is correct

## Security

- Only read-only query tools are auto-approved
- Mutation tools (modify, delete, create) require manual approval
- All data access is local to your machine
- No external API keys or credentials needed

## Configuration Format

Based on Cline MCP documentation: https://docs.cline.bot/mcp/mcp-overview

```json
{
  "mcpServers": {
    "server-name": {
      "command": "executable",
      "args": ["path/to/script"],
      "env": { "VAR": "value" },
      "disabled": false,
      "autoApprove": ["tool1", "tool2"]
    }
  }
}
```
