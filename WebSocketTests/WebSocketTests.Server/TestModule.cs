using System.Net.WebSockets;
using System.Text;
using WebSocketTests.Common;

namespace WebSocketTests.Server;

public static class TestModule
{
    public static async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        WebSocketReceiveResult? receiveResult = null;

        do
        {
            receiveResult = await webSocket.ReadMessage(buffer);

            if (receiveResult.CloseStatus.HasValue)
            {
                break;
            }

            var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            Console.WriteLine(message);

            var request = new ArraySegment<byte>(buffer, 0, receiveResult.Count);
            await webSocket.SendAsync(
                request,
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);
        } while (!receiveResult.CloseStatus.HasValue);


        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}