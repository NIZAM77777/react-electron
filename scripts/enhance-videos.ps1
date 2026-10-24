[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$FFmpegPath,

  [string]$OutputDirectory = ''
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$ffmpegExecutable = (Resolve-Path -LiteralPath $FFmpegPath).Path

if (-not (Test-Path -LiteralPath $ffmpegExecutable -PathType Leaf)) {
  throw 'FFmpegPath must point to an FFmpeg executable.'
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
  $OutputDirectory = Join-Path $projectRoot '.quality-video.local'
}

$outputRoot = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

# These are resampled, lightly sharpened copies, not native 4K source footage.
# Keep the square intro at 2160 pixels for a UHD display without the decoding
# overhead of a 3840 x 3840 frame. Do not crop the ocean's existing black bars:
# the application's existing framing and blur remain responsible for display.
$videos = @(
  @{
    Source = 'intro.mp4'
    Output = 'intro-enhanced-2160.mp4'
    Dimensions = '2160:2160'
    Timescale = '90000'
  },
  @{
    Source = 'ocean.mp4'
    Output = 'ocean-enhanced-4k.mp4'
    Dimensions = '3840:2160'
    Timescale = '15360'
  }
)

# Validate every destination before starting. Original video files are never
# overwritten, and an existing candidate must be reviewed or moved explicitly.
foreach ($video in $videos) {
  $sourcePath = Join-Path $projectRoot "public/videos/$($video.Source)"
  $outputPath = Join-Path $outputRoot $video.Output
  if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "Source video is missing: $sourcePath"
  }
  if (Test-Path -LiteralPath $outputPath) {
    throw "Output already exists; choose another output directory: $outputPath"
  }
}

foreach ($video in $videos) {
  $sourcePath = Join-Path $projectRoot "public/videos/$($video.Source)"
  $outputPath = Join-Path $outputRoot $video.Output
  $videoFilter = "scale=$($video.Dimensions):flags=lanczos+accurate_rnd+full_chroma_int,unsharp=5:5:0.2:5:5:0"

  $ffmpegArguments = @(
    '-hide_banner', '-loglevel', 'warning',
    '-i', $sourcePath,
    '-map', '0:v:0', '-map', '0:a?', '-map_metadata', '0',
    '-vf', $videoFilter,
    '-c:v', 'libx264', '-preset', 'slow', '-crf', '14',
    '-pix_fmt', 'yuv420p', '-profile:v', 'high', '-level:v', '5.1',
    '-threads', '6', '-fps_mode', 'passthrough',
    '-video_track_timescale', $video.Timescale,
    '-c:a', 'copy', '-movflags', '+faststart', '-n', $outputPath
  )

  Write-Output "Enhancing $($video.Source) -> $outputPath"
  & $ffmpegExecutable @ffmpegArguments
  if ($LASTEXITCODE -ne 0) {
    throw "FFmpeg failed for $($video.Source) with exit code $LASTEXITCODE."
  }
}
