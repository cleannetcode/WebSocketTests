// See https://aka.ms/new-console-template for more information

using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebSocketTests.Client;
using WebSocketTests.Common;


Console.WriteLine("Hello, World!");

using ClientWebSocket ws = new ClientWebSocket();
Uri serverUri = new Uri("wss://localhost:7102/ws");

var source = new CancellationTokenSource();
var claims = new List<Claim> { new Claim(ClaimTypes.Name, "Test Name") };

var jwt = new JwtSecurityToken(
    issuer: AuthOptions.ISSUER,
    audience: AuthOptions.AUDIENCE,
    claims: claims,
    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
var token = new JwtSecurityTokenHandler().WriteToken(jwt);

ws.Options.SetRequestHeader("Authorization", $"Bearer {token}");
await ws.ConnectAsync(serverUri, source.Token);
var iterationNo = 0;
const int dataPerPacket = 10;
var random = new Random();

while (ws.State == WebSocketState.Open && iterationNo++ < 5)
{
    var msg = "hello0123456789123456789123456789123456789123456789123456789";
    var chunksToSend = Encoding.UTF8.GetBytes(msg)
        .Chunk(dataPerPacket)
        .Select(chuck => new ArraySegment<byte>(chuck, 0, chuck.Length))
        .ToArray();

    for (var i = 0; i < chunksToSend.Length; i++)
    {
        if (random.Next(0, 2) == 1)
        {
            throw new Exception("bla bla bla");
        }

        var chunk = chunksToSend[i];
        var endOfMessage = i == chunksToSend.Length - 1;
        await ws.SendAsync(chunk, WebSocketMessageType.Text, endOfMessage, source.Token);
    }

    var receiveBuffer = new byte[200];

    var receiveResult = await ws.ReadMessage(receiveBuffer, dataPerPacket, source.Token);

    Console.WriteLine("Complete response: {0}", Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count));
}


Console.ReadKey();
await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
source.Cancel();