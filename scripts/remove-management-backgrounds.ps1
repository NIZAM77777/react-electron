Add-Type -AssemblyName System.Drawing

$source = @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public static class PortraitBackgroundRemover
{
    private static byte Median(byte[] values, int count)
    {
        var copy = new byte[count];
        Array.Copy(values, copy, count);
        Array.Sort(copy);
        return copy[count / 2];
    }

    public static void Process(string inputPath, string outputPath)
    {
        using (var input = new Bitmap(inputPath))
        using (var bitmap = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb))
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(input, 0, 0, input.Width, input.Height);
            }

            int width = bitmap.Width;
            int height = bitmap.Height;
            int total = width * height;
            var rect = new Rectangle(0, 0, width, height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            var pixels = new byte[stride * height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

            var reds = new byte[100000];
            var greens = new byte[100000];
            var blues = new byte[100000];
            int sampleCount = 0;
            int patchW = Math.Max(3, (int)(width * 0.055));
            int patchH = Math.Max(3, (int)(height * 0.055));

            Action<int, int> sample = (x, y) =>
            {
                int offset = y * stride + x * 4;
                blues[sampleCount] = pixels[offset];
                greens[sampleCount] = pixels[offset + 1];
                reds[sampleCount] = pixels[offset + 2];
                sampleCount++;
            };

            for (int y = 0; y < patchH; y += 2)
            {
                for (int x = 0; x < patchW; x += 2)
                {
                    sample(x, y);
                    sample(width - 1 - x, y);
                }
            }
            for (int y = (int)(height * 0.18); y < (int)(height * 0.48); y += 4)
            {
                for (int x = 0; x < Math.Max(2, (int)(width * 0.018)); x += 2)
                {
                    sample(x, y);
                    sample(width - 1 - x, y);
                }
            }

            byte bgR = Median(reds, sampleCount);
            byte bgG = Median(greens, sampleCount);
            byte bgB = Median(blues, sampleCount);
            var background = new byte[total];
            var queued = new byte[total];
            var queue = new int[total];
            int head = 0;
            int tail = 0;

            Func<int, double> distance = index =>
            {
                int y = index / width;
                int x = index - y * width;
                int offset = y * stride + x * 4;
                double db = pixels[offset] - bgB;
                double dg = pixels[offset + 1] - bgG;
                double dr = pixels[offset + 2] - bgR;
                return Math.Sqrt(dr * dr * 0.30 + dg * dg * 0.59 + db * db * 0.11);
            };

            Action<int> enqueue = null;
            enqueue = index =>
            {
                if (index < 0 || index >= total || queued[index] != 0 || distance(index) >= 36) return;
                queued[index] = 1;
                queue[tail++] = index;
            };

            for (int x = 0; x < width; x++) enqueue(x);
            for (int y = 0; y < height; y++)
            {
                enqueue(y * width);
                enqueue(y * width + width - 1);
            }
            for (int x = 0; x < (int)(width * 0.12); x++) enqueue((height - 1) * width + x);
            for (int x = (int)(width * 0.88); x < width; x++) enqueue((height - 1) * width + x);

            while (head < tail)
            {
                int index = queue[head++];
                background[index] = 1;
                int x = index % width;
                if (x > 0) enqueue(index - 1);
                if (x < width - 1) enqueue(index + 1);
                if (index >= width) enqueue(index - width);
                if (index < total - width) enqueue(index + width);
            }

            string fileName = Path.GetFileName(inputPath).ToLowerInvariant();
            bool preserveHeadwear =
                fileName.StartsWith("moosa-") ||
                fileName.StartsWith("ghanem.") ||
                fileName.StartsWith("mohammed-");

            for (int index = 0; index < total; index++)
            {
                int y = index / width;
                int x = index - y * width;
                int offset = y * stride + x * 4;
                double u = width > 1 ? (double)x / (width - 1) : 0.5;
                double v = height > 1 ? (double)y / (height - 1) : 0.5;

                double headRx = preserveHeadwear ? 0.35 : 0.285;
                double headRy = preserveHeadwear ? 0.315 : 0.30;
                double headDx = (u - 0.5) / headRx;
                double headDy = (v - 0.285) / headRy;
                double headMetric = headDx * headDx + headDy * headDy;
                bool inHead = headMetric <= 1.0;

                double innerDx = (u - 0.5) / 0.18;
                double innerDy = (v - 0.34) / 0.20;
                bool inInnerHead = innerDx * innerDx + innerDy * innerDy <= 1.0;

                double bodyProgress = Math.Max(0, Math.Min(1, (v - 0.44) / 0.56));
                double bodyHalfWidth = 0.14 + bodyProgress * 0.40;
                bool inBody = v >= 0.44 && Math.Abs(u - 0.5) <= bodyHalfWidth;
                bool inSilhouette = inHead || inBody;

                if (!inSilhouette)
                {
                    pixels[offset + 3] = 0;
                    continue;
                }

                bool protectedSubject = inBody || inInnerHead || (preserveHeadwear && inHead);
                if (background[index] != 0 && !protectedSubject)
                {
                    pixels[offset + 3] = 0;
                    continue;
                }

                double headAlpha = inHead ? Math.Min(1.0, Math.Max(0, (1.0 - headMetric) / 0.075)) : 0;
                double bodyDistance = bodyHalfWidth - Math.Abs(u - 0.5);
                double bodyAlpha = inBody ? Math.Min(1.0, Math.Max(0, bodyDistance / 0.022)) : 0;
                double shapeAlpha = Math.Max(headAlpha, bodyAlpha);
                pixels[offset + 3] = (byte)Math.Round(255 * shapeAlpha);
            }

            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);
            bitmap.Save(outputPath, ImageFormat.Png);
        }
    }
}
'@

$runtimeDirectory = Split-Path -Parent (Get-Process -Id $PID).Path
$references = @(
    [System.Object].Assembly.Location,
    [System.Console].Assembly.Location,
    [System.Collections.Generic.List[int]].Assembly.Location,
    [System.Linq.Enumerable].Assembly.Location,
    [System.Runtime.InteropServices.Marshal].Assembly.Location,
    (Join-Path $runtimeDirectory 'System.Drawing.Common.dll'),
    [System.Drawing.Rectangle].Assembly.Location,
    (Join-Path $runtimeDirectory 'System.Private.Windows.GdiPlus.dll'),
    (Join-Path $runtimeDirectory 'System.Private.Windows.Core.dll')
) | Select-Object -Unique

Add-Type -TypeDefinition $source -ReferencedAssemblies $references -ErrorAction Stop

$assetDirectory = Join-Path $PSScriptRoot '..\public\images\company\management'
$portraits = @(
    'venu.jpg',
    'firas.jpg',
    'abin.jpg',
    'moosa-khalfan.jpg',
    'ghanem.jpg',
    'anil.jpg',
    'anita.jpg',
    'benny.jpg',
    'rakhesh.jpg',
    'fabin.png',
    'mohammed-ahmed.jpg',
    'manzoor.jpg',
    'whatsapp.jpg'
)

foreach ($portrait in $portraits) {
    $inputPath = Join-Path $assetDirectory $portrait
    $outputName = "{0}-transparent.png" -f [System.IO.Path]::GetFileNameWithoutExtension($portrait)
    $outputPath = Join-Path $assetDirectory $outputName
    [PortraitBackgroundRemover]::Process($inputPath, $outputPath)
    Write-Output $outputName
}

