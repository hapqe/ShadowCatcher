using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
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

    List<EdgeCollider2D> colliders = new List<EdgeCollider2D>();
    
    void FixedUpdate()
    {
        
    }

    List<Vector3> drawnVertices = new List<Vector3>(20);
    List<int> drawnIndices = new List<int>(20);

    List<Vector3> colliderPoints = new List<Vector3>(10);


    void CalculateProjectionPoints(MeshFilter mesh, int index){
        Vector3[] vertices = mesh.sharedMesh.vertices;

        // Apply transform to vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = mesh.transform.TransformPoint(vertices[i]);
        }

        // Vertices and light to "Planespace"
        Vector3 lightPosition = lightTransform.position;
        
        List<int> indices = new List<int>(mesh.sharedMesh.triangles.Length / 2);

        // Backface culling
        for (int i = 0; i < mesh.sharedMesh.triangles.Length; i += 3)
        {
            
            Vector3 p1 = vertices[mesh.sharedMesh.triangles[i + 0]];
            Vector3 p2 = vertices[mesh.sharedMesh.triangles[i + 1]];
            Vector3 p3 = vertices[mesh.sharedMesh.triangles[i + 2]];

            var average = (p1 + p2 + p3) / 3;

            var lightView = 
            Matrix4x4.Scale(new Vector3(-1, 1, 1)) * 
            Matrix4x4.Perspective(90.0f, 1.0f, 0.1f, 100.0f) *
            Matrix4x4.Rotate(Quaternion.LookRotation(lightPosition - average, Vector3.up)) *
            Matrix4x4.Translate(lightPosition);

            var p1Light = lightView * new Vector4(p1.x, p1.y, -p1.z, 1);
            var p2Light = lightView * new Vector4(p2.x, p2.y, -p2.z, 1);
            var p3Light = lightView * new Vector4(p3.x, p3.y, -p3.z, 1);
            
            var clockwise = Vector3.Cross(p2Light - p1Light, p3Light - p1Light).z > 0;

            if(clockwise) {
                indices.Add(mesh.sharedMesh.triangles[i + 0]);
                indices.Add(mesh.sharedMesh.triangles[i + 1]);
                indices.Add(mesh.sharedMesh.triangles[i + 2]);
            }
        }

        lightPosition = planeTransform.worldToLocalMatrix * new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 1.0f);

        for(int i = 0; i < indices.Count; i += 3) {
            Vector3 p1 = vertices[indices[i + 0]];
            Vector3 p2 = vertices[indices[i + 1]];
            Vector3 p3 = vertices[indices[i + 2]];
            p1 = CutWithPlane(planeTransform, p1, lightPosition);
            p2 = CutWithPlane(planeTransform, p2, lightPosition);
            p3 = CutWithPlane(planeTransform, p3, lightPosition);
            vertices[indices[i + 0]] = p1;
            vertices[indices[i + 1]] = p2;
            vertices[indices[i + 2]] = p3;
        }

        if(shadowFilter.sharedMesh == null) {
            shadowFilter.sharedMesh = new Mesh();
        }

        var newIndices = indices.Select(i => i += drawnVertices.Count).ToList();


        drawnVertices.AddRange(vertices);
        drawnIndices.AddRange(newIndices);

        List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>(vertices.Length / 2);

        for (int i = 0; i < indices.Count; i += 3)
        {
            Vector3 p1 = vertices[indices[i + 0]];
            Vector3 p2 = vertices[indices[i + 1]];
            Vector3 p3 = vertices[indices[i + 2]];

            if(CalculateDoubleConnectedTriangles(p1, p2) < 2) {
                edges.Add((p1, p2));
            }
            if(CalculateDoubleConnectedTriangles(p1, p3) < 2) {
                edges.Add((p1, p3));
            }
            if(CalculateDoubleConnectedTriangles(p2, p3) < 2) {
                edges.Add((p2, p3));
            }
        }

        List<Vector3> colliderPoints = new List<Vector3>(edges.Count);
        if(edges.Count == 0) return;
        
        colliderPoints.Add(edges[0].Item1);
        colliderPoints.Add(edges[0].Item2);

        var corrupt = (Vector3.negativeInfinity, Vector3.negativeInfinity);

        edges[0] = corrupt;
        
        for(int i = 2; i < edges.Count; i++) {
            var lastPoint = colliderPoints[colliderPoints.Count - 1];
            
            var edge = edges.Where(e => e.Item1 == lastPoint || e.Item2 == lastPoint).FirstOrDefault();

            if(edge.Item1 == lastPoint) {
                colliderPoints.Add(edge.Item2);
            } else {
                colliderPoints.Add(edge.Item1);
            }

            var corruptIndex = edges.IndexOf(edge);
            if(corruptIndex != -1) {
                edges[corruptIndex] = corrupt;
            }
        }
        colliderPoints.Add(colliderPoints[0]);

        if(!UnityEditor.EditorApplication.isPaused)
        colliders[index].points = colliderPoints.Select(p => (Vector2)p).ToArray();
        
        int CalculateDoubleConnectedTriangles(Vector3 p1, Vector3 p2) {
            var count = 0;
            
            for(int i = 0; i < indices.Count; i+=3) {
                var currentTri = new Vector3[] {
                    vertices[indices[i]],
                    vertices[indices[i + 1]],
                    vertices[indices[i + 2]],
                };

                if(currentTri.Any(v => v == p1) && currentTri.Any(v => v == p2)) {
                    count++;
                }
            }
            return count;
        }
    }

    Vector3 CutWithPlane(Transform planeTransform, Vector3 point, Vector3 lightPosition) {
        point = planeTransform.worldToLocalMatrix * new Vector4(point.x, point.y, point.z, 1.0f);

        point = CutOn0Plane(point, lightPosition);

        point = planeTransform.localToWorldMatrix * new Vector4(point.x, point.y, point.z, 1.0f);

        return point;
    }

    Vector3 CutOn0Plane(Vector3 point, Vector3 lightPosition) {
        var dir = (point - lightPosition).normalized;

        var x = point.x - point.y / (dir.y / dir.x);
        var z = point.z - point.y / (dir.y / dir.z);
        
        return new Vector3(x, 0.0f, z);
    }

    private void Update()
    {
        meshes = shadowCatchers.GetComponentsInChildren<MeshFilter>();
        
        while(colliderParent.GetComponentsInChildren<EdgeCollider2D>().Length < meshes.Length) {
            colliderParent.AddComponent<EdgeCollider2D>();
        }
        colliders = colliderParent.GetComponentsInChildren<EdgeCollider2D>().ToList();
        
        drawnVertices.Clear();
        drawnIndices.Clear();
        int index = 0;
        foreach (var mesh in meshes)
        {
            CalculateProjectionPoints(mesh, index++);
        }
        try {
            shadowFilter.sharedMesh.SetVertices(drawnVertices);
            shadowFilter.sharedMesh.SetIndices(drawnIndices, MeshTopology.Triangles, 0);
        } catch {}
    }
}
