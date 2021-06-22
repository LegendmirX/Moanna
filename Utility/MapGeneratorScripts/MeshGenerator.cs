using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator 
{


    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
    {
        int skipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;//Level of detail amount

        int numVertsPerLine = meshSettings.NumVercitiesPerLine;

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.MeshWorldSize / 2f;
        
        int[,] vertexIndiciesMap = new int[numVertsPerLine, numVertsPerLine];

        MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.UseFlatShading);

        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;

        for (int y = 0; y < numVertsPerLine; y ++)
        {
            for (int x = 0; x < numVertsPerLine; x ++)
            {
                bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1; // will be true if any of the follwoing proves true
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0); //the % means easily devisable by

                if(isOutOfMeshVertex == true)
                {
                    vertexIndiciesMap[x, y] = outOfMeshVertexIndex;
                    outOfMeshVertexIndex--;
                }
                else if(isSkippedVertex == false)
                {
                    vertexIndiciesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }


        for (int y = 0; y < numVertsPerLine; y ++)
        {
            for (int x = 0; x < numVertsPerLine; x ++)
            {
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);
                if (isSkippedVertex == false)
                {
                    bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                    bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && isOutOfMeshVertex == false;
                    bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && isOutOfMeshVertex == false && isMeshEdgeVertex == false;
                    bool isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && isOutOfMeshVertex == false && isMeshEdgeVertex == false && isMainVertex == false;

                    int vertexIndex = vertexIndiciesMap[x, y];

                    //UVs need to be a percentage so a number between 0-1
                    Vector2 uvPercent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
                    Vector2 vertexPosition2D = topLeft + new Vector2(uvPercent.x, -uvPercent.y) * meshSettings.MeshWorldSize;
                    float height = heightMap[x, y];

                    if(isEdgeConnectionVertex == true)
                    {
                        bool isVertical = x == 2 || x == numVertsPerLine - 3;
                        int distToMainVertA = ((isVertical) ? y - 2 : x - 2) % skipIncrement;
                        int distToMainVertB = skipIncrement - distToMainVertA;
                        float distancePercentFromAtoB = distToMainVertA / (float)skipIncrement;

                        float heightOfMainVertA = heightMap[(isVertical) ? x : x - distToMainVertA, (isVertical) ? y - distToMainVertA : y];
                        float heightOfMainVertB = heightMap[(isVertical) ? x : x + distToMainVertB, (isVertical) ? y + distToMainVertB : y];

                        height = heightOfMainVertA * (1 - distancePercentFromAtoB) + heightOfMainVertB * distancePercentFromAtoB;
                    }

                    meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), uvPercent, vertexIndex);

                    bool createTriange = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (isEdgeConnectionVertex == false || (x != 2 && y != 2));

                    if (createTriange == true)
                    {
                        int currentIncremet = (isMainVertex == true && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;

                        //setting triangles for this section
                        //these are the 4 corners of the square in the mesh
                        int a = vertexIndiciesMap[x, y];
                        int b = vertexIndiciesMap[x + currentIncremet, y];
                        int c = vertexIndiciesMap[x, y + currentIncremet];
                        int d = vertexIndiciesMap[x + currentIncremet, y + currentIncremet];

                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }

                    vertexIndex++;
                }
            }
        }

        meshData.FinalizeNormals();

        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uVs;
    Vector3[] bakedNormals;
    Vector3[] outOfMeshVertices;
    int[] outOfMeshTriangle;

    int triangleIndex;
    int outOfMeshTriIndex;

    bool useFlatShading;

    public MeshData(int numVertsPerLine, int skipIncriment, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;

        int numOfMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
        int numEdgeConnectionVertices = (skipIncriment - 1) * (numVertsPerLine - 5) / skipIncriment * 4;
        int numMainVertsPerLine = (numVertsPerLine - 5) / skipIncriment + 1;
        int numMainVerts = numMainVertsPerLine * numMainVertsPerLine;

        vertices = new Vector3[numOfMeshEdgeVertices + numEdgeConnectionVertices + numMainVerts]; //number of points in the mesh
        uVs = new Vector2[vertices.Length]; //arrray for showing imgs

        int numMeshEdgeTris = 8 * (numVertsPerLine - 4);
        int numMainTris = (numMainVertsPerLine - 1) * (numMainVertsPerLine - 1) * 2;
        triangles = new int[(numMeshEdgeTris + numMainTris) * 3]; //number of tri's in the mesh. 3 tri's for every triangle

        outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4]; //size + boarder
        outOfMeshTriangle = new int[24 * (numVertsPerLine - 2)];// yes
    }

    public void AddVertex(Vector3 vertexPos, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)//is it a boarder vert?
        {
            outOfMeshVertices[-vertexIndex - 1] = vertexPos;
        }
        else
        {
            vertices[vertexIndex] = vertexPos;
            uVs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0) //is it a boarder tri?
        {
            outOfMeshTriangle[outOfMeshTriIndex] = a;
            outOfMeshTriangle[outOfMeshTriIndex + 1] = b;
            outOfMeshTriangle[outOfMeshTriIndex + 2] = c;

            outOfMeshTriIndex += 3;//incriment by 3 as we are on the next triangle now
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex+1] = b;
            triangles[triangleIndex+2] = c;

            triangleIndex += 3;//incriment by 3 as we are on the next triangle now
        }
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3; //triangles hold the 3 verticies so divifde by 3 for amount of tri's
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3; //I x 3 is the index in tri array
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int boarderTriangleCount = outOfMeshTriangle.Length / 3;
        for (int i = 0; i < boarderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3; //I x 3 is the index in tri array
            int vertexIndexA = outOfMeshTriangle[normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangle[normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangle[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0)//is boarder?
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if( vertexIndexB >= 0)//is boarder?
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if(vertexIndexC >= 0)//is boarder?
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        //Cross normal gives us the side for the normal on the mesh
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void FinalizeNormals()
    {
        if(useFlatShading == true)
        {
            FlatShading();
        }
        else
        {
            BakeNormals();
        }
    }

    void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUVs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUVs[i] = uVs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uVs = flatShadedUVs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uVs;
        if(useFlatShading == true)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = bakedNormals; 
        }


        return mesh; 
    }
}