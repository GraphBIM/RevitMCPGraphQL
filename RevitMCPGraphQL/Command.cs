using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RevitMCPGraphQL;

[Transaction(TransactionMode.Manual)]
public class Command : IExternalCommand
{
    private string? _httpUrl;
    public static Document? _doc;

    // HttpListener server state
    private static HttpListener? _listener;
    private static CancellationTokenSource? _cts;

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            // Initialize external event dispatcher (no RevitTask)
            RevitDispatcher.Init(commandData.Application);

            _doc = commandData.Application.ActiveUIDocument?.Document;
            if (_doc == null)
            {
                TaskDialog.Show("Error", "No active document found.");
                return Result.Failed;
            }

            int port = GetAvailablePort(5000, 6000);
            _httpUrl = $"http://localhost:{port}/graphql";

            // Start server on a background thread
            Task.Run(() => StartGraphQlServer(port));
            TaskDialog.Show("GraphQL Server", $"Server starting on {_httpUrl}. Use HTTP POST to this URL. Health: http://localhost:{port}/");
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
        try
        {
            _cts?.Cancel();
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
            }
        }
        catch { }
        finally
        {
            _listener = null;
            _cts = null;
        }
    }

    private void StartGraphQlServer(int port)
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");

        try
        {
            _listener.Start();
        }
        catch (HttpListenerException ex)
        {
            TaskDialog.Show("GraphQL Server Error", $"Failed to bind HttpListener on port {port}: {ex.Message}");
            return;
        }

        var schema = new Schema { Query = new RevitQueryProvider(() => _doc).GetQuery() };
        var executer = new DocumentExecuter();

        while (!token.IsCancellationRequested)
        {
            HttpListenerContext? ctx = null;
            try
            {
                ctx = _listener.GetContext();
            }
            catch (ObjectDisposedException)
            {
                break; // listener stopped
            }
            catch (Exception)
            {
                if (token.IsCancellationRequested) break;
                continue;
            }

            try
            {
                HandleRequest(ctx, schema, executer);
            }
            catch (Exception ex)
            {
                try
                {
                    ctx.Response.StatusCode = 500;
                    var payload = JsonConvert.SerializeObject(new { errors = new[] { new { message = ex.Message } } });
                    var bytes = Encoding.UTF8.GetBytes(payload);
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }
                catch { }
                finally
                {
                    try { ctx.Response.OutputStream.Close(); } catch { }
                }
            }
        }
    }

    private static void HandleRequest(HttpListenerContext ctx, ISchema schema, IDocumentExecuter executer)
    {
        var req = ctx.Request;
        var res = ctx.Response;

        // Health endpoint
        if (req.HttpMethod == "GET" && (req.Url?.AbsolutePath == "/" || req.Url?.AbsolutePath == "/health"))
        {
            var msg = "Revit GraphQL server is running. POST GraphQL to /graphql";
            var bytes = Encoding.UTF8.GetBytes(msg);
            res.ContentType = "text/plain";
            res.OutputStream.Write(bytes, 0, bytes.Length);
            res.OutputStream.Close();
            return;
        }

        if (req.Url?.AbsolutePath != "/graphql")
        {
            res.StatusCode = 404;
            res.OutputStream.Close();
            return;
        }

        if (req.HttpMethod == "GET")
        {
            var msg = "GraphQL endpoint. Send HTTP POST with JSON body: { query: \"...\" }";
            var bytes = Encoding.UTF8.GetBytes(msg);
            res.ContentType = "text/plain";
            res.OutputStream.Write(bytes, 0, bytes.Length);
            res.OutputStream.Close();
            return;
        }

        if (req.HttpMethod != "POST")
        {
            res.StatusCode = 405;
            res.OutputStream.Close();
            return;
        }

        using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
        var body = reader.ReadToEnd();
        var request = JsonConvert.DeserializeObject<GraphQLHttpRequest>(body) ?? new GraphQLHttpRequest();

        var result = executer.ExecuteAsync(options =>
        {
            options.Schema = schema;
            options.Query = request.Query ?? string.Empty;
        }).GetAwaiter().GetResult();

        var json = JsonConvert.SerializeObject(result);
        var outBytes = Encoding.UTF8.GetBytes(json);
        res.ContentType = "application/json";
        res.OutputStream.Write(outBytes, 0, outBytes.Length);
        res.OutputStream.Close();
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
                // Port is in use, try next
            }
            finally
            {
                listener.Stop();
            }
        }

        throw new Exception("No available port found.");
    }
}

internal class GraphQLHttpRequest
{
    public string? Query { get; set; }
    public JObject? Variables { get; set; }
}

// Synchronous resolvers (no RevitTask)
internal class RevitQueryProvider
{
    private readonly Func<Document?> _getDoc;
    public RevitQueryProvider(Func<Document?> getDoc) { _getDoc = getDoc; }

