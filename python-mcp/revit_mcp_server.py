"""FastMCP server that proxies the local Revit GraphQL endpoint.

Expose tools mirroring the Python MCPClient functionality plus a few helpers.
Environment variables:
  REVIT_GRAPHQL_URL (default: http://localhost:5000/graphql)
  REVIT_GRAPHQL_TIMEOUT (seconds, default: 10)
"""

import os
import sys
import json
from typing import Any, Dict, List, Optional
import logging
import requests

try:
    from mcp.server.fastmcp import FastMCP
except ImportError:
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
    return _post("{ document { title pathName isFamilyDocument } }")


@mcp.tool()
async def categories() -> Any:
    """Return list of categories (id, name)."""
    return _post("{ categories { id name } }")


@mcp.tool()
async def elements(category_name: Optional[str] = None, limit: Optional[int] = None) -> Any:
    """Return elements; optionally filter by category name and limit."""
    if category_name is not None:
        q = "query($name:String,$limit:Int){ elements(categoryName:$name, limit:$limit){ id name } }"
        return _post(q, {"name": category_name, "limit": limit})
    q = "query($limit:Int){ elements(limit:$limit){ id name } }"
    return _post(q, {"limit": limit})


@mcp.tool()
async def rooms() -> Any:
    """Return rooms list."""
    return _post("{ rooms { id name number area } }")


@mcp.tool()
async def active_view_and_selection() -> Any:
    """Return active view and current selection ids."""
    return _post("{ activeViewAndSelection { activeView { id name } selectionIds } }")


@mcp.tool()
async def materials(limit: Optional[int] = None) -> Any:
    q = "query($limit:Int){ materials(limit:$limit){ id name } }"
    return _post(q, {"limit": limit})


@mcp.tool()
async def worksets(limit: Optional[int] = None) -> Any:
    q = "query($limit:Int){ worksets(limit:$limit){ id name } }"
    return _post(q, {"limit": limit})


@mcp.tool()
async def phases(limit: Optional[int] = None) -> Any:
    q = "query($limit:Int){ phases(limit:$limit){ id name } }"
    return _post(q, {"limit": limit})


@mcp.tool()
async def units() -> Any:
    """Return unit specs and symbols."""
    return _post("{ units { typeId name symbol } }")


@mcp.tool()
async def elements_by_category(category: str, limit: Optional[int] = None) -> Any:
    """Return elements filtered by BuiltInCategory enum name (e.g., 'Walls')."""
    q = (
        "query($category:BuiltInCategoryEnum!,$limit:Int){ "
        "elementsByCategory(category:$category, limit:$limit){ id name } }"
    )
    return _post(q, {"category": category, "limit": limit})


@mcp.tool()
async def elements_in_bounding_box(
    minX: float, minY: float, minZ: float, maxX: float, maxY: float, maxZ: float,
    limit: Optional[int] = None,
) -> Any:
    """Return elements whose bounding boxes intersect the given outline."""
    q = (
        "query($minX:Float!,$minY:Float!,$minZ:Float!,$maxX:Float!,$maxY:Float!,$maxZ:Float!,$limit:Int){ "
        "elementsInBoundingBox(minX:$minX,minY:$minY,minZ:$minZ,maxX:$maxX,maxY:$maxY,maxZ:$maxZ, limit:$limit){ id name } }"
    )
    return _post(q, {
        "minX": minX, "minY": minY, "minZ": minZ,
        "maxX": maxX, "maxY": maxY, "maxZ": maxZ,
        "limit": limit,
    })


@mcp.tool()
async def set_element_parameter(element_id: int, parameter_name: str, value: str) -> Any:
    """Set one parameter on an element by name (string value)."""
    q = (
        "mutation($elementId:ID!,$parameterName:String!,$value:String!){ "
        "setElementParameter(elementId:$elementId, parameterName:$parameterName, value:$value) }"
    )
    return _post(q, {"elementId": element_id, "parameterName": parameter_name, "value": value})


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
async def set_element_type(element_id: int, type_id: int) -> Any:
    q = (
        "mutation($elementId:ID!,$typeId:ID!){ "
        "setElementType(elementId:$elementId, typeId:$typeId) }"
    )
    return _post(q, {"elementId": element_id, "typeId": type_id})


