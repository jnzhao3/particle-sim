using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typof(MeshFilter))]
public class plane_generator_cubes : MonoBehaviour
{
    Mesh mesh;
    private List<Vector3> vertices;
    Vector3[] mesh_vertices;
    private List<Triangle> triangles;
    int[] mesh_triangles;
    Vector2[] mesh_uvs;

    [Header("Efficiency settings:")]
    public bool realtime_update = false; // Adjust this for depth/thickness
    [Header("Mesh Settings")]
    public int gridSize = 64; // Change this to set the number of grid cells
    public float planeSize = 16f;
    public float thickness = 1.0f; // Adjust this for depth/thickness
    //public Gradient gradient;
    [Range(0.0f, 10.0f)]
    public float lowerbound = 0.5f;
    [Range(0.0f, 10.0f)]
    public float upperbound = 2f;

    [Header("Perlin Noise Settings")]
    [Range(0.1f, 10.0f)]
    public float frequency = 0.5f;
    [Range(0.1f, 10.0f)]
    public float heightScale = 2f;
    public float p_offset_x = 8f;
    public float p_offset_z = 8f;
    [Range(0.0f, 1.0f)]
    public float scroll_speed = 0.0f;


    [Header("Perlin Noise 2 Settings")]
    [Range(0.1f, 10.0f)]
    public float frequency2 = 0.5f;
    [Range(0.1f, 10.0f)]
    public float heightScale2 = 2f;
    public float p_offset_x2 = 8f;
    public float p_offset_z2 = 8f;

    Color[] colors;

    private Tables tables;
    Vector3[] Grid;

    float isoLevel = 0.5f;

    Texture2D texture;

    struct Triangle {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreatePlane();
        // SetPerlin();

        tables = new Tables();
        InitGrid();
        MarchCubes();
        //GenerateTexture();
        // ApplyTextureToMesh();
        // UpdatePlane();
    }

