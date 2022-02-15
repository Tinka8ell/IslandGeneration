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

    public Anews(bool a, bool n, bool e, bool s, bool w)
    {
        index = BoolAsInt(a) + 2 * (BoolAsInt(n) + 2 * (BoolAsInt(e) + 2 * (BoolAsInt(s) + 2 * BoolAsInt(w))));

        tostring = "<anesw:" 
            + BoolAsStr(a) + BoolAsStr(n) + BoolAsStr(w) + BoolAsStr(e) + BoolAsStr(s) 
            + ">(" + index + ")";

        if (a)
        {
            edges[CompassCentre] = 0f;
            if (!n) // nothing to the north, so slope that way
            {
                edges[(int)Compass.NW] = 1f;
                edges[(int)Compass.N] = 1f;
                edges[(int)Compass.NE] = 1f;
            }
            if (!e) // nothing to the east, so slope that way
            {
                edges[(int)Compass.NE] = 1f;
                edges[(int)Compass.E] = 1f;
                edges[(int)Compass.SE] = 1f;
            }
            if (!s) // nothing to the south, so slope that way
            {
                edges[(int)Compass.SE] = 1f;
                edges[(int)Compass.S] = 1f;
                edges[(int)Compass.SW] = 1f;
            }
            if (!w) // nothing to the west, so slope that way
            {
                edges[(int)Compass.SW] = 1f;
                edges[(int)Compass.W] = 1f;
                edges[(int)Compass.NW] = 1f;
            }
        }
        else // nothing here!
            for (int i = 0; i < CompassFullSize; i++) edges[i] = 1f;
    }

    public float [] getCorners(CornorDirection cornorDirection)
    {
        float[] values = { 0, 0, 0, 0 };
        switch (cornorDirection)
        {
            case CornorDirection.NW:
                values = new float[] { edges[(int)Compass.NW], edges[(int)Compass.N], edges[CompassCentre], edges[(int)Compass.E] };
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
