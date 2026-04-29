using System.IO;
using System.Reflection;
using Nice3point.Revit.Toolkit.External;
using SonnyBIM;

namespace Sonny.Framing;

/// <summary>
/// Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        // 1. Đăng ký bộ giải quyết thư viện NGAY ĐẦU TIÊN
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        // 2. Thêm try-catch để nếu có lỗi Ribbon thì nó vẫn báo cho bạn biết thay vì im lặng biến mất
        try
        {
            CreateRibbon();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Sonny Ribbon Error: " + ex.Message);
        }
        // // Nạp đích danh file DLL mà XAML đang đòi hỏi
        // System.Reflection.Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MaterialDesignColors.dll"));
        // CreateRibbon();
    }

    public override void OnShutdown()
    {
        // Hủy đăng ký khi tắt Revit để tránh rác bộ nhớ
        AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
    }
    // 3. Hàm quan trọng nhất: Tự tìm DLL trong thư mục cài đặt của Sonny
    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        // Lấy đường dẫn thư mục hiện tại của Sonny.Framing.dll
        string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // Phân tích tên thư viện cần nạp
        string assemblyName = new AssemblyName(args.Name).Name;
        string assemblyPath = Path.Combine(folderPath, assemblyName + ".dll");
        // Nếu thấy file DLL đó trong thư mục của Sonny thì nạp nó lên
        if (File.Exists(assemblyPath))
        {
            return Assembly.LoadFrom(assemblyPath);
        }
        return null;
    }

    private void CreateRibbon()
    {
        var panel = Application.CreatePanel("Model from CAD", "Sonny");

            panel.AddPushButton<FramingFromCadCmd>("Beam from CAD")
                .SetImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon32.png");
            panel.AddPushButton<WallFromCadCmd>("Wall from CAD")
                .SetImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon32.png");
            panel.AddPushButton<FloorFromCadCmd>("Floor from CAD")
                .SetImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon32.png");
            panel.AddPushButton<FoundationFromCadCmd>("Foundation from CAD")
                .SetImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon32.png");
            panel.AddPushButton<PileFromCadCmd>("Pile from CAD")
                .SetImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/Sonny.Framing;component/Resources/Icons/RibbonIcon32.png");

    }

}
