using CryptoExchange.Net.Web3;
using HyperLiquid.Net;
using HyperLiquid.Net.Interfaces.Clients;
using Microsoft.AspNetCore.Mvc;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the HyperLiquid services
builder.Services.AddHyperLiquid();

// OR to provide API credentials for accessing private endpoints, or setting other options:

//builder.Services.AddHyperLiquid(options =>
//{
//    options.ApiCredentials = new HyperLiquidCredentials(new HyperLiquidCredential(EthKey.FromPrivateKey("<PRIVATEKEY>")));
//    options.Rest.RequestTimeout = TimeSpan.FromSeconds(5);
//});


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Map the endpoint and inject the rest client
app.MapGet("/{Symbol}", async ([FromServices] IHyperLiquidRestClient client, string symbol) =>
{
    var decoded = HttpUtility.UrlDecode(symbol);
    var result = await client.SpotApi.ExchangeData.GetPricesAsync();
    var symbolInfo = result.Data.Single(x => x.Key == decoded);
    return symbolInfo.Value;
})
.WithOpenApi();


app.MapGet("/Balances", async ([FromServices] IHyperLiquidRestClient client) =>
{
    var result = await client.SpotApi.Account.GetBalancesAsync();
    return (object)(result.Success ? result.Data : result.Error!);
})
.WithOpenApi();

app.Run();