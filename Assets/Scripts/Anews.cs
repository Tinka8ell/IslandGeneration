using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anews 
{
    public enum Compass
    {
        N = 0,
        NE = 1,
        E = 2,
        SE = 3,
        S = 4,
        SW = 5,
        W = 6,
        NW = 7,
    }

    public Compass Add(Compass direction, int toRight)
    {
        int value =  (int) direction + toRight;
        value %= CompassSize;
        return (Compass)value;
    }

    public const int CompassCentre = 8;
    public const int CompassSize = 8; // all points
    public const int CompassFullSize = 9;

    private Slope[] directions = new Slope[4];

    private readonly int index;
    private readonly string tostring;

    private float[] edges = new float[CompassFullSize];

    public Anews(bool nw, bool n, bool ne, bool w, bool e, bool sw, bool s, bool se)
    {
        directions[(int)CornorDirection.NE] = CalculateSlope(n, ne, e);
        directions[(int)CornorDirection.SE] = CalculateSlope(e, se, s);
        directions[(int)CornorDirection.SW] = CalculateSlope(s, sw, w);
        directions[(int)CornorDirection.NW] = CalculateSlope(w, nw, n);

        index = (int)directions[0] + 4 * ((int)directions[1] + 4 * ((int)directions[2] + 4 * (int)directions[3]));

        tostring = "<" + BoolAsStr(nw) + BoolAsStr(n) + BoolAsStr(ne) +
            "/" + BoolAsStr(w) + "1" + BoolAsStr(e) +
            "/" + BoolAsStr(sw) + BoolAsStr(s) + BoolAsStr(se) +
            ">(" + index + ")";
    }

    private string ShowEdges()
    {
        string value = "{" + 
            edges[(int) Compass.NW] +
            edges[(int)Compass.N] +
            edges[(int)Compass.NE] + "/" +
            edges[(int)Compass.W] +
            edges[(int)CompassCentre] +
            edges[(int)Compass.E] + "/" +
            edges[(int)Compass.SW] +
            edges[(int)Compass.S] +
            edges[(int)Compass.SE] +
            "}";
        return value;
    }
    private string ShowCorners(float [] c)
    {
        string value = "[" +
            c[0] + ", " +
            c[1] + ", " +
            c[2] + ", " +
            c[3] + 
            "]";
        return value;
    }
    public Anews(bool a, bool n, bool e, bool s, bool w)
    {
        index = BoolAsInt(a) + 2 * (BoolAsInt(n) + 2 * (BoolAsInt(e) + 2 * (BoolAsInt(s) + 2 * BoolAsInt(w))));
        bool debug = index == 13;

        tostring = "<anesw:" 
            + BoolAsStr(a) + BoolAsStr(n) + BoolAsStr(e) + BoolAsStr(s) + BoolAsStr(w) 
            + ">(" + index + ")";

        if (debug)
        {
            Debug.Log("Anews: " + tostring + ", " + ShowEdges());
            Debug.Log("North: " + (n ? "T" : "F"));
            Debug.Log("East: " + (e ? "T" : "F"));
            Debug.Log("South: " + (s ? "T" : "F"));
            Debug.Log("West: " + (w ? "T" : "F"));
        }
        if (a)
        {
            edges[CompassCentre] = 0f; // actuall will be by initialisation!
            if (debug) Debug.Log("a: " + ShowEdges());
            if (!n) // nothing to the north, so slope that way
            {
                edges[(int)Compass.NW] = 1f;
                edges[(int)Compass.N] = 1f;
                edges[(int)Compass.NE] = 1f;
                if (debug) Debug.Log("n: " + ShowEdges());
            }
            if (!e) // nothing to the east, so slope that way
            {
                edges[(int)Compass.NE] = 1f;
                edges[(int)Compass.E] = 1f;
                edges[(int)Compass.SE] = 1f;
                if (debug) Debug.Log("e: " + ShowEdges());
            }
            if (!s) // nothing to the south, so slope that way
            {
                edges[(int)Compass.SE] = 1f;
                edges[(int)Compass.S] = 1f;
                edges[(int)Compass.SW] = 1f;
                if (debug) Debug.Log("s: " + ShowEdges());
            }
            if (!w) // nothing to the west, so slope that way
            {
                edges[(int)Compass.SW] = 1f;
                edges[(int)Compass.W] = 1f;
                edges[(int)Compass.NW] = 1f;
                if (debug) Debug.Log("w: " + ShowEdges());
            }
        }
        else // nothing here!
            for (int i = 0; i < CompassFullSize; i++) edges[i] = 1f;
        if (debug)
        {
            Debug.Log("Result: " + tostring + " - " + ShowEdges());
            Debug.Log(CornorDirection.NE + " = " + ShowCorners(getCorners(CornorDirection.NE)));
            Debug.Log(CornorDirection.NW + " = " + ShowCorners(getCorners(CornorDirection.NW)));
            Debug.Log(CornorDirection.SW + " = " + ShowCorners(getCorners(CornorDirection.SW)));
            Debug.Log(CornorDirection.SE + " = " + ShowCorners(getCorners(CornorDirection.SE)));
        }

    }

    public float [] getCorners(CornorDirection cornorDirection)
    {
        float[] values = { 0, 0, 0, 0 };
        switch (cornorDirection)
        {
            case CornorDirection.NW:
                values = new float[] { edges[(int)Compass.NW], edges[(int)Compass.N], edges[CompassCentre], edges[(int)Compass.W] };
                break;
            case CornorDirection.NE:
                values = new float[] { edges[(int)Compass.N], edges[(int)Compass.NE], edges[(int)Compass.E], edges[CompassCentre] };
                break;
            case CornorDirection.SE:
                values = new float[] { edges[CompassCentre], edges[(int)Compass.E], edges[(int)Compass.SE], edges[(int)Compass.S] };
                break;
            case CornorDirection.SW:
                values = new float[] { edges[(int)Compass.W], edges[CompassCentre], edges[(int)Compass.S], edges[(int)Compass.SW] };
                break;
        }
        return values;
    }

    public Slope GetSlope(CornorDirection cornor)
    {
        return directions[(int) cornor];
    }

    private Slope CalculateSlope(bool l, bool m, bool r)
    {
        if (l)
        {
            if (r)
            {
                if (m) return Slope.FLAT;
                else return Slope.BOTH;
            }
            else return Slope.RIGHT;
        }
        else
        {
            if (r) return Slope.LEFT;
            else return Slope.BOTH;
        }
    }

    private float BoolAsFloat(bool b)
    {
        return b ? 0f : 1f;
    }

    private int BoolAsInt(bool b)
    {
        return b ? 1 : 0;
    }

    private string BoolAsStr(bool b)
    {
        return b ? "1" : "0";
    }

    public override string ToString()
    {
        return tostring;
    }

    public int ToIndex()
    {
        return index;
    }
}
