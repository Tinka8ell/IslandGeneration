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
	private static int size;
	private static FalloffSettings falloffSettings;

	public static Dictionary<int, FalloffMap> falloffMaps = new Dictionary<int, FalloffMap>();
//	public static FalloffMap emptyMap;
    private static int centreMapSize;

    public void GenerateFalloffMaps(int numberOfVertices, FalloffSettings fs)
	{
		// update generate data
		size = numberOfVertices;
		falloffSettings = fs;

		// make sure Islands are up to date
		Islands.settings = falloffSettings.islandNoiseSettings;

		// build all the maps:
		CreateCentreFalloffMaps();
	}

	private void CreateCentreFalloffMaps()
    {
		falloffMaps.Clear(); // remove old ones
		centreMapSize = size - 2; // set size of the centre maps 
		
		bool nw;
		bool n;
		bool ne;
		bool w;
		bool us;
		bool e;
		bool sw;
		bool s;
		bool se;

		for (int i = 1; i < 0b10_0000_0000; i++)
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
			if (!falloffMaps.ContainsKey(index)) // haven't drawn it yet
			{
				// they are called centre falloff maps as they leave a boarder of one ...
				falloffMaps.Add(index, GenerateQuadFalloffMap(centreMapSize, anews));
			}
		}
	}

	internal static float[,] GetCentreFalloffMap(Anews anews)
    {
		int index = anews.ToIndex();
		if (!falloffMaps.ContainsKey(index)) Debug.LogError("Not found falloutmap number: " + anews.ToIndex()
			+ " of " + FalloffGenerator.falloffMaps.Keys.Count + " for anews: " + anews);
		return falloffMaps[index].values;
	}

	internal static float[,] BuildFalloffMap(Vector2 coord)
	{
		float[,] falloffMap = new float[size, size];

		// first fill centre
		Anews anews = Islands.LocalNews(coord);
		float[,] centreMap = GetCentreFalloffMap(anews);
		for (int j = 0; j < centreMapSize; j++) // for each row ...
        {
			for (int i = 0; i < centreMapSize; i++) // for each row ...
			{
			falloffMap[j + 1, i + 1] = centreMap[j, i];
			}
		}

		// now work on each edge ...
		int last = size - 1; // last column or row of the map 

		// north edge
		Vector2 nextDoor = Islands.NextDoor(coord, Anews.Compass.N);
		centreMap = GetCentreFalloffMap(Islands.LocalNews(nextDoor));
		for (int i = 0; i < centreMapSize; i++)
		{
			falloffMap[0, i + 1] = centreMap[centreMapSize - 1, i]; // our top border is north's bottom
		}
		
		// south edge
		nextDoor = Islands.NextDoor(coord, Anews.Compass.S);
		centreMap = GetCentreFalloffMap(Islands.LocalNews(nextDoor));
		for (int i = 0; i < centreMapSize; i++)
		{
			falloffMap[last, i + 1] = centreMap[0, i]; // our bottom border is south's top
		}
		
		// east edge
		nextDoor = Islands.NextDoor(coord, Anews.Compass.E);
		centreMap = GetCentreFalloffMap(Islands.LocalNews(nextDoor));
		for (int j = 0; j < centreMapSize; j++)
		{
			falloffMap[j + 1, last] = centreMap[j, 0]; // our right border is east's left
		}
		
		// west edge
		nextDoor = Islands.NextDoor(coord, Anews.Compass.W);
		centreMap = GetCentreFalloffMap(Islands.LocalNews(nextDoor));
		for (int j = 0; j < centreMapSize; j++)
		{
			falloffMap[j + 1, 0] = centreMap[j, centreMapSize - 1]; // our left border is east's right
		}
		
		// finally fill in the corners ...
		// cornor cheat - all corners will match exactly as our generator forces this
		int centreLast = last - 1; // last column or row of the center map of the map
		falloffMap[0, 0] = falloffMap[1, 1];
		falloffMap[last, 0] = falloffMap[centreLast, 1];
		falloffMap[0, last] = falloffMap[1, centreLast];
		falloffMap[last, last] = falloffMap[centreLast, centreLast];
		return falloffMap;
	}

	public static FalloffMap GenerateQuadFalloffMap(int size, Anews anews)
	{
		float[,] values = new float[size, size]; 
		int cornerSize = (size + 1) / 2; // assumes size is odd!
		int offset = size - cornerSize;
		int col = 0;
		int row = 0;

		for (CornorDirection cd = 0; (int)cd < 4; cd++)
		{
			switch (cd)
			{
				case CornorDirection.NE:
					col = offset;
					row = 0;
					break;
				case CornorDirection.SE:
					col = offset;
					row = offset;
					break;
				case CornorDirection.SW:
					col = 0;
					row = offset;
					break;
				case CornorDirection.NW:
					col = 0;
					row = 0;
					break;
			}
			QuadSlope(values, col, row, cornerSize, anews.GetCorners(cd));
		}
		return new FalloffMap(values);
	}

	private static void QuadSlope(
		float[,] values, int col, int row, int size, float[] abcd)
	{
		float a = abcd[0];
		float b = abcd[1];
		float c = abcd[2];
		float d = abcd[3];

		for (int j = 0; j < size; j++) // rows
		{
			for (int i = 0; i < size; i++) // columns
			{
				float value = QuadLerp(a, b, c, d,
							i / (size - 1f), j / (size - 1f));
				value = Evaluate(value, falloffSettings.a, falloffSettings.b);
				values[row + j, col + i] = value;
			}
		}
	}

	static float Evaluate(float value, float a, float b)
	{
		float powA = Mathf.Pow(value, a);
		return powA / (powA + Mathf.Pow(b - b * value, a));
	}

	public static float QuadLerp(float a, float b, float c, float d, float u, float v)
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

