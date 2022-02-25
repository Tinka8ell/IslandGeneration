using UnityEngine;
using System.Collections;

public static class MeshGenerator {

	public static MeshData GenerateSeaMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
	{
		return GenerateTerrainMesh(heightMap, meshSettings, levelOfDetail);
	}

	public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
	{
		int skipIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
		int numVertsPerLine = meshSettings.numVertsPerLine;

		// Assuming relative to centre, top left is half width to left (-1) and half width forward (+1)
		Vector2 topLeft = new Vector2 (-1, +1) * meshSettings.meshWorldSize / 2f;

		MeshData meshData = new MeshData (numVertsPerLine, skipIncrement, meshSettings.useFlatShading);

		int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
		int meshVertexIndex = 0;
		int outOfMeshVertexIndex = -1;

		for (int row = 0; row < numVertsPerLine; row ++) {
			for (int col = 0; col < numVertsPerLine; col ++) {
				bool isOutOfMeshVertex = row == 0 || row == numVertsPerLine - 1 || col == 0 || col == numVertsPerLine - 1;
				bool isSkippedVertex = col > 2 && col < numVertsPerLine - 3 && row > 2 && row < numVertsPerLine - 3 && ((col - 2) % skipIncrement != 0 || (row - 2) % skipIncrement != 0);
				if (isOutOfMeshVertex) {
					vertexIndicesMap [col, row] = outOfMeshVertexIndex;
					outOfMeshVertexIndex--;
				} else if (!isSkippedVertex) {
					vertexIndicesMap [col, row] = meshVertexIndex;
					meshVertexIndex++;
				}
			}
		}

		for (int row = 0; row < numVertsPerLine; row ++) {
			for (int col = 0; col < numVertsPerLine; col++) {
				bool isSkippedVertex = col > 2 && col < numVertsPerLine - 3 && row > 2 && row < numVertsPerLine - 3 && ((col - 2) % skipIncrement != 0 || (row - 2) % skipIncrement != 0);

				if (!isSkippedVertex) {
					bool isOutOfMeshVertex = row == 0 || row == numVertsPerLine - 1 || col == 0 || col == numVertsPerLine - 1;
					bool isMeshEdgeVertex = (row == 1 || row == numVertsPerLine - 2 || col == 1 || col == numVertsPerLine - 2) && !isOutOfMeshVertex;
					bool isMainVertex = (col - 2) % skipIncrement == 0 && (row - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;
					bool isEdgeConnectionVertex = (row == 2 || row == numVertsPerLine - 3 || col == 2 || col == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

					int vertexIndex = vertexIndicesMap [col, row];
					// calc relative position as %
					Vector2 percent = new Vector2(col - 1, row - 1) / (numVertsPerLine - 3);
					// calc offset poision from top left
					Vector2 vertexPosition2D = topLeft + new Vector2(percent.x,-percent.y) * meshSettings.meshWorldSize;
					// was: float height = heightMap[col, row]; // but this has swapped coordinates!
					float height = heightMap[row, col];  // match top left to bottom right in height map

					/* Removed for now as causes spikes at edges of LOD > 0 chunks!
					if (isEdgeConnectionVertex) {
						bool isVertical = col == 2 || col == numVertsPerLine - 3;
						int dstToMainVertexA = ((isVertical)?row - 2: col - 2) % skipIncrement;
						int dstToMainVertexB = skipIncrement - dstToMainVertexA;
						float dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement;

						float heightMainVertexA = heightMap [(isVertical) ? col : col - dstToMainVertexA, (isVertical) ? row - dstToMainVertexA : row];
						float heightMainVertexB = heightMap [(isVertical) ? col : col + dstToMainVertexB, (isVertical) ? row + dstToMainVertexB : row];

						height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
					}
					*/

					// Create the vertex using relative (x, y) => (x, y=h, z=y), but Unity uses left-handed axes!
					// Shouldn't it be (x, h, -y)?
					meshData.AddVertex (new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

					bool createTriangle = col < numVertsPerLine - 1 && row < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (col != 2 && row != 2));

					if (createTriangle) {
						int currentIncrement = (isMainVertex && col != numVertsPerLine - 3 && row != numVertsPerLine - 3) ? skipIncrement : 1;

						int a = vertexIndicesMap [col, row];
						int b = vertexIndicesMap [col + currentIncrement, row];
						int c = vertexIndicesMap [col, row + currentIncrement];
						int d = vertexIndicesMap [col + currentIncrement, row + currentIncrement];
						meshData.AddTriangle (a, d, c);
						meshData.AddTriangle (d, a, b);
					}
				}
			}
		}

		meshData.ProcessMesh ();

		return meshData;

	}
}

public class MeshData {
	Vector3[] vertices;
	int[] triangles;
	Vector2[] uvs;
	Vector3[] bakedNormals;

	Vector3[] outOfMeshVertices;
	int[] outOfMeshTriangles;

	int triangleIndex;
	int outOfMeshTriangleIndex;

	bool useFlatShading;

	public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading) {
		this.useFlatShading = useFlatShading;

		int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
		int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
		int numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
		int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

		vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
		uvs = new Vector2[vertices.Length];

		int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
		int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
		triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

		outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
		outOfMeshTriangles = new int[24 * (numVertsPerLine-2)];
	}

	public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex) {
		if (vertexIndex < 0) {
			outOfMeshVertices [-vertexIndex - 1] = vertexPosition;
		} else {
			vertices [vertexIndex] = vertexPosition;
			uvs [vertexIndex] = uv;
		}
	}

