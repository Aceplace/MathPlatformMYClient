using System.Net.Http.Headers;
using System.Text;
using ImGuiNET;
using MCYRay_cs.Auto;
using MCYRay_cs.Auto.Gui;
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
        public string? student;
        public string? token;
    }

    [Serializable]
    public class CreateNewStudentData
    {
        public string studentId = "";
        public string lastName = "";
        public string firstName = "";
        public string email = "";
        public string parentEmail1 = "";
        public string parentEmail2 = "";
        public string parentEmail3 = "";
        public int classroomPeriod = 0;
    }
    
    static string messageString = "";
    static HttpClient httpClient;
    static CreateNewStudentData newStudentData = new();
    
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
                if (!string.IsNullOrEmpty(serverMessage.token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", serverMessage.token);
                }
                Console.WriteLine($"Server message... {serverMessage.message}");
            }
            
            if (ImGui.Button("Check Protected Route"))
            {
                Task<ServerMessage> protectedRouteTask = ProtectedRouteTask();
                ServerMessage serverMessage = protectedRouteTask.Result;
                Console.WriteLine($"Server message... {serverMessage.message}");
            }
            ImGui.Separator();
            
            AutoGui.Auto("createStudentData", ref newStudentData);
            if (ImGui.Button("Create New Student"))
            {
                Task<ServerMessage> createNewStudentTask = CreateStudentTask();
                ServerMessage serverMessage = createNewStudentTask.Result;
                Console.WriteLine($"Server message... {serverMessage.message}");
                if (serverMessage.student != null)
                    Console.WriteLine($"Server message... {serverMessage.student}");
            }
            
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
        
        return JsonConvert.DeserializeObject<ServerMessage>(responseContent)!;
    }
    
    public static async Task<ServerMessage> CreateStudentTask()
    {
        string json = JsonConvert.SerializeObject(newStudentData);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync("/api/superuser/createstudent", content);

        // response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonConvert.DeserializeObject<ServerMessage>(responseContent)!;
    }
    
    public static async Task<ServerMessage> ProtectedRouteTask()
    {
        HttpResponseMessage response = await httpClient.PostAsync("/api/superuser/protected", null);

        // response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonConvert.DeserializeObject<ServerMessage>(responseContent)!;
    }
}
