using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionTests : MonoBehaviour
{
    public MeshFilter casterFilter;
    public MeshFilter recieverFilter;
    public Transform lightTransform;
    
    [System.Serializable]
    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }
    }

    public class Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public Ray ray1;
    public Ray ray2;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(lightTransform.position, 0.1f);

        VerticesAction(casterFilter, (pos) =>
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pos, 0.1f);
            var dir = pos - lightTransform.position;
            Gizmos.DrawLine(lightTransform.position, dir * 1000f);
            // Gizmos.DrawRay(lightTransform.position, pos);

            var O = pos;
            var D = dir;

            TrianglesAction(casterFilter, (tri) =>
            {
                var V0 = tri.a;
                var V1 = tri.b;
                var V2 = tri.c;

                var E1 = V1 - V0;
                var E2 = V2 - V0;
                var T = O - V0;

                var P = Vector3.Cross(D, E2);
                var Q = Vector3.Cross(T, E1);

                var det = Vector3.Dot(P, E1);
                var invDet = 1f / det;
                

                var t = Vector3.Dot(Q, E2) * invDet;
                var u = Vector3.Dot(P, T) * invDet;
                var v = Vector3.Dot(Q, D) * invDet;

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(O + D * t, 0.1f);
            });
        });

        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(ray1.origin, ray1.origin + ray1.direction);
        // Gizmos.color = Color.blue;
        // Gizmos.DrawLine(ray2.origin, ray2.origin + ray2.direction);

        // Gizmos.color = Color.green;
        // Gizmos.DrawSphere(GetIntersectionPoint(ray1, ray2), .1f);
    }

    void VerticesAction(MeshFilter meshFilter, System.Action<Vector3> action)
    {
        foreach (var vertex in meshFilter.mesh.vertices)
        {
            action(meshFilter.transform.TransformPoint(vertex));
        }
    }

    void TrianglesAction(MeshFilter meshFilter, System.Action<Triangle> action)
    {
        var vertices = meshFilter.mesh.vertices;
        var triangles = meshFilter.mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var a = meshFilter.transform.TransformPoint(vertices[triangles[i]]);
            var b = meshFilter.transform.TransformPoint(vertices[triangles[i + 1]]);
            var c = meshFilter.transform.TransformPoint(vertices[triangles[i + 2]]);
            action(new Triangle(a, b, c));
        }
    }

    private Vector3 GetIntersectionPoint(Ray ray1, Ray ray2)
    {
        var Ot = ray2.origin - ray1.origin;
        var D1 = ray1.direction;
        var D2 = ray2.direction;

        var det = D1.x * D2.y - D1.y * D2.x;
        var t1 = (Ot.x * D2.y - D1.y * Ot.y) / det;
        var t2 = (D1.x * Ot.y - Ot.y * D2.x) / det;

        var P1 = ray1.origin + D1 * t1;

        var intersects = t1 >= 0 && t2 >= 0 && t2 <= 1;

        Debug.Log(P1);
        Debug.Log("det is " + det);
        return P1;
    }

}

public static class Extensions{
    public static Vector4 Homo(this Vector3 v){
        return new Vector4(v.x, v.y, v.z, 1);
    }
}