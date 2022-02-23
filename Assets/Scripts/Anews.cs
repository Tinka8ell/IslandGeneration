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

    private int index;
    private string tostring;

    private float[] edges = new float[CompassFullSize];

    private string ShowEdges()
    {
        string value = "{" + 
            edges[(int)Compass.NW] +
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

    public Anews(bool nw, bool n, bool ne, bool w, bool c, bool e, bool sw, bool s, bool se)
    {
        if (c)
        {
            edges[CompassCentre] = 0f; // actuall will already be by initialisation!
            if (!n) // some reason n/s seems swapped nothing to the north, so slope that way
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
            if (!s) // some reason n/s seems swapped nothing to the south, so slope that way
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
            if (!nw) // some reason n/s seems swapped nothing to the north-west, so slope that way
            {
                edges[(int)Compass.NW] = 1f;
            }
            if (!ne) // some reason n/s seems swapped nothing to the north-east, so slope that way
            {
                edges[(int)Compass.NE] = 1f;
            }
            if (!sw) // some reason n/s seems swapped nothing to the south-west, so slope that way
            {
                edges[(int)Compass.SW] = 1f;
            }
            if (!se) // some reason n/s seems swapped nothing to the south-east, so slope that way
            {
                edges[(int)Compass.SE] = 1f;
            }
        }
        else // nothing here!
            for (int i = 0; i < CompassFullSize; i++) edges[i] = 1f;

        index = 0;
        for(int i = 0; i < edges.Length; i++)
        {
            index *= 2;
            index += (int)edges[i];
        }

        tostring = "<" + BoolAsStr(nw) + BoolAsStr(n) + BoolAsStr(ne) +
            "/" + BoolAsStr(w) + BoolAsStr(c) + BoolAsStr(e) +
            "/" + BoolAsStr(sw) + BoolAsStr(s) + BoolAsStr(se) +
            ">(" + index + ")" + ShowEdges();
    }

    public float [] GetCorners(CornorDirection cornorDirection)
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
