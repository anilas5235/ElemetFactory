
using System;
using UnityEngine;

namespace Project.Scripts.Utilities
{
	public static class MeshUtils
	{
		private static Quaternion[] cachedQuaternionEulerArr;

		private static void CacheQuaternionEuler()
		{
			if (cachedQuaternionEulerArr != null) return;
			cachedQuaternionEulerArr = new Quaternion[360];
			for (var i = 0; i < 360; i++)
			{
				cachedQuaternionEulerArr[i] = Quaternion.Euler(0, 0, i);
			}
		}

		private static Quaternion GetQuaternionEuler(float rotFloat)
		{
			var rot = Mathf.RoundToInt(rotFloat);
			rot %= 360;
			if (rot < 0) rot += 360;
			if (rot >= 360) rot -= 360;
			if (cachedQuaternionEulerArr == null) CacheQuaternionEuler();
			return cachedQuaternionEulerArr[rot];
		}


		public static Mesh CreateEmptyMesh()
		{
			var mesh = new Mesh
			{
				vertices = Array.Empty<Vector3>(),
				uv = Array.Empty<Vector2>(),
				triangles = Array.Empty<int>()
			};
			return mesh;
		}

		public static void CreateEmptyMeshArrays(int quadCount, out Vector3[] vertices, out Vector2[] uvs,
			out int[] triangles)
		{
			vertices = new Vector3[4 * quadCount];
			uvs = new Vector2[4 * quadCount];
			triangles = new int[6 * quadCount];
		}

		public static Mesh CreateMesh(Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11)
		{
			return AddToMesh(null, pos, rot, baseSize, uv00, uv11);
		}

		public static Mesh AddToMesh(Mesh mesh, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11)
		{
			if (mesh == null)
			{
				mesh = CreateEmptyMesh();
			}

			var vertices = new Vector3[4 + mesh.vertices.Length];
			var uvs = new Vector2[4 + mesh.uv.Length];
			var triangles = new int[6 + mesh.triangles.Length];

			mesh.vertices.CopyTo(vertices, 0);
			mesh.uv.CopyTo(uvs, 0);
			mesh.triangles.CopyTo(triangles, 0);

			var index = vertices.Length / 4 - 1;
			//Relocate vertices
			var vIndex = index * 4;
			var vIndex0 = vIndex;
			var vIndex1 = vIndex + 1;
			var vIndex2 = vIndex + 2;
			var vIndex3 = vIndex + 3;

			baseSize *= .5f;

			var skewed = Math.Abs(baseSize.x - baseSize.y) > float.Epsilon;
			if (skewed)
			{
				vertices[vIndex0] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, baseSize.y);
				vertices[vIndex1] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, -baseSize.y);
				vertices[vIndex2] = pos + GetQuaternionEuler(rot) * new Vector3(baseSize.x, -baseSize.y);
				vertices[vIndex3] = pos + GetQuaternionEuler(rot) * baseSize;
			}
			else
			{
				vertices[vIndex0] = pos + GetQuaternionEuler(rot - 270) * baseSize;
				vertices[vIndex1] = pos + GetQuaternionEuler(rot - 180) * baseSize;
				vertices[vIndex2] = pos + GetQuaternionEuler(rot - 90) * baseSize;
				vertices[vIndex3] = pos + GetQuaternionEuler(rot - 0) * baseSize;
			}

			//Relocate UVs
			uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
			uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
			uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
			uvs[vIndex3] = new Vector2(uv11.x, uv11.y);

			//Create triangles
			var tIndex = index * 6;

			triangles[tIndex + 0] = vIndex0;
			triangles[tIndex + 1] = vIndex3;
			triangles[tIndex + 2] = vIndex1;

			triangles[tIndex + 3] = vIndex1;
			triangles[tIndex + 4] = vIndex3;
			triangles[tIndex + 5] = vIndex2;

			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = uvs;

			//mesh.bounds = bounds;

			return mesh;
		}

		public static void AddToMeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos,
			float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11)
		{
			//Relocate vertices
			var vIndex = index * 4;
			var vIndex0 = vIndex;
			var vIndex1 = vIndex + 1;
			var vIndex2 = vIndex + 2;
			var vIndex3 = vIndex + 3;

			baseSize *= .5f;

			var skewed = Math.Abs(baseSize.x - baseSize.y) > float.Epsilon;
			if (skewed)
			{
				vertices[vIndex0] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, baseSize.y);
				vertices[vIndex1] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, -baseSize.y);
				vertices[vIndex2] = pos + GetQuaternionEuler(rot) * new Vector3(baseSize.x, -baseSize.y);
				vertices[vIndex3] = pos + GetQuaternionEuler(rot) * baseSize;
			}
			else
			{
				vertices[vIndex0] = pos + GetQuaternionEuler(rot - 270) * baseSize;
				vertices[vIndex1] = pos + GetQuaternionEuler(rot - 180) * baseSize;
				vertices[vIndex2] = pos + GetQuaternionEuler(rot - 90) * baseSize;
				vertices[vIndex3] = pos + GetQuaternionEuler(rot - 0) * baseSize;
			}

			//Relocate UVs
			uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
			uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
			uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
			uvs[vIndex3] = new Vector2(uv11.x, uv11.y);

			//Create triangles
			var tIndex = index * 6;

			triangles[tIndex + 0] = vIndex0;
			triangles[tIndex + 1] = vIndex3;
			triangles[tIndex + 2] = vIndex1;

			triangles[tIndex + 3] = vIndex1;
			triangles[tIndex + 4] = vIndex3;
			triangles[tIndex + 5] = vIndex2;
		}
		
		/// <summary>
		/// Creates a quad Mesh
		/// </summary>
		/// <param name="center">in unit space / UV space</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static Mesh CreateQuad(Vector2 center,float width = 1, float height = 1)
		{
			var mesh = new Mesh();

			var offsetCenter = new Vector2(.5f, .5f) - center;
			var scale = new Vector2(width,height);

			var vertices = new Vector3[]
			{
				(offsetCenter + new Vector2(-.5f,-.5f)) * scale,
				(offsetCenter + new Vector2(-.5f,.5f))*scale,
				(offsetCenter + new Vector2(.5f,.5f))*scale,
				(offsetCenter + new Vector2(.5f,-.5f))*scale,
			};
			mesh.vertices = vertices;

			var tris = new[]
			{
				// lower left triangle
				0, 1, 3,
				// upper right triangle
				1, 2, 3
			};
			mesh.triangles = tris;

			var normals = new[]
			{
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward
			};
			mesh.normals = normals;

			var uv = new Vector2[]
			{
				new (0, 0),
				new (0, 1),
				new (1, 1),
				new (1, 0),
			};
			mesh.uv = uv;

			return mesh;
		}
	}
}
