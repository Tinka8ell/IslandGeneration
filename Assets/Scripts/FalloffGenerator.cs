using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum Slope
{
	FLAT = 0,
	LEFT = 1,
	RIGHT = 2,
	BOTH = 3
}

public enum CornorDirection
{
	NE = 0,
	SE = 1,
	SW = 2,
	NW = 3,
}

public struct FalloffGenerator
{
	private static int size = 0;
	private static float a = 3;
	private static float b = 2.2f;
	private static FalloffSettings falloffSettings;

	public static Dictionary<int, FalloffMap> falloffMaps = new Dictionary<int, FalloffMap>();
	public static FalloffMap emptyMap;

	public void GenerateFalloffMaps(int numberOfVertices, FalloffSettings fs)
	{
		// do we need to regenerate ...
		bool regen = false;

		if (a != fs.a)
		{
			a = fs.a;
			regen = true;
		}
		if (b != fs.b)
		{
			b = fs.b;
			regen = true;
		}
		if (size != numberOfVertices)
		{
			size = numberOfVertices;
			regen = true;
		}
		if (regen)
		{
			// Debug.LogWarning("Regenning falloff: size = " + size + ", a = " + a + ", b = " + b);
			// build all the maps:
			falloffMaps.Clear(); // remove old ones
			falloffSettings = fs;
			/*
			 * Now build on demand ...
			*/
			bool nw;
			bool n;
			bool ne;
			bool w;
			bool us;
			bool e;
			bool sw;
			bool s;
			bool se;

			for (int i = 1; i < 0b10_0000_0000; i ++)
            {
				nw = (i & 0b1_0000_0000) != 0;
				n = (i & 0b_1000_0000) != 0;
				ne = (i & 0b_0100_0000) != 0;
				w = (i & 0b_0010_0000) != 0;
				us = (i & 0b_0001_0000) != 0;
				e = (i & 0b_0000_1000) != 0;
				sw = (i & 0b_0000_0100) != 0;
				s = (i & 0b_0000_0010) != 0;
				se = (i & 0b_0000_0001) != 0;
				Anews anews = new Anews(nw, n, ne, w, us, e, se, s, sw);
				int index = anews.ToIndex();
				bool debug = false; //  (index == 13);
				if (!falloffMaps.ContainsKey(index)) // haven't drawn it yet
				{
					if (debug) Debug.LogWarning("Adding falloutmap number: " + anews.ToIndex()
						+ " of " + FalloffGenerator.falloffMaps.Keys.Count + " for anews: " + anews);
					falloffMaps.Add(index, GenerateQuadFalloffMap(size, anews, falloffSettings, debug, index));
				}
				else
				{
					if (debug) Debug.Log("Skipping falloutmap number: " + anews.ToIndex()
						+ " of " + FalloffGenerator.falloffMaps.Keys.Count + " for anews: " + anews);
				}
			}

			// Add the empty one too;
			emptyMap = FalloffGenerator.GenerateEmptyMap(size);
		}
	}

    internal static FalloffMap GetFalloffMap(Anews anews)
    {
		int index = anews.ToIndex();
		bool debug = false; // (index == 183);
		if (!falloffMaps.ContainsKey(index)) // haven't drawn it yet
		{
			if (debug) Debug.LogWarning("Adding falloutmap number: " + anews.ToIndex()
				+ " of " + FalloffGenerator.falloffMaps.Keys.Count + " for anews: " + anews);
			// falloffMaps.Add(index, GenerateQuadFalloffMap(size, anews, falloffSettings, debug, index));
		}
		else
		{
			if (debug) Debug.Log("Already got falloutmap number: " + anews.ToIndex()
				+ " of " + FalloffGenerator.falloffMaps.Keys.Count + " for anews: " + anews);
		}
		return falloffMaps[index];
	}

