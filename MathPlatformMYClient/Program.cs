using System.Net.Http.Headers;
using System.Text;
using ImGuiNET;
using MCYRay_cs.Auto;
using MCYRay_cs.Auto.Menu;
using MCYRay_cs.Auto.Settings;
using Newtonsoft.Json;
using Raylib_cs;

namespace MCYRay_cs;

public static class MainTemplate
{

    [AutoSettings("Settings")]
    [Serializable]
    public class Settings
    {
        public string superUserName = "";
        public string password = "";
    }
    
    static string messageString = "sdklsjfkljdskldfsjdklsjfdklsjdfskldfjsldsjkldfsjdfklsjdflkjdskldfjdfklsjkl";
    static HttpClient httpClient;

    [Serializable]
    public class PasswordLoginData
    {
        public string superUserName;
        public string passwordEnter;
    }
    
    [Serializable]
    public class ServerMessage
    {
        public string message;
    }
    
    public static void Main()
    {
        MCYRay.MCYRayInit(640, 480, "Math Platform - My Client");
        MCYRayAutoGenerators.GenerateAutoContent();
        
        httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:3000")
        };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
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

            Settings settingsObject = AutoSettings.GetSetting<Settings>();
            MCYRay.BeginImGui("Controls");
            ImGui.InputText("Username", ref settingsObject.superUserName, 20, ImGuiInputTextFlags.Password);
            ImGui.InputText("Password", ref settingsObject.password, 25, ImGuiInputTextFlags.Password);
            if (ImGui.Button("Connect"))
            {
                AutoSettings.SetSetting(new Settings
                {
                    superUserName = settingsObject.superUserName,
                    password = settingsObject.password
                });
                
                Task<ServerMessage> serverLoginTask = LoginTask(new PasswordLoginData
                {
                    superUserName = settingsObject.superUserName,
                    passwordEnter = settingsObject.password
                });
                ServerMessage serverMessage = serverLoginTask.Result;
                Console.WriteLine($"Server Message {serverMessage.message}");
            }
            ImGui.TextWrapped(messageString);
            MCYRay.EndImGui();
        
            MCYRay.EndFrame();
        }
        
        MCYRay.MCYRayShutdown();
    }
    
    public static async Task<ServerMessage> LoginTask(PasswordLoginData loginData)
    {
        string json = JsonConvert.SerializeObject(loginData);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync("/api/superuser/login", content);

        // response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonConvert.DeserializeObject<ServerMessage>(responseContent);
    }
}
