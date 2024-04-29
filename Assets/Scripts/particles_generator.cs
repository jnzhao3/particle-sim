using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ParticleDisplay : MonoBehaviour
{
    public GameObject prefab;
    public ParticlesList particlesList;
    public List<SandParticle> particles;
    public List<GameObject> all_particles_go;

    Vector3[] vertices;
    int gridSize;
    public int numParticles;
    public float scale; 
    public Mesh mesh;
    int fixedUpdateCounter = 0;
    void Start()
    {
        scale = 0.7f;
        // Initialize particles list from ParticlesList constructor
        particlesList = new ParticlesList(scale);
        particles = particlesList.particles;
 
        all_particles_go = new List<GameObject>();

        vertices = plane_generator.vertices;
        gridSize = plane_generator.gridSize;
        mesh = plane_generator.mesh;

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
        numParticles = particles.Count;
    }


    void FixedUpdate()
    {
        // Increment the counter
        fixedUpdateCounter++;

        // // Check if it's the 5th call
        // if (fixedUpdateCounter % 25 == 0)
        // {
        //     int prevNumParticles = particles.Count;
        //     particlesList.AddParticles(5f, scale);
        //     // Iterate through new particle objects
        //     foreach (SandParticle particle in particles.Skip(prevNumParticles))
        //     {
        //         // Debug.Log("Loop");
        //         GameObject particleGO = Instantiate(prefab, particle.position, Quaternion.identity);
        //         particleGO.transform.SetParent(transform); // Parent the particle to this GameObject
                
        //         // Set the size of the instantiated prefab
        //         particleGO.transform.localScale = new Vector3(scale, scale, scale);
        //         all_particles_go.Add(particleGO);
        //     }
        //     numParticles = particles.Count;
        // }




        for (int i = 0; i < particles.Count; i++)
        {
            // Handle collisions
            particlesList.HandleCollisions(particles[i], vertices, gridSize);

            // Debug.Log(particles[i].position);
            // particles[i].velocity += particles[i].acceleration;
            // particles[i].position += particles[i].velocity;
            particles[i].position += particles[i].velocity * Time.fixedDeltaTime;
            Debug.Log(particles[i].position);

            //Update game object position
            all_particles_go[i].transform.position = particles[i].position;
        }

    }
}

public class SandParticle
{
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 shape;
    public float radius;
    public float mass;

    //Constructor
    public SandParticle(Vector3 pos, Vector3 vel, Vector3 accel, float ms, float size)
    {
        position = pos;
        radius = size /2 ;
        velocity = vel;
        acceleration = accel;
        mass = ms;
    }
}


public class ParticlesList
{
    public List<SandParticle> particles;
    public Vector3 center;
    public float spacing;
    // Constructor
    public ParticlesList(float size)
    {

        particles = new List<SandParticle>();


        int numParticlesPerSide = 5; // Number of particles per side
        spacing = 2.0f; // Spacing between particles
        center = new Vector3(0, 20, 0); // Center of the square (vertical)

        for (int i = 0; i < numParticlesPerSide; i++)
        {
            for (int j = 0; j < numParticlesPerSide; j++)
            {
                // Calculate the position of the particle
                float x = center.x + i * spacing;
                // float y = center.y;
                float y = UnityEngine.Random.Range(2, 5); //(10, , 40);
                float z = center.z + j * spacing;

                Vector3 position = new Vector3(x, y, z);

                // Set direction based on gravity
                float ms = 3;
                // Vector3 force = new Vector3(0, -9.8f * ms, 0);
                // Vector3 vel = new Vector3(0, -5f, 0); //* Time.fixedDeltaTime, 0);
                Vector3 vel = new Vector3(0, -9.8f * Time.fixedDeltaTime, 0);
                Vector3 accel = new Vector3(0, -0.08f, 0);

                // Add particle to the list
                particles.Add(new SandParticle(position, vel, accel, ms, size));
            }
        }
    }

