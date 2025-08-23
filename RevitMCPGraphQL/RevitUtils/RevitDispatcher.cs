using System.Collections.Concurrent;
using Autodesk.Revit.UI;

namespace RevitMCPGraphQL;

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

    public static T Invoke<T>(Func<UIApplication, T> func)
    {
        if (!_initialized || _extEvent == null || _handler == null)
            throw new InvalidOperationException("RevitDispatcher not initialized.");

        var tcs = new TaskCompletionSource<T>();
        var uiApp = _handler.UIApplication;
        _handler.Enqueue(() =>
        {
            try { tcs.SetResult(func(uiApp)); }
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

    public UIApplication UIApplication => _uiApp;

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

