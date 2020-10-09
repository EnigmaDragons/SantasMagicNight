using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Procedural Sphere and Cube:
 * http://wiki.unity3d.com/index.php/ProceduralPrimitives
 * 
 * slightly modified for shape occluders
 */
public static class ProceduralPrimitives
{
    private static Mesh _occluderCube;
    public static Mesh occluderCube
    {
        get
        {
            if (_occluderCube == null)
            {
                _occluderCube = new Mesh();

                float length = 1.0f;
                float width = 1.0f;
                float height = 1.0f;

                #region Vertices
                Vector3 p0 = new Vector3(-length * 0.5f, -width * 0.5f, height * 0.5f);
                Vector3 p1 = new Vector3(length * 0.5f, -width * 0.5f, height * 0.5f);
                Vector3 p2 = new Vector3(length * 0.5f, -width * 0.5f, -height * 0.5f);
                Vector3 p3 = new Vector3(-length * 0.5f, -width * 0.5f, -height * 0.5f);

                Vector3 p4 = new Vector3(-length * 0.5f, width * 0.5f, height * 0.5f);
                Vector3 p5 = new Vector3(length * 0.5f, width * 0.5f, height * 0.5f);
                Vector3 p6 = new Vector3(length * 0.5f, width * 0.5f, -height * 0.5f);
                Vector3 p7 = new Vector3(-length * 0.5f, width * 0.5f, -height * 0.5f);

                Vector3[] vertices = new Vector3[]
                {
                    p0, p1, p2, p3,
                    p7, p4, p0, p3,
                    p4, p5, p1, p0,
                    p6, p7, p3, p2,
                    p5, p6, p2, p1,
                    p7, p6, p5, p4
                };
                #endregion

                #region Triangles
                int[] triangles = new int[]
                {
                    3, 1, 0, 3, 2, 1,
                    3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1, 3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
                    3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2, 3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
                    3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3, 3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
                    3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4, 3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
                    3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5, 3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,

                };
                #endregion

                _occluderCube.vertices = vertices;
                _occluderCube.triangles = triangles;

                _occluderCube.name = "Snow Box Occluder";

                _occluderCube.RecalculateBounds();
            }

            return _occluderCube;
        }
    }

    private static Mesh _occluderSphere;
    public static Mesh occluderSphere
    {
        get
        {
            if (_occluderSphere == null)
            {
                _occluderSphere = new Mesh();

                float radius = 1.0f;
                // Longitude |||
                int nbLong = 24;
                // Latitude ---
                int nbLat = 16;

                #region Vertices
                Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
                float _pi = Mathf.PI;
                float _2pi = _pi * 2f;

                vertices[0] = Vector3.up * radius;
                for (int lat = 0; lat < nbLat; lat++)
                {
                    float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
                    float sin1 = Mathf.Sin(a1);
                    float cos1 = Mathf.Cos(a1);

                    for (int lon = 0; lon <= nbLong; lon++)
                    {
                        float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                        float sin2 = Mathf.Sin(a2);
                        float cos2 = Mathf.Cos(a2);

                        vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
                    }
                }
                vertices[vertices.Length - 1] = Vector3.up * -radius;
                #endregion

                #region Triangles
                int nbFaces = vertices.Length;
                int nbTriangles = nbFaces * 2;
                int nbIndexes = nbTriangles * 3;
                int[] triangles = new int[nbIndexes];

                //Top Cap
                int i = 0;
                for (int lon = 0; lon < nbLong; lon++)
                {
                    triangles[i++] = lon + 2;
                    triangles[i++] = lon + 1;
                    triangles[i++] = 0;
                }

                //Middle
                for (int lat = 0; lat < nbLat - 1; lat++)
                {
                    for (int lon = 0; lon < nbLong; lon++)
                    {
                        int current = lon + lat * (nbLong + 1) + 1;
                        int next = current + nbLong + 1;

                        triangles[i++] = current;
                        triangles[i++] = current + 1;
                        triangles[i++] = next + 1;

                        triangles[i++] = current;
                        triangles[i++] = next + 1;
                        triangles[i++] = next;
                    }
                }

                //Bottom Cap
                for (int lon = 0; lon < nbLong; lon++)
                {
                    triangles[i++] = vertices.Length - 1;
                    triangles[i++] = vertices.Length - (lon + 2) - 1;
                    triangles[i++] = vertices.Length - (lon + 1) - 1;
                }
                #endregion

                _occluderSphere.vertices = vertices;
                _occluderSphere.triangles = triangles;

                _occluderSphere.name = "Snow Sphere Occluder";

                _occluderSphere.RecalculateBounds();
            }

            return _occluderSphere;
        }
    }