	public void AddTriangle(int a, int b, int c) {
		if (a < 0 || b < 0 || c < 0) {
			outOfMeshTriangles [outOfMeshTriangleIndex] = a;
			outOfMeshTriangles [outOfMeshTriangleIndex + 1] = b;
			outOfMeshTriangles [outOfMeshTriangleIndex + 2] = c;
			outOfMeshTriangleIndex += 3;
		} else {
			triangles [triangleIndex] = a;
			triangles [triangleIndex + 1] = b;
			triangles [triangleIndex + 2] = c;
			triangleIndex += 3;
		}
	}

	Vector3[] CalculateNormals() {

		Vector3[] vertexNormals = new Vector3[vertices.Length];
		int triangleCount = triangles.Length / 3;
		for (int i = 0; i < triangleCount; i++) {
			int normalTriangleIndex = i * 3;
			int vertexIndexA = triangles [normalTriangleIndex];
			int vertexIndexB = triangles [normalTriangleIndex + 1];
			int vertexIndexC = triangles [normalTriangleIndex + 2];

			Vector3 triangleNormal = SurfaceNormalFromIndices (vertexIndexA, vertexIndexB, vertexIndexC);
			vertexNormals [vertexIndexA] += triangleNormal;
			vertexNormals [vertexIndexB] += triangleNormal;
			vertexNormals [vertexIndexC] += triangleNormal;
		}

		int borderTriangleCount = outOfMeshTriangles.Length / 3;
		for (int i = 0; i < borderTriangleCount; i++) {
			int normalTriangleIndex = i * 3;
			int vertexIndexA = outOfMeshTriangles [normalTriangleIndex];
			int vertexIndexB = outOfMeshTriangles [normalTriangleIndex + 1];
			int vertexIndexC = outOfMeshTriangles [normalTriangleIndex + 2];

			Vector3 triangleNormal = SurfaceNormalFromIndices (vertexIndexA, vertexIndexB, vertexIndexC);
			if (vertexIndexA >= 0) {
				vertexNormals [vertexIndexA] += triangleNormal;
			}
			if (vertexIndexB >= 0) {
				vertexNormals [vertexIndexB] += triangleNormal;
			}
			if (vertexIndexC >= 0) {
				vertexNormals [vertexIndexC] += triangleNormal;
			}
		}


		for (int i = 0; i < vertexNormals.Length; i++) {
			vertexNormals [i].Normalize ();
		}

		return vertexNormals;

	}

	Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
		Vector3 pointA = (indexA < 0)?outOfMeshVertices[-indexA-1] : vertices [indexA];
		Vector3 pointB = (indexB < 0)?outOfMeshVertices[-indexB-1] : vertices [indexB];
		Vector3 pointC = (indexC < 0)?outOfMeshVertices[-indexC-1] : vertices [indexC];

		Vector3 sideAB = pointB - pointA;
		Vector3 sideAC = pointC - pointA;
		return Vector3.Cross (sideAB, sideAC).normalized;
	}

	public void ProcessMesh() {
		if (useFlatShading) {
			FlatShading ();
		} else {
			BakeNormals ();
		}
	}

	void BakeNormals() {
		bakedNormals = CalculateNormals ();
	}

	void FlatShading() {
		Vector3[] flatShadedVertices = new Vector3[triangles.Length];
		Vector2[] flatShadedUvs = new Vector2[triangles.Length];

		for (int i = 0; i < triangles.Length; i++) {
			flatShadedVertices [i] = vertices [triangles [i]];
			flatShadedUvs [i] = uvs [triangles [i]];
			triangles [i] = i;
		}

		vertices = flatShadedVertices;
		uvs = flatShadedUvs;
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		if (useFlatShading) {
			mesh.RecalculateNormals ();
		} else {
			mesh.normals = bakedNormals;
		}
		return mesh;
	}

}
