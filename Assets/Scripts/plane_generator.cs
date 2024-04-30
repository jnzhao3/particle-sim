using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typof(MeshFilter))]
public class plane_generator : MonoBehaviour
{
    public static Mesh mesh;

    public static Vector3[] vertices;
    // public static Vector3[] positions; //added
    int[] triangles;
    //public int gridSize = 64; // Change this to set the number of grid cells
    //float planeSize = 16f; // Change this to set the overall size of the plane
    [Header("Recompute Tree Positions (Currently not realtime):")]
    public bool recalculate;

    [Header("Map Scroll Settings:")]
    public Vector3 globalScrollSpeed;
    public Vector3 globalScroll = new Vector3(0, 0, 0);

    [Header("Efficiency settings:")]
    public bool realtime_update = false; // Adjust this for depth/thickness
    public int instances;
    public Mesh treeMesh;
    public Material[] materials;
    private List<List<Matrix4x4>> batches = new List<List<Matrix4x4>>();
    private List<Vector3> vectorList = new List<Vector3>();

    [Header("Mesh Settings")]
    public static int gridSize = 64; // Change this to set the number of grid cells
    public static float planeSize = 16f;
    public float thickness = 1.0f; // Adjust this for depth/thickness
    public Gradient worldColor;
    [Range(0.0f, 10.0f)]
    public float lowerBound = 0.5f;
    [Range(0.0f, 10.0f)]
    public float upperBound = 2f;

    [Header("Generation Settings")]
    public bool trees = false;

    [Header("Perlin Noise Layer 1 Settings")]
    [Range(0.1f, 10.0f)]
    public float frequency = 0.5f;
    [Range(0.0f, 10.0f)]
    public float heightScale = 2f;
    public float p_offset_x = 8f;
    public float p_offset_z = 8f;
    //[Range(0.0f, 1.0f)]
    //public float scroll_speed = 0.0f;


    [Header("Perlin Layer 2 Settings")]
    [Range(0.1f, 10.0f)]
    public float frequency2 = 0.5f;
    [Range(0.0f, 10.0f)]
    public float heightScale2 = 2f;
    public float p_offset_x2 = 8f;
    public float p_offset_z2 = 8f;

    [Header("Other")]
    //public GameObject spawnedobj;
    [Range(0.0f, 0.1f)]
    public float speed;
    public float scaleofwaves;
    public float wavelength;
    Color[] colors;

    Texture2D texture;
    float t;
    
