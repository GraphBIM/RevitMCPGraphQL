"""FastMCP server that proxies the local Revit GraphQL endpoint.

This MCP server exposes comprehensive Revit BIM data and operations through tools:

QUERY TOOLS (with pagination support):
  - Model Info: health, model_health, document, project_info, project_location, units
  - Elements: elements, element_types, elements_by_id, elements_by_category, elements_in_bounding_box
  - Element Relationships: element_relationship, active_view_and_selection
  - Spatial: rooms, levels, coordinates
  - Views & Sheets: views, sheets, schedules, grids
  - Families: families, family_types, family_instances
  - Categories & Materials: categories, materials
  - Project Management: worksets, phases, design_options, links, warnings

MUTATION TOOLS:
  - Modify Elements: set_element_parameter, set_element_parameters_batch, set_element_type
  - Transform Elements: move_elements, rotate_elements, delete_elements
  - Create Elements: create_family_instance, duplicate_view, create_sheet
  - Import/Export: export_schedules_to_excel, import_schedule_from_excel

UTILITY TOOLS:
  - raw: Execute arbitrary GraphQL queries/mutations
  - set_server: Change GraphQL endpoint at runtime
  - server_info: Get server configuration

All collection-returning tools support pagination:
- Default limit: 100 items per request
- Maximum limit: 1000 items per request
- Default offset: 0

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
async def categories(offset: int = 0, limit: int = 100) -> Any:
    """Return list of categories (id, name) with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)  # Cap at 1000
    q = "query($offset:Int,$limit:Int){ categories(offset:$offset, limit:$limit){ id name } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def elements(category_name: Optional[str] = None, offset: int = 0, limit: int = 100) -> Any:
    """Return elements with pagination; optionally filter by category name.
    
    Args:
        category_name: Optional category filter (e.g., 'Walls', 'Doors')
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)  # Cap at 1000
    if category_name is not None:
        q = "query($name:String,$offset:Int,$limit:Int){ elements(categoryName:$name, offset:$offset, limit:$limit){ id name } }"
        return _post(q, {"name": category_name, "offset": offset, "limit": limit})
    q = "query($offset:Int,$limit:Int){ elements(offset:$offset, limit:$limit){ id name } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def rooms(offset: int = 0, limit: int = 100) -> Any:
    """Return rooms list with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)  # Cap at 1000
    q = "query($offset:Int,$limit:Int){ rooms(offset:$offset, limit:$limit){ id name number area } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def active_view_and_selection() -> Any:
    """Return active view and current selection ids."""
    return _post("{ activeViewAndSelection { activeView { id name } selectionIds } }")


@mcp.tool()
async def materials(offset: int = 0, limit: int = 100) -> Any:
    """Return materials list with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)  # Cap at 1000
    q = "query($offset:Int,$limit:Int){ materials(offset:$offset, limit:$limit){ id name } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def worksets(offset: int = 0, limit: int = 100) -> Any:
    """Return worksets list with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)  # Cap at 1000
    q = "query($offset:Int,$limit:Int){ worksets(offset:$offset, limit:$limit){ id name } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def phases(offset: int = 0, limit: int = 100) -> Any:
    """Return phases list with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)  # Cap at 1000
    q = "query($offset:Int,$limit:Int){ phases(offset:$offset, limit:$limit){ id name } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def units() -> Any:
    """Return unit specs and symbols."""
    return _post("{ units { typeId name symbol } }")


@mcp.tool()
async def elements_by_category(category: str, offset: int = 0, limit: int = 100) -> Any:
    """Return elements filtered by BuiltInCategory enum name with pagination.
    
    Args:
        category: BuiltInCategory enum name (e.g., 'Walls', 'Doors', 'Windows')
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)  # Cap at 1000
    q = (
        "query($category:BuiltInCategoryEnum!,$offset:Int,$limit:Int){ "
        "elementsByCategory(category:$category, offset:$offset, limit:$limit){ id name } }"
    )
    return _post(q, {"category": category, "offset": offset, "limit": limit})


