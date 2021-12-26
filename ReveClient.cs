using System.Text.Json;

namespace Reve;

public class ReveClient
{
    private readonly HttpClient _client;
    private readonly ILogger _log;

    public ReveClient(HttpClient client, ILogger<ReveClient> logger)
    {
        _client = client;
        _log = logger;
    }

    public async Task<string> List(string dir, bool isRetry = true)
    {
        string json = await _client.GetStringAsync($"directory{dir}");
        if (ReveHelper.NeedLogin(json) && isRetry)
        {
            await ReveHelper.Login();
            return await List(dir, false);
        }
        return json;
    }

    public async Task<string> Down(string id, bool isRetry = true)
    {
        var rspMsg = await _client.PutAsync($"file/download/{id}", null);
        string json = await rspMsg.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        if (ReveHelper.NeedLogin(root) && isRetry)
        {
            await ReveHelper.Login();
            return await Down(id, false);
        }
        return root.GetProperty("data").GetString();
    }
}
