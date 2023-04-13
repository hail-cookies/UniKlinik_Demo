using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class RBEdge
{
    public SpringJoint joint;
    public Rigidbody rb;

    public RBEdge(Rigidbody rb, SpringJoint joint)
    {
        this.rb = rb;
        this.joint = joint;
    }
}

[System.Serializable]
public class RBVertex
{
    public Rigidbody rb;
    public SphereCollider sphereCollider;
    public Vector3 offset;
    public Vector3 dir;

    public RBVertex(Rigidbody a, SphereCollider sphereCollider, Vector3 offset, Vector3 dir)
    {
        this.rb = a;
        this.offset = offset;
        this.dir = dir;
        this.sphereCollider = sphereCollider;
    }
}

public class BouncyBall : MonoBehaviour
{
    public Transform core;
    public Rigidbody centerRb;
    public List<RBVertex> vertices = new List<RBVertex>();
    public List<RBEdge> edges;

    [Min(0.001f)]
    public float colliderRadius = 0.2f;

    [SerializeField, HideInInspector]
    Transform _vertexParent;
    public Transform VertexParent
    {
        get
        {
            if (!_vertexParent)
                _vertexParent = new GameObject(name + " VertexParent").transform;

            return _vertexParent;
        }
    }

    float c_drag = 0.9f;
    [Button("Create Vertices"), ShowIf("@vertices.Count == 0 && !UnityEngine.Application.isPlaying && core != null")]
    public void CreateVertices()
    {
        if (Application.isPlaying) return;

        ClearVertices();

        float offsetValue = -colliderRadius;
        Vector3 center = transform.position;
        for(int i = 0; i < core.childCount; i++)
        {
            Transform child = core.GetChild(i);
            GameObject go = new GameObject("Vertex");
            go.transform.SetParent(VertexParent, false);
            go.layer = LayerMask.NameToLayer("SoftBodies");

            Vector3 dir = (child.position - center).normalized;
            Vector3 offset = dir * offsetValue;
            go.transform.position = child.position + offset;
            
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.radius = colliderRadius;

            Rigidbody vertex = go.AddComponent<Rigidbody>();
            vertex.drag = c_drag;
            vertex.angularDrag = c_drag;
            vertex.useGravity = false;
            vertices.Add(new RBVertex(vertex, collider, offset, dir));
        }

        ConnectSprings();
    }

    void ConnectSprings()
    {
        for(int i = 0; i < vertices.Count; i++)
        {
            var vertex = vertices[i];
            edges.Add(new RBEdge(vertex.rb, CreateSpring(vertex.rb, centerRb)));

            if(IsPolar(vertex.dir))
            {
                for(int j = 0; j < vertices.Count; j++)
                {
                    var other = vertices[j];
                    if(Vector3.Dot(vertex.dir, other.dir) > 0)
                        edges.Add(new RBEdge(vertex.rb, CreateSpring(vertex.rb, other.rb)));
                }
            }
        }
    }

    SpringJoint CreateSpring(Rigidbody a, Rigidbody b)
    {
        var joint = a.gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = b;

        joint.anchor = Vector3.zero;
        joint.autoConfigureConnectedAnchor = true;
        joint.enablePreprocessing = false;

        return ConfigureSpring(joint);
    }

    [Min(0.1f)]
    public float spring = 50;
    [Min(0.001f)]
    public float damper = 0.8f;
    public bool enableCollision = false;
    SpringJoint ConfigureSpring(SpringJoint joint)
    {
        joint.spring = spring;
        joint.damper = damper;
        joint.enableCollision = enableCollision;

        return joint;
    }

    [Button("Clear Vertices"), ShowIf("@vertices.Count > 0 && !UnityEngine.Application.isPlaying && core")]
    public void ClearVertices()
    {
        if(Application.isPlaying) return;

        foreach (var v in vertices)
            if (v.rb != null)
                DestroyImmediate(v.rb.gameObject);
        if (_vertexParent != null)
        {
            DestroyImmediate(_vertexParent.gameObject);
            while (_vertexParent.childCount > 0)
                DestroyImmediate(_vertexParent.GetChild(0).gameObject);
        }

        vertices.Clear();
        edges.Clear();
    }

    bool IsPolar(Vector3 dir)
    {
        return
            Mathf.Abs(Vector3.Dot(dir, Vector3.right)) > 0.95f ||
            Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.95f ||
            Mathf.Abs(Vector3.Dot(dir, Vector3.forward)) > 0.95f;
    }

    private void OnDrawGizmos()
    {
        foreach(var edge in edges)
        {
            if(edge.rb != null &&
                edge.joint.connectedBody != null)
            {
                Gizmos.DrawLine(edge.rb.position, edge.joint.connectedBody.position);
            }
        }
    }

    private void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (!core)
            return;

        foreach(var edge in edges)
            edge.joint = ConfigureSpring(edge.joint);

        for(int i = 0; i < core.childCount; i++)
        {
            if (i < vertices.Count && vertices[i] != null)
            {
                var vertex = vertices[i];
                vertex.sphereCollider.radius = colliderRadius;
                core.GetChild(i).position = vertex.rb.position - vertex.offset;
            }
        }
    }
}
