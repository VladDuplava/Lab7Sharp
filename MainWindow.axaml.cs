using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

// Аліас щоб уникнути конфлікту Path (Avalonia) vs System.IO.Path
using ShapePath = Avalonia.Controls.Shapes.Path;

namespace LabAvalonia;

// ───────────────────────────────────────────
// Модель для рядка RichTextBox (завдання 1.5)
// ───────────────────────────────────────────
public class RichTextEntry
{
    public string Time { get; set; } = "";
    public string Text { get; set; } = "";
}

// ───────────────────────────────────────────
// Абстрактний базовий клас фігури (завдання 3.5)
// ───────────────────────────────────────────
public abstract class ShapeBase
{
    public double X { get; set; }
    public double Y { get; set; }
    public Color FillColor { get; set; }
    public Color StrokeColor { get; set; } = Colors.Black;
    public double StrokeThickness { get; set; } = 2;

    public abstract void Draw(Canvas canvas);

    public virtual void Move(double dx, double dy)
    {
        X += dx;
        Y += dy;
    }

    protected SolidColorBrush FillBrush   => new SolidColorBrush(FillColor);
    protected SolidColorBrush StrokeBrush => new SolidColorBrush(StrokeColor);
}

// ── Круг ──────────────────────────────────
public class CircleShape : ShapeBase
{
    public double Radius { get; set; }

    public override void Draw(Canvas canvas)
    {
        var ellipse = new Ellipse
        {
            Width  = Radius * 2,
            Height = Radius * 2,
            Fill   = FillBrush,
            Stroke = StrokeBrush,
            StrokeThickness = StrokeThickness
        };
        Canvas.SetLeft(ellipse, X - Radius);
        Canvas.SetTop(ellipse,  Y - Radius);
        canvas.Children.Add(ellipse);
        AddLabel(canvas, "Круг", X - Radius, Y + Radius + 2);
    }

    private static void AddLabel(Canvas canvas, string text, double lx, double ly)
    {
        var tb = new TextBlock
        {
            Text       = text,
            FontSize   = 10,
            Foreground = Brushes.DarkSlateGray
        };
        Canvas.SetLeft(tb, lx);
        Canvas.SetTop(tb,  ly);
        canvas.Children.Add(tb);
    }
}

// ── Сектор ────────────────────────────────
public class SectorShape : ShapeBase
{
    public double Radius     { get; set; }
    public double StartAngle { get; set; }
    public double SweepAngle { get; set; }

    public override void Draw(Canvas canvas)
    {
        double startRad = StartAngle * Math.PI / 180.0;
        double endRad   = (StartAngle + SweepAngle) * Math.PI / 180.0;

        double x1 = X + Radius * Math.Cos(startRad);
        double y1 = Y + Radius * Math.Sin(startRad);
        double x2 = X + Radius * Math.Cos(endRad);
        double y2 = Y + Radius * Math.Sin(endRad);

        var geometry = new PathGeometry();
        var figure   = new PathFigure
        {
            StartPoint = new Point(X, Y),
            IsClosed   = true,
            IsFilled   = true
        };
        figure.Segments!.Add(new LineSegment { Point = new Point(x1, y1) });
        figure.Segments.Add(new ArcSegment
        {
            Point          = new Point(x2, y2),
            Size           = new Size(Radius, Radius),
            IsLargeArc     = SweepAngle > 180,
            SweepDirection = SweepDirection.Clockwise
        });
        geometry.Figures!.Add(figure);

        var path = new ShapePath
        {
            Data            = geometry,
            Fill            = FillBrush,
            Stroke          = StrokeBrush,
            StrokeThickness = StrokeThickness
        };
        canvas.Children.Add(path);

        var tb = new TextBlock { Text = "Сектор", FontSize = 10, Foreground = Brushes.DarkSlateGray };
        Canvas.SetLeft(tb, X - 15);
        Canvas.SetTop(tb,  Y + Radius + 2);
        canvas.Children.Add(tb);
    }
}

// ── Зафарбований прямокутник ──────────────
public class FilledRectShape : ShapeBase
{
    public double Width  { get; set; }
    public double Height { get; set; }

    public override void Draw(Canvas canvas)
    {
        var rect = new Rectangle
        {
            Width           = Width,
            Height          = Height,
            Fill            = FillBrush,
            Stroke          = StrokeBrush,
            StrokeThickness = StrokeThickness
        };
        Canvas.SetLeft(rect, X);
        Canvas.SetTop(rect,  Y);
        canvas.Children.Add(rect);

        var tb = new TextBlock { Text = "Прямокутник", FontSize = 10, Foreground = Brushes.DarkSlateGray };
        Canvas.SetLeft(tb, X);
        Canvas.SetTop(tb,  Y + Height + 2);
        canvas.Children.Add(tb);
    }
}

// ── Зірка (5-кутна) ───────────────────────
public class StarShape : ShapeBase
{
    public double OuterRadius { get; set; }
    public double InnerRadius { get; set; }
    public int    Points      { get; set; } = 5;

