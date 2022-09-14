using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShadowCatcher : MonoBehaviour
{
    public MeshFilter caster;
    public MeshFilter reciever;
    public Transform light;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    struct Triangle4
    {
        public Vector4 a;
        public Vector4 b;
        public Vector4 c;
        public Triangle4(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a.Homo();
            this.b = b.Homo();
            this.c = c.Homo();
        }
        public void Transform(Matrix4x4 transformation)
        {
            a = transformation * a;
            b = transformation * b;
            c = transformation * c;
        }
        public static implicit operator Triangle3(Triangle4 t)
        {
            return new Triangle3(t.a, t.b, t.c);
        }
    }

    struct Triangle3
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Triangle3(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        public static explicit operator Triangle4(Triangle3 t)
        {
            return new Triangle4(t.a, t.b, t.c);
        }
        public Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return a;
                    case 1: return b;
                    case 2: return c;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: a = (Vector3)value; break;
                    case 1: b = (Vector3)value; break;
                    case 2: c = (Vector3)value; break;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        DrawMesh(Color.green, reciever);
        DrawMesh(Color.red, caster);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(light.position, 0.2f);

        TriAction(caster, (tri_4) =>
        {
            tri_4.Transform(caster.transform.localToWorldMatrix);
            var tri = (Triangle3)tri_4;

            Gizmos.color = Color.yellow;

            var lightPos = light.transform.position;

            var O = lightPos;

            TriAction(reciever, (tri2_4) =>
            {
                tri2_4.Transform(reciever.transform.localToWorldMatrix);
                var tri2 = (Triangle3)tri2_4;

                var proj = new Triangle3();
                var onTri = new bool[3];
                var bary = new Vector3[3];
                
                for (int i = 0; i < 3; i++)
                {
                    var D = tri[i] - lightPos;

                    var V0 = tri2.a;
                    var V1 = tri2.b;
                    var V2 = tri2.c;

                    var E1 = V1 - V0;
                    var E2 = V2 - V0;
                    var T = O - V0;

                    var P = Vector3.Cross(D, E2);
                    var Q = Vector3.Cross(O - V0, E1);

                    var det = Vector3.Dot(P, E1);
                    var invDet = 1 / det;

                    var t = Vector3.Dot(Q, E2) * invDet;
                    var u = Vector3.Dot(P, T) * invDet;
                    var v = Vector3.Dot(Q, D) * invDet;

                    proj[i] = t * D + O;
                    onTri[i] = u >= 0 && v >= 0 && u + v <= 1;
                    bary[i] = new Vector3(1 - u - v, u, v);
                    
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(proj[i], 0.1f);
                }

                Gizmos.color = Color.black;
                Gizmos.DrawLine(proj[0], proj[1]);
                Gizmos.DrawLine(proj[1], proj[2]);
                Gizmos.DrawLine(proj[2], proj[0]);

                Debug.Log("Shadow Hits: " + onTri.Count(x => x));

            });
        });
    }

    private void DrawMesh(Color c, MeshFilter m)
    {
        Gizmos.color = c;
        Gizmos.DrawMesh(m.mesh, m.transform.position, m.transform.rotation, m.transform.lossyScale);
    }

    private void TriAction(MeshFilter m, System.Action<Triangle4> action)
    {
        var tris = m.mesh.triangles;
        var verts = m.mesh.vertices;
        for (int i = 0; i < tris.Length; i += 3)
        {
            var tri = new Triangle4(
                verts[tris[i]],
                verts[tris[i + 1]],
                verts[tris[i + 2]]
            );
            action(tri);
        }
    }

}

public static class VectorExtensions
{
    public static Vector4 Homo(this Vector3 v) => new Vector4(v.x, v.y, v.z, 1);
}