    public IObjectGraphType GetQuery()
    {
        var query = new ObjectGraphType();

        // Simple health check query
        query.Field<StringGraphType>("health").Resolve(_ => "ok");

        query.Field<ListGraphType<CategoryType>>("categories").Resolve(context =>
        {
            return RevitDispatcher.Invoke(() =>
            {
                var doc = _getDoc();
                if (doc == null) return new List<CategoryDto>();
                return doc.Settings.Categories
                    .Cast<Category>()
                    .Where(c => c != null && !string.IsNullOrEmpty(c.Name))
                    .Select(c => new CategoryDto { Name = c.Name, Id = c.Id?.IntegerValue ?? 0 })
                    .ToList();
            });
        });

        query.Field<ListGraphType<ElementType>>("elements")
            .Arguments(new QueryArguments(new QueryArgument<StringGraphType> { Name = "categoryName" }))
            .Resolve(context =>
            {
                var categoryName = context.GetArgument<string>("categoryName");
                return RevitDispatcher.Invoke(() =>
                {
                    var doc = _getDoc();
                    if (doc == null) return new List<ElementDto>();
                    var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        var category = doc.Settings.Categories.Cast<Category>().FirstOrDefault(c => c.Name == categoryName);
                        if (category != null)
                        {
                            try { collector = collector.OfCategory((BuiltInCategory)category.Id.IntegerValue); } catch { }
                        }
                    }
                    return collector.ToElements()
                        .Cast<Element>()
                        .Select(e => new ElementDto { Id = e.Id?.IntegerValue ?? 0, Name = e.Name })
                        .ToList();
                });
            });

        query.Field<ListGraphType<RoomType>>("rooms").Resolve(context =>
        {
            return RevitDispatcher.Invoke(() =>
            {
                var doc = _getDoc();
                if (doc == null) return new List<RoomDto>();
                return new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<SpatialElement>()
                    .Select(r => new RoomDto {
                        Id = r.Id?.IntegerValue ?? 0,
                        Name = r.Name,
                        Number = r.Number,
                        Area = r.Area
                    })
                    .ToList();
            });
        });

        return query;
    }
}

// DTOs to avoid exposing Revit types directly
public sealed class CategoryDto { public string Name { get; set; } = string.Empty; public int Id { get; set; } }
public sealed class ElementDto { public int Id { get; set; } public string? Name { get; set; } }
public sealed class RoomDto { public long Id { get; set; } public string? Name { get; set; } public string? Number { get; set; } public double Area { get; set; } }

// GraphQL types over DTOs (no ElementId leakage)
public class CategoryType : ObjectGraphType<CategoryDto>
{
    public CategoryType()
    {
        Field(x => x.Name).Description("Category name");
        Field(x => x.Id).Description("Category ID");
    }
}

public class ElementType : ObjectGraphType<ElementDto>
{
    public ElementType()
    {
        Field(x => x.Id).Description("Element ID");
        Field(x => x.Name, nullable: true).Description("Element name");
    }
}

public class RoomType : ObjectGraphType<RoomDto>
{
    public RoomType()
    {
        Field(x => x.Id).Description("Room ID");
        Field(x => x.Name, nullable: true).Description("Room name");
        Field(x => x.Number, nullable: true).Description("Room number");
        Field(x => x.Area).Description("Room area");
    }
}

// ParameterType can remain as-is or be removed if not exposed in queries
public class ParameterType : ObjectGraphType<Parameter>
{
    public ParameterType()
    {
        Field<StringGraphType>("name").Resolve(context => context.Source.Definition.Name);
        Field<StringGraphType>("value").Resolve(context => context.Source.AsValueString());
    }
}

internal static class RevitDispatcher
{
    private static ExternalEvent? _extEvent;
    private static RevitExternalEventHandler? _handler;
    private static bool _initialized;

    public static void Init(UIApplication uiApp)
    {
        if (_initialized) return;
        _handler = new RevitExternalEventHandler(uiApp);
        _extEvent = ExternalEvent.Create(_handler);
        _initialized = true;
    }

    public static T Invoke<T>(Func<T> func)
    {
        if (!_initialized || _extEvent == null || _handler == null)
            throw new InvalidOperationException("RevitDispatcher not initialized.");

        var tcs = new TaskCompletionSource<T>();
        _handler.Enqueue(() =>
        {
            try { tcs.SetResult(func()); }
            catch (Exception ex) { tcs.SetException(ex); }
        });
        _extEvent.Raise();
        return tcs.Task.GetAwaiter().GetResult();
    }
}

internal sealed class RevitExternalEventHandler : IExternalEventHandler
{
    private readonly UIApplication _uiApp;
    private readonly ConcurrentQueue<Action> _queue = new();

    public RevitExternalEventHandler(UIApplication uiApp)
    {
        _uiApp = uiApp;
    }

    public void Enqueue(Action action) => _queue.Enqueue(action);

    public void Execute(UIApplication app)
    {
        if (_queue.TryDequeue(out var action))
        {
            action();
        }
    }

    public string GetName() => "Revit GraphQL ExternalEvent Handler";
}