    public override void Draw(Canvas canvas)
    {
        var pts  = new List<Point>();
        double step = Math.PI / Points;

        for (int i = 0; i < Points * 2; i++)
        {
            double angle = i * step - Math.PI / 2;
            double r     = (i % 2 == 0) ? OuterRadius : InnerRadius;
            pts.Add(new Point(X + r * Math.Cos(angle), Y + r * Math.Sin(angle)));
        }

        var geometry = new PathGeometry();
        var figure   = new PathFigure { StartPoint = pts[0], IsClosed = true, IsFilled = true };
        for (int i = 1; i < pts.Count; i++)
            figure.Segments!.Add(new LineSegment { Point = pts[i] });
        geometry.Figures!.Add(figure);

        var path = new ShapePath
        {
            Data            = geometry,
            Fill            = FillBrush,
            Stroke          = StrokeBrush,
            StrokeThickness = StrokeThickness
        };
        canvas.Children.Add(path);

        var tb = new TextBlock { Text = "Зірка", FontSize = 10, Foreground = Brushes.DarkSlateGray };
        Canvas.SetLeft(tb, X - 15);
        Canvas.SetTop(tb,  Y + OuterRadius + 2);
        canvas.Children.Add(tb);
    }
}

// ───────────────────────────────────────────
// Головне вікно
// ───────────────────────────────────────────
public partial class MainWindow : Window
{
    // ----- Завдання 1.5 -----
    private readonly ObservableCollection<RichTextEntry> _entries = new();

    // ----- Завдання 2.5 -----
    private byte[]?          _originalPixels;
    private int              _bmpWidth, _bmpHeight;
    private WriteableBitmap? _originalBitmap;

    // ----- Завдання 3.5 -----
    private readonly Random          _rng    = new();
    private readonly List<ShapeBase> _shapes = new();

    public MainWindow()
    {
        InitializeComponent();
        RichTextList.ItemsSource = _entries;
    }

    // ════════════════════════════════════════
    // ЗАВДАННЯ 1.5 — TextBox → RichTextBox
    // ════════════════════════════════════════

    private void OnAddSentence(object? sender, RoutedEventArgs e)
    {
        var text = InputTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            StatusLabel1.Text = "Введіть речення перед додаванням.";
            return;
        }

        var start = DateTime.Now;
        _entries.Add(new RichTextEntry
        {
            Time = start.ToString("[HH:mm:ss.fff]"),
            Text = text
        });
        var elapsed = (DateTime.Now - start).TotalMilliseconds;

