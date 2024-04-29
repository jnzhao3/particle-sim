using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ParticleDisplay : MonoBehaviour
{
    public GameObject prefab;
    public ParticlesList particlesList;
    public List<SandParticle> particles;
    public List<GameObject> all_particles_go;

    Vector3[] vertices;
    int gridSize;
    // public Vector3 velocity;
    void Start()
    {
        // Initialize particles list from ParticlesList constructor
        float scale = 0.1f;
        particlesList = new ParticlesList(scale);
        particles = particlesList.particles;
 
        all_particles_go = new List<GameObject>();

        vertices = plane_generator.vertices;
        gridSize = plane_generator.gridSize;

        // Iterate through the list of particle objects
        foreach (SandParticle particle in particles)
        {
            // Debug.Log("Loop");
            GameObject particleGO = Instantiate(prefab, particle.position, Quaternion.identity);
            particleGO.transform.SetParent(transform); // Parent the particle to this GameObject
            
            // Set the size of the instantiated prefab
            particleGO.transform.localScale = new Vector3(scale, scale, scale);
            all_particles_go.Add(particleGO);
        }
    }


    void FixedUpdate()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            // Handle collisions
            // Debug.Log("Old Force: " + particles[i].total_force);
            particlesList.HandleCollisions(particles[i], vertices, gridSize);
            // Debug.Log("New Force: " + particles[i].total_force);

            // Store the current position of the particle
            // Vector3 currentPosition = particles[i].position;

            // Calculate acceleration using total force and mass
            // Vector3 acceleration = particles[i].total_force / particles[i].mass;

            // Update the position of the particle using Verlet integration
            // particles[i].position += acceleration * Time.fixedDeltaTime; //* Time.fixedDeltaTime;
            Debug.Log(particles[i].position);
            particles[i].position += particles[i].velocity * Time.fixedDeltaTime;

            // Debug.Log("Position: " + particles[i].position);

            //Update game object position
            all_particles_go[i].transform.position = particles[i].position;
        }

    }
}

public class SandParticle
{
    public Vector3 position;
    // public Vector3 previousPosition;
    // public Vector3 total_force;
    public Vector3 velocity;

    public Vector3 shape;
    public float radius;

    public float mass;

    //Constructor
    public SandParticle(Vector3 pos, Vector3 vel, float ms, float size)
    {
        position = pos;
        radius = size /2 ;
        // direction = dir;
        // speed = sp;
        velocity = vel;
        // previousPosition = pos;
        // total_force = force;
        // shape = sz;
        mass = ms;
    }
}


public class ParticlesList
{
    public List<SandParticle> particles;
    // public Mesh mesh;
    // public MeshCollider meshCollider; 

    // Constructor
    public ParticlesList(float size)
    {

        particles = new List<SandParticle>();


        int numParticlesPerSide = 5; // Number of particles per side
        float spacing = 2.0f; // Spacing between particles
        Vector3 center = new Vector3(0, 20, 0); // Center of the square (vertical)

        for (int i = 0; i < numParticlesPerSide; i++)
        {
            for (int j = 0; j < numParticlesPerSide; j++)
            {
                // Calculate the position of the particle
                float x = center.x + i * spacing;
                float y = center.y;
                float z = center.z + j * spacing;

                Vector3 position = new Vector3(x, y, z);

                // Set direction based on gravity
                float ms = 3;
                // Vector3 force = new Vector3(0, -9.8f * ms, 0);
                Vector3 vel = new Vector3(0, -9.8f, 0); //* Time.fixedDeltaTime, 0);

                // Add particle to the list
                particles.Add(new SandParticle(position, vel, ms, size));
            }
        }
    }
    // public ParticlesList(float size)
    // {
    //     particles = new List<SandParticle>();
    //     int numParticles = 20; // Number of particles to generate

    //     // Define the range of x and z coordinates
    //     float minX = -6;
    //     float maxX = 6;
    //     float minZ = -6;
    //     float maxZ = 6;

