using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;
using Microsoft.AspNetCore.Hosting;

namespace RevitMCPGraphQL;

public class GraphQlRevitAddIn : IExternalApplication
{
    public static IWebHost? Host;
    public Result OnStartup(UIControlledApplication application)
    {
        // Resolve load Assembly path
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;


        // Add buttons to the default Add-Ins tab
        string panelName = "MCP GraphQl";
        RibbonPanel? panel = null;
        panel = application.CreateRibbonPanel(panelName);

        string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        PushButtonData startButton = new PushButtonData(
            "StartButton",
            "Start",
            assemblyPath,
            "RevitMCPGraphQL.Command.StartCommand"
        );
        startButton.ToolTip = "Start the GraphQL server";

        PushButtonData stopButton = new PushButtonData(
            "StopButton",
            "Stop",
            assemblyPath,
            "RevitMCPGraphQL.Command.StopCommand"
        );
        stopButton.ToolTip = "Stop the GraphQL server";

        panel.AddItem(startButton);
        panel.AddItem(stopButton);

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        Host?.StopAsync().Wait();
        return Result.Succeeded;
    }
    private static readonly string AssemblyLocation  = Assembly.GetExecutingAssembly().Location;
    private static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyLocation);
    private static string GetPathInAssemblyDirectory(string filename) => Path.Combine(AssemblyDirectory, filename);

    private static Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
    {
        // You could either check for input argument against a hardcoded
        // list of dlls (in your case Microsoft.Bcl.AsyncInterfaces.dll)
        // or like below, check if the dll attempted to be loaded (as
        // per to the input argument is something you bundled.
        string filename = args.Name.Split(',')[0] + ".dll".ToLower();
        string assemblyFilename = GetPathInAssemblyDirectory(filename);
        if (!File.Exists(assemblyFilename))
            return null;

        return Assembly.LoadFrom(assemblyFilename);
    }


}