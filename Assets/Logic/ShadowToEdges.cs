using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [ExecuteAlways]
public class ShadowToEdges : MonoBehaviour
{
    [SerializeField]
    GameObject edgeColliderPrefab;
    
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

    Rigidbody2D[] collidersRbs;

    Line[] lines;
    
    private void Awake()
    {
        meshes = shadowCatchers.GetComponentsInChildren<MeshFilter>();
    }

    private void Start()
    {
        CalculateLines(meshes[0]);
        collidersRbs = AddColliders().ToArray();
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

    
    void CalculateLines(MeshFilter mesh){
        
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
        // void CalculateVertex()
        
        for (int i = 0; i < lines.Length; i++)
        {
            ref var line = ref lines[i];

            var start = Project(transform, line.start);
            var end = Project(transform, line.end);

            var rigidbody = collidersRbs[i];

            rigidbody.position = start;

            var direction = end - start;
            var magnitude = direction.magnitude;
            rigidbody.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rigidbody.transform.localScale = Vector3.one * magnitude;

            // Debug.DrawLine(start, end);

        }

        Vector3 Project(Transform transform, Vector3 point)
        {
            var p = new Vector4(point.x, point.y, point.z, 1.0f);
            p = transform.localToWorldMatrix * p;
            return CutWithPlane(planeTransform, p, lightTransform);
        }
    }

    Vector3 CutWithPlane(Transform planeTransform, Vector4 point, Transform light) {
        var lightPos = light.position;
        var transformedLight = planeTransform.worldToLocalMatrix * new Vector4(lightPos.x, lightPos.y, lightPos.z, 1.0f);
        
        point = planeTransform.worldToLocalMatrix * new Vector4(point.x, point.y, point.z, 1.0f);

        point = CutOn0Plane(point, transformedLight);

        point = planeTransform.localToWorldMatrix * new Vector4(point.x, point.y, point.z, 1.0f);

        return point;
    }

    Vector4 CutOn0Plane(Vector4 point, Vector4 lightPosition) {
        var dir = (point - lightPosition).normalized;

        var x = point.x - point.y / (dir.y / dir.x);
        var z = point.z - point.y / (dir.y / dir.z);
        
        return new Vector4(x, 0.0f, z, 1.0f);
    }


    IEnumerable<Rigidbody2D> AddColliders() {
        foreach (var line in lines)
        {
            yield return Instantiate(edgeColliderPrefab, colliderParent.transform).GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        CalculateProjection(meshes[0].transform);
    }

    
}
