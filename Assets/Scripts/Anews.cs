using System;
using System.Collections;
using System.Collections.Generic;

public enum Compass
{
    N, NE, E, SE, S, SW, W, NW, Centre
}

public static class Directions
{
    public static int CompassFullSize = Enum.GetNames(typeof(Compass)).Length;
    public static int CompassSize = CompassSize - 1; // exclude Centre 

    public static Compass Add(this Compass direction, int toRight)
    {
        int value = (int)direction + toRight;
        value %= CompassSize;
        return (Compass)value;
    }
}

public class Corner
{
    public float[] corners = new float[4];
    public ulong index = 0;

    public Corner(int nw, int ne, int se, int sw, uint nwi, uint nei, uint sei, uint swi)
    {
        uint factor = (uint)IslandNoiseSettings.maxLevel;
        corners[0] = nw / 4f;
        corners[1] = ne / 4f;
        corners[2] = se / 4f;
        corners[3] = sw / 4f;
        index = swi + 
            factor * sei + 
            factor * factor * nei + 
            factor * factor * factor * nwi;
    }
}

public class Anews 
{
    private string tostring;

    private int[] edges = new int[Directions.CompassFullSize];
    private uint[] indexs = new uint[Directions.CompassFullSize];

    private string ShowEdges()
    {
        string value = "{" + 
            edges[(int)Compass.NW] +
            edges[(int)Compass.N] +
            edges[(int)Compass.NE] + "/" +
            edges[(int)Compass.W] +
            edges[(int)Compass.Centre] +
            edges[(int)Compass.E] + "/" +
            edges[(int)Compass.SW] +
            edges[(int)Compass.S] +
            edges[(int)Compass.SE] +
            "}";
        return value;
    }

    private int Extreme(int value)
    {
        return (value > 0) ? value + IslandNoiseSettings.levels : 0;
    }

    public Anews(int nw, int n, int ne, int w, int c, int e, int sw, int s, int se)
    {
        int nwv = Extreme(nw); 
        int nv = Extreme(n);
        int nev = Extreme(ne);
        int wv = Extreme(w);
        int cv = Extreme(c);
        int ev = Extreme(e);
        int swv = Extreme(sw);
        int sv = Extreme(s);
        int sev = Extreme(se);
        edges[(int)Compass.Centre] = 4 * cv;
        edges[(int)Compass.N] = 2 * (nv + cv);
        edges[(int)Compass.NE] = nev + cv + nv + ev;
        edges[(int)Compass.E] = 2 * (ev + cv);
        edges[(int)Compass.SE] = sev + cv + sv + ev;
        edges[(int)Compass.S] = 2 * (sv + cv);
        edges[(int)Compass.SW] = swv + cv + sv + wv;
        edges[(int)Compass.W] = 2 * (wv + cv);
        edges[(int)Compass.NW] = nwv + cv + nv + wv;

        uint nwi = (uint) nw;
        uint ni = (uint) n;
        uint nei = (uint) ne;
        uint wi = (uint) w;
        uint ci = (uint) c;
        uint ei = (uint) e;
        uint swi = (uint) sw;
        uint si = (uint) s;
        uint sei = (uint) se;
        indexs[(int)Compass.Centre] = 4 * ci;
        indexs[(int)Compass.N] = 2 * (ni + ci);
        indexs[(int)Compass.NE] = nei + ci + ni + ei;
        indexs[(int)Compass.E] = 2 * (ei + ci);
        indexs[(int)Compass.SE] = sei + ci + si + ei;
        indexs[(int)Compass.S] = 2 * (si + ci);
        indexs[(int)Compass.SW] = swi + ci + si + wi;
        indexs[(int)Compass.W] = 2 * (wi + ci);
        indexs[(int)Compass.NW] = nwi + ci + ni + wi;

        tostring = "<" + nw + n + ne +
            "/" + w + c + e +
            "/" + sw + s + se +
            ">" + ShowEdges();
    }

    public Corner GetCorner(CornorDirection cornorDirection)
    {
        Corner corner = new Corner(0, 0, 0, 0, 0, 0, 0, 0);
        switch (cornorDirection)
        {
            case CornorDirection.NW:
                corner = new Corner(edges[(int)Compass.NW], edges[(int)Compass.N], edges[(int)Compass.Centre], edges[(int)Compass.W],
                    indexs[(int)Compass.NW], indexs[(int)Compass.N], indexs[(int)Compass.Centre], indexs[(int)Compass.W]);
                break;
            case CornorDirection.NE:
                corner = new Corner(edges[(int)Compass.N], edges[(int)Compass.NE], edges[(int)Compass.E], edges[(int)Compass.Centre],
                    indexs[(int)Compass.N], indexs[(int)Compass.NE], indexs[(int)Compass.E], indexs[(int)Compass.Centre]); 
                break;
            case CornorDirection.SE:
                corner = new Corner(edges[(int)Compass.Centre], edges[(int)Compass.E], edges[(int)Compass.SE], edges[(int)Compass.S],
                    indexs[(int)Compass.Centre], indexs[(int)Compass.E], indexs[(int)Compass.SE], indexs[(int)Compass.S]); 
                break;
            case CornorDirection.SW:
                corner = new Corner(edges[(int)Compass.W], edges[(int)Compass.Centre], edges[(int)Compass.S], edges[(int)Compass.SW],
                    indexs[(int)Compass.W], indexs[(int)Compass.Centre], indexs[(int)Compass.S], indexs[(int)Compass.SW]); 
                break;
        }
        return corner;
    }

    public override string ToString()
    {
        return tostring;
    }

}
