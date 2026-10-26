$ErrorActionPreference = 'Stop'
$releaseRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$packageRoot = Join-Path $releaseRoot 'CoelhoWorldBuilder'
$packageInfo = Get-Content (Join-Path $packageRoot 'package.json') -Raw | ConvertFrom-Json
$version = $packageInfo.version
$outputRoot = Join-Path $releaseRoot 'dist'
New-Item -ItemType Directory -Force $outputRoot | Out-Null

# The manual uses only headings, paragraphs and ordered/unordered lists.
$manualPath = Join-Path $packageRoot 'Documentation/Manual-pt-BR.md'
$htmlPath = Join-Path $packageRoot 'Documentation/Manual-pt-BR.html'
$html = [System.Collections.Generic.List[string]]::new()
$toc = [System.Collections.Generic.List[string]]::new()
$listTag = ''
$sectionIndex = 0
foreach ($line in (Get-Content -LiteralPath $manualPath -Encoding utf8)) {
    if ($line -match '^(\d+\. |\- )(.+)$') {
        $tag = if ($line.StartsWith('- ')) { 'ul' } else { 'ol' }
        if ($listTag -ne $tag) {
            if ($listTag) { $html.Add("</$listTag>") }
            $html.Add("<$tag>"); $listTag = $tag
        }
        $html.Add('<li>' + [System.Net.WebUtility]::HtmlEncode($Matches[2]) + '</li>')
        continue
    }
    if ($listTag) { $html.Add("</$listTag>"); $listTag = '' }
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    if ($line -match '^(#{1,2}) (.+)$') {
        $level = $Matches[1].Length
        $heading = [System.Net.WebUtility]::HtmlEncode($Matches[2])
        $sectionIndex++
        $html.Add("<h$level id='section-$sectionIndex'>$heading</h$level>")
        if ($level -eq 2) { $toc.Add("<li><a href='#section-$sectionIndex'>$heading</a></li>") }
    } else { $html.Add('<p>' + [System.Net.WebUtility]::HtmlEncode($line) + '</p>') }
}
if ($listTag) { $html.Add("</$listTag>") }
$document = @"
<!doctype html>
<html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Manual — Easy Level Design Tool $version</title>
<style>
:root{color-scheme:light;font-family:Segoe UI,Arial,sans-serif;color:#192b33;background:#eef3f4}
body{margin:0;line-height:1.65}main{max-width:900px;margin:32px auto;padding:36px 48px;background:#fff;border-radius:14px;border-top:7px solid #16855b}
.brand{color:#16855b;font-weight:700;letter-spacing:.14em;font-size:12px}h1{line-height:1.15;font-size:38px;margin-bottom:12px}h2{font-size:23px;margin-top:40px;padding-top:16px;border-top:1px solid #dfe8e8;scroll-margin-top:24px}
nav{background:#f0f8f4;border-radius:8px;padding:18px 26px}nav strong{color:#156647}nav ol{columns:2;column-gap:32px;padding-left:24px}nav li{break-inside:avoid;margin:5px 0}
a{color:#126b50}p,li{overflow-wrap:anywhere}li{margin:7px 0}footer{margin-top:36px;font-size:13px;color:#53646b;border-top:1px solid #dfe8e8;padding-top:16px}
@media(max-width:650px){main{margin:0;padding:24px 20px;border-radius:0}h1{font-size:30px}nav ol{columns:1}}
@page{size:A4;margin:16mm}@media print{body{background:#fff}main{max-width:none;margin:0;padding:0;border:0}nav ol{columns:2}h2{break-after:avoid}li{break-inside:avoid}a{color:inherit;text-decoration:none}}
</style></head><body><main><div class="brand">COELHO LABS · MANUAL OFFLINE · $version</div>
<nav aria-label="Sumário"><strong>Sumário</strong><ol>$($toc -join "`n")</ol></nav>
$($html -join "`n")
<footer>Easy Level Design Tool $version · Coelho Labs · Abra este arquivo no navegador. Para imprimir ou salvar em PDF, use Ctrl+P.</footer>
</main></body></html>
"@
[System.IO.File]::WriteAllText($htmlPath, $document, [System.Text.UTF8Encoding]::new($false))
foreach ($documentPath in @($manualPath, $htmlPath)) {
    if (-not (Test-Path -LiteralPath ($documentPath + '.meta'))) {
        $guid = [Guid]::NewGuid().ToString('N')
        [System.IO.File]::WriteAllText(($documentPath + '.meta'), "fileFormatVersion: 2`nguid: $guid`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n", [System.Text.UTF8Encoding]::new($false))
    }
}

$stageRoot = Join-Path $releaseRoot ('.validation/ReleaseStaging-' + [Guid]::NewGuid().ToString('N'))
$tarStage = Join-Path $stageRoot 'tar'
$zipStage = Join-Path $stageRoot 'zip'
New-Item -ItemType Directory -Force $tarStage, $zipStage | Out-Null
Copy-Item -LiteralPath $packageRoot -Destination (Join-Path $tarStage 'package') -Recurse
Copy-Item -LiteralPath $packageRoot -Destination (Join-Path $zipStage 'CoelhoWorldBuilder') -Recurse
Copy-Item -LiteralPath $htmlPath -Destination (Join-Path $zipStage 'MANUAL.html')
$readme = @"
Easy Level Design Tool $version — Coelho Labs

1. Extraia este ZIP para uma pasta permanente.
2. Unity: Window > Package Manager > + > Add package from disk.
3. Selecione CoelhoWorldBuilder/package.json nesta pasta.
4. Abra Tools > Coelho Labs > Easy Level Design Tool.

Abra MANUAL.html no navegador para ver as instruções completas.
Não use Assets > Import Package: este arquivo contém um pacote UPM, não um .unitypackage.
Consulte CoelhoWorldBuilder/LICENSE.md para os termos de uso.
"@
[System.IO.File]::WriteAllText((Join-Path $zipStage 'LEIA-ME.txt'), $readme, [System.Text.UTF8Encoding]::new($false))
$tgz = Join-Path $outputRoot "com.coelholabs.worldbuilder-$version.tgz"
$zip = Join-Path $outputRoot "Easy-Level-Design-Tool-$version.zip"
$manual = Join-Path $outputRoot 'Manual-Easy-Level-Design-Tool.html'
& tar -czf $tgz -C $tarStage package
if ($LASTEXITCODE -ne 0) { throw 'Falha ao gerar o pacote .tgz' }
Compress-Archive -Path (Join-Path $zipStage '*') -DestinationPath $zip -Force
Copy-Item -LiteralPath $htmlPath -Destination $manual -Force
$checksumLines = foreach ($asset in @($tgz, $zip, $manual)) {
    (Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + [System.IO.Path]::GetFileName($asset)
}
[System.IO.File]::WriteAllLines((Join-Path $outputRoot 'SHA256SUMS.txt'), $checksumLines, [System.Text.UTF8Encoding]::new($false))
Get-Item -LiteralPath $tgz, $zip, $manual | Select-Object Name, Length
