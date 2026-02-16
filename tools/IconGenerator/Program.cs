using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;

// Carregar a imagem de fundo (Instaldor.png)
string bgPath = Path.Combine(Directory.GetCurrentDirectory(), "Instaldor.png");
if (!File.Exists(bgPath))
{
    Console.WriteLine($"ERRO: Instaldor.png nao encontrado em {bgPath}");
    return 1;
}

Console.WriteLine($"Usando fundo: {Path.GetFullPath(bgPath)}");
using var background = new Bitmap(bgPath);

int[] sizes = [16, 24, 32, 48, 64, 128, 256];
var bitmaps = new List<Bitmap>();

foreach (int size in sizes)
{
    var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(bmp);

    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    g.Clear(Color.Transparent);

    // Desenhar a imagem de fundo redimensionada
    g.DrawImage(background, 0, 0, size, size);

    // Desenhar a letra Q centralizada com sombra
    float fontSize = size * 0.55f;
    using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
    var format = new StringFormat
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
    };
    var textRect = new RectangleF(0, -size * 0.01f, size, size);

    // Sombra sutil em tamanhos maiores
    if (size >= 48)
    {
        float shadowOffset = size * 0.02f;
        var shadowRect = new RectangleF(shadowOffset, shadowOffset - size * 0.01f, size, size);
        using var shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
        g.DrawString("Q", font, shadowBrush, shadowRect, format);
    }

    // Letra branca
    using var textBrush = new SolidBrush(Color.White);
    g.DrawString("Q", font, textBrush, textRect, format);

    bitmaps.Add(bmp);
}

// Salvar icone no Assets e no installer
string icoAssets = Path.Combine(Directory.GetCurrentDirectory(),
    "src", "QuizCraft.Presentation", "Assets", "quizcraft.ico");
string icoInstaller = Path.Combine(Directory.GetCurrentDirectory(),
    "installer", "quizcraft.ico");

WriteIco(icoAssets, bitmaps);
Console.WriteLine($"Icone criado: {Path.GetFullPath(icoAssets)}");

File.Copy(icoAssets, icoInstaller, true);
Console.WriteLine($"Icone copiado: {Path.GetFullPath(icoInstaller)}");

foreach (var bmp in bitmaps) bmp.Dispose();
Console.WriteLine("Pronto!");
return 0;

static void WriteIco(string path, List<Bitmap> bitmaps)
{
    var dir = Path.GetDirectoryName(path);
    if (!string.IsNullOrEmpty(dir))
        Directory.CreateDirectory(dir);

    var pngDataList = new List<byte[]>();
    foreach (var bmp in bitmaps)
    {
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        pngDataList.Add(ms.ToArray());
    }

    using var fs = new FileStream(path, FileMode.Create);
    using var bw = new BinaryWriter(fs);

    int imageCount = bitmaps.Count;
    int dataOffset = 6 + (16 * imageCount);

    // ICO Header
    bw.Write((short)0);
    bw.Write((short)1);
    bw.Write((short)imageCount);

    // Directory entries
    int currentOffset = dataOffset;
    for (int i = 0; i < imageCount; i++)
    {
        int size = bitmaps[i].Width;
        bw.Write((byte)(size >= 256 ? 0 : size));
        bw.Write((byte)(size >= 256 ? 0 : size));
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((short)1);
        bw.Write((short)32);
        bw.Write(pngDataList[i].Length);
        bw.Write(currentOffset);
        currentOffset += pngDataList[i].Length;
    }

    // Image data (PNG)
    foreach (var pngData in pngDataList)
        bw.Write(pngData);
}
