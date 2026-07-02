"""MCP Client for Revit GraphQL Server

This client demonstrates how to connect to an MCP server using the Model Context Protocol.
It can connect to the revit_mcp_server.py or any other MCP-compliant server.

Usage:
    # Connect to an MCP server via STDIO
    client = MCPClient(command="python", args=["revit_mcp_server.py"])
    
    # Or use the direct GraphQL HTTP client (legacy mode)
    client = RevitGraphQLClient(url="http://localhost:5000/graphql")
"""

import asyncio
import json
import logging
import sys
from typing import Any, Dict, List, Optional

# Configure logging to stderr (MCP requirement)
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    stream=sys.stderr  # Important: log to stderr, not stdout
)
logger = logging.getLogger(__name__)

try:
    from mcp import ClientSession, StdioServerParameters
    from mcp.client.stdio import stdio_client
    MCP_AVAILABLE = True
except ImportError:
    MCP_AVAILABLE = False
    logger.warning("MCP SDK not available. Install with: pip install mcp")

try:
    import requests
    REQUESTS_AVAILABLE = True
except ImportError:
    REQUESTS_AVAILABLE = False
    logger.warning("requests not available. Install with: pip install requests")


class MCPClient:
    """
    Model Context Protocol client for connecting to MCP servers.
    
    This client follows the MCP specification for tool-based interactions.
    """
    
    def __init__(self, command: str = "python", args: Optional[List[str]] = None):
        """
        Initialize MCP client.
        
        Args:
            command: Command to launch the MCP server (e.g., "python", "node", "uv")
            args: Arguments for the server command (e.g., ["revit_mcp_server.py"])
        """
        if not MCP_AVAILABLE:
            raise ImportError("MCP SDK not installed. Install with: pip install mcp")
        
        self.command = command
        self.args = args or ["revit_mcp_server.py"]
        self.session: Optional[ClientSession] = None
        self.exit_stack = None
        
    async def connect(self):
        """Connect to the MCP server."""
        from contextlib import AsyncExitStack
        
        self.exit_stack = AsyncExitStack()
        
        # Create server parameters
        server_params = StdioServerParameters(
            command=self.command,
            args=self.args
        )
        
        # Connect to the server
        stdio_transport = await self.exit_stack.enter_async_context(
            stdio_client(server_params)
        )
        self.stdio, self.write = stdio_transport
        self.session = await self.exit_stack.enter_async_context(
            ClientSession(self.stdio, self.write)
        )
        
        # Initialize the session
        await self.session.initialize()
        
        logger.info(f"Connected to MCP server: {self.command} {' '.join(self.args)}")
        
    async def disconnect(self):
        """Disconnect from the MCP server."""
        if self.exit_stack:
            await self.exit_stack.aclose()
            logger.info("Disconnected from MCP server")
    
    async def list_tools(self) -> List[Dict[str, Any]]:
        """List all available tools from the MCP server."""
        if not self.session:
            raise RuntimeError("Not connected. Call connect() first.")
        
        result = await self.session.list_tools()
        return [{"name": tool.name, "description": tool.description} for tool in result.tools]
    
    async def call_tool(self, tool_name: str, arguments: Optional[Dict[str, Any]] = None) -> Any:
        """
        Call a tool on the MCP server.
        
        Args:
            tool_name: Name of the tool to call
            arguments: Arguments to pass to the tool
            
        Returns:
            The tool's result
        """
        if not self.session:
            raise RuntimeError("Not connected. Call connect() first.")
        
        result = await self.session.call_tool(tool_name, arguments or {})
        
        # Extract text content from the result
        if hasattr(result, 'content') and result.content:
            content = result.content[0]
            if hasattr(content, 'text'):
                return json.loads(content.text)
        
        return result
    
    # Convenience methods that match the original MCPClient interface
    async def health(self) -> Dict[str, Any]:
        """Get health status from the Revit GraphQL server."""
        return await self.call_tool("health")
    
    async def categories(self) -> Dict[str, Any]:
        """Get list of categories."""
        return await self.call_tool("categories")
    
    async def elements(self, category_name: Optional[str] = None, limit: Optional[int] = None) -> Dict[str, Any]:
        """Get elements, optionally filtered by category."""
        args = {}
        if category_name:
            args["category_name"] = category_name
        if limit:
            args["limit"] = limit
        return await self.call_tool("elements", args)
    
    async def rooms(self) -> Dict[str, Any]:
        """Get list of rooms."""
        return await self.call_tool("rooms")
    
    async def document(self) -> Dict[str, Any]:
        """Get document information."""
        return await self.call_tool("document")
    
    async def active_view_and_selection(self) -> Dict[str, Any]:
        """Get active view and selection."""
        return await self.call_tool("active_view_and_selection")