    public void AddParticles(float num, float size)
    {
        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < num; j++)
            {
                // Calculate the position of the particle
                float x = center.x + i * spacing;
                float y = UnityEngine.Random.Range(10, 40);
                float z = center.z + j * spacing;

                Vector3 position = new Vector3(x, y, z);

                // Set direction based on gravity
                float ms = 3;
                // Vector3 vel = new Vector3(0, -9.8f, 0); //* Time.fixedDeltaTime, 0);
                Vector3 vel = new Vector3(0, -9.8f * Time.fixedDeltaTime, 0);
                Vector3 accel = new Vector3(0, -0.08f, 0);

                // Add particle to the list
                particles.Add(new SandParticle(position, vel, accel, ms, size));
            }
        }
    }

    public void HandleCollisions(SandParticle particle, Vector3[] vertices, int gridSize)
    {
        
        for (int j = 0; j < particles.Count; j++)
            {
                if (particle != particles[j])
                {
                    // If there is a collision along all axes, the cubes are colliding
                    if (CheckCollision(particle, particles[j]))
                    {
                        Debug.Log("Collision detected!");
                        // particle.velocity.x += .1f;

                        // Apply elastic collision forces
                        // ApplyElasticCollision(particle, particles[j]);
                    }
                }
            }

        //get the coordinates of the 4 surrounding vertices
        int x_down = (int) Math.Floor(particle.position.x);
        int z_down = (int) Math.Floor(particle.position.z);
        int x_up = (int) Math.Ceiling(particle.position.x);
        int z_up = (int) Math.Ceiling(particle.position.z);

        //get coordinates of surrounding pixels within the radius of the particle?

        float s = particle.position.x - x_down;
        float t = particle.position.z - z_down;

        float h00 = vertices[z_down * (gridSize + 1) + x_down].y;
        float h10 = vertices[z_down * (gridSize + 1) + x_up].y; ;
        float h01 = vertices[z_up * (gridSize + 1) + x_down].y; ;
        float h11 = vertices[z_up * (gridSize + 1) + x_up].y;

        //do lerp 
        float h0 = h00 + s * (h10 + (-1 * h00));
        float h1 = h01 + s * (h11 + (-1 * h01));
        float terrain_height = h0 + t * (h1 + (-1 * h0));

        // //get the highest point beneath the particle
        // float high_point = Math.Max(Math.Max(h00, h10), Math.Max(h01, h11));
        
        Vector3 p1 = new Vector3(x_down, h00, z_down);
        Vector3 p2 = new Vector3(x_up, h10, z_down);
        Vector3 p3 = new Vector3(x_down, h01, z_up);
        Vector3 p4 = new Vector3(x_up, h11, z_up);


        // Calculate normal vector of the plane
        Vector3 normalVector = CalculateNormalVector(p1, p2, p3);

        // Calculate the equation of the tangent plane
        (Vector3 tangentPlaneNormal, float D) = CalculateTangentPlane(normalVector, p1);

        // Project particle's position onto the tangent plane
        Vector3 projectedPosition = ProjectPointOntoPlane(particle.position, tangentPlaneNormal, D);

        // Modify particle's velocity to make it roll along the tangent plane
        Vector3 rolledVelocity = RollParticleAlongTangentPlane(particle.velocity, tangentPlaneNormal);
        Debug.Log(rolledVelocity + " and " + particle.velocity);
        // particle.velocity = RollParticleAlongTangentPlane(particle.velocity, tangentPlaneNormal);



        // Check for collisions with the terrain
        // Debug.Log(particle.position.y +" and " + terrain_height);
        if (particle.position.y <= terrain_height + particle.radius)
        {
            // Reverse the direction of the particle's velocity
            float damping_factor = 0.1f;
            particle.velocity.y = -damping_factor * particle.velocity.y; // Add a force in the opposite direction

            // Ensure the particle comes to a full stop if its velocity magnitude becomes very small
            if (particle.velocity.magnitude < 0.2f)
            {
                Debug.Log("IN stop");
                particle.position.y = terrain_height + particle.radius + 0.001f; // Add a small offset
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
    Vector3 CalculateNormalVector(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Calculate vectors between points
        Vector3 v1 = p2 - p1;
        Vector3 v2 = p3 - p1;
        // Calculate cross product to get normal vector
        Vector3 normalVector = Vector3.Cross(v1, v2);
        return normalVector.normalized;
    }

    (Vector3, float) CalculateTangentPlane(Vector3 normalVector, Vector3 point)
    {
        // Calculate D in the plane equation Ax + By + Cz + D = 0
        float D = -Vector3.Dot(normalVector, point);
        return (normalVector, D);
    }

    Vector3 ProjectPointOntoPlane(Vector3 point, Vector3 normalVector, float D)
    {
        // Calculate distance from point to plane
        float distance = Vector3.Dot(normalVector, point) + D;
        Debug.Log("Distance: "+ distance);
        // Project point onto plane by moving along the normal vector
        Vector3 projectedPoint = point - distance * normalVector;
        return projectedPoint;
    }

    Vector3 RollParticleAlongTangentPlane(Vector3 particleVelocity, Vector3 normalVector)
    {
        // Calculate the component of velocity perpendicular to the tangent plane
        Vector3 perpendicularComponent = Vector3.Dot(particleVelocity, normalVector) * normalVector;
        // Zero out the perpendicular component to make the particle roll
        Vector3 rolledVelocity = particleVelocity - perpendicularComponent;
        return rolledVelocity;
    }

    public bool CheckCollision(SandParticle particle1, SandParticle particle2)
    {
        return Vector3.Distance(particle1.position, particle2.position) < (particle2.radius + particle1.radius);
    }
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