using ImGuiNET;
using MCYRay_cs.Auto.Menu;
using Raylib_cs;

namespace MCYRay_cs;

public static class MainTemplate
{
    public static void Main()
    {
        MCYRay.MCYRayInit(640, 480, "Math Platform - My Client");
        
        bool shouldQuit = false;
        while (!Raylib.WindowShouldClose() && !shouldQuit)
        {
            MCYRay.BeginFrame();

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMainMenuBar())
                {
                    AutoMenus.DoAutoMenus();
                    ImGui.EndMainMenuBar();
                }
            }

            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport());
        
            MCYRay.EndFrame();
        }
        
        MCYRay.MCYRayShutdown();
    }
}