@mcp.tool()
async def move_elements(element_ids: List[int], x: float, y: float, z: float) -> Any:
    q = (
        "mutation($ids:[ID!]!,$t:VectorInput!){ "
        "moveElements(elementIds:$ids, translation:$t) }"
    )
    return _post(q, {"ids": element_ids, "t": {"x": x, "y": y, "z": z}})


@mcp.tool()
async def rotate_elements(
    element_ids: List[int],
    px: float, py: float, pz: float,
    ax: float, ay: float, az: float,
    angle: float,
) -> Any:
    q = (
        "mutation($ids:[ID!]!,$p:PointInput!,$a:VectorInput!,$angle:Float!){ "
        "rotateElements(elementIds:$ids, point:$p, axis:$a, angle:$angle) }"
    )
    return _post(q, {
        "ids": element_ids,
        "p": {"x": px, "y": py, "z": pz},
        "a": {"x": ax, "y": ay, "z": az},
        "angle": angle,
    })


@mcp.tool()
async def delete_elements(element_ids: List[int]) -> Any:
    q = "mutation($ids:[ID!]!){ deleteElements(elementIds:$ids) }"
    return _post(q, {"ids": element_ids})


@mcp.tool()
async def create_family_instance(
    symbol_id: int, x: float, y: float, z: float,
    level_id: Optional[int] = None, structural_type: Optional[str] = None,
) -> Any:
    q = (
        "mutation($symbolId:ID!,$loc:PointInput!,$levelId:ID,$struct:String){ "
        "createFamilyInstance(symbolId:$symbolId, location:$loc, levelId:$levelId, structuralType:$struct) }"
    )
    vars = {"symbolId": symbol_id, "loc": {"x": x, "y": y, "z": z}}
    if level_id is not None:
        vars["levelId"] = level_id
    if structural_type is not None:
        vars["struct"] = structural_type
    return _post(q, vars)


@mcp.tool()
async def duplicate_view(view_id: int, with_detailing: bool = False) -> Any:
    q = "mutation($id:ID!,$d:Boolean){ duplicateView(viewId:$id, withDetailing:$d) }"
    return _post(q, {"id": view_id, "d": with_detailing})


@mcp.tool()
async def create_sheet(
    title_block_type_id: Optional[int] = None,
    sheet_number: Optional[str] = None,
    sheet_name: Optional[str] = None,
) -> Any:
    q = (
        "mutation($tb:ID,$no:String,$name:String){ "
        "createSheet(titleBlockTypeId:$tb, sheetNumber:$no, sheetName:$name) }"
    )
    return _post(q, {"tb": title_block_type_id, "no": sheet_number, "name": sheet_name})


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
async def set_server(url: str) -> Dict[str, Any]:
    """Update the GraphQL endpoint URL for this MCP server at runtime."""
    global GRAPHQL_URL
    GRAPHQL_URL = url
    return {"ok": True, "graphqlUrl": GRAPHQL_URL}


@mcp.tool()
async def server_info() -> Dict[str, Any]:
    """Return server configuration info."""
    return {
        "graphqlUrl": GRAPHQL_URL,
        "timeout": DEFAULT_TIMEOUT,
        "tools": [
            "health", "document", "categories", "elements", "rooms",
            "materials", "worksets", "phases", "units",
            "elements_by_category", "elements_in_bounding_box", "active_view_and_selection",
            "set_element_parameter", "set_element_parameters_batch", "set_element_type",
            "move_elements", "rotate_elements", "delete_elements",
            "create_family_instance", "duplicate_view", "create_sheet",
            "set_server", "raw", "server_info",
        ],
    }


if __name__ == "__main__":
    log.info("Starting Revit GraphQL MCP bridge at %s", GRAPHQL_URL)
    mcp.run(transport="stdio")
