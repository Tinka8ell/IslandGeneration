using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class Anews 
{
    private Slope[] directions = new Slope[4];

    private long index;
    private string tostring;

    private int[] edges = new int[Directions.CompassFullSize];

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

    public Anews(int nw, int n, int ne, int w, int c, int e, int sw, int s, int se)
    {
        edges[(int)Compass.Centre] = c + c;
        edges[(int)Compass.N] = n + c;
        edges[(int)Compass.NE] = ne + c;
        edges[(int)Compass.E] = e + c;
        edges[(int)Compass.SE] = se + c;
        edges[(int)Compass.S] = s + c;
        edges[(int)Compass.SW] = sw + c;
        edges[(int)Compass.W] = w + c;
        edges[(int)Compass.NW] = nw + c;
        index = 0;
        for(int i = 0; i < edges.Length; i++)
        {
            index *= 8;
            index += edges[i];
        }

        tostring = "<" + nw + n + ne +
            "/" + w + c + e +
            "/" + sw + s + se +
            ">(" + index + ")" + ShowEdges();
    }

    public int [] GetCorners(CornorDirection cornorDirection)
    {
        int[] values = { 0, 0, 0, 0 };
        switch (cornorDirection)
        {
            case CornorDirection.NW:
                values = new int[] { edges[(int)Compass.NW], edges[(int)Compass.N], edges[(int)Compass.Centre], edges[(int)Compass.W] };
                break;
            case CornorDirection.NE:
                values = new int[] { edges[(int)Compass.N], edges[(int)Compass.NE], edges[(int)Compass.E], edges[(int)Compass.Centre] };
                break;
            case CornorDirection.SE:
                values = new int[] { edges[(int)Compass.Centre], edges[(int)Compass.E], edges[(int)Compass.SE], edges[(int)Compass.S] };
                break;
            case CornorDirection.SW:
                values = new int[] { edges[(int)Compass.W], edges[(int)Compass.Centre], edges[(int)Compass.S], edges[(int)Compass.SW] };
                break;
        }
        return values;
    }

    public override string ToString()
    {
        return tostring;
    }

    public long ToIndex()
    {
        return index;
    }
}