    private void RecalculateBatches()
    {
        int add_matrices = 0;
        batches = new List<List<Matrix4x4>>();
        batches.Add(new List<Matrix4x4>());

        for (int i = 0; i < vectorList.Count; i++)
        {
            if (add_matrices < 1000)
            {
                float sizeval = Random.Range(0.6f, 0.9f);
                batches[batches.Count - 1].Add(item:
                                    Matrix4x4.TRS(
                                            pos: vectorList[i],
                                            q: Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f),
                                            s: new Vector3(x: sizeval, y: sizeval, z: sizeval))
                                        );
                add_matrices++;
            }
            else
            {
                batches.Add(new List<Matrix4x4>());
                add_matrices = 0;
            }
        }
    }


    private void RenderBatches()
    {
        //Debug.Log(batches.Count);
        foreach (var batch in batches) {
            for (int i = 0; i < treeMesh.subMeshCount; i++)
            {
                //Debug.Log("hi there");
                Graphics.DrawMeshInstanced(treeMesh, i, materials[i], batch);
            }
        }
    }
    bool initial;

    void Start()
    {
        initial = true;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreatePlane();
        SetPerlin();
        ApplyEffects();
        GenerateTexture();
        ApplyTextureToMesh();
        //GenTrees();
        UpdatePlane();


        RecalculateBatches();
        // create instances
        /*int add_matrices = 0;
        batches.Add(new List<Matrix4x4>());
        
        for (int i=0; i < vectorList.Count; i++)
        {
            if (add_matrices < 1000)
            {
                float sizeval = Random.Range(0.6f, 0.9f);
                batches[batches.Count - 1].Add(item:
                                    Matrix4x4.TRS(
                                            pos: vectorList[i],
                                            q: Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f),
                                            s: new Vector3(x: sizeval, y: sizeval, z: sizeval))
                                        );
                add_matrices++;
            } else
            {
                batches.Add(new List<Matrix4x4>());
                add_matrices = 0;
            }
        }*/

    }

    void Update()
    {
        initial = false;
        t += speed;
        if (recalculate)
        {
            RecalculateBatches();
            recalculate = false;
        }
        if (realtime_update)
        {
            //CreatePlane();
            SetPerlin();
            //ApplyEffects();
            //GenerateTexture();
            //ApplyTextureToMesh();
            UpdatePlane();
            //p_offset_z += scroll_speed;
        }
        RenderBatches();
    }

    void CreatePlane()
    {
        vertices = new Vector3[(gridSize + 1) * (gridSize + 1)]; // Increase vertex count for grid corners
        // positions = new Vector3[(gridSize + 1) * (gridSize + 1)]; //added
        triangles = new int[gridSize * gridSize * 6]; // Each grid cell has 2 triangles (6 vertices)
        colors = new Color[vertices.Length]; // Initialize color array

        // Initialize vertex positions
        for (int z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                float xPos = (float)x / gridSize * planeSize - planeSize / 2.0f;
                float zPos = (float)z / gridSize * planeSize - planeSize / 2.0f;
                // positions[zPos * (planeSize + 1) + xPos] = new Vector3(x, 0, z); //added
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
        int counter = 0;
        // vectorList = new List<Vector3>();
        for (int i = 0; i < vertices.Length; i++)
        {
            float new_y = Mathf.PerlinNoise(vertices[i].x * frequency + p_offset_x + globalScroll.x, vertices[i].z * frequency + p_offset_z + globalScroll.z);
            float new_y2 = Mathf.PerlinNoise(vertices[i].x * frequency2 + p_offset_x2 + globalScroll.x, vertices[i].z * frequency2 + p_offset_z2 + globalScroll.z);
            float new_y3 = Mathf.PerlinNoise(vertices[i].x * 3 + globalScroll.x, vertices[i].z * 3 + globalScroll.z) * 0.2f;

            //Debug.Log(vertices[i].x * 3.23f);
            //Debug.Log(new_y);
            new_y = new_y * heightScale + new_y2 * heightScale2;
            new_y = Mathf.Clamp(new_y, lowerBound, upperBound);
            vertices[i] = new Vector3(vertices[i].x, new_y + new_y3, vertices[i].z);

            /* if (Random.Range(-1.0f, 1.0f) < -0.9f)
             {
                 Instantiate(spawnedobj, vertices[i] + new Vector3(0, 0.1f, 0), Quaternion.identity);
             }*/
            //float funcval = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(vertices[i].z, 2f) + Mathf.Pow(vertices[i].x, 2f)) / 10);
            //vertices[i] += new Vector3(0f, funcval, 0f);

            if (trees)
            {
                float treenoise = Mathf.PerlinNoise(vertices[i].x * 0.5f+100, vertices[i].z * 0.5f + 100);
                Vector3 tree_pos = new Vector3(vertices[i].x + Random.Range(-0.2f, 0.2f), 0.1f, vertices[i].z + Random.Range(-0.2f, 0.2f));
                if (treenoise > 0.5f)
                {
                    new_y = Mathf.PerlinNoise(tree_pos.x * frequency + p_offset_x, tree_pos.z * frequency + p_offset_z);
                    new_y2 = Mathf.PerlinNoise(tree_pos.x * frequency2 + p_offset_x2, tree_pos.z * frequency2 + p_offset_z2);
                    new_y3 = Mathf.PerlinNoise(tree_pos.x * 3, tree_pos.z * 3) * 0.2f;
                    new_y = new_y * heightScale + new_y2 * heightScale2;
                    new_y = Mathf.Clamp(new_y, lowerBound, upperBound);
                    tree_pos.y = new_y + new_y3;

                    if (tree_pos.y > 2.2f)
                    {
                        //vectorList.Add(tree_pos);
                        if (initial)
                        {
                            vectorList.Add(tree_pos);
                        } else
                        {
                            if (counter < vectorList.Count)
                            {
                                vectorList[counter] = (tree_pos);
                            }
                            counter++;
                        }


                        //GameObject tree = Instantiate(spawnedobj, tree_pos, Quaternion.identity);
                        //tree.transform.localScale = new Vector3(treenoise, treenoise, treenoise);

                        //tree.transform.Rotate(0.0f, Random.Range(-90.0f, 90.0f), 0.0f, Space.World);
                    }
                }
            }
        }
    }

    void GenTrees()
    {
        /*if (trees)
        {
            for (int i = 0; i < vertices.Length; i++)
            {

                float new_y3 = Mathf.PerlinNoise(vertices[i].x * 0.5f, vertices[i].z * 0.5f);

                //Debug.Log(vertices[i].x * 3.23f);
                //Debug.Log(new_y);
                //new_y = new_y * heightScale + new_y2 * heightScale2;
                //new_y = Mathf.Clamp(new_y, lowerbound, upperbound);
                //ertices[i] = new Vector3(vertices[i].x, new_y + new_y3, vertices[i].z);

                if (new_y3 > 0.5f)
                {
                    GameObject tree = Instantiate(spawnedobj, vertices[i] + new Vector3(Random.Range(-0.2f, 0.2f), 0.1f, Random.Range(-0.2f, 0.2f)), Quaternion.identity);
                    tree.transform.localScale = new Vector3(new_y3, new_y3, new_y3);

                        tree.transform.Rotate(0.0f, Random.Range(-90.0f, 90.0f), 0.0f, Space.World);
                }
            }
        }*/
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

    void ApplyEffects()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
           // float funcval = Mathf.Sin(Mathf.Pow(vertices[i].z, 2) + Mathf.Pow(vertices[i].x, 2) * wavelength + t) * scaleofwaves;
            //float funcval = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(vertices[i].z, 2f) + Mathf.Pow(vertices[i].x, 2f)) / 10);
            //vertices[i] += new Vector3( 0f, funcval, 0f);
        }
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
        mesh.colors = colors; // Assign colors to the mesh

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