        InputTextBox.Text = "";
        StatusLabel1.Text = $"Додано о {start:HH:mm:ss.fff} | Час операції: {elapsed:F3} мс | Рядків: {_entries.Count}";
        RichScroll.ScrollToEnd();
    }

    private void OnClearRich(object? sender, RoutedEventArgs e)
    {
        _entries.Clear();
        StatusLabel1.Text = "Список очищено.";
    }

    // ════════════════════════════════════════
    // ЗАВДАННЯ 2.5 — BMP пригнічення каналів
    // ════════════════════════════════════════

    private async void OnOpenBmp(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Відкрити зображення",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images") { Patterns = new[] { "*.bmp", "*.png", "*.jpg", "*.jpeg" } }
            }
        });

        if (files.Count == 0) return;

        try
        {
            await using var stream = await files[0].OpenReadAsync();
            var bitmap = new Bitmap(stream);
            _bmpWidth  = bitmap.PixelSize.Width;
            _bmpHeight = bitmap.PixelSize.Height;

            _originalBitmap = new WriteableBitmap(
                bitmap.PixelSize, bitmap.Dpi,
                Avalonia.Platform.PixelFormat.Bgra8888);

            using (var fb = _originalBitmap.Lock())
            {
                bitmap.CopyPixels(
                    new PixelRect(0, 0, _bmpWidth, _bmpHeight),
                    fb.Address, fb.RowBytes * _bmpHeight, fb.RowBytes);
            }

            int byteCount = _bmpWidth * _bmpHeight * 4;
            _originalPixels = new byte[byteCount];
            using (var fb = _originalBitmap.Lock())
            {
                System.Runtime.InteropServices.Marshal.Copy(
                    fb.Address, _originalPixels, 0, byteCount);
            }

            OriginalImage.Source = _originalBitmap;
            ResultImage.Source   = null;
            BmpInfoLabel.Text    = $"Файл: {files[0].Name}\nРозмір: {_bmpWidth}x{_bmpHeight} пкс";
            StatusLabel2.Text    = "Зображення завантажено успішно.";
        }
        catch (Exception ex)
        {
            StatusLabel2.Text = $"Помилка: {ex.Message}";
        }
    }

    private void OnApplyChannel(object? sender, RoutedEventArgs e)
    {
        if (_originalPixels == null || _originalBitmap == null)
        {
            StatusLabel2.Text = "Спочатку завантажте зображення.";
            return;
        }

        // BGRA: індекс 0=B, 1=G, 2=R, 3=A
        int    suppressChannel;
        string channelName;

        if (RadioRed.IsChecked == true)
        {
            suppressChannel = 2;
            channelName = "Червоний (R)";
        }
        else if (RadioGreen.IsChecked == true)
        {
            suppressChannel = 1;
            channelName = "Зелений (G)";
        }
        else
        {
            suppressChannel = 0;
            channelName = "Синій (B)";
        }

        var start    = DateTime.Now;
        var modified = (byte[])_originalPixels.Clone();

        for (int i = 0; i < modified.Length; i += 4)
            modified[i + suppressChannel] = 0;

        var resultBitmap = new WriteableBitmap(
            _originalBitmap.PixelSize, _originalBitmap.Dpi,
            Avalonia.Platform.PixelFormat.Bgra8888);

        using (var fb = resultBitmap.Lock())
            System.Runtime.InteropServices.Marshal.Copy(
                modified, 0, fb.Address, modified.Length);

        ResultImage.Source = resultBitmap;
        var elapsed = (DateTime.Now - start).TotalMilliseconds;
        StatusLabel2.Text = $"Канал '{channelName}' пригнічено | Час: {elapsed:F1} мс";
    }

    private async void OnSaveBmp(object? sender, RoutedEventArgs e)
    {
        if (ResultImage.Source is not WriteableBitmap result)
        {
            StatusLabel2.Text = "Спочатку застосуйте ефект.";
            return;
        }

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Зберегти результат",
            SuggestedFileName = "result",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PNG Image") { Patterns = new[] { "*.png" } }
            }
        });

        if (file == null) return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            result.Save(stream);
            StatusLabel2.Text = $"Збережено: {file.Name}";
        }
        catch (Exception ex)
        {
            StatusLabel2.Text = $"Помилка збереження: {ex.Message}";
        }
    }

    // ════════════════════════════════════════
    // ЗАВДАННЯ 3.5 — Малювання фігур
    // ════════════════════════════════════════

    private void OnGenerateShapes(object? sender, RoutedEventArgs e)
    {
        DrawingCanvas.Children.Clear();
        _shapes.Clear();

        double w = DrawingCanvas.Bounds.Width  > 10 ? DrawingCanvas.Bounds.Width  : 700;
        double h = DrawingCanvas.Bounds.Height > 10 ? DrawingCanvas.Bounds.Height : 400;

        int count = 3; // по 3 штуки кожного типу

        for (int i = 0; i < count; i++)
        {
            // Круг
            double cr = _rng.Next(20, 50);
            _shapes.Add(new CircleShape
            {
                X = Clamp(_rng.NextDouble() * w, cr, w - cr),
                Y = Clamp(_rng.NextDouble() * h, cr, h - cr - 20),
                Radius    = cr,
                FillColor = RandomColor()
            });

            // Сектор
            double sr = _rng.Next(25, 55);
            _shapes.Add(new SectorShape
            {
                X = Clamp(_rng.NextDouble() * w, sr, w - sr),
                Y = Clamp(_rng.NextDouble() * h, sr, h - sr - 20),
                Radius     = sr,
                StartAngle = _rng.Next(0, 360),
                SweepAngle = _rng.Next(45, 270),
                FillColor  = RandomColor()
            });

            // Прямокутник
            double rw = _rng.Next(40, 100), rh = _rng.Next(30, 70);
            _shapes.Add(new FilledRectShape
            {
                X = Clamp(_rng.NextDouble() * w, 0, w - rw),
                Y = Clamp(_rng.NextDouble() * h, 0, h - rh - 20),
                Width     = rw,
                Height    = rh,
                FillColor = RandomColor()
            });

            // Зірка
            double outerR = _rng.Next(25, 50);
            _shapes.Add(new StarShape
            {
                X = Clamp(_rng.NextDouble() * w, outerR, w - outerR),
                Y = Clamp(_rng.NextDouble() * h, outerR, h - outerR - 20),
                OuterRadius = outerR,
                InnerRadius = outerR * 0.4,
                FillColor   = RandomColor()
            });
        }

        foreach (var shape in _shapes)
            shape.Draw(DrawingCanvas);

        ShapeCountLabel.Text = $"Фігур: {_shapes.Count}  (по {count} кожного типу)";
        StatusLabel3.Text    = $"Згенеровано о {DateTime.Now:HH:mm:ss}";
    }

    private void OnClearShapes(object? sender, RoutedEventArgs e)
    {
        DrawingCanvas.Children.Clear();
        _shapes.Clear();
        ShapeCountLabel.Text = "";
        StatusLabel3.Text    = "Полотно очищено.";
    }

    // ── Утиліти ───────────────────────────────

    private Color RandomColor(byte alpha = 210) =>
        Color.FromArgb(alpha,
            (byte)_rng.Next(50, 230),
            (byte)_rng.Next(50, 230),
            (byte)_rng.Next(50, 230));

    private static double Clamp(double v, double min, double max) =>
        max < min ? min : Math.Max(min, Math.Min(max, v));
}
