[CmdletBinding(PositionalBinding=$false)]
param(
    [string] $CodeS3Bucket
)

$buildId =  (Get-Date -Format "yyyy-MM-dd-HH-mm")

Add-Type -assembly "System.IO.Compression.Filesystem"

if (!(Test-Path '_builds\')) {
    New-Item -ItemType Directory -Path .\_builds\ | Out-Null
}

Write-Host "Building version " $buildId -ForegroundColor Yellow

$apiFilename = "SesCollector-Api-" + $buildId + ".zip"
$collectorFilename = "SesCollector-Collector-" + $buildId + ".zip"

dotnet publish .\src\SesCollector.Api\SesCollector.Api.csproj -o .\_deploy\Api
dotnet publish .\src\SesCollector.Collector\SesCollector.Collector.csproj -o .\_deploy\Collector

Write-Host "    > Project build and publish complete" -ForegroundColor Cyan

$apiSource = Resolve-Path _deploy\Api
$collectorSource = Resolve-Path _deploy\Collector

$buildPath = (Resolve-Path _builds\).Path
$apiDestination = $buildPath + $apiFilename
$collectorDestination = $buildPath + $collectorFilename


Write-Host "Compressing artifacts" -ForegroundColor Yellow

[io.compression.zipfile]::CreateFromDirectory($apiSource, $apiDestination)
[io.compression.zipfile]::CreateFromDirectory($collectorSource, $collectorDestination)

Write-Host "    > Artifacts compressed" -ForegroundColor Cyan

Write-Host "Uploading artifacts to S3 bucket" -ForegroundColor Yellow

aws s3 cp $apiDestination s3://$CodeS3Bucket/$apiFilename
aws s3 cp $collectorDestination s3://$CodeS3Bucket/$collectorFilename

Write-Host "    > Artifacts uploaded to S3." -ForegroundColor Cyan
