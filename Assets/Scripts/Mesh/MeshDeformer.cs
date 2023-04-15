using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    public float tension = 20f;
    public float damping = 5f;

    Mesh mesh;
    Vector3[] originalVertices, displacedVertices, velocities;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;

        velocities = new Vector3[originalVertices.Length];
        displacedVertices = new Vector3[originalVertices.Length];
        for(int i = 0; i < originalVertices.Length; i++)
            displacedVertices[i] = originalVertices[i];
    }

    public void ApplyForce(Vector3 origin, float force, float dt)
    {
        for (int i = 0; i < displacedVertices.Length; i++)
            ApplyForceToVertex(i, origin, force, dt);
    }

    void ApplyForceToVertex(int vertex, Vector3 origin, float force, float dt)
    {
        origin = transform.InverseTransformPoint(origin);
        Vector3 delta = displacedVertices[vertex] - origin;
        float attenuatedForce = force / (1f + delta.sqrMagnitude);
        float velocity = attenuatedForce * dt;
        velocities[vertex] += delta.normalized * velocity;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for(int i = 0; i < displacedVertices.Length; i++)
            UpdateVertex(i, dt);

        mesh.vertices = displacedVertices;
        mesh.RecalculateNormals();
    }

    void UpdateVertex(int vertex, float dt)
    {
        Vector3 v = velocities[vertex];
        Vector3 displacement = displacedVertices[vertex] - originalVertices[vertex];
        v -= displacement * tension * dt;
        v *= 1f - damping * dt;
        velocities[vertex] = v;
        displacedVertices[vertex] += v * dt;
    }
}
