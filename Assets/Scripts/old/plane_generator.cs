using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typof(MeshFilter))]
public class plane_generator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    //public int gridSize = 64; // Change this to set the number of grid cells
    //float planeSize = 16f; // Change this to set the overall size of the plane
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

    Texture2D texture;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreatePlane();
        SetPerlin();
        //GenerateTexture();
        ApplyTextureToMesh();
        UpdatePlane();
    }

    void Update()
    {
        if (realtime_update)
        {
            CreatePlane();
            SetPerlin();
            //GenerateTexture();
            //ApplyTextureToMesh();
            UpdatePlane();
            p_offset_z += scroll_speed;
        }
    }

    void CreatePlane()
    {
        vertices = new Vector3[(gridSize + 1) * (gridSize + 1)]; // Increase vertex count for grid corners
        triangles = new int[gridSize * gridSize * 6]; // Each grid cell has 2 triangles (6 vertices)
        colors = new Color[vertices.Length]; // Initialize color array

        // Initialize vertex positions
        for (int z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                float xPos = (float)x / gridSize * planeSize - planeSize / 2.0f;
                float zPos = (float)z / gridSize * planeSize - planeSize / 2.0f;
                vertices[z * (gridSize + 1) + x] = new Vector3(xPos, 0, zPos);
            }
        }

        // Generate triangles
        int vertIndex = 0;
        int triIndex = 0;

        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int topLeft = vertIndex;
                int topRight = vertIndex + 1;
                int bottomLeft = vertIndex + (gridSize + 1);
                int bottomRight = vertIndex + (gridSize + 1) + 1;

                triangles[triIndex] = topLeft;
                triangles[triIndex + 1] = bottomLeft;
                triangles[triIndex + 2] = topRight;
                triangles[triIndex + 3] = topRight;
                triangles[triIndex + 4] = bottomLeft;
                triangles[triIndex + 5] = bottomRight;

                triIndex += 6;
                vertIndex++;
            }
            vertIndex++;
        }
    }

    void SetVertexPositions(Vector3[] positions)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = positions[i];
        }
    }


    void SetPerlin()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            float new_y = Mathf.PerlinNoise(vertices[i].x * frequency + p_offset_x, vertices[i].z * frequency + p_offset_z);
            float new_y2 = Mathf.PerlinNoise(vertices[i].x * frequency2 + p_offset_x2, vertices[i].z * frequency2 + p_offset_z2);
            float new_y3 = Mathf.PerlinNoise(vertices[i].x * 3, vertices[i].z * 3) * 0.2f;

            //Debug.Log(vertices[i].x * 3.23f);
            //Debug.Log(new_y);
            new_y = new_y * heightScale + new_y2 * heightScale2;
            new_y = Mathf.Clamp(new_y, lowerbound, upperbound);
        vertices[i] = new Vector3(vertices[i].x, new_y + new_y3, vertices[i].z);
        }
    }

    void SetColors()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            float normalizedHeight = vertices[i].y / heightScale; // Normalize height to range [0, 1]
            colors[i] = new Color(1.0f - normalizedHeight, 0.0f, normalizedHeight);
        }
    }


    void GenerateTexture()
    {
        texture = new Texture2D(gridSize + 1, gridSize + 1);

        for (int z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                float normalizedHeight = vertices[z * (gridSize + 1) + x].y / heightScale;
                Color color = new Color(1.0f - normalizedHeight, 0.0f, normalizedHeight);
                texture.SetPixel(x, z, color);
            }
        }

        texture.Apply();
    }

    void ApplyTextureToMesh()
    {
     //   Material material = GetComponent<Renderer>().material;
       // material.mainTexture = texture;
    }

    void UpdatePlane()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        //mesh.colors = colors; // Assign colors to the mesh

        mesh.RecalculateNormals();


        /*mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors; // Colors are not used since we're using a texture

        // Calculate normals manually for each face
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i + 1]] - vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 2]] - vertices[triangles[i]];
            Vector3 normal = Vector3.Cross(v1, v2).normalized;

            normals[triangles[i]] += normal;
            normals[triangles[i + 1]] += normal;
            normals[triangles[i + 2]] += normal;
        }

        // Assign normals to the mesh
        mesh.normals = normals;

        // Recalculate bounds to avoid clipping issues
        mesh.RecalculateBounds();*/
    }
}
