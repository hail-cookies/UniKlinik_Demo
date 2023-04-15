using System.Collections.Generic;
using UnityEngine;

public class TrackerPhysics : MonoBehaviour
{
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
        public Tracker tracker;
        public Vector3 position;
        public Vector3 offset;
        public Vector3 dir;

        public RBVertex(Rigidbody rb, SphereCollider sphereCollider, Tracker tracker, Vector3 offset, Vector3 dir)
        {
            this.rb = rb;
            this.offset = offset;
            this.dir = dir;
            this.sphereCollider = sphereCollider;
            this.tracker = tracker;
        }
    }

    public Transform core;
    public Rigidbody selfRb;
    public TrackingSettings trackingSettings;
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
    //[Button("Create Vertices"), ShowIf("@vertices.Count == 0 && !UnityEngine.Application.isPlaying && core != null")]
    public void CreateVertices()
    {
        float offsetValue = -colliderRadius;
        Vector3 center = transform.position;
        for (int i = 0; i < core.childCount; i++)
        {
            Transform child = core.GetChild(i);
            GameObject go = new GameObject(child.name);
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
            vertex.mass = 0.1f;

            Tracker tracker = trackingSettings.Apply(vertex);
            tracker.StartTracking(selfRb);

            vertices.Add(new RBVertex(vertex, collider, tracker, offset, dir));
        }

        //ConnectSprings();
    }

    private void Awake()
    {
        CreateVertices();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (!core)
            return;

        for (int i = 0; i < core.childCount; i++)
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