	public static FalloffMap GenerateEmptyMap(int size)
	{
		float[,] map = new float[size, size];
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				map[i, j] = 1f;
			}
		}

		return new FalloffMap(map);
	}

	public static FalloffMap GenerateQuadFalloffMap(int size, Anews anews, FalloffSettings falloffSettings,
		bool debug = false, int index = 0)
	{
		float[,] values = new float[size, size];
		int cornerSize = (size + 1) / 2; // assumes size is odd!
		int x = 0;
		int y = 0;

		for (CornorDirection cd = 0; (int)cd < 4; cd++)
		{
			switch (cd)
			{
				case CornorDirection.NE:
					x = (size - 1) / 2;
					y = 0;
					break;
				case CornorDirection.SE:
					x = (size - 1) / 2;
					y = (size - 1) / 2;
					break;
				case CornorDirection.SW:
					x = 0;
					y = (size - 1) / 2;
					break;
				case CornorDirection.NW:
					x = 0;
					y = 0;
					break;
			}
			if (debug) Debug.LogWarning("Direction: " + cd + ", index: " + index);
			QuadSlope(values, x, y, cornerSize, falloffSettings, anews.GetCorners(cd), debug, cd, index);
		}
		return new FalloffMap(values);
	}

	private static void QuadSlope(
		float[,] values, int x, int y, int numberOfVertices,
		FalloffSettings falloffSettings, float[] abcd, 
		bool debug = false, CornorDirection cd = CornorDirection.NE, int index = 0)
	{
		float a = abcd[0];
		float b = abcd[1];
		float c = abcd[2];
		float d = abcd[3];

		if (debug) Debug.Log("QuadSlope: (" + x + ", " + y + "), size = " + size  
			+ "), size = " + numberOfVertices + ", abce = (" + a + ", " + b + ", " + c + ", " + d + ")"
			+ ", direction = " + cd);

		for (int i = 1; i < numberOfVertices; i++)
		{
			for (int j = 1; j < numberOfVertices; j++)
			{
				bool debug2 = debug && (i == 10) && (j == 10);
				if (debug2) Debug.Log("At: i = " + i + ", j = " + j + " => (" + (x + i) + ", " + (y + j) + ")");
				values[x + i, y + j] = // ((i==10)||(j==10))? 1: 
					Evaluate(
						QuadLerp(
							a, b, c, d, 
							i / (numberOfVertices - 1f), j / (numberOfVertices - 1f), 
							debug2),
						falloffSettings.a, falloffSettings.b
					);
			}
		}
	}

	static float Evaluate(float value, float a, float b)
	{
		float powA = Mathf.Pow(value, a);
		return powA / (powA + Mathf.Pow(b - b * value, a));
	}

	public static float QuadLerp(float a, float b, float c, float d, float u, float v, bool debug = false)
	{
		// Given a (u,v) coordinate that defines a 2D local position inside a planar quadrilateral, find the
		// absolute 3D (x,y,z) coordinate at that location.
		//
		//  0 <----u----> 1
		//  a ----------- b    0
		//  |             |   /|\
		//  |             |    |
		//  |             |    v
		//  |  *(u,v)     |    |
		//  |             |   \|/
		//  d------------ c    1
		//
		// a, b, c, and d are the vertices of the quadrilateral. They are assumed to exist in the
		// same plane in 3D space, but this function will allow for some non-planar error.
		//
		// Variables u and v are the two-dimensional local coordinates inside the quadrilateral.
		// To find a point that is inside the quadrilateral, both u and v must be between 0 and 1 inclusive.  
		// For example, if you send this function u=0, v=0, then it will return coordinate "a".  
		// Similarly, coordinate u=1, v=1 will return vector "c". Any values between 0 and 1
		// will return a coordinate that is bi-linearly interpolated between the four vertices.		

		float abu = Mathf.Lerp(a, b, u);
		float dcu = Mathf.Lerp(d, c, u);
		float result = Mathf.Lerp(abu, dcu, v);
		if (debug) Debug.Log("QuadLerp: a = " + a + ", b = " + b + ", u = " + u + " -> " + abu
			+ " and d = " + d + ", c = " + c + ", u = " + u + " -> " + dcu
			+ " with v = " + v + " -> " + result);
		return result;
	}
}

public struct FalloffMap
{
	public readonly float[,] values;

	public FalloffMap(float[,] values)
	{
		this.values = values;
	}
}

