$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. "$PSScriptRoot/../Release-PackageFunctions.ps1"

# Use the real web cmdlets against a local server: a mocked cmdlet would not
# catch unsupported named parameters such as StatusCodeVariable on WebRequest.
Add-Type -TypeDefinition @'
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
public sealed class ReleaseHttpFixture : IDisposable {
    private readonly HttpListener listener = new HttpListener();
    private readonly Task worker;
    public string Url { get; }
    public ReleaseHttpFixture() {
        var portProbe = new TcpListener(IPAddress.Loopback, 0);
        portProbe.Start();
        var port = ((IPEndPoint)portProbe.LocalEndpoint).Port;
        portProbe.Stop();
        Url = "http://127.0.0.1:" + port + "/";
        listener.Prefixes.Add(Url);
        listener.Start();
        worker = Task.Run(() => {
            while (listener.IsListening) {
                HttpListenerContext context;
                try { context = listener.GetContext(); } catch { break; }
                var index = context.Request.Url.AbsolutePath == "/index.json";
                context.Response.StatusCode = index ? 200 : int.Parse(context.Request.Headers["X-Test-Status"] ?? "200");
                var body = index ? "{\"resources\":[{\"@type\":\"PackageBaseAddress/3.0.0\",\"@id\":\"" + Url + "flat/\"}]}" : "package payload";
                var bytes = Encoding.UTF8.GetBytes(body);
                context.Response.ContentType = index ? "application/json" : "application/octet-stream";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.Close();
            }
        });
    }
    public void Dispose() { listener.Stop(); worker.GetAwaiter().GetResult(); listener.Close(); }
}
'@
$server = [ReleaseHttpFixture]::new()
$destination = Join-Path ([IO.Path]::GetTempPath()) ('release-http-' + [guid]::NewGuid().ToString('N') + '.nupkg')
try {
    $found = Get-RemotePackage ($server.Url + 'index.json') Example 1.2.3 @{ 'X-Test-Status' = '200' } $destination
    if (-not $found -or (Get-Content -LiteralPath $destination -Raw) -cne 'package payload') { throw 'Package download failed.' }
    if (Get-RemotePackage ($server.Url + 'index.json') Example 1.2.3 @{ 'X-Test-Status' = '404' } $destination) { throw 'Missing package was treated as present.' }
    $failed = $false
    try { Get-RemotePackage ($server.Url + 'index.json') Example 1.2.3 @{ 'X-Test-Status' = '500' } $destination } catch {
        if ($_.Exception.Message -notlike '*HTTP 500*') { throw }
        $failed = $true
    }
    if (-not $failed) { throw 'Registry failure was ignored.' }
    Write-Host 'Real HTTP cmdlet checks passed: package download, missing version, and server failure. Only localhost was contacted.'
} finally { $server.Dispose() }

# The GitHub transport is mocked here so a 403 message is tested without a token
# or a write request to GitHub. Preserve its message, not request credentials.
function Invoke-RestMethod([string]$Uri, [string]$Method, [hashtable]$Headers, [switch]$SkipHttpErrorCheck,
    [string]$StatusCodeVariable, [string]$Body, [string]$ContentType) {
    Set-Variable -Name $StatusCodeVariable -Value 403 -Scope 1
    return [pscustomobject]@{ message = 'Resource not accessible by integration' }
}
$rejected = $false
try { Invoke-ReleaseApi 'repos/owner/library/git/refs' POST @{ ref = 'refs/tags/Example.1.2.3'; sha = 'example' } } catch {
    if ($_.Exception.Message -notlike '*HTTP 403. Resource not accessible by integration*') { throw }
    $rejected = $true
}
if (-not $rejected) { throw 'GitHub failure was ignored.' }
Write-Host 'GitHub error detail regression check passed without contacting GitHub.'
