# RevitMCPGraphQL

A Revit add-in that exposes a GraphQL API for querying Revit data. Built with .NET 8 and ASP.NET Core, this project allows you to interact with Autodesk Revit models using GraphQL queries.

## Features
- Host a GraphQL server inside Revit
- Query Revit elements, categories, and parameters
- Playground UI for testing queries

## Requirements
- Autodesk Revit 2025 or newer
- .NET 8 SDK
- Visual Studio, JetBrains Rider, or compatible IDE

## Getting Started
1. **Clone the repository**
   ```sh
   git clone <your-repo-url>
   ```
2. **Open the solution** in your IDE.
3. **Restore NuGet packages**
   ```sh
   dotnet restore
   ```
4. **Build the solution**
   ```sh
   dotnet build
   ```
5. **Deploy the add-in**
   - The build process copies the add-in files to the Revit Addins folder.
   - Start Revit and the add-in will be available.

## Usage
- When Revit starts, the add-in will launch a GraphQL server on a local port.
- Use the provided URL to access the GraphQL Playground and run queries.

## Project Structure
- `RevitMCPGraphQL/` - Main add-in source code
- `Commands/` - Revit external commands
- `Resources/` - Icons and resources

## License
MIT License
