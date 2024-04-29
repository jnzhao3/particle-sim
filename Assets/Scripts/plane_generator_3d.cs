using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plane_generator_3d : MonoBehaviour
{
    static public GameObject[] createdObjects;

    [Header("3D Perlin Noise Settings")]
    public bool update = false;
    public int width = 64;
    public int height = 64;
    public int depth = 64;
    public float planeSize = 16f;
    public float x_off = 0f;
    public float y_off = 0f;
    public float z_off = 0f;
    [Range(0.0f, 1.0f)]
    public float frequency = 1f;
    [Range(0.0f, 1.0f)]
    public float amplitude = 1f;

    public GameObject gameObj;

    private void Start() {
        DrawPerlin();
    }

    private void Update() {
        if (update) {
            PerlinUpdate();
        }
    }

    private void DrawPerlin() {
        createdObjects = new GameObject[width * height * depth];
        int count = 0;

        for (float x = -1f * (float) width / 2f; x < (float) width / 2f; x += 1f) {
            for (float y = -1f * (float) height / 2f; y < (float) height / 2f; y += 1f) {
                for (float z = -1f * (float) depth / 2f; z < (float) depth / 2f; z += 1f) {
                    createdObjects[count] = Instantiate(gameObj, new Vector3(x, y, z), Quaternion.identity);
                    if (Perlin3D(x, y, z) >= .5f) {
                        createdObjects[count].SetActive(true);
                    } else {
                        createdObjects[count].SetActive(false);
                    }
                    count++;
                }
            }
        }
    }

    private void PerlinUpdate() {
        for (int i = 0; i < createdObjects.Length; i++) {
            Vector3 pos = createdObjects[i].transform.position;
            if (Perlin3D(pos.x, pos.y, pos.z) >= .5f) {
                createdObjects[i].SetActive(true);
            } else {
                createdObjects[i].SetActive(false);
            }
        }
    }

    private float Perlin3D(float x, float y, float z) {
        float a, b, c, d, e, f;
        a = Mathf.PerlinNoise((x + x_off) * frequency, (y + y_off) * frequency) / amplitude;
        b = Mathf.PerlinNoise((y + y_off) * frequency, (z + z_off) * frequency) / amplitude;
        c = Mathf.PerlinNoise((z + z_off) * frequency, (x + x_off) * frequency) / amplitude;
        d = Mathf.PerlinNoise((x + x_off) * frequency, (z + z_off) * frequency) / amplitude;
        e = Mathf.PerlinNoise((y + y_off) * frequency, (x + x_off) * frequency) / amplitude;
        f = Mathf.PerlinNoise((z + z_off) * frequency, (y + y_off) * frequency) / amplitude;

        return (a + b + c + d + e + f) / 6f;
    }
}
