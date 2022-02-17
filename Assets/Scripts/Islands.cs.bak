using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Islands 
{
	public static bool IsIsland(Vector2 coord, float threshold, float offset = 1f)
	{
		Vector2 movedCoord = coord * offset;
		float perlinValue = Mathf.PerlinNoise(movedCoord.x, movedCoord.y) * 2 - 1;
		return perlinValue > threshold;
	}

	public static bool IsFixedIsland(Vector2 coord, float xAdjust = 1f, float yAdjust = 1f, bool debug = false)
	{
		const int radix = 3;
		const int slice = 2;
		int pos = Mathf.RoundToInt(coord.x * xAdjust + coord.y * yAdjust);
		if (debug) Debug.Log("IsFixed: coord = " + coord + ", pos = " + pos + ", mod = " + pos % radix);
		pos %= radix;
		if (pos < 0) pos += radix;
		return pos < slice;
	}

	public static bool EveryOther(Vector2 coord)
    {
		return ((coord.x + coord.y) % 2) == 0;
    }

	static bool Check(Vector2 coord, bool debug = false)
	{
		bool isLand = false;
		// isLand = IsIsland(coord, .95f); 
		isLand = IsFixedIsland(coord, 1f, 1f, debug);
		// isLand = EveryOther(coord);
		if (debug) Debug.Log("Checking: " + coord + " returning: " + isLand);
		return isLand;
	}

	public static Anews LocalNews(Vector2 coord, bool debug=false)
	{
		bool nw = false;
		bool n = false;
		bool ne = false;
		bool w = false;
		bool e = false;
		bool sw = false;
		bool s = false;
		bool se = false;
		bool isIsland = Check(coord, debug);
		if (isIsland)
        {
			nw = Check(coord + new Vector2(-1, -1), debug);
			n =  Check(coord + new Vector2(0, -1), debug);
			ne = Check(coord + new Vector2(1, -1), debug);
			w =  Check(coord + new Vector2(-1, 0), debug);
			e =  Check(coord + new Vector2(1, 0), debug);
			sw = Check(coord + new Vector2(-1, 1), debug);
			s =  Check(coord + new Vector2(0, 1), debug);
			se = Check(coord + new Vector2(1, 1), debug);
		}
		Anews anews = new Anews(nw, n, ne, w, isIsland, e, sw, s, se);
		if (debug) Debug.Log("LocalNews for: " + coord + " is " + anews + " with index: " + anews.ToIndex());
		return anews;
	}


	public static float[,] GetIslandMap(int width)
	{
		// as we always use squares, use width for both dimentions!
		float[,] values = new float[width, width];
		int halfX = (width + 1) / 2;
		int halfY = (width - 1) / 2;

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < width; j++)
			{

				values[i, j] = Check(new Vector2(i - halfX, halfY - j))? 1f: 0f; 
			}
		}
		return values;
	}

}

