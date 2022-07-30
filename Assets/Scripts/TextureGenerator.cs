using UnityEngine;
using System.Collections;

public static class TextureGenerator {
	public static Texture2D TextureFromHeightMap(HeightMap heightMap) {
		// heightMap values are a float array where coord (x, y)
		// is reperesented by heightMap.values[x, y]
		int width = heightMap.values.GetLength (0);
		int height = heightMap.values.GetLength (1);

		Color[] colourMap = new Color[width * height];
		for (int row = 0; row < height; row++) {
			for (int col = 0; col < width; col++) {
				colourMap [row * width + col] = 
					Color.Lerp (
						Color.black, 
						Color.white, 
						Mathf.InverseLerp(
							heightMap.minValue,
							heightMap.maxValue,
							heightMap.values[width - 1 - col, height - 1 - row]
							)
						);
			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}

	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
	{
		Texture2D texture = new Texture2D(width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colourMap);
		texture.Apply();
		return texture;
	}

}