@mcp.tool()
async def elements_in_bounding_box(
    minX: float, minY: float, minZ: float, maxX: float, maxY: float, maxZ: float,
    offset: int = 0, limit: int = 100,
) -> Any:
    """Return elements whose bounding boxes intersect the given outline, with pagination.
    
    Args:
        minX, minY, minZ: Minimum corner coordinates
        maxX, maxY, maxZ: Maximum corner coordinates
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)  # Cap at 1000
    q = (
        "query($minX:Float!,$minY:Float!,$minZ:Float!,$maxX:Float!,$maxY:Float!,$maxZ:Float!,$offset:Int,$limit:Int){ "
        "elementsInBoundingBox(minX:$minX,minY:$minY,minZ:$minZ,maxX:$maxX,maxY:$maxY,maxZ:$maxZ, offset:$offset, limit:$limit){ id name } }"
    )
    return _post(q, {
        "minX": minX, "minY": minY, "minZ": minZ,
        "maxX": maxX, "maxY": maxY, "maxZ": maxZ,
        "offset": offset, "limit": limit,
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


# ========== Additional Query Tools ==========

@mcp.tool()
async def model_health() -> Any:
    """Return model health metrics and statistics."""
    return _post("{ modelHealth { totalElements warningCount errorCount } }")


@mcp.tool()
async def element_types(category_name: Optional[str] = None, offset: int = 0, limit: int = 100) -> Any:
    """Return element types with pagination; optionally filter by category.
    
    Args:
        category_name: Optional category filter
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    if category_name:
        q = "query($name:String,$offset:Int,$limit:Int){ elementTypes(categoryName:$name, offset:$offset, limit:$limit){ id name familyName } }"
        return _post(q, {"name": category_name, "offset": offset, "limit": limit})
    q = "query($offset:Int,$limit:Int){ elementTypes(offset:$offset, limit:$limit){ id name familyName } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def family_types(family_name: Optional[str] = None, offset: int = 0, limit: int = 100) -> Any:
    """Return family types (symbols) with pagination.
    
    Args:
        family_name: Optional family name filter
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    if family_name:
        q = "query($name:String,$offset:Int,$limit:Int){ familyTypes(familyName:$name, offset:$offset, limit:$limit){ id name familyName } }"
        return _post(q, {"name": family_name, "offset": offset, "limit": limit})
    q = "query($offset:Int,$limit:Int){ familyTypes(offset:$offset, limit:$limit){ id name familyName } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def levels(offset: int = 0, limit: int = 100, document_id: Optional[int] = None) -> Any:
    """Return levels with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
        document_id: Optional RevitLinkInstance element id for linked documents
    """
    limit = min(limit, 1000)
    q = "query($offset:Int,$limit:Int,$docId:ID){ levels(offset:$offset, limit:$limit, documentId:$docId){ id name elevation } }"
    return _post(q, {"offset": offset, "limit": limit, "docId": document_id})


@mcp.tool()
async def views(view_type: Optional[str] = None, include_templates: bool = False, offset: int = 0, limit: int = 100) -> Any:
    """Return views with pagination.
    
    Args:
        view_type: Optional view type filter (e.g., 'FloorPlan', 'Section', '3D')
        include_templates: Include view templates (default: False)
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($type:String,$templates:Boolean,$offset:Int,$limit:Int){ views(viewType:$type, includeTemplates:$templates, offset:$offset, limit:$limit){ id name viewType isTemplate } }"
    return _post(q, {"type": view_type, "templates": include_templates, "offset": offset, "limit": limit})


@mcp.tool()
async def families(category_name: Optional[str] = None, offset: int = 0, limit: int = 100) -> Any:
    """Return families with pagination.
    
    Args:
        category_name: Optional category filter
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($name:String,$offset:Int,$limit:Int){ families(categoryName:$name, offset:$offset, limit:$limit){ id name categoryName } }"
    return _post(q, {"name": category_name, "offset": offset, "limit": limit})


@mcp.tool()
async def family_instances(family_name: Optional[str] = None, offset: int = 0, limit: int = 100) -> Any:
    """Return family instances with pagination.
    
    Args:
        family_name: Optional family name filter
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($name:String,$offset:Int,$limit:Int){ familyInstances(familyName:$name, offset:$offset, limit:$limit){ id name familyName symbolName } }"
    return _post(q, {"name": family_name, "offset": offset, "limit": limit})


@mcp.tool()
async def project_info() -> Any:
    """Return project information."""
    return _post("{ projectInfo { name number address author organizationName } }")


@mcp.tool()
async def elements_by_id(element_ids: List[int]) -> Any:
    """Return elements by their IDs.
    
    Args:
        element_ids: List of element IDs to retrieve
    """
    q = "query($ids:[ID!]!){ elementsById(elementIds:$ids){ id name categoryName } }"
    return _post(q, {"ids": element_ids})


