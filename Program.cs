using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

StartupCheck();

// Read configuration
var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("config.json")
    .Build();
string username = configBuilder["username"] ?? "";
string password = configBuilder["password"] ?? "";
string hostname = configBuilder["hostname"] ?? "";
if (username == "" || password == "" || hostname == "")
{
    Output("Please fill in all the required fields in config.json.");
    System.Console.WriteLine("Press any key to exit.");
    Console.ReadKey();
    Environment.Exit(0);
}

// Get IPv6 address
IPAddress ip = GetIPv6Address();
Output("Get IPv6 Address: " + ip.ToString());

// Update Google Domain
var url = $"https://domains.google.com/nic/update?hostname={hostname}&myip={ip}";
var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

using (var client = new HttpClient())
{
    client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");
    try
    {
        var response = await client.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();
        Output("Update Result: " + content);
    }
    catch (Exception ex)
    {
        Output($"Exception: {ex.Message}");
    }
}

// Function Definition

void Output(string message)
{
    Console.WriteLine($"[{DateTime.Now}] {message}");
}

IPAddress GetIPv6Address()
{
    var ipv6Address = NetworkInterface.GetAllNetworkInterfaces()
    .Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
    .SelectMany(i => i.GetIPProperties().UnicastAddresses)
    .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 &&
                !a.Address.IsIPv6LinkLocal && a.PrefixLength == 64).First();
    return ipv6Address.Address;
}

void StartupCheck()
{
    if (!File.Exists("config.json"))
    {
        var configJsonTemplate = """
        {
            "username": "",
            "password": "",
            "hostname": ""
        }
        """;
        File.WriteAllText("config.json", configJsonTemplate);
        Output("No config.json file found so we generated one for you.");
        System.Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
        Environment.Exit(0);
    }
}