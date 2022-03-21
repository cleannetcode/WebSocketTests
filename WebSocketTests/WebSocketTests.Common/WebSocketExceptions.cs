using System.Net.WebSockets;
using System.Text;

namespace WebSocketTests.Common;

public static class WebSocketExceptions
{
    public static async Task<WebSocketReceiveResult?> ReadMessage(
        this WebSocket clientWebSocket,
        byte[] buffer,
        int dataPerPacket = 10,
        CancellationToken cancellationToken = default)
    {

        WebSocketReceiveResult? result = null;

        var offset = 0;
        do
        {
            var bytesReceived = new ArraySegment<byte>(buffer, offset, dataPerPacket);
            result = await clientWebSocket.ReceiveAsync(bytesReceived, cancellationToken);
            offset += result.Count;
        } while (!result.EndOfMessage);

        return new WebSocketReceiveResult(
            offset,
            result.MessageType,
            result.EndOfMessage,
            result.CloseStatus,
            result.CloseStatusDescription);
    }
}