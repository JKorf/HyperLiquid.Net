using HyperLiquid.Net.Clients;

// REST
var restClient = new HyperLiquidRestClient();
var result = await restClient.SpotApi.ExchangeData.GetPricesAsync();
var symbolInfo = result.Data.Single(x => x.Key == "HYPE/USDC");
Console.WriteLine($"Rest client ticker price for HYPEUSDC: {symbolInfo.Value}");

Console.WriteLine();
Console.WriteLine("Press enter to start websocket subscription");
Console.ReadLine();

// Websocket
var socketClient = new HyperLiquidSocketClient();
var subscription = await socketClient.SpotApi.SubscribeToSymbolUpdatesAsync("HYPE/USDC", update =>
{
    Console.WriteLine($"Websocket client ticker price for HYPEUSDC: {update.Data.MidPrice}");
});

Console.ReadLine();
