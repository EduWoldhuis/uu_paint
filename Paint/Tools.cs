using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;


public interface ISchetsTool
{
    void MuisVast(SchetsControl s, Point p);
    void MuisDrag(SchetsControl s, Point p);
    void MuisLos(SchetsControl s, Point p);
    void Letter(SchetsControl s, char c);

}

public abstract class StartpuntTool : ISchetsTool
{
    protected Point startpunt;
    protected Brush kwast;
    
    public virtual void MuisVast(SchetsControl s, Point p)
    {
        startpunt = p;
    }
    public virtual void MuisLos(SchetsControl s, Point p)
    {
        kwast = new SolidBrush(s.PenKleur);
    }
    public abstract void MuisDrag(SchetsControl s, Point p);
    public abstract void Letter(SchetsControl s, char c);
}

public class TekstTool : StartpuntTool
{
    public override string ToString() { return "tekst"; }

    public override void MuisDrag(SchetsControl s, Point p) { }

    public override void Letter(SchetsControl s, char c)
    {
        Graphics gr = s.MaakBitmapGraphics();

        Font font = new Font("Tahoma", 40);
        this.startpunt.X += (int)gr.MeasureString(c.ToString(), font, this.startpunt, StringFormat.GenericTypographic).Width;

        HistoryAction action = new HistoryAction(this, this.startpunt, c, s.PenKleur);
        s.AddHistory(action);
        s.Invalidate();
    }
    public void DrawLetter(SchetsControl s, Graphics g, char c, Point startpunt)
    {
        if (c >= 32)
        {
            if (kwast == null)
            {
                kwast = new SolidBrush(s.PenKleur);
            }
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();
            SizeF sz = g.MeasureString(tekst, font, startpunt, StringFormat.GenericTypographic);
            g.DrawString(tekst, font, kwast, startpunt, StringFormat.GenericTypographic);
        }
    }
}

public abstract class TweepuntTool : StartpuntTool
{
    public static Rectangle Punten2Rechthoek(Point p1, Point p2)
    {
        return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y)), new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y)));
    }
    public static Pen MaakPen(Brush b, int dikte)
    {
        Pen pen = new Pen(b, dikte);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        return pen;
    }
    public override void MuisVast(SchetsControl s, Point p)
    {
        base.MuisVast(s, p);
        kwast = Brushes.Gray;
    }
    public override void MuisDrag(SchetsControl s, Point p)
    {
        s.Refresh();
        this.Bezig(s.CreateGraphics(), this.startpunt, p);
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        base.MuisLos(s, p);
        HistoryAction action = new HistoryAction(this, this.startpunt, p, s.PenKleur);

        if (this.ToString() == "gum")
        {
            s.eraseLocation(p);
            Debug.WriteLine("GUM");
        }
        else
        {
            s.AddHistory(action);
        }
        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c)
    {
    }
    public abstract void Bezig(Graphics g, Point p1, Point p2);

    public virtual void Compleet(Graphics g, Point p1, Point p2)
    {
        this.Bezig(g, p1, p2);
    }
}

public class RechthoekTool : TweepuntTool
{
    public override string ToString() { return "kader"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class VolRechthoekTool : RechthoekTool
{
    public override string ToString() { return "vlak"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class OvaalTool : TweepuntTool
{
    public override string ToString() { return "ovaal"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class VolOvaalTool : OvaalTool
{
    public override string ToString() { return "volOvaal"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class LijnTool : TweepuntTool
{
    public override string ToString() { return "lijn"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        g.DrawLine(MaakPen(this.kwast, 3), p1, p2);
    }
}

public class PenTool : LijnTool
{
    public override string ToString() { return "pen"; }

    public override void MuisDrag(SchetsControl s, Point p)
    {
        this.MuisLos(s, p);
        this.MuisVast(s, p);
    }
}

public class GumTool : PenTool
{
    public override string ToString() { return "gum"; }
    public override void MuisVast(SchetsControl s, Point p)
    {
        base.MuisLos(s, p);
    }
    public override void MuisLos(SchetsControl s, Point p)
    {
        
    }
    public override void MuisDrag(SchetsControl s, Point p)
    {
        
    }
    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        g.DrawLine(MaakPen(Brushes.White, 7), p1, p2);
    }
}