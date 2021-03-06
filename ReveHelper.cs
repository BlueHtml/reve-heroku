using System.Net;
using System.Text;
using System.Text.Json;

namespace Reve;

public class ReveHelper
{
    public static string Origin = Environment.GetEnvironmentVariable("Origin").TrimEnd('/');
    public static string UserName = Environment.GetEnvironmentVariable("UserName");
    public static string Password = Environment.GetEnvironmentVariable("Password");

    public static string Cookie { get; private set; }

    public static Uri GetBaseAddress() => new($"{Origin}/api/v3/");

    public static bool NeedLogin(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        return NeedLogin(root);
    }

    public static bool NeedLogin(JsonElement root)
    {
        return root.GetProperty("code").GetRawText() == "401";
    }

    public static async Task Login()
    {
        Uri baseAddress = GetBaseAddress();
        CookieContainer container = new();
        HttpClient client = new(new SocketsHttpHandler { CookieContainer = container }) { BaseAddress = baseAddress };
        //登录
        await client.PostAsync("user/session", new StringContent($"{{\"userName\":\"{UserName}\",\"Password\":\"{Password}\",\"captchaCode\":\"\"}}", Encoding.UTF8, "application/json"));
        //获取指定URI下的Cookie的值(字符串)
        Cookie = container.GetCookieHeader(baseAddress);
        await SaveCookie();
        client.Dispose();
    }

    #region cookie缓存

    #region api

    public static async Task LoadCookie()
    {
        HttpClient client = GetDbClient();
        Cookie = await client.GetStringAsync("?ReveCk");
        client.Dispose();
        if (string.IsNullOrWhiteSpace(Cookie))
        {
            Console.WriteLine("cookie not exist");
        }
        else
        {
            Console.WriteLine("cookie load ok");
        }
    }
    static async Task SaveCookie()
    {
        HttpClient client = GetDbClient();
        await client.PutAsync($"?ReveCk={Cookie}", null);
        client.Dispose();
        Console.WriteLine("cookie save ok");
    }

    static HttpClient GetDbClient()
    {
        HttpClient client = new() { BaseAddress = new(Environment.GetEnvironmentVariable("DbUrl")) };
        client.DefaultRequestHeaders.Add("Secret", Environment.GetEnvironmentVariable("Secret"));
        return client;
    }

    #endregion

    #region File

    //const string PATH = "Cookie";
    //public static async Task LoadCookie()
    //{
    //    if (File.Exists(PATH))
    //    {
    //        Cookie = await File.ReadAllTextAsync(PATH);
    //        Console.WriteLine("cookie load ok");
    //    }
    //    else
    //    {
    //        Console.WriteLine("cookie not exist");
    //    }
    //}
    //static async Task SaveCookie()
    //{
    //    await File.WriteAllTextAsync(PATH, Cookie);
    //    Console.WriteLine("cookie save ok");
    //}

    #endregion

    #endregion
}