class RevitGraphQLClient:
    """
    Legacy HTTP client for direct GraphQL queries to Revit GraphQL server.
    
    This is a simple HTTP client that doesn't use the MCP protocol.
    Use MCPClient for MCP-compliant interactions.
    """
    
    def __init__(self, url: str = "http://localhost:5000/graphql"):
        """
        Initialize the HTTP GraphQL client.
        
        Args:
            url: The GraphQL endpoint URL
        """
        if not REQUESTS_AVAILABLE:
            raise ImportError("requests not installed. Install with: pip install requests")
        
        self.url = url
        logger.info(f"Initialized GraphQL client for {url}")

    def run_query(self, query: str, variables: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        """Execute a GraphQL query."""
        payload = {"query": query}
        if variables:
            payload["variables"] = variables
        
        try:
            response = requests.post(self.url, json=payload, timeout=10)
            return response.json()
        except requests.exceptions.RequestException as e:
            logger.error(f"Request failed: {e}", file=sys.stderr)
            return {"error": str(e)}
        except Exception as e:
            logger.error(f"Unexpected error: {e}", file=sys.stderr)
            return {"error": str(e), "raw": response.text if 'response' in locals() else None}

    def health(self) -> Dict[str, Any]:
        """Get health status."""
        return self.run_query("{ health }")

    def categories(self) -> Dict[str, Any]:
        """Get categories."""
        return self.run_query("{ categories { id name } }")

    def elements(self, category_name: Optional[str] = None) -> Dict[str, Any]:
        """Get elements, optionally filtered by category."""
        if category_name:
            query = f'{{ elements(categoryName: "{category_name}") {{ id name }} }}'
        else:
            query = "{ elements { id name } }"
        return self.run_query(query)

    def rooms(self) -> Dict[str, Any]:
        """Get rooms."""
        return self.run_query("{ rooms { id name number area } }")

    def set_element_parameters_batch(self, inputs: List[Dict[str, Any]]) -> Dict[str, Any]:
        """
        Set parameters on multiple elements.
        
        Args:
            inputs: List of dicts with elementId and parameters
                   [{"elementId": 123, "parameters": [{"parameterName": "Mark", "value": "A"}]}]
        """
        mutation = '''
        mutation setElementParametersBatch($inputs: [ElementParametersInput!]!) {
            setElementParametersBatch(inputs: $inputs)
        }
        '''
        return self.run_query(mutation, {"inputs": inputs})


async def main_mcp_example():
    """Example using the MCP client."""
    logger.info("=== MCP Client Example ===")
    
    client = MCPClient(command="python", args=["revit_mcp_server.py"])
    
    try:
        await client.connect()
        
        # List available tools
        tools = await client.list_tools()
        logger.info(f"Available tools: {json.dumps(tools, indent=2)}")
        
        # Call some tools
        health = await client.health()
        print("\n[Health]", json.dumps(health, indent=2))
        
        categories = await client.categories()
        print("\n[Categories]", json.dumps(categories, indent=2))
        
        rooms = await client.rooms()
        print("\n[Rooms]", json.dumps(rooms, indent=2))
        
    except Exception as e:
        logger.error(f"Error: {e}", exc_info=True)
    finally:
        await client.disconnect()


def main_http_example():
    """Example using the legacy HTTP client."""
    logger.info("=== HTTP GraphQL Client Example ===")
    
    client = RevitGraphQLClient()
    
    print("\n[Health]", json.dumps(client.health(), indent=2))
    print("\n[Categories]", json.dumps(client.categories(), indent=2))
    print("\n[Elements (Walls)]", json.dumps(client.elements("Walls"), indent=2))
    print("\n[Rooms]", json.dumps(client.rooms(), indent=2))
    
    # Example: Batch set parameters
    batch_inputs = [
        {
            "elementId": 349315,  # Use a real element id from your model
            "parameters": [
                {"parameterName": "Mark", "value": "5555"},
                {"parameterName": "Width", "value": "1500"}
            ]
        }
    ]
    print("\n[Batch Set Parameters]", json.dumps(client.set_element_parameters_batch(batch_inputs), indent=2))


if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(description="Revit MCP/GraphQL Client")
    parser.add_argument("--mode", choices=["mcp", "http"], default="http",
                       help="Client mode: 'mcp' for MCP protocol, 'http' for direct GraphQL")
    
    args = parser.parse_args()
    
    if args.mode == "mcp":
        if not MCP_AVAILABLE:
            print("ERROR: MCP SDK not available. Install with: pip install mcp", file=sys.stderr)
            sys.exit(1)
        asyncio.run(main_mcp_example())
    else:
        if not REQUESTS_AVAILABLE:
            print("ERROR: requests not available. Install with: pip install requests", file=sys.stderr)
            sys.exit(1)
        main_http_example()
