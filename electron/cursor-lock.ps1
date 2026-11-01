param(
  [Parameter(Mandatory = $true)][int]$X,
  [Parameter(Mandatory = $true)][int]$Y,
  [Parameter(Mandatory = $true)][int]$Width,
  [Parameter(Mandatory = $true)][int]$Height,
  [Parameter(Mandatory = $true)][int]$ParentPid
)

$source = @"
using System;
using System.Runtime.InteropServices;

public static class CursorClipper {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", EntryPoint = "ClipCursor", SetLastError = true)]
    public static extern bool ClipCursorRect(ref RECT rect);

    [DllImport("user32.dll", EntryPoint = "ClipCursor", SetLastError = true)]
    public static extern bool ReleaseClip(IntPtr rect);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public int X;
        public int Y;
    }
}
"@

Add-Type -TypeDefinition $source -ErrorAction Stop

$rect = New-Object CursorClipper+RECT
$rect.Left = $X
$rect.Top = $Y
$rect.Right = $X + $Width
$rect.Bottom = $Y + $Height

try {
  # If the pointer starts outside the kiosk display, move it inside first.
  $point = New-Object CursorClipper+POINT
  if ([CursorClipper]::GetCursorPos([ref]$point)) {
    if ($point.X -lt $rect.Left -or $point.X -ge $rect.Right -or
        $point.Y -lt $rect.Top -or $point.Y -ge $rect.Bottom) {
      $targetX = [Math]::Max($rect.Left, [Math]::Min($point.X, $rect.Right - 1))
      $targetY = [Math]::Max($rect.Top, [Math]::Min($point.Y, $rect.Bottom - 1))
      [void][CursorClipper]::SetCursorPos($targetX, $targetY)
    }
  }

  if (-not [CursorClipper]::ClipCursorRect([ref]$rect)) {
    exit 2
  }

  # Keep the clip active while Electron is alive.
  while ($true) {
    Start-Sleep -Milliseconds 250
    try {
      Get-Process -Id $ParentPid -ErrorAction Stop | Out-Null
    } catch {
      break
    }
  }
}
finally {
  # Release the system-wide cursor clip when Electron exits.
  [void][CursorClipper]::ReleaseClip([IntPtr]::Zero)
}
