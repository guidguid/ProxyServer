using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class ProxyServer
{
    private const int BufferSize = 8192;
    private readonly int _port;
    private static readonly object logLock = new object();
    private static readonly string logFilePath = "proxy_log.txt";

    private readonly string ProxyUsername;
    private readonly string ProxyPassword;

    public ProxyServer(int port)
    {
        _port = port;
        
        // Load credentials from environment variables
        ProxyUsername = Environment.GetEnvironmentVariable("PROXY_USERNAME") ?? "admin";
        ProxyPassword = Environment.GetEnvironmentVariable("PROXY_PASSWORD") ?? "password";
    }

    public void Start()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();
        Console.WriteLine($"[+] Proxy server started on port {_port}");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Task.Run(() => HandleClient(client));
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        using (client)
        {
            NetworkStream clientStream = client.GetStream();
            byte[] buffer = new byte[BufferSize];

            int bytesRead = await clientStream.ReadAsync(buffer, 0, BufferSize);
            if (bytesRead == 0) return;

            string requestHeader = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Check authentication
            if (!IsAuthenticated(requestHeader))
            {
                await SendProxyAuthRequired(clientStream);
                return;
            }

            string[] requestLines = requestHeader.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            if (requestLines.Length == 0) return;

            string[] requestParts = requestLines[0].Split(' ');
            if (requestParts.Length < 2) return;

            string method = requestParts[0];
            string url = requestParts[1];

            if (method.ToUpper() == "CONNECT")
            {
                LogRequest($"https://{url}");
                await HandleHttpsProxy(clientStream, url);
            }
            else if (IsValidHttpUrl(url))
            {
                LogRequest(url);
                await HandleHttpProxy(clientStream, requestHeader);
            }
        }
    }

    private async Task HandleHttpProxy(NetworkStream clientStream, string requestHeader)
    {
        string firstLine = requestHeader.Split("\r\n")[0];
        string[] parts = firstLine.Split(' ');

        if (parts.Length < 2) return;
        string url = parts[1];

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri targetUri)) return;

        TcpClient remoteServer = new TcpClient();
        try
        {
            await remoteServer.ConnectAsync(targetUri.Host, targetUri.Port == -1 ? 80 : targetUri.Port);
            using (NetworkStream remoteStream = remoteServer.GetStream())
            {
                byte[] requestBytes = Encoding.UTF8.GetBytes(requestHeader);
                await remoteStream.WriteAsync(requestBytes, 0, requestBytes.Length);
                await RelayData(clientStream, remoteStream);
            }
        }
        finally
        {
            remoteServer.Close();
        }
    }

    private async Task HandleHttpsProxy(NetworkStream clientStream, string targetHost)
    {
        string[] targetParts = targetHost.Split(':');
        string host = targetParts[0];
        int port = targetParts.Length > 1 ? int.Parse(targetParts[1]) : 443;

        TcpClient remoteServer = new TcpClient();
        try
        {
            await remoteServer.ConnectAsync(host, port);
            using (NetworkStream remoteStream = remoteServer.GetStream())
            {
                byte[] successResponse = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
                await clientStream.WriteAsync(successResponse, 0, successResponse.Length);

                await Task.WhenAll(
                    RelayData(clientStream, remoteStream),
                    RelayData(remoteStream, clientStream)
                );
            }
        }
        finally
        {
            remoteServer.Close();
        }
    }

    private async Task RelayData(NetworkStream fromStream, NetworkStream toStream)
    {
        byte[] buffer = new byte[BufferSize];
        int bytesRead;
        while ((bytesRead = await fromStream.ReadAsync(buffer, 0, BufferSize)) > 0)
        {
            await toStream.WriteAsync(buffer, 0, bytesRead);
        }
    }

    private void LogRequest(string url)
    {
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {url}";
        
        lock (logLock)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[LOG] {logEntry}");
        Console.ResetColor();
    }

    private bool IsValidHttpUrl(string url)
    {
        return Regex.IsMatch(url, @"^https?:\/\/[^\s\/$.?#].[^\s]*$", RegexOptions.IgnoreCase);
    }

    private bool IsAuthenticated(string requestHeader)
    {
        const string authPrefix = "Proxy-Authorization: Basic ";
        foreach (string line in requestHeader.Split("\r\n"))
        {
            if (line.StartsWith(authPrefix))
            {
                string encodedCredentials = line.Substring(authPrefix.Length).Trim();
                string decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                
                string expectedCredentials = $"{ProxyUsername}:{ProxyPassword}";
                return decodedCredentials == expectedCredentials;
            }
        }
        return false;
    }

    private async Task SendProxyAuthRequired(NetworkStream clientStream)
    {
        string response = "HTTP/1.1 407 Proxy Authentication Required\r\n" +
                          "Proxy-Authenticate: Basic realm=\"My Proxy\"\r\n" +
                          "Content-Length: 0\r\n\r\n";

        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await clientStream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    static void Main(string[] args)
    {
        ProxyServer proxy = new ProxyServer(8080);
        proxy.Start();
    }
}
