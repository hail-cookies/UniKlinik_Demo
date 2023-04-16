using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MassSpring : MonoBehaviour
{
    Mesh _mesh;
    public Mesh Mesh
    {
        get
        {
            if(_mesh == null)
                _mesh = GetComponent<MeshFilter>().sharedMesh;

            return _mesh;
        }
    }

    Transform _parent;
    public Transform Parent
    {
        get
        {
            if (_parent == null)
                _parent = new GameObject(name + "_VertexParent").transform;

            return _parent;
        }
    }

    private void Awake()
    {
        foreach(var vertex in _mesh.vertices)
        {

        }
    }
}
