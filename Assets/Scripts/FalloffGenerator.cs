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

	public static Dictionary<string, Quadrant> falloffCorners = new();
 //   private static int centreMapSize;
	//private static int cornerSize;

	public static float[,] noFalloff;

	public void GenerateFalloffMaps(int numberOfVertices, FalloffSettings fs)
	{
		// update generate data
		size = numberOfVertices;
		falloffSettings = fs;

		// make sure Islands are up to date
		Islands.settings = falloffSettings.islandNoiseSettings;

		falloffCorners.Clear(); // remove old ones
		//centreMapSize = size - 2; // set size of the centre maps 
		//cornerSize = (centreMapSize + 1) / 2; // assumes size is odd!

		noFalloff = new float[size, size];
		for (int j = 1; j < size; j++)
		{
			for (int i = 1; i < size; i++)
			{
				noFalloff[i, j] = 1;
			}
		}
        //Debug.LogWarningFormat("GenerateFalloffMaps, numberOfVertices: {0}, size: {1}, centreMapSize: {2}",
        //    numberOfVertices, size, centreMapSize);
    }

	private static CornorDirection[] cornorDirections = new CornorDirection[]{
			CornorDirection.SW,
			CornorDirection.SE,
			CornorDirection.NW,
			CornorDirection.NE,
		};

	public static float[,] BuildFalloffMap(Vector2 coord, int size)
	{
		//Debug.LogWarningFormat("BuildFalloffMap, coord: {0} =====================================", coord);
		float[,] falloffMap = new float[size, size];
		int centreSize = size - 2;
		int cornerSize = (centreSize + 1) / 2;

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
		{
			for (int cellCol = 1; cellCol < 5; cellCol++)
			{
				int offsetX = cellCol / 2 - 1;
				int offsetY = cellRow / 2 - 1;
				int phaseX = cellCol % 2;
				int phaseY = cellRow % 2;
				Vector2 cell = new(coord.x + offsetX, coord.y + offsetY);
				Anews anews = Islands.LocalNews(cell);

				int edgeNumber = phaseX + 2 * phaseY;
				CornorDirection cornorDirection = cornorDirections[edgeNumber];
				Corner corner = anews.GetCorner(cornorDirection);

                Debug.LogFormat("BuildFalloffMap, loop[{0}, {1}], cell: {2}, direction: {3}, offset: ({4}, {5}), phase: ({6}, {7}), anews: {8}, corner: {9}",
                    cellCol, cellRow, cell, cornorDirection,
                    offsetX, offsetY, phaseX, phaseY,
                    anews, corner.index);

                int col = offsetX * centreSize;
				col += (phaseX == 0) ? 1 : cornerSize;
				int row = offsetY * centreSize;
				row += (phaseY == 0) ? 1 : cornerSize;
				CopyQuadrant(falloffMap, corner, col, row, cornerSize);
			}
        }

		return falloffMap;
	}

    private static void CopyQuadrant(float[,] falloffMap, Corner corner, int x, int y, int cornerSize)
    {
        float[,] quadrant = GetQuadrant(corner, cornerSize);
		int minx = Mathf.Max(0, -x);
		int maxx = Mathf.Min(cornerSize, size - x);
		int miny = Mathf.Max(0, -y);
		int maxy = Mathf.Min(cornerSize, size - y);
		//Debug.LogFormat("CopyQuadrant for corner: {0}, ({1}, {2}), minx: {3}, maxx: {4}, miny: {5}, maxy {6}",
		//	corner.index, x, y, minx, maxx, miny, maxy);
		for (int i = minx; i < maxx; i++)
        {
			for (int j = miny; j < maxy; j++)
			{
				float value = quadrant[i, j];
				falloffMap[x + i, y + j] = value;
			}
		}
	}

    public static float[,] GetQuadrant(Corner corner, int cornerSize)
    {
		string index = corner.index;
		Quadrant quadrant;
		bool found = falloffCorners.TryGetValue(index, out quadrant);
		//Debug.LogFormat("GetQuadrant for corner: {0}, found: {1}",
		//	corner.index, found);
		if (!found){
			quadrant = GenerateQuadrant(corner, cornerSize);
			falloffCorners.Add(index, quadrant);
        }
		return quadrant.values;
	}

    private static Quadrant GenerateQuadrant(Corner corner, int cornerSize)
	{
		Debug.LogFormat("GenerateCorner({0}, {1})", corner.index, cornerSize);
		float[,] values = new float[cornerSize, cornerSize];
		QuadSlope(values, cornerSize, corner.corners);
		return new(values);
	}

	private static void QuadSlope(
		float[,] values, int size, float[] abcd)
	{
		float a = abcd[0];
		float b = abcd[1];
		float c = abcd[2];
		float d = abcd[3];

        for (int row = 0; row < size; row++) // rows
		{
			for (int col = 0; col < size; col++) // columns
			{
				float value = QuadLerp(a, b, c, d,
							col / (size - 1f), row / (size - 1f));
				//value = Evaluate(value, falloffSettings.a, falloffSettings.b);
				values[col, row] = value;
			}
		}

		int step = size / 3;
		string rep = "";
		for (int row = size - 1; row >= 0; row -= step) // rows
		{
			rep += "/ ";
			for (int col = 0; col < size; col += step) // columns
			{
				rep += values[col, row].ToString("N2") + ", ";
			}
		}
		Debug.LogFormat("QuadSlope: size: {0}, abcd: {1}, {2}, {3}, {4}: {5})",
			size, a, b, c, d, rep);
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
		//  0 <--- u ---> 1
		//  a ----------- b    1
		//  |             |   /|\
		//  |             |    
		//  |             |    v
		//  |  * (u, v)   |     
		//  |             |   \|/
		//  d------------ c    0
		//
		// a, b, c, and d are the vertices of the quadrilateral. They are assumed to exist in the
		// same plane in 3D space, but this function will allow for some non-planar error.
		//
		// Variables u and v are the two-dimensional local coordinates inside the quadrilateral.
		// To find a point that is inside the quadrilateral, both u and v must be between 0 and 1 inclusive.  
		// For example, if you send this function u=0, v=0, then it will return coordinate "d".  
		// Similarly, coordinate u=1, v=1 will return vector "b". Any values between 0 and 1
		// will return a coordinate that is bi-linearly interpolated between the four vertices.		

		float abCol = Mathf.Lerp(a, b, u);
		float dcCol = Mathf.Lerp(d, c, u);
		float result = Mathf.Lerp(dcCol, abCol, v);
		return result;
	}
}

public struct Quadrant 
{
	public readonly float[,] values;

	public Quadrant(float[,] values)
	{
		this.values = values;
	}
}