    /*
     * needed a mesh with infinite bounds, but never renderable. Create a mesh with cube vertices, but
     * no triangles. This means we can assign bounds to it, without it ever rendering. This means it can
     * expect OnWillRenderObject to be called each frame in the scene view if it's assigned to a mesh.
     * OnWillRenderObject is much more reliable than Update in edit mode, and it allows SnowRenderer to 'detect' when
     * the scene camera is rendering the scene so it does special rendering for that camera
     */
    private static Mesh _boundsCube;
    public static Mesh boundsCube
    {
        get
        {
            if (_boundsCube == null)
            {
                _boundsCube = new Mesh();

                #region Vertices
                Vector3 p0 = new Vector3(-0.5f, -0.5f,  0.5f);
                Vector3 p1 = new Vector3( 0.5f, -0.5f,  0.5f);
                Vector3 p2 = new Vector3( 0.5f, -0.5f, -0.5f);
                Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f);

                Vector3 p4 = new Vector3(-0.5f, 0.5f,  0.5f);
                Vector3 p5 = new Vector3( 0.5f, 0.5f,  0.5f);
                Vector3 p6 = new Vector3( 0.5f, 0.5f, -0.5f);
                Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f);

                Vector3[] vertices = new Vector3[]
                {
                    p0, p1, p2, p3,
                    p7, p4, p0, p3,
                    p4, p5, p1, p0,
                    p6, p7, p3, p2,
                    p5, p6, p2, p1,
                    p7, p6, p5, p4
                };
                #endregion

                _boundsCube.name = "Infinite Bounds";

                _boundsCube.vertices = vertices;
                _boundsCube.bounds.SetMinMax(Vector3.one * float.MinValue, Vector3.one * float.MaxValue);
            }

            return _boundsCube;
        }
    }

    private static Mesh _blitQuad;
    public static Mesh blitQuad
    {
        get
        {
            if (_blitQuad == null)
            {
                _blitQuad = new Mesh();

                var vertices = new Vector3[4];
                var triangles = new int[6];
                var normals = new Vector3[4];
                var UVs = new Vector2[4];

                vertices[0] = new Vector3(-1.0f, -1.0f, 0.0f); vertices[1] = new Vector3(1.0f, -1.0f, 0.0f);
                vertices[2] = new Vector3(-1.0f, 1.0f, 0.0f); vertices[3] = new Vector3(1.0f, 1.0f, 0.0f);

                triangles[0] = 0; triangles[1] = 2; triangles[2] = 1;
                triangles[3] = 2; triangles[4] = 3; triangles[5] = 1;

                normals[0] = -Vector3.forward; normals[1] = -Vector3.forward;
                normals[2] = -Vector3.forward; normals[3] = -Vector3.forward;

                UVs[0] = new Vector2(0, 1); UVs[1] = new Vector2(1, 1);
                UVs[2] = new Vector2(0, 0); UVs[3] = new Vector2(1, 0);

                _blitQuad.vertices = vertices;
                _blitQuad.normals = normals;
                _blitQuad.uv = UVs;
                _blitQuad.triangles = triangles;

                _blitQuad.name = "Blit Quad";

                _blitQuad.RecalculateBounds();
            }
            return _blitQuad;
        }
    }
}
