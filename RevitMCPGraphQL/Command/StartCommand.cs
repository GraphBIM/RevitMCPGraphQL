using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitMCPGraphQL.Server;

namespace RevitMCPGraphQL.Command;

// Shared server logic for Start/Stop commands


[Transaction(TransactionMode.Manual)]
public class StartCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        return GraphQlServerManager.Start(commandData, ref message) ? Result.Succeeded : Result.Failed;
    }
}