    //     // Generate random positions for each particle
    //     for (int i = 0; i < numParticles; i++)
    //     {
    //         // Generate random x and z coordinates within the specified range
    //         // UnityEngine.Random.InitState(42);
    //         float x = UnityEngine.Random.Range(minX, maxX);
    //         float z = UnityEngine.Random.Range(minZ, maxZ);
    //         // Debug.Log(x + " and " + z);

    //         // Set y coordinate to the desired height
    //         float y = 20f; // Assuming a vertical height of 20 units

    //         // Create the particle's position vector
    //         Vector3 position = new Vector3(x, y, z);

    //         // Set direction based on gravity
    //         float ms = 3;
    //         Vector3 vel = new Vector3(0, -9.8f, 0); // Assuming a constant downward velocity

    //         // Add particle to the list
    //         particles.Add(new SandParticle(position, vel, ms, size));
    //     }
    // }

    public void HandleCollisions(SandParticle particle, Vector3[] vertices, int gridSize)
    {
        
        // for (int j = 0; j < particles.Count; j++)
        //     {
        //         if (particle != particles[j])
        //         {
        //             // If there is a collision along all axes, the cubes are colliding
        //             if (CheckCollision(particle, particles[j]))
        //             {
        //                 Debug.Log("Collision detected!");

        //                 // Apply elastic collision forces
        //                 ApplyElasticCollision(particle, particles[j]);
        //             }
        //         }
        //     }

        //get the coordinates of the 4 surrounding vertices
        // float planeSize = 16f;
        // float x_index = (particle.position.x + planeSize / 2.0f) / (planeSize / gridSize);
        // float z_index = (particle.position.z + planeSize / 2.0f) / (planeSize / gridSize);

        // int x_down = (int) Math.Floor(x_index);
        // int z_down = (int) Math.Floor(z_index);
        // int x_up = (int) Math.Ceiling(x_index);
        // int z_up = (int) Math.Ceiling(z_index);
        // // Debug.Log(particle.position.z + " and " + z_index + " and " + z_down + " and " + z_up);
        // Debug.Log(z_down + " and " + z_up + " and " + x_down + " and " + x_up);
        // Debug.Log(particle.position.x + " and " + x_index + " and " + x_down + " and " + x_up);
        int x_down = (int) Math.Floor(particle.position.x);
        int z_down = (int) Math.Floor(particle.position.z);
        int x_up = (int) Math.Ceiling(particle.position.x);
        int z_up = (int) Math.Ceiling(particle.position.z);

        //get coordinates of surrounding pixels within the radius of the particle?

        float s = particle.position.x - x_down;
        float t = particle.position.z - z_down;

        //get heights of each surrounding vertex
        // float x_d = (float)x_down / gridSize * planeSize - planeSize / 2.0f;
        // float z_d = (float)z_down / gridSize * planeSize - planeSize / 2.0f;
        // float x_u = (float)x_up / gridSize * planeSize - planeSize / 2.0f;
        // float z_u = (float)z_up / gridSize * planeSize - planeSize / 2.0f;
        // float h00 = vertices[z_d * (gridSize + 1) + x_d].y; //mip.get_texel(x_down, y_down);
        // float h10 = vertices[z_d * (gridSize + 1) + x_u].y; //mip.get_texel(x_up, y_down);
        // float h01 = vertices[z_u * (gridSize + 1) + x_d].y; //mip.get_texel(x_down, y_up);
        // float h11 = vertices[z_u * (gridSize + 1) + x_u].y; //mip.get_texel(x_up, y_up);

        float h00 = vertices[z_down * (gridSize + 1) + x_down].y;
        float h10 = vertices[z_down * (gridSize + 1) + x_up].y; ;
        float h01 = vertices[z_up * (gridSize + 1) + x_down].y; ;
        float h11 = vertices[z_up * (gridSize + 1) + x_up].y;

        //do lerp 
        float h0 = h00 + s * (h10 + (-1 * h00));
        float h1 = h01 + s * (h11 + (-1 * h01));
        float terrain_height = h0 + t * (h1 + (-1 * h0));

        // //get the highest point beneath the particle
        // float terrain_height = Math.Max(Math.Max(h00, h10), Math.Max(h01, h11));

        // Check for collisions with the terrain
        Debug.Log(particle.position.y +" and " + terrain_height);
        if (particle.position.y <= terrain_height + particle.radius)
        {
            // Reverse the direction of the particle's velocity
            float damping_factor = 0.1f;
            particle.velocity.y = -damping_factor * particle.velocity.y; // Add a force in the opposite direction

            // // Calculate the normal vector of the collision surface
            // Vector3 surfaceNormal = Vector3.Cross(Vector3.right * (h10 - h00) + Vector3.forward * (h01 - h00),
            //     Vector3.right * (h01 - h00) + Vector3.forward * (h11 - h01)).normalized;

            // // Reflect the particle's velocity about the collision surface normal
            // particle.velocity = Vector3.Reflect(particle.velocity, surfaceNormal);
            
            

        // Ensure the particle comes to a full stop if its velocity magnitude becomes very small
        if (particle.velocity.magnitude < 0.2f)
        {
            // particle.velocity = Vector3.zero;
            // Calculate edge vectors v1 and v2
            Vector3 p1 = new Vector3(x_down, h00, z_down);
            Vector3 p2 = new Vector3(x_up, h10, z_down);
            Vector3 p3 = new Vector3(x_down, h01, z_up);
            Vector3 p4 = new Vector3(x_up, h11, z_up);

            Vector3 v1 = p2 - p1;
            Vector3 v2 = p4 - p1;

            // Calculate the cross product of v1 and v2 to get the surface normal
            Vector3 normal = Vector3.Cross(v1, v2).normalized;
            // particle.velocity = Vector3.Reflect(particle.velocity, normal);

            if (normal == Vector3.zero) {
                particle.velocity = Vector3.zero;
            } else {
                particle.velocity = Vector3.Reflect(particle.velocity, normal);
            }
        } else {
            // Update particle position to ensure it's above the terrain surface
            particle.position.y = terrain_height + particle.radius + 0.001f; // Add a small offset
        }
        
    }
    else
    {
        // Apply gravity only if the particle is in the air
        particle.velocity.y -= 9.8f * Time.fixedDeltaTime;
    }
    }


