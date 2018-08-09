using System.Collections.Generic;
using ProtoBuf;
using System.Numerics;

[ProtoContract]
public class NavMeshVector3
{
    [ProtoMember(1)]
    public float x;
    //
    // Summary:
    //     Y component of the vector.
    [ProtoMember(2)]
    public float y;
    //
    // Summary:
    //     Z component of the vector.
    [ProtoMember(3)]
    public float z;

    public NavMeshVector3() { }

    public NavMeshVector3(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.z = 0;
    }

    public NavMeshVector3(float x, float y, float z)
    {

        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[ProtoContract]
public class NavMeshData
{
    [ProtoMember(1)]
    public List<NavMeshVector3> Vertices { get; set; }
    [ProtoMember(2)]
    public int[] Triangles { get; set; }
}