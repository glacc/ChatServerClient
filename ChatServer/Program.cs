using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections;

List<string> chatLog = new();
ChatServer.ChatLog = chatLog;

WebSocketServer webSocketSrv = new(12345);
webSocketSrv.AddWebSocketService<ChatServer>("/chat");

WebSocketServiceHost chat = webSocketSrv.WebSocketServices["/chat"];
const string cmdSend = "send ";

webSocketSrv.Start();

Console.CancelKeyPress += delegate
{
    webSocketSrv.Stop();
};

printHelp();

while (true)
{
    if (Console.IsOutputRedirected)
    {
        Thread.Sleep(Timeout.Infinite);
        continue;
    }

    Console.Write(">");

    string? input = Console.ReadLine();
    if (input == null)
        continue;

    if (input == "exit")
        break;

    bool printHelpFlag = false;

    switch (input)
    {
        case "help":
            printHelpFlag = true;
            break;
        case "clear":
            chatLog.Clear();
            break;
        default:
            bool validCmd = false;

            if (input.StartsWith(cmdSend))
            {
                validCmd = true;

                ChatMessage message = new(input[cmdSend.Length..]);
                string messageJson = message.ToJson();

                chat.Sessions.Broadcast(messageJson);

                chatLog.Add(messageJson);

                Console.WriteLine(message);
            }

            if (!validCmd)
                break;

            break;
    }

    if (printHelpFlag)
        printHelp();
}

webSocketSrv.Stop();

return;

void printHelp()
{
    Console.Write
    (
        "- available commands -\n" +
        "help\n" +
        "send [message]\n" +
        "clear\n" +
        "exit\n"
    );
}

struct ChatMessage
{
    [JsonIgnore]
    public readonly DateTime time;

    public long TimeUtc { get => (time.ToUniversalTime() - new DateTime(1970, 1, 1)).Ticks / 10000L; }
    public string Text { get; set; }

    public string ToJson()
        => JsonSerializer.Serialize(this);

    public override string ToString()
        => $"[{time}]{Text}";

    public ChatMessage(string message)
    {
        time = DateTime.Now;
        Text = message.Trim();
    }
}

class ChatServer : WebSocketBehavior
{
    private const int maxLogCount = 500;
    public static List<string>? ChatLog;

    protected override void OnOpen()
    {
        if (ChatLog == null)
            return;

        int messageCount = ChatLog.Count;

        for (int i = int.Max(messageCount - maxLogCount, 0); i < messageCount; i++)
            Send(ChatLog[i]);
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        ChatMessage chatMessage = new(e.Data);
        string chatMessageJson = chatMessage.ToJson();

        Console.WriteLine(chatMessage.ToString());
        Sessions.Broadcast(chatMessageJson);

        ChatLog?.Add(chatMessageJson);
    }
}
