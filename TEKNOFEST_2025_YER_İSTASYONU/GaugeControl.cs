using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class GaugeControl : Control
{
    public float MinValue { get; set; } = 0;
    public float MaxValue { get; set; } = 200;

    private float value = 0;
    public float Value
    {
        get => value;
        set
        {
            float clampedValue = Math.Max(MinValue, Math.Min(MaxValue, value));
            if (Math.Abs(this.value - clampedValue) > 0.01f)
            {
                this.value = clampedValue;
                this.Invalidate();
            }
        }
    }

    private Region gaugeRegion;
    private readonly Font gaugeFont;
    private readonly Pen backPen;
    private readonly Pen greenPen;
    private readonly Pen goldPen;
    private readonly Pen redPen;
    private readonly SolidBrush backgroundBrush;
    private readonly SolidBrush textBrush;
    private readonly StringFormat centerFormat;

    public GaugeControl()
    {
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
        this.Size = new Size(200, 200);

        // GDI kaynakları
        gaugeFont = new Font("Segoe UI", 16, FontStyle.Bold);
        backPen = new Pen(Color.LightGray, 20) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        greenPen = new Pen(Color.Green, 20) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        goldPen = new Pen(Color.Gold, 20) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        redPen = new Pen(Color.OrangeRed, 20) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        backgroundBrush = new SolidBrush(Color.FromArgb(50, 50, 50));
        textBrush = new SolidBrush(Color.White);

        // Merkez hizalama
        centerFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        UpdateRegion();
    }

    private void UpdateRegion()
    {
        gaugeRegion?.Dispose(); // önceki Region'u temizle
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddEllipse(0, 0, this.Width, this.Height);
            gaugeRegion = new Region(path);
            this.Region = gaugeRegion;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Arka plan
        g.FillEllipse(backgroundBrush, this.ClientRectangle);

        float startAngle = 135f;
        float sweepAngle = 270f;
        float percent = (Value - MinValue) / (MaxValue - MinValue);
        float sweep = percent * sweepAngle;

        Rectangle rect = new Rectangle(15, 15, this.Width - 30, this.Height - 30);

        // Arka gösterge
        g.DrawArc(backPen, rect, startAngle, sweepAngle);

        // Renk seçimi
        Pen valuePen;
        if (percent < 0.4f)
            valuePen = greenPen;
        else if (percent < 0.75f)
            valuePen = goldPen;
        else
            valuePen = redPen;

        // Aktif değer gösterimi
        g.DrawArc(valuePen, rect, startAngle, sweep);

        // Ortadaki değer
        string text = $"{Value:0.0}";
        g.DrawString(text, gaugeFont, textBrush,
            this.ClientRectangle, centerFormat);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            gaugeFont?.Dispose();
            backPen?.Dispose();
            greenPen?.Dispose();
            goldPen?.Dispose();
            redPen?.Dispose();
            backgroundBrush?.Dispose();
            textBrush?.Dispose();
            centerFormat?.Dispose();
            gaugeRegion?.Dispose();
        }
        base.Dispose(disposing);
    }
}


