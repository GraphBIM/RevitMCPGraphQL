using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitMCPGraphQL.Server;

namespace RevitMCPGraphQL.Command
{
    [Transaction(TransactionMode.Manual)]
    public class StopCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            GraphQlServerManager.Stop();
            return Result.Succeeded;
        }
    }
}
