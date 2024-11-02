using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        // Als het een TweepuntTool is, zet de Tool om, zodat de Compleet() functie aangeroepen kan worden.
        if (Tool is TweepuntTool tweepuntTool)
        {
            tweepuntTool.Compleet(g, StartPoint, EndPoint);
        }
        // Als het een TekstTool is, zet de Tool om, zodat de Letter() functie aangeroepen kan worden.
        else if (Tool is TekstTool tekstTool) 
        {
            tekstTool.Letter(s, Character);
        }
        
    }
}
public class SchetsControl : UserControl
{
    private Schets schets;
    private Color penkleur;
    private List<HistoryAction> history = new List<HistoryAction>();
    public void PopHistory()
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
                    // Als twee lijnen achter elkaar zijn getekend, verwijder ze niet. Alleen de continu� drag verwijderen.
                    // Omdat tijdens de drag de mouseDown() een korte tijd na de mouseUp() aangeroepen wordt, is het mogelijk dat bij langzame CPUs in de tussentijd de muis bewogen is, en een continu� lijn dus in meerdere continu� lijnen wordt opgesplitst, maar dit lijkt mij de beste methode.
                    if (history.Count > 1 && history[history.Count - 1].StartPoint != history[history.Count - 2].EndPoint)
                    {
                        break;
                    }
                }
            }
            else
            {
                history.RemoveAt(history.Count - 1);
            }
        }
    }
    public void AddHistory(HistoryAction action)
    {
        history.Add(action);
    }

    public List<HistoryAction> GetHistory()
    {
        return history;
    }

    public void RedrawHistory()
    {
        // Maakt eerst de bitmap leeg, en tekent dan de history.
        schets.Schoon();
        Debug.WriteLine(history.Count);
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