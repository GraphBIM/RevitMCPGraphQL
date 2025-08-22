using System.Net;
using System.Net.Sockets;
using Autodesk.Revit.UI;

namespace RevitMCPGraphQL.Server;

public static class GraphQlServerManager
{
    public static Document? Doc;
    private static HttpGraphQlServer? _server;
    private static string? _httpUrl;

    public static bool Start(ExternalCommandData commandData, ref string message)
    {
        try
        {
            RevitDispatcher.Init(commandData.Application);
            Doc = commandData.Application.ActiveUIDocument?.Document;
            if (Doc == null)
            {
                TaskDialog.Show("Error", "No active document found.");
                return false;
            }
            int port = GetAvailablePort(5000, 6000);
            _httpUrl = $"http://localhost:{port}/graphql";
            _server = new HttpGraphQlServer(() => Doc);
            _server.Start(port);
            TaskDialog.Show("GraphQL Server",
                $"Server starting on {_httpUrl}. Use HTTP POST to this URL. Health: http://localhost:{port}/");
            return true;
        }
        catch (Exception e)
        {
            message = $"Error starting GraphQL server: {e.Message}";
            TaskDialog.Show("Error", message);
            return false;
        }
    }

    public static void Stop()
    {
        if (_server != null)
        {
            _server.Stop();
            _server = null;
            TaskDialog.Show("GraphQL Server", "Server stopped.");
        }
        else
        {
            TaskDialog.Show("GraphQL Server", "Server is not running.");
        }
    }

    private static int GetAvailablePort(int start, int end)
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