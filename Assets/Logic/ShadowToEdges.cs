using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [ExecuteAlways]
public class ShadowToEdges : MonoBehaviour
{
    [SerializeField]
    GameObject shadowCatchers;

    [SerializeField]
    Transform planeTransform;

    [SerializeField]
    Transform lightTransform;

    MeshFilter[] meshes;

    [SerializeField]
    MeshFilter shadowFilter;

    [SerializeField]
    GameObject colliderParent;

    List <EdgeCollider2D> edgeColliders = new List<EdgeCollider2D>();

    List<EdgeCollider2D> colliders = new List<EdgeCollider2D>();

    Line[] lines;
    
    private void Awake()
    {
        meshes = shadowCatchers.GetComponentsInChildren<MeshFilter>();

        var lineCount = meshes[0].sharedMesh.GetIndices(0).Length;

        for (int i = 0; i < lineCount; i++)
        {
            var collider = colliderParent.AddComponent<EdgeCollider2D>();
            collider.points = new Vector2[2];
            colliders.Add(collider);
        }
    }

    List<Vector3> drawnVertices = new List<Vector3>(20);
    List<int> drawnIndices = new List<int>(20);

    List<Vector3> colliderPoints = new List<Vector3>(10);

    struct Line : IEquatable<Line> {
        public Vector3 start;
        public Vector3 end;

        public Vector3 triangleNormal;
        public bool use;
        public Line(Vector3 start, Vector3 end, Vector3 triangleNormal) {
            this.start = start;
            this.end = end;

            this.triangleNormal = triangleNormal;
            use = true;
        }

        public bool Equals(Line other) {
            if(System.Object.ReferenceEquals(other, this)) return true;
            
            return start == other.start && end == other.end || 
            end == other.start && start == other.end;
        }

        public override string ToString()
        {
            return $"{start} -> {end}";
        }
    }
    
    void CalculateLines(MeshFilter mesh, int index){
        
        var indices = mesh.sharedMesh.triangles;
        var vertices = mesh.sharedMesh.vertices;
        var normals = mesh.sharedMesh.normals;

        
        var allLines = new List<Line>(indices.Length);

        for (int i = 0; i < indices.Length; i += 3)
        {
            var triangle = (indices[i], indices[i + 1], indices[i + 2]);
            
            var normal = normals[triangle.Item1] + normals[triangle.Item2] + normals[triangle.Item3];
            normal /= 3.0f;
            normal.Normalize();
            
            var lines = new Line[] {
                new Line(vertices[triangle.Item1], vertices[triangle.Item2], normal),
                new Line(vertices[triangle.Item1], vertices[triangle.Item3], normal),
                new Line(vertices[triangle.Item2], vertices[triangle.Item3], normal)
            };

            allLines.AddRange(lines);
        }

        lines = allLines.ToArray();
        
        for (int i = 0; i < allLines.Count; i++)
        {
            var lineCenter = (allLines[i].start + allLines[i].end) / 2.0f;
            
            var match = allLines.Skip(i + 1).ToList().IndexOf(allLines[i]);
            
            if(match != -1) {
                lines[i].use = false;
                if(allLines[match + i + 1].triangleNormal == allLines[i].triangleNormal) {
                    lines[match + i + 1].use = false;
                }
            }
        }

        lines = lines.Where(l => l.use).ToArray();
    }

    void CalculateProjection(Transform transform) {
        foreach (var line in lines)
        {
            var v1 = new Vector4(line.start.x, line.start.y, line.start.z, 1.0f);
            v1 = transform.localToWorldMatrix * v1;
            v1 = CutWithPlane(planeTransform, v1, lightTransform);
            var v2 = new Vector4(line.end.x, line.end.y, line.end.z, 1.0f);
            v2 = transform.localToWorldMatrix * v2;
            v2 = CutWithPlane(planeTransform, v2, lightTransform);

            Debug.DrawLine(v1, v2, Color.red);
        }
        
        // for(int i = 0; i < old.Length; i++) {
        //     var vertex = new Vector4(old[i].x, old[i].y, old[i].z, 1.0f);
        //     vertex = mesh.transform.localToWorldMatrix * vertex;

        //     vertices[i] = CutWithPlane(planeTransform, vertex, lightTransform.position);
        // }
    }

    Vector3 CutWithPlane(Transform planeTransform, Vector4 point, Transform light) {
        var lightPos = light.position;
        var transformedLight = planeTransform.localToWorldMatrix * new Vector4(lightPos.x, lightPos.y, lightPos.z, 1.0f);

        Debug.DrawRay(transformedLight, Vector3.up, Color.green);
        
        point = planeTransform.localToWorldMatrix * point;

        Debug.DrawRay(point, Vector3.up, Color.red);

        point = CutOn0Plane(point, transformedLight);

        point = planeTransform.worldToLocalMatrix * point;

        return point;
    }

    Vector4 CutOn0Plane(Vector4 point, Vector4 lightPosition) {
        var dir = (point - lightPosition).normalized;

        var x = point.x - point.y / (dir.y / dir.x);
        var z = point.z - point.y / (dir.y / dir.z);
        
        return new Vector4(x, 0.0f, z, 1.0f);
    }

    private void Start()
    {
        CalculateLines(meshes[0], 0);
    }
    private void Update()
    {
        CalculateProjection(meshes[0].transform);
    }
}
