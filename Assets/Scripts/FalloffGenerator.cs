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

	public static Dictionary<long, FalloffMap> falloffCorners = new();
    private static int centreMapSize;
	private static int cornerSize;

	public static float[,] noFalloff;

	public void GenerateFalloffMaps(int numberOfVertices, FalloffSettings fs)
	{
		// update generate data
		size = numberOfVertices;
		falloffSettings = fs;

		// make sure Islands are up to date
		Islands.settings = falloffSettings.islandNoiseSettings;

		falloffCorners.Clear(); // remove old ones
		centreMapSize = size - 2; // set size of the centre maps 
		cornerSize = (centreMapSize + 1) / 2; // assumes size is odd!

		noFalloff = new float[size, size];
		for (int j = 1; j < size; j++)
		{
			for (int i = 1; i < size; i++)
			{
				noFalloff[i, j] = 1;
			}
		}
		//Debug.LogFormat("GenerateFalloffMaps, numberOfVertices: {0}, size: {1}, centreMapSize: {2}",
		//	numberOfVertices, size, centreMapSize);
	}

	private static CornorDirection[] cornorDirections = new CornorDirection[]{
			CornorDirection.NW,
			CornorDirection.NE,
			CornorDirection.SW,
			CornorDirection.SE,
		};

	public static float[,] BuildFalloffMap(Vector2 coord)
	{
		Debug.LogFormat("BuildFalloffMap, coord: {0}",
			coord);
		float[,] falloffMap = new float[size, size];

        /*
		 * Fill in these corners
		 *   |  |  
		 *  *|**|* 
		 * --+--+--
		 *  *|**|* 
		 *  *|**|* 
		 * --+--+--
		 *  *|**|* 
		 *   |  |  
		 * Note that there is only one line / column 
		 * from the neighbouring cells.
		 */

		for (int cellRow = 1; cellRow < 5; cellRow++)
		//int cellRow = 1;
		{
			for (int cellCol = 1; cellCol < 5; cellCol++)
			//int cellCol = 1;
			{
				Vector2 cell = new(coord.x - 1 + (cellCol / 2), coord.y - 1 + (cellRow / 2));
				Anews anews = Islands.LocalNews(cell);
				int edgeNumber = (cellCol % 2) + 2 * (cellRow % 2);
				CornorDirection cornorDirection = cornorDirections[edgeNumber];
				Debug.LogFormat("BuildFalloffMap, loop[{0}, {1}], cell: {2}, cornorDirection: {3}",
					cellCol, cellRow, cell, cornorDirection);
				int col = (cellCol) / 2 * centreMapSize + 1 - centreMapSize;
				if (cellCol % 2 > 0) col += centreMapSize / 2;
				int row = (cellRow) / 2 * centreMapSize + 1 - centreMapSize;
				if (cellRow % 2 > 0) row += centreMapSize / 2;
				CopyCorner(falloffMap, anews, cornorDirection, col, row);
			}
        }

		return falloffMap;
	}

    private static void CopyCorner(float[,] falloffMap, Anews anews, CornorDirection cornorDirection, int x, int y)
    {
        Debug.LogFormat("CopyCorner for anews: {0}, direction: {1}, ({2}, {3})",
			anews, cornorDirection, x, y);
        float[,] corner = GetCorner(anews, cornorDirection);
		int minx = Mathf.Max(0, -x);
		int maxx = Mathf.Min(cornerSize, size - x);
		int miny = Mathf.Max(0, -y);
		int maxy = Mathf.Min(cornerSize, size - y);
		Debug.LogFormat("CopyCorner: minx: {0}, maxx: {1}, miny: {2}, maxy {3}",
			minx, maxx, miny, maxy);
		for (int j = miny; j < maxy; j++)
        {
			for (int i = minx; i < maxx; i++)
			{
				float value = corner[i, j];
				falloffMap[x + i, y + j] = value;
			}
		}
	}

    private static float[,] GetCorner(Anews anews, CornorDirection cornorDirection)
    {
		FalloffMap corner;
		if (!falloffCorners.TryGetValue(anews.ToIndex(), out corner)){
			corner = GenerateCorner(anews, cornorDirection);
			falloffCorners.Add(anews.ToIndex(), corner);
        }
		return corner.values;
	}

    private static FalloffMap GenerateCorner(Anews anews, CornorDirection cornorDirection)
    {
		float[,] values = new float[cornerSize, cornerSize];
		QuadSlope(values, 0, 0, cornerSize, anews.GetCorners(cornorDirection));
		return new(values);
	}

	//public static FalloffMap GenerateQuadFalloffMap(int size, Anews anews)
	//{
	//	float[,] values = new float[size, size]; 
	//	int cornerSize = (size + 1) / 2; // assumes size is odd!
	//	int offset = size - cornerSize;
	//	int col = 0;
	//	int row = 0;

	//	for (CornorDirection cd = 0; (int)cd < 4; cd++)
	//	{
	//		switch (cd)
	//		{
	//			case CornorDirection.NE:
	//				col = offset;
	//				row = 0;
	//				break;
	//			case CornorDirection.SE:
	//				col = offset;
	//				row = offset;
	//				break;
	//			case CornorDirection.SW:
	//				col = 0;
	//				row = offset;
	//				break;
	//			case CornorDirection.NW:
	//				col = 0;
	//				row = 0;
	//				break;
	//		}
	//		QuadSlope(values, col, row, cornerSize, anews.GetCorners(cd));
	//	}
	//	return new FalloffMap(values);
	//}

	private static void QuadSlope(
		float[,] values, int col, int row, int size, float[] abcd)
	{
		float a = abcd[0];
		float b = abcd[1];
		float c = abcd[2];
		float d = abcd[3];
		Debug.LogFormat("QuadSlope: ({0}, {1}), size: {2}, abcd: {3}, {4}, {5}, {6})",
			col, row, size, a, b, c, d);

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

