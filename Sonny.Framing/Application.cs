using System.IO;
using System.Reflection;
using Nice3point.Revit.Toolkit.External;
using SonnyBIM;

namespace Sonny.Framing;

/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        // Nạp đích danh file DLL mà XAML đang đòi hỏi
        System.Reflection.Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MaterialDesignColors.dll"));
        CreateRibbon();
    }

    private void CreateRibbon()
    {
        var panel = Application.CreatePanel("Commands", "Sonny.Framing");

            panel.AddPushButton<FramingFromCadCmd>("CreateBeam")
                .SetImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon32.png");
    }

}