    public bool CheckCollision(SandParticle particle1, SandParticle particle2)
    {
        return Vector3.Distance(particle1.position, particle2.position) < (particle2.radius + particle1.radius);
    }

    // private void ApplyElasticCollision(SandParticle particle1, SandParticle particle2)
    // {
    //     Vector3 relativeVelocity = particle2.total_force - particle1.total_force;

    //     // Calculate impulse
    //     float impulse = Vector3.Dot(relativeVelocity, particle2.position - particle1.position) /
    //                     (particle1.mass + particle2.mass);

    //     // Calculate new velocities after collision
    //     Vector3 newVelocity1 = particle1.total_force + impulse * (particle2.position - particle1.position) / particle1.mass;
    //     Vector3 newVelocity2 = particle2.total_force - impulse * (particle2.position - particle1.position) / particle2.mass;

    //     // Update particle forces
    //     particle1.total_force = newVelocity1;
    //     particle2.total_force = newVelocity2;
    // }
    private void ApplyElasticCollision(SandParticle particle1, SandParticle particle2)
    {
        // Calculate relative velocity
        Vector3 relativeVelocity = particle2.velocity - particle1.velocity;

        // Calculate impulse
        float impulse = Vector3.Dot(relativeVelocity, particle2.position - particle1.position) /
                        (particle1.mass + particle2.mass);

        // Calculate new velocities after collision
        Vector3 newVelocity1 = particle1.velocity + impulse * (particle2.position - particle1.position) / particle1.mass;
        Vector3 newVelocity2 = particle2.velocity - impulse * (particle2.position - particle1.position) / particle2.mass;

        // Update particle velocities
        particle1.velocity = newVelocity1;
        particle2.velocity = newVelocity2;
    }
}