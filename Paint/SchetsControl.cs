using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

public class HistoryAction
{
    public StartpuntTool Tool { get; }
    public Point StartPoint { get; }
    public Point EndPoint { get; }
    public Char Character { get; }

    public HistoryAction(StartpuntTool tool, Point start, Point end)
    {
        Tool = tool;
        StartPoint = start;
        EndPoint = end;
    }

    public HistoryAction(StartpuntTool tool, Point start, Char character)
    {
        Tool = tool;
        StartPoint = start;
        Character = character;
    }

    public void Draw(Graphics g, SchetsControl s)
    {
        if (Tool is TweepuntTool tweepuntTool)
        {
            tweepuntTool.Compleet(g, StartPoint, EndPoint);
        }
        else if (Tool is TekstTool tekstTool)
        {
            tekstTool.DrawLetter(s, g, Character, StartPoint);
        }
    }

    public bool CollidesWith(Point p)
    {
        if (Tool is RechthoekTool || Tool is VolRechthoekTool)
        {
            return RectangleContains(p, StartPoint, EndPoint);
        }
        else if (Tool is OvaalTool || Tool is VolOvaalTool)
        {
            return EllipseContains(p, StartPoint, EndPoint);
        }
        else if (Tool is LijnTool || Tool is PenTool)
        {
            return LineContains(p, StartPoint, EndPoint, tolerance: 3);
        }
        return false;
    }

    private bool RectangleContains(Point p, Point topLeft, Point bottomRight)
    {
        int left = Math.Min(topLeft.X, bottomRight.X);
        int right = Math.Max(topLeft.X, bottomRight.X);
        int top = Math.Min(topLeft.Y, bottomRight.Y);
        int bottom = Math.Max(topLeft.Y, bottomRight.Y);
        return p.X >= left && p.X <= right && p.Y >= top && p.Y <= bottom;
    }

    private bool EllipseContains(Point p, Point topLeft, Point bottomRight)
    {
        double centerX = (topLeft.X + bottomRight.X) / 2.0;
        double centerY = (topLeft.Y + bottomRight.Y) / 2.0;
        double radiusX = Math.Abs(bottomRight.X - topLeft.X) / 2.0;
        double radiusY = Math.Abs(bottomRight.Y - topLeft.Y) / 2.0;

        // dx en dy naar het midden van de cirkel zetten
        double dx = (p.X - centerX) / radiusX;
        double dy = (p.Y - centerY) / radiusY;
        return dx * dx + dy * dy <= 1;
    }

    private bool LineContains(Point p, Point lineStart, Point lineEnd, double tolerance)
    {
        // Algoritme dat ik online heb gevonden
        double dx = lineEnd.X - lineStart.X;
        double dy = lineEnd.Y - lineStart.Y;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
            return Distance(p, lineStart) <= tolerance;

        double t = ((p.X - lineStart.X) * dx + (p.Y - lineStart.Y) * dy) / lengthSquared;
        t = Math.Max(0, Math.Min(1, t));

        Point closestPoint = new Point((int)(lineStart.X + t * dx), (int)(lineStart.Y + t * dy));
        return Distance(p, closestPoint) <= tolerance;
    }

    private double Distance(Point p1, Point p2)
    {
        double dx = p1.X - p2.X;
        double dy = p1.Y - p2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
public class SchetsControl : UserControl
{
    private Schets schets;
    private Color penkleur;
    private List<HistoryAction> history = new List<HistoryAction>();
    public void PopHistory(object o, EventArgs ea)
    {
        if (history.Count > 0)
        {
            // Als iemand met en pen tekent, direct de hele strook verwijderen.
            if (history[history.Count - 1].Tool.ToString() == "pen")
            {
                // IndexOutOfRange omzeilen
                while (history.Count > 0 && history[history.Count - 1].Tool.ToString() == "pen")
                {
                    // Eerst de lijn verwijderen, dan checken om hangende lijnen ter groote van 1 HistoryAction te voorkomen.
                    history.RemoveAt(history.Count - 1);
                    // Als twee lijnen achter elkaar zijn getekend, verwijder ze niet. Alleen de continuë drag verwijderen.
                    // Omdat tijdens de drag de mouseDown() een korte tijd na de mouseUp() aangeroepen wordt, is het mogelijk dat bij langzame CPUs in de tussentijd de muis bewogen is, en een continuë lijn dus in meerdere continuë lijnen wordt opgesplitst, maar dit lijkt mij de beste methode.
                    if (history.Count > 1 && history[history.Count - 1].StartPoint != history[history.Count - 2].EndPoint)
                    {
                        break;
                    }
                }
            }
            else
            {
                history.RemoveAt(history.Count - 1);
                this.Invalidate();
            }
        }
    }
    public void eraseLocation(Point p)
    {
        
        for (int i = history.Count - 1; i >= 0; i--)
        {
            // Check om te zorgen dat het punt binnen de twee andere punten valt.
            if (history[i].CollidesWith(p))
            {
                history.RemoveAt(i);
                return;
            }
        }
    }
    public void AddHistory(HistoryAction action)
    {
        history.Add(action);
    }   

    public void RedrawHistory()
    {
        // Maakt eerst de bitmap leeg, en tekent dan de history.
        schets.Schoon();
        Graphics g = MaakBitmapGraphics();
        
        foreach (var action in history)
        {
            action.Draw(g, this);
        }

    }
    public Color PenKleur
    {
        get { return penkleur; }
    }
    public Schets Schets
    {
        get { return schets; }
    }
    public SchetsControl()
    {
        this.BorderStyle = BorderStyle.Fixed3D;
        this.schets = new Schets();
        this.Paint += this.teken;
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
    private void teken(object o, PaintEventArgs pea)
    {
        RedrawHistory();
        schets.Teken(pea.Graphics);
    }
    private void veranderAfmeting(object o, EventArgs ea)
    {
        schets.VeranderAfmeting(this.ClientSize);
        this.Invalidate();
    }
    public Graphics MaakBitmapGraphics()
    {
        Graphics g = schets.BitmapGraphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        return g;
    }
    public void Schoon(object o, EventArgs ea)
    {
        history.Clear();
        schets.Schoon();
        this.Invalidate();
    }
    public void Roteer(object o, EventArgs ea)
    {
        schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
        schets.Roteer();
        this.Invalidate();
    }
    public void VeranderKleur(object obj, EventArgs ea)
    {
        string kleurNaam = ((ComboBox)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }
    public void VeranderKleurViaMenu(object obj, EventArgs ea)
    {
        string kleurNaam = ((ToolStripMenuItem)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }
    public void Opslaan(object obj, EventArgs ea) // Nieuw
    {
        // Standaard Windows file-opslaan dialoog.
        SaveFileDialog dialog = new SaveFileDialog();
        // prompt de user als er een bestaand file overschreven moet worden.
        dialog.OverwritePrompt = true;
        string filetype = obj.ToString();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            switch (filetype)
            {
                case "PNG":
                    schets.KrijgBitmap().Save(dialog.FileName, ImageFormat.Png);
                    break;
                case "JPG":
                    schets.KrijgBitmap().Save(dialog.FileName, ImageFormat.Jpeg);
                    break;
                case "BMP":
                    schets.KrijgBitmap().Save(dialog.FileName);
                    break;
            }
        }
    }
}