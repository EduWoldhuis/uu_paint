using System;
using System.Collections.Generic;
using System.Drawing;

public interface drawInterface
{
    void teken();
    string returnTool();
}
public class twoPointObject : drawInterface
{
    public void teken()
    {

    }
    public string returnTool()
    {
        return tool;
    }
    public string tool;
    public Point p1;
    public Point p2;
}

public class textObject : drawInterface
{
    public void teken()
    {

    }
    public string returnTool()
    {
        return tool;
    }
    public string tool;
    public Point startpoint;
    public string text;
}
public class Schets
{
    private Bitmap bitmap;
    public List<StartpuntTool> history = new List<StartpuntTool>();

    public Schets()
    {
        bitmap = new Bitmap(1, 1);
    }
    public Graphics BitmapGraphics
    {
        get { return Graphics.FromImage(bitmap); }
    }
    public void VeranderAfmeting(Size sz)
    {
        if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
        {
            Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                     , Math.Max(sz.Height, bitmap.Size.Height)
                                     );
            Graphics gr = Graphics.FromImage(nieuw);
            gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
            gr.DrawImage(bitmap, 0, 0);
            bitmap = nieuw;
        }
    }
    public void Teken(Graphics gr)
    {
        gr.DrawImage(bitmap, 0, 0);
    }
    public void Schoon()
    {
        Graphics gr = Graphics.FromImage(bitmap);
        gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
    }
    public void Roteer()
    {
        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
    }
    public Bitmap KrijgBitmap()
    {
        return bitmap;
    }
}