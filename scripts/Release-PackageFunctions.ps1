Set-StrictMode -Version Latest

function Get-ArchiveMetadata([string]$Path) {
    $zip = [IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $Path).Path)
    try {
        $entries = @($zip.Entries | Where-Object { $_.FullName.EndsWith('.nuspec', [StringComparison]::Ordinal) })
        if ($entries.Count -ne 1) { throw 'A package must contain exactly one nuspec.' }
        $reader = [IO.StreamReader]::new($entries[0].Open())
        try { $xml = [xml]$reader.ReadToEnd() } finally { $reader.Dispose() }
        $id = $xml.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='id']")
        $version = $xml.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='version']")
        $repository = $xml.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='repository']")
        return [pscustomobject]@{ Id = $id.InnerText; Version = $version.InnerText; Repository = $repository }
    } finally { $zip.Dispose() }
}

function Assert-ArchivePayloadEqual([string]$Expected, [string]$Actual) {
    $maps = @()
    foreach ($path in @($Expected, $Actual)) {
        $map = [Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
        $zip = [IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $path).Path)
        try {
            foreach ($entry in $zip.Entries) {
                # NuGet.org adds its own signature; compare every other entry.
                if ($entry.FullName -eq '.signature.p7s' -or $entry.FullName.EndsWith('/')) { continue }
                $stream = $entry.Open()
                $hash = [Security.Cryptography.SHA256]::Create()
                try { $map.Add($entry.FullName, [Convert]::ToHexString($hash.ComputeHash($stream))) }
                finally { $stream.Dispose(); $hash.Dispose() }
            }
        } finally { $zip.Dispose() }
        $maps += ,$map
    }
    if ($maps[0].Count -ne $maps[1].Count) { throw 'An existing package has different contents. Refusing to reuse its version.' }
    foreach ($entry in $maps[0].GetEnumerator()) {
        if (-not $maps[1].ContainsKey($entry.Key) -or $maps[1][$entry.Key] -cne $entry.Value) {
            throw "An existing package differs at $($entry.Key). Refusing to reuse its version."
        }
    }
}

function Assert-ReleaseArtifacts([string]$Directory, [string]$Repository, [string]$Commit, [int]$PullRequest) {
    $manifest = Get-Content -LiteralPath (Join-Path $Directory 'manifest.json') -Raw | ConvertFrom-Json
    if ($manifest.repository -cne $Repository -or $manifest.commit -cne $Commit -or $manifest.pullRequest -ne $PullRequest) { throw 'Artifact source identity does not match this merged PR.' }
    if ($manifest.packageId -cnotmatch '^[A-Za-z0-9_.-]+$' -or $manifest.version -cnotmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') { throw 'Invalid artifact package identity.' }
    $expected = @("$($manifest.packageId).$($manifest.version).nupkg", "$($manifest.packageId).$($manifest.version).snupkg", 'release-notes.md')
    if ($manifest.packageFile -cne $expected[0] -or $manifest.symbolFile -cne $expected[1]) { throw 'Unexpected package filenames.' }
    if (@($manifest.files).Count -ne 3) { throw 'Expected checksums for the package, symbols and release notes.' }
    foreach ($name in $expected) {
        $records = @($manifest.files | Where-Object { $_.name -ceq $name })
        if ($records.Count -ne 1) { throw "Missing/duplicate checksum for $name" }
        $hash = (Get-FileHash -LiteralPath (Join-Path $Directory $name) -Algorithm SHA256).Hash
        if ($hash -cne $records[0].sha256) { throw "Artifact checksum mismatch: $name" }
    }
    foreach ($name in $expected[0..1]) {
        $metadata = Get-ArchiveMetadata (Join-Path $Directory $name)
        if ($metadata.Id -cne $manifest.packageId -or $metadata.Version -cne $manifest.version) { throw 'Package metadata does not match the manifest.' }
        if ($name -ceq $manifest.packageFile) {
            if ($null -eq $metadata.Repository -or $metadata.Repository.GetAttribute('commit') -cne $Commit) { throw 'Package source commit does not match the merged PR.' }
            if ($metadata.Repository.GetAttribute('url').TrimEnd('/').Replace('.git', '') -ine "https://github.com/$Repository") { throw 'Package repository URL does not match.' }
        }
    }
    return $manifest
}

function Invoke-ReleaseApi([string]$Path, [string]$Method = 'GET', $Body = $null, [switch]$AllowMissing) {
    $headers = @{ Authorization = "Bearer $env:GH_TOKEN"; Accept = 'application/vnd.github+json'; 'X-GitHub-Api-Version' = '2022-11-28' }
    $request = @{ Uri = "https://api.github.com/$Path"; Method = $Method; Headers = $headers; SkipHttpErrorCheck = $true; StatusCodeVariable = 'status' }
    if ($null -ne $Body) { $request.Body = $Body | ConvertTo-Json -Depth 8 -Compress; $request.ContentType = 'application/json' }
    $result = Invoke-RestMethod @request
    if ($status -eq 404 -and $AllowMissing) { return $null }
    if ($status -lt 200 -or $status -ge 300) {
        $detail = ''
        if ($null -ne $result -and $result.PSObject.Properties.Name -contains 'message') {
            $detail = ' ' + [string]$result.message
        }
        throw "GitHub API $Method $Path returned HTTP $status.$detail"
    }
    return $result
}

function Find-ReleaseByTag([string]$Repository, [string]$Tag) {
    $encodedTag = [Uri]::EscapeDataString($Tag)
    $published = Invoke-ReleaseApi "repos/$Repository/releases/tags/$encodedTag" -AllowMissing
    if ($null -ne $published) { return $published }
    # The by-tag endpoint only finds published releases. Authenticated release
    # listings include drafts, which must be reused when a publication is retried.
    $page = 1
    do {
        $releases = @(Invoke-ReleaseApi "repos/$Repository/releases?per_page=100&page=$page")
        $matching = @($releases | Where-Object { $_.tag_name -ceq $Tag })
        if ($matching.Count -gt 1) { throw 'Multiple releases use the requested tag.' }
        if ($matching.Count -eq 1) { return $matching[0] }
        $page++
    } while ($releases.Count -eq 100)
    return $null
}

function Get-RemotePackage([string]$Source, [string]$Id, [string]$Version, [hashtable]$Headers, [string]$Destination) {
    $index = Invoke-RestMethod -Uri $Source -Headers $Headers
    $resource = @($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' })
    if ($resource.Count -ne 1) { throw "Cannot locate the package download endpoint for $Source" }
    $base = $resource[0].'@id'.TrimEnd('/')
    $lowerId = $Id.ToLowerInvariant()
    $lowerVersion = $Version.ToLowerInvariant()
    $response = Invoke-WebRequest -Uri "$base/$lowerId/$lowerVersion/$lowerId.$lowerVersion.nupkg" -Headers $Headers -OutFile $Destination -SkipHttpErrorCheck -PassThru
    $status = [int]$response.StatusCode
    if ($status -eq 404) { return $false }
    if ($status -ne 200) { throw "Package lookup returned HTTP $status from $Source" }
    return $true
}
