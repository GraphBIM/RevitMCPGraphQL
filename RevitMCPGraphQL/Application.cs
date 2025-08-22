using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        startButton.LargeImage = DrawAGreenSquare(32, 32);
        startButton.Image = DrawAGreenSquare(16, 16);
        PushButtonData stopButton = new PushButtonData(
            "StopButton",
            "Stop",
            assemblyPath,
            "RevitMCPGraphQL.Command.StopCommand"
        );
        stopButton.ToolTip = "Stop the GraphQL server";
        stopButton.LargeImage = DrawARedSquare(32, 32);
        stopButton.Image = DrawAGreenSquare(16, 16);
        panel.AddItem(startButton);
        panel.AddItem(stopButton);

        return Result.Succeeded;
    }
    public BitmapImage DrawAGreenSquare(int w, int h)
    {
        int width = w;
        int height = h;

        // Create a WriteableBitmap
        var writeableBitmap = new WriteableBitmap(
            width,
            height,
            96, // DPI X
            96, // DPI Y
            PixelFormats.Bgra32,
            null);

        // Fill pixel data with green color
        var pixelData = new byte[width * height * 4];
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = 0;       // Blue
            pixelData[i + 1] = 255; // Green
            pixelData[i + 2] = 0;   // Red
            pixelData[i + 3] = 255; // Alpha
        }

        // Write the pixel data to the bitmap
        writeableBitmap.WritePixels(
            new System.Windows.Int32Rect(0, 0, width, height),
            pixelData,
            width * 4,
            0);

        // Convert WriteableBitmap to BitmapImage
        var bitmapImage = new BitmapImage();
        using (var stream = new MemoryStream())
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
            encoder.Save(stream);
            stream.Position = 0;

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Make it thread-safe
        }

        return bitmapImage;
    }
    public BitmapImage DrawARedSquare(int w, int h)
    {
        int width = w;
        int height = h;

        // Create WriteableBitmap
        var writeableBitmap = new WriteableBitmap(
            width,
            height,
            96,
            96,
            PixelFormats.Bgra32,
            null);

        // Fill pixel data with red color
        var pixelData = new byte[width * height * 4];
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = 0;       // Blue
            pixelData[i + 1] = 0;   // Green
            pixelData[i + 2] = 255; // Red
            pixelData[i + 3] = 255; // Alpha
        }

        // Write pixels to bitmap
        writeableBitmap.WritePixels(
            new System.Windows.Int32Rect(0, 0, width, height),
            pixelData,
            width * 4,
            0);

        // Convert WriteableBitmap to BitmapImage
        var bitmapImage = new BitmapImage();
        using (var stream = new MemoryStream())
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
            encoder.Save(stream);
            stream.Position = 0;

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
        }

        return bitmapImage;
    }


    public Result OnShutdown(UIControlledApplication application)
    {
        Host?.StopAsync().Wait();
        return Result.Succeeded;
    }

    private static readonly string AssemblyLocation = Assembly.GetExecutingAssembly().Location;
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