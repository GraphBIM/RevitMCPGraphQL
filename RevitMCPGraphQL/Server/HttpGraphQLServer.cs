using System.IO;
using System.Net;
using System.Text;
using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RevitMCPGraphQL.GraphQL;

namespace RevitMCPGraphQL.Server;

public sealed class HttpGraphQlServer
{
    private readonly Func<Autodesk.Revit.DB.Document?> _getDoc;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;

    public HttpGraphQlServer(Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        _getDoc = getDoc;
    }

    public void Start(int port)
    {
        // run in background thread
        Task.Run(() => Run(port));
    }

    public void Stop()
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
        catch
        {
        }
        finally
        {
            _listener = null;
            _cts = null;
        }
    }

    private void Run(int port)
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
        catch
        {
            return;
        }

        var schema = new Schema
        {
            Query = new RevitQueryProvider(_getDoc).GetQuery(),
            Mutation = new RevitExecuteProvider(_getDoc)
        };
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
                break;
            }
            catch
            {
                if (token.IsCancellationRequested) break;
                continue;
            }

            try
            {
                HandleRequest(ctx, schema, executer);
            }
            catch
            {
                try
                {
                    ctx.Response.StatusCode = 500;
                    ctx.Response.OutputStream.Close();
                }
                catch
                {
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
        var request = JsonConvert.DeserializeObject<GraphQlHttpRequest>(body) ?? new GraphQlHttpRequest();
        var result = executer.ExecuteAsync(options =>
        {
            options.Schema = schema;
            options.Query = request.Query ?? string.Empty;
            if (request.Variables != null)
            {
                var native = (Dictionary<string, object?>)ToNative(request.Variables)!;
                options.Variables = new Inputs(native);
            }
        }).GetAwaiter().GetResult();

        var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Include,
            Formatting = Newtonsoft.Json.Formatting.None
        };
        jsonSettings.Converters.Add(new ExecutionResultJsonConverter());
        var json = JsonConvert.SerializeObject(result, jsonSettings);
        var outBytes = Encoding.UTF8.GetBytes(json);
        res.ContentType = "application/json";
        res.OutputStream.Write(outBytes, 0, outBytes.Length);
        res.OutputStream.Close();
    }
    private static object? ToNative(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Object => token.Children<JProperty>()
                .ToDictionary(p => p.Name, p => ToNative(p.Value), StringComparer.Ordinal),
            JTokenType.Array => token.Children().Select(ToNative).ToList(),
            JTokenType.Integer => (long)token,
            JTokenType.Float => (double)token,
            JTokenType.Boolean => (bool)token,
            JTokenType.Null => null,
            JTokenType.String => (string)token,
            _ => ((JValue)token).Value
        };
    }
}