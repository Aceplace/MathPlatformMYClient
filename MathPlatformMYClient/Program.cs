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
        public string serializedStudentInfos;
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

    [Serializable]
    public class RequestStudentsInPeriod
    {
        public int period;

        public RequestStudentsInPeriod(int period)
        {
            this.period = period;
        }
    }
    
    [Serializable]
    public class StudentInfo
    {
        public string lastName = "";
        public string firstName = "";
        public string studentId = "";
        public string email = "";
        public string parentEmail1 = "";
        public string parentEmail2 = "";
        public string parentEmail3 = "";
        public bool hasSetPassword = false;
        public string passwordHash = "";
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
            MCYRay.BeginImGui("Connect");
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
            MCYRay.EndImGui();

            MCYRay.BeginImGui("Create Student Data");
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

            MCYRay.BeginImGui("Class View");
            ImGui.Text("View Class period...");
            {
                int periodButtonPressed = -1;
                if (ImGui.Button("1"))
                    periodButtonPressed = 1;
                ImGui.SameLine();
                if (ImGui.Button("2"))
                    periodButtonPressed = 2;
                ImGui.SameLine();
                if (ImGui.Button("5"))
                    periodButtonPressed = 5;
                ImGui.SameLine();
                if (ImGui.Button("7"))
                    periodButtonPressed = 7;
                ImGui.SameLine();
                if (ImGui.Button("8"))
                    periodButtonPressed = 8;
                
                if (periodButtonPressed != -1)
                {
                    Task<ServerMessage> getStudentsInPeriodTask = GetStudentsInPeriod(periodButtonPressed);
                    ServerMessage serverMessage = getStudentsInPeriodTask.Result;
                    Console.WriteLine($"{serverMessage.message}");

                    StudentInfo[] studentInfos = JsonConvert.DeserializeObject<StudentInfo[]>(serverMessage.serializedStudentInfos)!;
                    Console.WriteLine($"Number of students in period {periodButtonPressed}: {studentInfos.Length}");
                }
            }
            
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
    
    public static async Task<ServerMessage> GetStudentsInPeriod(int period)
    {
        string json = JsonConvert.SerializeObject(new RequestStudentsInPeriod(period));
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        
        HttpResponseMessage response = await httpClient.PostAsync("/api/superuser/getstudentsinperiod", content);

        // response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        
        return JsonConvert.DeserializeObject<ServerMessage>(responseContent)!;
    }
}