    void InitGrid() {
        Grid = new Vector3[gridSize * gridSize * gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    float xPos = (float)x / gridSize * planeSize - planeSize / 2.0f;
                    float zPos = (float)z / gridSize * planeSize - planeSize / 2.0f;
                    float yPos = (float)y / gridSize * planeSize - planeSize / 2.0f;
                    Grid[x * gridSize * gridSize + y * gridSize + z] = new Vector3(xPos, yPos, zPos);
                }
            }
        }
    }

    float Noise3D(float x, float y, float z, float frequency = 0.2f, float amplitude = 1, float persistence = 0.1f, int octave = 1, int seed = 12)
	{
		// float noise = 0.0f;

		// for (int i = 0; i < octave; ++i)
		// {
		// 	// Get all permutations of noise for each individual axis
		// 	float noiseXY = Mathf.PerlinNoise(x * frequency + seed, y * frequency + seed) * amplitude;
		// 	float noiseXZ = Mathf.PerlinNoise(x * frequency + seed, z * frequency + seed) * amplitude;
		// 	float noiseYZ = Mathf.PerlinNoise(y * frequency + seed, z * frequency + seed) * amplitude;

		// 	// Reverse of the permutations of noise for each individual axis
		// 	float noiseYX = Mathf.PerlinNoise(y * frequency + seed, x * frequency + seed) * amplitude;
		// 	float noiseZX = Mathf.PerlinNoise(z * frequency + seed, x * frequency + seed) * amplitude;
		// 	float noiseZY = Mathf.PerlinNoise(z * frequency + seed, y * frequency + seed) * amplitude;

		// 	// Use the average of the noise functions
		// 	noise += (noiseXY + noiseXZ + noiseYZ + noiseYX + noiseZX + noiseZY) / 6.0f;

		// 	amplitude *= persistence;
		// 	frequency *= 2.0f;
		// }

		// // Use the average of all octaves
		// return noise / octave;

        float xy = Mathf.PerlinNoise(x * frequency + seed, y * frequency + seed);
        float xz = Mathf.PerlinNoise(x * frequency + seed, z * frequency + seed);
        float yz = Mathf.PerlinNoise(y * frequency + seed, z * frequency + seed);
        float yx = Mathf.PerlinNoise(y * frequency + seed, x * frequency + seed);
        float zx = Mathf.PerlinNoise(z * frequency + seed, x * frequency + seed);
        float zy = Mathf.PerlinNoise(z * frequency + seed, y * frequency + seed);

        return (xy + xz + yz + yx + zx + zy) / 6.0f;
	}

    void MarchCubes() {
        Debug.Log("Marching cubes");

        triangles = new List<plane_generator_cubes.Triangle>();
        vertices = new List<Vector3>();

        int triIndex = 0;

        for (int x = 0; x < gridSize - 1; x++) // TODO: this is hacky, fix
        {
            for (int y = 0; y < gridSize - 1; y++)
            {
                for (int z = 0; z < gridSize - 1; z++)
                {
                    List<plane_generator_cubes.Triangle> tris = MarchCube(x, y, z);
                    // add tris to triangles
                    if (tris != null) {
                        foreach (Triangle tri in tris) {
                            triangles.Add(tri);
                        }
                    }
                }
            }
        }
        ProcessTriangles();
    }

    List<plane_generator_cubes.Triangle> MarchCube(int x, int y, int z) {
        int cubeIndex = 0;
        Vector3[] cubeCorners = new Vector3[8];
        float[] cubeValues = new float[8];

        public double[] cubeCoords = table.cubeCoords;

        for (int i = 0; i < 8; i++)
        {
            cubeCorners[i] = Grid[(x + cubeCoords[0]) * gridSize * gridSize + (y + cubeCoords[1]) * gridSize + z + cubeCoords[2]];
        }

        // cubeCorners[0] = Grid[x * gridSize * gridSize + y * gridSize + z]; // 0, 0, 0
        // cubeCorners[1] = Grid[x * gridSize * gridSize + y * gridSize + z + 1]; // 0, 0, 1
        // cubeCorners[2] = Grid[(x + 1) * gridSize * gridSize + y * gridSize + z + 1]; // 1, 0, 1
        // cubeCorners[3] = Grid[(x + 1) * gridSize * gridSize + y * gridSize + z]; // 1, 0, 0
        // cubeCorners[4] = Grid[x * gridSize * gridSize + (y + 1) * gridSize + z]; // 0, 1, 0
        // cubeCorners[5] = Grid[x * gridSize * gridSize + (y + 1) * gridSize + z + 1]; // 0, 1, 1
        // cubeCorners[6] = Grid[(x + 1) * gridSize * gridSize + (y + 1) * gridSize + z + 1]; // 1, 1, 1
        // cubeCorners[7] = Grid[(x + 1) * gridSize * gridSize + (y + 1) * gridSize + z]; // 1, 1, 0

        for (int i = 0; i < 8; i++)
        {
            cubeValues[i] = Noise3D(cubeCorners[i].x, cubeCorners[i].y, cubeCorners[i].z);
        }

        for (int i = 0; i < 8; i++)
        {
            if (cubeValues[i] < isoLevel) cubeIndex |= 1 << i;
        }

        // if (cubeValues[0] < isoLevel) cubeIndex |= 1;
        // if (cubeValues[1] < isoLevel) cubeIndex |= 2;
        // if (cubeValues[2] < isoLevel) cubeIndex |= 4;
        // if (cubeValues[3] < isoLevel) cubeIndex |= 8;
        // if (cubeValues[4] < isoLevel) cubeIndex |= 16;
        // if (cubeValues[5] < isoLevel) cubeIndex |= 32;
        // if (cubeValues[6] < isoLevel) cubeIndex |= 64;
        // if (cubeValues[7] < isoLevel) cubeIndex |= 128;



        int edgeIndex = tables.getFromEdgeTable(cubeIndex);
        if (edgeIndex == 0) return new List<plane_generator_cubes.Triangle>();

        Vector3[] edgeVertices = new Vector3[12];

        // 0, 0, 0 + 0, 0, 1
        if ((edgeIndex & 1) == 1) edgeVertices[0] = VertexInterp(cubeCorners[0], cubeValues[0], cubeCorners[1], cubeValues[1]);
        // 0, 0, 1 + 1, 0, 1
        if ((edgeIndex & 2) == 2) edgeVertices[1] = VertexInterp(cubeCorners[1], cubeValues[1], cubeCorners[2], cubeValues[2]);
        // 1, 0, 1 + 1, 0, 0
        if ((edgeIndex & 4) == 4) edgeVertices[2] = VertexInterp(cubeCorners[2], cubeValues[2], cubeCorners[3], cubeValues[3]);
        // 1, 0, 0 + 0, 0, 0
        if ((edgeIndex & 8) == 8) edgeVertices[3] = VertexInterp(cubeCorners[3], cubeValues[3], cubeCorners[0], cubeValues[0]);
        // 0, 1, 0 + 0, 1, 1
        if ((edgeIndex & 16) == 16) edgeVertices[4] = VertexInterp(cubeCorners[4], cubeValues[4], cubeCorners[5], cubeValues[5]);
        // 0, 1, 1 + 1, 1, 1
        if ((edgeIndex & 32) == 32) edgeVertices[5] = VertexInterp(cubeCorners[5], cubeValues[5], cubeCorners[6], cubeValues[6]);
        // 1, 1, 1 + 1, 1, 0
        if ((edgeIndex & 64) == 64) edgeVertices[6] = VertexInterp(cubeCorners[6], cubeValues[6], cubeCorners[7], cubeValues[7]);
        // 1, 1, 0 + 0, 1, 0
        if ((edgeIndex & 128) == 128) edgeVertices[7] = VertexInterp(cubeCorners[7], cubeValues[7], cubeCorners[4], cubeValues[4]);
        // 0, 0, 0 + 0, 1, 0
        if ((edgeIndex & 256) == 256) edgeVertices[8] = VertexInterp(cubeCorners[0], cubeValues[0], cubeCorners[4], cubeValues[4]);
        // 0, 0, 1 + 0, 1, 1
        if ((edgeIndex & 512) == 512) edgeVertices[9] = VertexInterp(cubeCorners[1], cubeValues[1], cubeCorners[5], cubeValues[5]);
        // 1, 0, 1 + 1, 1, 1
        if ((edgeIndex & 1024) == 1024) edgeVertices[10] = VertexInterp(cubeCorners[2], cubeValues[2], cubeCorners[6], cubeValues[6]);
        // 1, 0, 0 + 1, 1, 0
        if ((edgeIndex & 2048) == 2048) edgeVertices[11] = VertexInterp(cubeCorners[3], cubeValues[3], cubeCorners[7], cubeValues[7]);

        List<plane_generator_cubes.Triangle> tris = new List<plane_generator_cubes.Triangle>();

        for (int i = 0; tables.getFromTriTable(cubeIndex, i) != -1; i += 3) {
            tris.Add(new Triangle {
                v1 = edgeVertices[tables.getFromTriTable(cubeIndex, i)],
                v2 = edgeVertices[tables.getFromTriTable(cubeIndex, i + 1)],
                v3 = edgeVertices[tables.getFromTriTable(cubeIndex, i + 2)]
            });
        }

        return tris;
    }

    // implement interpolate
    Vector3 VertexInterp(Vector3 p1, float v1, Vector3 p2, float v2) 
    {
        if (Mathf.Abs((float)isoLevel - v1) < 0.0001) return p1;
        if (Mathf.Abs((float)isoLevel - v2) < 0.0001) return p2;
        if (Mathf.Abs(v1 - v2) < 0.0001) return p1;

        float mu = ((float)isoLevel - v1) / (v2 - v1);
        return new Vector3(p1.x + mu * (p2.x - p1.x), p1.y + mu * (p2.y - p1.y), p1.z + mu * (p2.z - p1.z));
    }

    void ProcessTriangles() 
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        mesh_vertices = new Vector3[triangles.Count * 3];
        mesh_triangles = new int[triangles.Count * 3];
        mesh_uvs = new Vector2[triangles.Count * 3];

        for (int i = 0; i < triangles.Count; i++) {

            mesh_vertices[i * 3].Set(triangles[i].v1.x, triangles[i].v1.y, triangles[i].v1.z);
            mesh_vertices[i * 3 + 1].Set(triangles[i].v2.x, triangles[i].v2.y, triangles[i].v2.z);
            mesh_vertices[i * 3 + 2].Set(triangles[i].v3.x, triangles[i].v3.y, triangles[i].v3.z);

            mesh_uvs[i * 3] = new Vector2(0, 0);
            mesh_uvs[i * 3 + 1] = new Vector2(0, 1);
            mesh_uvs[i * 3 + 2] = new Vector2(1, 0);

            // mesh_triangles[i * 3] = i * 3 + 2;
            // mesh_triangles[i * 3 + 1] = i * 3 + 1;
            // mesh_triangles[i * 3 + 2] = i * 3;

            mesh_triangles[i * 3] = i * 3;
            mesh_triangles[i * 3 + 1] = i * 3 + 1;
            mesh_triangles[i * 3 + 2] = i * 3 + 2;

        }

        mesh.vertices = mesh_vertices;
        mesh.triangles = mesh_triangles;
        mesh.uv = mesh_uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

    }

    void Update()
    {
        if (realtime_update)
        {
            CreatePlane();
            MarchCubes();
            // p_offset_z += scroll_speed;
        }
    }

    void CreatePlane()
    {
        vertices = new List<Vector3>();
        triangles = new List<plane_generator_cubes.Triangle>();
    }

    void SetPerlin()
    {
            // for (int i = 0; i < vertices.Length; i++)
            // {
            //     float new_y = Mathf.PerlinNoise(vertices[i].x * frequency + p_offset_x, vertices[i].z * frequency + p_offset_z);
            //     float new_y2 = Mathf.PerlinNoise(vertices[i].x * frequency2 + p_offset_x2, vertices[i].z * frequency2 + p_offset_z2);
            //     float new_y3 = Mathf.PerlinNoise(vertices[i].x * 3, vertices[i].z * 3) * 0.2f;

            //     //Debug.Log(vertices[i].x * 3.23f);
            //     //Debug.Log(new_y);
            //     new_y = new_y * heightScale + new_y2 * heightScale2;
            //     new_y = Mathf.Clamp(new_y, lowerbound, upperbound);
            // vertices[i] = new Vector3(vertices[i].x, new_y + new_y3, vertices[i].z);
            // }
            return;
    }

    void SetColors()
    {
            // for (int i = 0; i < vertices.Length; i++)
            // {
            //     float normalizedHeight = vertices[i].y / heightScale; // Normalize height to range [0, 1]
            //     colors[i] = new Color(1.0f - normalizedHeight, 0.0f, normalizedHeight);
            // }
            return;
    }


    void GenerateTexture()
    {
            // texture = new Texture2D(gridSize + 1, gridSize + 1);

            // for (int z = 0; z <= gridSize; z++)
            // {
            //     for (int x = 0; x <= gridSize; x++)
            //     {
            //         float normalizedHeight = vertices[z * (gridSize + 1) + x].y / heightScale;
            //         Color color = new Color(1.0f - normalizedHeight, 0.0f, normalizedHeight);
            //         texture.SetPixel(x, z, color);
            //     }
            // }

            // texture.Apply();
            return;
    }

    void ApplyTextureToMesh()
    {
            //   Material material = GetComponent<Renderer>().material;
            // material.mainTexture = texture;
            return;
    }

    // void UpdatePlane()
    // {
    //     mesh.Clear();
    //     // mesh.vertices = vertices;
    //     // mesh.vertices = vertices.ToArray();
    //     mesh.vertices = mesh_vertices;
    //     mesh.triangles = mesh_triangles;
    //     mesh.uv = mesh_uvs;
    //     // mesh.triangles = triangles;
    //     // mesh.triangles = triangles.ToArray();
    //     //mesh.colors = colors; // Assign colors to the mesh

    //     mesh.RecalculateNormals();
    //     mesh.RecalculateBounds();


    //     /*mesh.Clear();
    //     mesh.vertices = vertices;
    //     mesh.triangles = triangles;
    //     mesh.colors = colors; // Colors are not used since we're using a texture

    //     // Calculate normals manually for each face
    //     Vector3[] normals = new Vector3[vertices.Length];
    //     for (int i = 0; i < triangles.Length; i += 3)
    //     {
    //         Vector3 v1 = vertices[triangles[i + 1]] - vertices[triangles[i]];
    //         Vector3 v2 = vertices[triangles[i + 2]] - vertices[triangles[i]];
    //         Vector3 normal = Vector3.Cross(v1, v2).normalized;

    //         normals[triangles[i]] += normal;
    //         normals[triangles[i + 1]] += normal;
    //         normals[triangles[i + 2]] += normal;
    //     }

    //     // Assign normals to the mesh
    //     mesh.normals = normals;

    //     // Recalculate bounds to avoid clipping issues
    //     mesh.RecalculateBounds();*/
    // }
}