@mcp.tool()
async def design_options(offset: int = 0, limit: int = 100) -> Any:
    """Return design options with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($offset:Int,$limit:Int){ designOptions(offset:$offset, limit:$limit){ id name isPrimary } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def links(offset: int = 0, limit: int = 100) -> Any:
    """Return Revit link instances with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($offset:Int,$limit:Int){ links(offset:$offset, limit:$limit){ id name linkType } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def sheets(offset: int = 0, limit: int = 100) -> Any:
    """Return sheets with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($offset:Int,$limit:Int){ sheets(offset:$offset, limit:$limit){ id sheetNumber name placedViews } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def schedules(offset: int = 0, limit: int = 100) -> Any:
    """Return schedules with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($offset:Int,$limit:Int){ schedules(offset:$offset, limit:$limit){ id name } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def grids(offset: int = 0, limit: int = 100) -> Any:
    """Return grids with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($offset:Int,$limit:Int){ grids(offset:$offset, limit:$limit){ id name } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def project_location() -> Any:
    """Return project location information."""
    return _post("{ projectLocation { name latitude longitude elevation timeZone } }")


@mcp.tool()
async def warnings(offset: int = 0, limit: int = 100) -> Any:
    """Return model warnings with pagination.
    
    Args:
        offset: Number of items to skip (default: 0)
        limit: Maximum number of items to return (default: 100, max: 1000)
    """
    limit = min(limit, 1000)
    q = "query($offset:Int,$limit:Int){ warnings(offset:$offset, limit:$limit){ description severity elementIds } }"
    return _post(q, {"offset": offset, "limit": limit})


@mcp.tool()
async def element_relationship(element_id: int) -> Any:
    """Return relationships for an element (host, dependencies, etc.).
    
    Args:
        element_id: Element ID to get relationships for
    """
    q = "query($id:ID!){ elementRelationship(elementId:$id){ hostId dependentIds } }"
    return _post(q, {"id": element_id})


@mcp.tool()
async def coordinates(x: float, y: float, z: float, from_coord: str = "project", to_coord: str = "shared") -> Any:
    """Convert coordinates between different coordinate systems.
    
    Args:
        x, y, z: Coordinate values
        from_coord: Source coordinate system ('project' or 'shared')
        to_coord: Target coordinate system ('project' or 'shared')
    """
    q = "query($x:Float!,$y:Float!,$z:Float!,$from:String!,$to:String!){ coordinates(x:$x, y:$y, z:$z, from:$from, to:$to){ x y z } }"
    return _post(q, {"x": x, "y": y, "z": z, "from": from_coord, "to": to_coord})


# ========== Additional Mutation Tools ==========

@mcp.tool()
async def export_schedules_to_excel(schedule_ids: List[int], output_path: str) -> Any:
    """Export schedules to Excel file.
    
    Args:
        schedule_ids: List of schedule element IDs to export
        output_path: Full path where Excel file will be saved
    """
    q = "mutation($ids:[ID!]!,$path:String!){ exportSchedulesToExcel(scheduleIds:$ids, outputPath:$path) }"
    return _post(q, {"ids": schedule_ids, "path": output_path})


@mcp.tool()
async def import_schedule_from_excel(schedule_id: int, excel_path: str) -> Any:
    """Import schedule data from Excel file.
    
    Args:
        schedule_id: Schedule element ID to import into
        excel_path: Full path to Excel file
    """
    q = "mutation($id:ID!,$path:String!){ importScheduleFromExcel(scheduleId:$id, excelPath:$path) }"
    return _post(q, {"id": schedule_id, "path": excel_path})


# ========== Utility Tools ==========

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
            # Query tools
            "health", "model_health", "document", "project_info", "project_location",
            "categories", "elements", "element_types", "elements_by_id", "elements_by_category", 
            "elements_in_bounding_box", "element_relationship",
            "rooms", "levels", "views", "sheets", "schedules", "grids",
            "families", "family_types", "family_instances",
            "materials", "worksets", "phases", "design_options", "links", "warnings",
            "units", "coordinates", "active_view_and_selection",
            # Mutation tools
            "set_element_parameter", "set_element_parameters_batch", "set_element_type",
            "move_elements", "rotate_elements", "delete_elements",
            "create_family_instance", "duplicate_view", "create_sheet",
            "export_schedules_to_excel", "import_schedule_from_excel",
            # Utility tools
            "raw", "set_server", "server_info",
        ],
    }


if __name__ == "__main__":
    log.info("Starting Revit GraphQL MCP bridge at %s", GRAPHQL_URL)
    mcp.run(transport="stdio")
