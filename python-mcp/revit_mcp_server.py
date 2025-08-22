"""FastMCP server that proxies the local Revit GraphQL endpoint.

Expose tools mirroring the Python MCPClient functionality plus a few helpers.
Environment variables:
  REVIT_GRAPHQL_URL (default: http://localhost:5000/graphql)
"""

import os
import sys
import json
from typing import Any, Dict, List, Optional
import logging
import requests

try:
    from mcp.server.fastmcp import FastMCP
except ImportError as e:
    print("[ERROR] fastmcp package not installed. Install dependencies first (pip install -r requirements.txt)", file=sys.stderr)
    raise

log = logging.getLogger("revit_graphql_mcp")
logging.basicConfig(level=logging.INFO)

mcp = FastMCP("revit_graphql")

GRAPHQL_URL = os.environ.get("REVIT_GRAPHQL_URL", "http://localhost:5000/graphql")
DEFAULT_TIMEOUT = float(os.environ.get("REVIT_GRAPHQL_TIMEOUT", "10"))

def _post(query: str, variables: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
    payload: Dict[str, Any] = {"query": query}
    if variables is not None:
        payload["variables"] = variables
    try:
        r = requests.post(GRAPHQL_URL, json=payload, timeout=DEFAULT_TIMEOUT)
    except Exception as ex:  # network error
        return {"error": f"Network error: {ex}"}
    try:
        data = r.json()
    except ValueError:
        return {"error": "Invalid JSON response", "status": r.status_code, "text": r.text[:500]}
    if r.status_code != 200:
        data.setdefault("httpStatus", r.status_code)
    return data

@mcp.tool()
async def health() -> Any:
    """Return health check result from Revit GraphQL server."""
    return _post("{ health }")

@mcp.tool()
async def document() -> Any:
    """Return document info."""
    return _post("{ document { title pathName isFamilyDocument  } }")

@mcp.tool()
async def categories() -> Any:
    """Return list of categories (id, name)."""
    return _post("{ categories { id name } }")

@mcp.tool()
async def elements(category_name: Optional[str] = None) -> Any:
    """Return elements; optionally filter by category name."""
    if category_name:
        q = f"{{ elements(categoryName: \"{category_name}\") {{ id name }} }}"
    else:
        q = "{ elements { id name } }"
    return _post(q)

@mcp.tool()
async def rooms() -> Any:
    """Return rooms list."""
    return _post("{ rooms { id name number area } }")

@mcp.tool()
async def set_element_parameters_batch(inputs: List[Dict[str, Any]]) -> Any:
    """Batch set parameters on elements.
    inputs: [{ elementId: <long>, parameters: [{ parameterName, value }] }]
    Returns GraphQL execution result.
    """
    mutation = (
        "mutation SetElementParameters($inputs: [ElementParametersInput!]!) { "
        "setElementParametersBatch(inputs: $inputs) }"
    )
    return _post(mutation, {"inputs": inputs})

@mcp.tool()
async def raw(query: str, variables_json: Optional[str] = None) -> Any:
    """Run an arbitrary GraphQL query/mutation. variables_json should be a JSON object string."""
    vars_obj = None
    if variables_json:
        try:
            vars_obj = json.loads(variables_json)
        except Exception as ex:
            return {"error": f"Invalid variables_json: {ex}"}
    return _post(query, vars_obj)

@mcp.tool()
async def server_info() -> Dict[str, Any]:
    """Return server configuration info."""
    return {
        "graphqlUrl": GRAPHQL_URL,
        "timeout": DEFAULT_TIMEOUT,
        "tools": [
            "health", "categories", "elements", "rooms",
            "set_element_parameters_batch", "raw", "server_info"
        ]
    }

if __name__ == "__main__":
    log.info("Starting Revit GraphQL MCP bridge at %s", GRAPHQL_URL)
    mcp.run(transport="stdio")
