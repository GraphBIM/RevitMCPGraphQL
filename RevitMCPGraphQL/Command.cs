using System.Net;
using System.Net.Sockets;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitMCPGraphQL.Server;

namespace RevitMCPGraphQL;

[Transaction(TransactionMode.Manual)]
public class Command : IExternalCommand
{
    private string? _httpUrl;
    public static Document? Doc;

    private static HttpGraphQlServer? _server;

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            // Init dispatcher for safe Revit API access
            RevitDispatcher.Init(commandData.Application);

            Doc = commandData.Application.ActiveUIDocument?.Document;
            if (Doc == null)
            {
                TaskDialog.Show("Error", "No active document found.");
                return Result.Failed;
            }

            int port = GetAvailablePort(5000, 6000);
            _httpUrl = $"http://localhost:{port}/graphql";

            // Start server
            _server = new HttpGraphQlServer(() => Doc);
            _server.Start(port);

            TaskDialog.Show("GraphQL Server",
                $"Server starting on {_httpUrl}. Use HTTP POST to this URL. Health: http://localhost:{port}/");
        }
        catch (Exception e)
        {
            message = $"Error starting GraphQL server: {e.Message}";
            TaskDialog.Show("Error", message);
            return Result.Failed;
        }

        return Result.Succeeded;
    }

    public static void StopServer()
    {
        _server?.Stop();
    }

    private int GetAvailablePort(int start, int end)
    {
        for (int p = start; p <= end; p++)
        {
            var listener = new TcpListener(IPAddress.Loopback, p);
            try
            {
                listener.Start();
                return p;
            }
            catch
            {
                // try next
            }
            finally
            {
                listener.Stop();
            }
        }

        throw new Exception("No available port found.");
    }
}