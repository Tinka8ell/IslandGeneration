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
    public string index = "";

    public Corner(int nw, int ne, int se, int sw)
    {
        corners[0] = nw / 4f;
        corners[1] = ne / 4f;
        corners[2] = se / 4f;
        corners[3] = sw / 4f;
        index = string.Format("{0:D3}|{1:D3}|{2:D3}|{3:D3}", 
            nw, ne, se, sw);
    }
}

public class Anews 
{
    private string tostring;

    private int[] edges = new int[Directions.CompassFullSize];

    private string ShowEdges()
    {
        string value = string.Format("<{0:D3}{1:D3}{2:D3}/{3:D3}{4:D3}{5:D3}/{6:D3}{7:D3}{8:D3}>",
            edges[(int)Compass.NW],
            edges[(int)Compass.N],
            edges[(int)Compass.NE],
            edges[(int)Compass.W],
            edges[(int)Compass.Centre],
            edges[(int)Compass.E],
            edges[(int)Compass.SW],
            edges[(int)Compass.S],
            edges[(int)Compass.SE]);
        return value;
    }

    public Anews(int nw, int n, int ne, int w, int c, int e, int sw, int s, int se)
    {
        edges[(int)Compass.Centre] = 4 * c;
        edges[(int)Compass.N] = 2 * (n + c);
        edges[(int)Compass.NE] = ne + c + n + e;
        edges[(int)Compass.E] = 2 * (e + c);
        edges[(int)Compass.SE] = se + c + s + e;
        edges[(int)Compass.S] = 2 * (s + c);
        edges[(int)Compass.SW] = sw + c + s + w;
        edges[(int)Compass.W] = 2 * (w + c);
        edges[(int)Compass.NW] = nw + c + n + w;
        tostring = "{" + nw + n + ne +
            "/" + w + c + e +
            "/" + sw + s + se +
            "}" + ShowEdges();
    }

    public Corner GetCorner(CornorDirection cornorDirection)
    {
        Corner corner = new Corner(0, 0, 0, 0);
        switch (cornorDirection)
        {
            case CornorDirection.NW:
                corner = new Corner(edges[(int)Compass.NW], edges[(int)Compass.N], edges[(int)Compass.Centre], edges[(int)Compass.W]);
                break;
            case CornorDirection.NE:
                corner = new Corner(edges[(int)Compass.N], edges[(int)Compass.NE], edges[(int)Compass.E], edges[(int)Compass.Centre]); 
                break;
            case CornorDirection.SE:
                corner = new Corner(edges[(int)Compass.Centre], edges[(int)Compass.E], edges[(int)Compass.SE], edges[(int)Compass.S]); 
                break;
            case CornorDirection.SW:
                corner = new Corner(edges[(int)Compass.W], edges[(int)Compass.Centre], edges[(int)Compass.S], edges[(int)Compass.SW]); 
                break;
        }
        return corner;
    }

    public override string ToString()
    {
        return tostring;
    }

}
