using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

public static class TerrainGenerator
{
    public static GameObject CreatePlane(int size)
    {
        GameObject plane = new GameObject("GeneratedTerrainPlane");
        plane.transform.position = Vector3.zero;

        MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
        plane.AddComponent(typeof(MeshRenderer));

        Mesh m = new Mesh
        {
            name = plane.name
        };

        //https://docs.unity3d.com/ScriptReference/Mesh-indexFormat.html
        //Index buffer can either be 16 bit(supports up to 65536 vertices in a mesh), or 32 bit(supports up to 4 billion vertices).Default index format is 16 bit, since that takes less memory and bandwidth.
        //Note that GPU support for 32 bit indices is not guaranteed on all platforms; for example Android devices with Mali - 400 GPU do not support them.
        //When using 32 bit indices on such a platform, a warning message will be logged and mesh will not render.
        m.indexFormat = IndexFormat.UInt32;

        int vertexCount = size + 1;
        int numTriangles = size * size * 6;
        int numVertices = vertexCount * vertexCount;

        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] triangles = new int[numTriangles];


        /* https://docs.unity3d.com/ScriptReference/Mesh-tangents.html
         * Tangents are mostly used in bump-mapped Shaders. A tangent is a unit-length vector that follows Mesh surface along horizontal (U) texture direction. 
         * Tangents in Unity are represented as Vector4, with x,y,z components defining the vector, and w used to flip the binormal if needed.
         * Unity calculates the other surface vector (binormal) by taking a cross product between the normal and the tangent, and multiplying the result by tangent.w. Therefore, w should always be 1 or -1.
         * You should calculate tangents yourself if you plan to use bump-mapped shaders on the Mesh. Assign tangents after assigning normals or using RecalculateNormals.
         */
        Vector4[] tangents = new Vector4[numVertices];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f); //<- unit-length vector that follows Mesh surface along horizontal (U) texture direction

        int index = 0;

        //Fill vertices, tangents and uv arrays
        for (float y = 0.0f; y < vertexCount; y++)
        {
            for (float x = 0.0f; x < vertexCount; x++)
            {
                vertices[index] = new Vector3(x - size / 2f, 0.0f, y - size / 2f);
                tangents[index] = tangent;
                uvs[index++] = new Vector2(x, y);
            }
        }

        index = 0;
        //Generate triangles for the plane
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                //quad vertices: first triangle
                triangles[index] = (y * vertexCount) + x;
                triangles[index + 1] = ((y + 1) * vertexCount) + x;
                triangles[index + 2] = (y * vertexCount) + x + 1;
                //quad vertices: second triangle
                triangles[index + 3] = ((y + 1) * vertexCount) + x;
                triangles[index + 4] = ((y + 1) * vertexCount) + x + 1;
                triangles[index + 5] = (y * vertexCount) + x + 1;
                index += 6;
            }
        }

        //Set verts, uvs, triangles, tangents and recalculate normals
        m.vertices = vertices;
        m.uv = uvs;
        m.triangles = triangles;
        m.tangents = tangents;
        m.RecalculateNormals();

        meshFilter.sharedMesh = m;

        //Add mesh collider
        plane.AddComponent<MeshCollider>();

        return plane;
    }

    public static void DeformTerrainMesh(float heightScale, Color[] colorArray, bool plateauStyle, GameObject terrainGameObject)
    {
        Mesh terrainMesh = terrainGameObject.GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = terrainMesh.vertices;
        Vector3[] newPosVertices = new Vector3[terrainMesh.vertexCount];

        for (int index = 0; index < newPosVertices.Length; index++)
        {
            //pick pixel from color array red (r) channel. Multiple color value (0...1) by height scale
            float newHeight = plateauStyle ? Mathf.Floor(colorArray[index].r * heightScale) : colorArray[index].r * heightScale;

            Vector3 newVertexPosition = new Vector3(vertices[index].x, terrainGameObject.transform.position.y + newHeight, vertices[index].z); ;
            newPosVertices[index] = newVertexPosition;
        }

        //Set new vertex positions
        terrainMesh.vertices = newPosVertices;
        RecalculateMesh(terrainMesh, terrainGameObject.GetComponent<MeshCollider>());
    }

    public static void RecalculateMesh(Mesh mesh, MeshCollider meshCollider = null)
    {
        //Update bounds, normals and tangents
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        if (meshCollider != null)
            meshCollider.sharedMesh = mesh;
    }

    public static void PushPullVerticesBrush(Vector3 brushHitPoint, GameObject terrainGameObject, float brushRadius, float strength, bool flatten = false)
    {
        Mesh terrainMesh = terrainGameObject.GetComponent<MeshFilter>().mesh;

        //Only process distance on xz axes
        brushHitPoint.y = 0;

        Vector3[] vertices = terrainMesh.vertices;
        List<Vector3> verticesWithinRadius = new List<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(brushHitPoint, new Vector3(vertices[i].x, 0, vertices[i].z));
            if (distance < brushRadius)
            {
                if (!flatten)
                    vertices[i].y = Interpolation.Lerp(vertices[i].y, vertices[i].y + strength, 1 - Interpolation.EaseInOut(distance / brushRadius));
                else
                    verticesWithinRadius.Add(vertices[i]);
            }
        }

        //TODO: Oh boy, not very optimal :/
        if (flatten && verticesWithinRadius.Count > 0)
        {
            float avgY = verticesWithinRadius.Average(v => v.y);
            
            for (int i = 0; i < vertices.Length; i++)
            {
                float distance = Vector3.Distance(brushHitPoint, new Vector3(vertices[i].x, 0, vertices[i].z));
                if (distance < brushRadius)
                {
                    vertices[i].y = Interpolation.Lerp(vertices[i].y, avgY, 0.1f);
                }
            }
        }

        terrainMesh.vertices = vertices;
        RecalculateMesh(terrainMesh, terrainGameObject.GetComponent<MeshCollider>());
    }

    public static Stack<Vector3> GetVerticesWithNormalAngleUpTreshold(Mesh mesh, float angleTreshold)
    {
        Vector3[] normals = mesh.normals;
        Vector3[] vertices = mesh.vertices;
        Stack<Vector3> angleFilteredVertices = new Stack<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            float angle = Vector3.Angle(normals[i], Vector3.up);
            if (angle < angleTreshold)
                angleFilteredVertices.Push(vertices[i]);
        }

        return angleFilteredVertices;
    }

    public static void RemoveSharedVertices(Mesh mesh)
    {
        //Process the triangles
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];

        //Re-process the triangles so that they don't share vertices
        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];
            triangles[i] = i;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        RecalculateMesh(mesh);
    }
}

