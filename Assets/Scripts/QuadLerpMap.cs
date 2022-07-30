using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class QuadLerpMap 
{
	// Generate a map based on the four courners ...
	public float[,] values;

	public QuadLerpMap(int numberOfVertices, FalloffSettings falloffSettings, bool tl, bool tr, bool bl, bool br)
	{
		float a = (tl) ? 0f : 1f;
		float b = (tr) ? 0f : 1f;
		float c = (br) ? 0f : 1f;
		float d = (bl) ? 0f : 1f;

		Debug.Log("QuadLerpMap: size = " + numberOfVertices + ", abce = (" + a + ", " + b + ", " + c + ", " + d + ")");

		values = new float[numberOfVertices, numberOfVertices];
		for (int i = 1; i < numberOfVertices; i++)
        {
			for (int j = 1; j < numberOfVertices; j++)
			{
				values[i, j] = Evaluate(
					QuadLerp(a, b, c, d, i / (numberOfVertices - 1f), j / (numberOfVertices - 1f)),
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
		return Mathf.Lerp(abu, dcu, v);
	}

}
