using System;

public static partial class Detour{

    /**
    @defgroup detour Detour

    Members in this module are used to create, manipulate, and query navigation 
    meshes.

    @note This is a summary list of members.  Use the index or search 
    feature to find minor members.
    */

    /// Derives the closest point on a triangle from the specified reference point.
    ///  @param[out]	closest	The closest point on the triangle.	
    ///  @param[in]		p		The reference point from which to test. [(x, y, z)]
    ///  @param[in]		a		Vertex A of triangle ABC. [(x, y, z)]
    ///  @param[in]		b		Vertex B of triangle ABC. [(x, y, z)]
    ///  @param[in]		c		Vertex C of triangle ABC. [(x, y, z)]
    public static void dtClosestPtPointTriangle(float[] closest, float[] p,
							      float[] a, float[] b, float[] c)
    {
	    // Check if P in vertex region outside A
	    float[] ab = new float[3];//, ac[3], ap[3];
        float[] ac = new float[3];
        float[] ap = new float[3];
	    dtVsub(ab, b, a);
	    dtVsub(ac, c, a);
	    dtVsub(ap, p, a);
	    float d1 = dtVdot(ab, ap);
	    float d2 = dtVdot(ac, ap);
	    if (d1 <= 0.0f && d2 <= 0.0f)
	    {
		    // barycentric coordinates (1,0,0)
		    dtVcopy(closest, a);
		    return;
	    }
	
	    // Check if P in vertex region outside B
	    float[] bp = new float[3];
	    dtVsub(bp, p, b);
	    float d3 = dtVdot(ab, bp);
	    float d4 = dtVdot(ac, bp);
	    if (d3 >= 0.0f && d4 <= d3)
	    {
		    // barycentric coordinates (0,1,0)
		    dtVcopy(closest, b);
		    return;
	    }
	
	    // Check if P in edge region of AB, if so return projection of P onto AB
	    float vc = d1*d4 - d3*d2;
	    if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
	    {
		    // barycentric coordinates (1-v,v,0)
		    float _v = d1 / (d1 - d3);
		    closest[0] = a[0] + _v * ab[0];
		    closest[1] = a[1] + _v * ab[1];
		    closest[2] = a[2] + _v * ab[2];
		    return;
	    }
	
	    // Check if P in vertex region outside C
	    float[] cp = new float[3];
	    dtVsub(cp, p, c);
	    float d5 = dtVdot(ab, cp);
	    float d6 = dtVdot(ac, cp);
	    if (d6 >= 0.0f && d5 <= d6)
	    {
		    // barycentric coordinates (0,0,1)
		    dtVcopy(closest, c);
		    return;
	    }
	
	    // Check if P in edge region of AC, if so return projection of P onto AC
	    float vb = d5*d2 - d1*d6;
	    if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
	    {
		    // barycentric coordinates (1-w,0,w)
		    float _w = d2 / (d2 - d6);
		    closest[0] = a[0] + _w * ac[0];
		    closest[1] = a[1] + _w * ac[1];
		    closest[2] = a[2] + _w * ac[2];
		    return;
	    }
	
	    // Check if P in edge region of BC, if so return projection of P onto BC
	    float va = d3*d6 - d5*d4;
	    if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
	    {
		    // barycentric coordinates (0,1-w,w)
		    float _w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
		    closest[0] = b[0] + _w * (c[0] - b[0]);
		    closest[1] = b[1] + _w * (c[1] - b[1]);
		    closest[2] = b[2] + _w * (c[2] - b[2]);
		    return;
	    }
	
	    // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
	    float denom = 1.0f / (va + vb + vc);
	    float v = vb * denom;
	    float w = vc * denom;
	    closest[0] = a[0] + ab[0] * v + ac[0] * w;
	    closest[1] = a[1] + ab[1] * v + ac[1] * w;
	    closest[2] = a[2] + ab[2] * v + ac[2] * w;
    }

    public static bool dtIntersectSegmentPoly2D(float[] p0, float[] p1,
							      float[] verts, int nverts,
							      ref float tmin, ref float tmax,
							      ref int segMin, ref int segMax)
    {
	    const float EPS = 0.00000001f;
	
	    tmin = 0;
	    tmax = 1;
	    segMin = -1;
	    segMax = -1;
	
	    float[] dir = new float[3];
	    dtVsub(dir, p1, p0);
	
	    for (int i = 0, j = nverts-1; i < nverts; j=i++)
	    {
		    float[] edge = new float[3];
            float[] diff = new float[3];
		    dtVsub(edge,0, verts,i*3, verts,j*3);
		    dtVsub(diff,0, p0,0, verts,j*3);
		    float n = dtVperp2D(edge, diff);
		     float d = dtVperp2D(dir, edge);
		    if (Math.Abs(d) < EPS)
		    {
			    // S is nearly parallel to this edge
			    if (n < 0)
				    return false;
			    else
				    continue;
		    }
		    float t = n / d;
		    if (d < 0)
		    {
			    // segment S is entering across this edge
			    if (t > tmin)
			    {
				    tmin = t;
				    segMin = j;
				    // S enters after leaving polygon
				    if (tmin > tmax)
					    return false;
			    }
		    }
		    else
		    {
			    // segment S is leaving across this edge
			    if (t < tmax)
			    {
				    tmax = t;
				    segMax = j;
				    // S leaves before entering polygon
				    if (tmax < tmin)
					    return false;
			    }
		    }
	    }
	
	    return true;
    }

    public static float dtDistancePtSegSqr2D(float[] pt, int ptStart, float[] p, int pStart, float[] q, int qStart, ref float t)
    {
	    float pqx = q[qStart + 0] - p[pStart + 0];
	    float pqz = q[qStart + 2] - p[pStart + 2];
	    float dx = pt[ptStart + 0] - p[pStart + 0];
	    float dz = pt[ptStart + 2] - p[pStart + 2];
	    float d = pqx*pqx + pqz*pqz;
	    t = pqx*dx + pqz*dz;
	    if (d > 0) t /= d;
	    if (t < 0) t = 0;
	    else if (t > 1) t = 1;
	    dx = p[pStart + 0] + t*pqx - pt[ptStart + 0];
	    dz = p[pStart + 2] + t*pqz - pt[ptStart + 2];
	    return dx*dx + dz*dz;
    }

    /// Derives the centroid of a convex polygon.
    ///  @param[out]	tc		The centroid of the polgyon. [(x, y, z)]
    ///  @param[in]		idx		The polygon indices. [(vertIndex) * @p nidx]
    ///  @param[in]		nidx	The number of indices in the polygon. [Limit: >= 3]
    ///  @param[in]		verts	The polygon vertices. [(x, y, z) * vertCount]
    public static void dtCalcPolyCenter(float[] tc, ushort[] idx, int nidx, float[] verts)
    {
	    tc[0] = 0.0f;
	    tc[1] = 0.0f;
	    tc[2] = 0.0f;
	    for (int j = 0; j < nidx; ++j)
	    {
            int vIndex = idx[j]*3;
		    tc[0] += verts[vIndex + 0];
		    tc[1] += verts[vIndex + 1];
		    tc[2] += verts[vIndex + 2];
	    }
	    float s = 1.0f / nidx;
	    tc[0] *= s;
	    tc[1] *= s;
	    tc[2] *= s;
    }

    /// Derives the y-axis height of the closest point on the triangle from the specified reference point.
    ///  @param[in]		p		The reference point from which to test. [(x, y, z)]
    ///  @param[in]		a		Vertex A of triangle ABC. [(x, y, z)]
    ///  @param[in]		b		Vertex B of triangle ABC. [(x, y, z)]
    ///  @param[in]		c		Vertex C of triangle ABC. [(x, y, z)]
    ///  @param[out]	h		The resulting height.
    public static bool dtClosestHeightPointTriangle(float [] p, int pStart, float[] a, int aStart, float[] b, int bStart, float[] c, int cStart, ref float h)
    {
	    float[] v0 = new float [3];
        float[] v1 = new float [3];
        float[] v2 = new float [3];
	    dtVsub(v0,0, c,cStart,a,aStart);
	    dtVsub(v1,0, b,bStart,a,aStart);
	    dtVsub(v2,0, p,pStart,a,aStart);
	
	    float dot00 = dtVdot2D(v0, v0);
	    float dot01 = dtVdot2D(v0, v1);
	    float dot02 = dtVdot2D(v0, v2);
	    float dot11 = dtVdot2D(v1, v1);
	    float dot12 = dtVdot2D(v1, v2);
	
	    // Compute barycentric coordinates
	    float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
	    float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
	    float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

	    // The (sloppy) epsilon is needed to allow to get height of points which
	    // are interpolated along the edges of the triangles.
	    const float EPS = 1e-4f;
	
	    // If point lies inside the triangle, return interpolated ycoord.
	    if (u >= -EPS && v >= -EPS && (u+v) <= 1+EPS)
	    {
		    h = a[aStart + 1] + v0[1]*u + v1[1]*v;
		    return true;
	    }
	
	    return false;
    }

    /// Determines if the specified point is inside the convex polygon on the xz-plane.
    ///  @param[in]		pt		The point to check. [(x, y, z)]
    ///  @param[in]		verts	The polygon vertices. [(x, y, z) * @p nverts]
    ///  @param[in]		nverts	The number of vertices. [Limit: >= 3]
    /// @return True if the point is inside the polygon.
    /// @par
    ///
    /// All points are projected onto the xz-plane, so the y-values are ignored.
    public static bool dtPointInPolygon(float[] pt, float[] verts, int nverts)
    {
	    // TODO: Replace pnpoly with triArea2D tests?
	    int i, j;
	    bool c = false;
	    for (i = 0, j = nverts-1; i < nverts; j = i++)
	    {
            int viIndex = i*3;
            int vjIndex = j*3;
		    if (((verts[viIndex + 2] > pt[2]) != (verts[vjIndex + 2] > pt[2])) &&
			    (pt[0] < (verts[vjIndex + 0]-verts[viIndex + 0]) * (pt[2]-verts[viIndex + 2]) / (verts[vjIndex + 2]-verts[viIndex + 2]) + verts[viIndex + 0]) )
			    c = !c;
	    }
	    return c;
    }

    public static bool dtDistancePtPolyEdgesSqr(float[] pt, int ptStart, float[] v, int nverts,
							      float[] ed, float[] et)
    {
	    // TODO: Replace pnpoly with triArea2D tests?
	    int i, j;
	    bool c = false;
	    for (i = 0, j = nverts-1; i < nverts; j = i++)
	    {
            int vi = i*3;
            int vj = j*3;
            if (((v[vi + 2] > pt[ptStart + 2]) != (v[vj + 2] > pt[ptStart + 2])) &&
                (pt[ptStart + 0] < (v[vj + 0] - v[vi + 0]) * (pt[ptStart + 2] - v[vi + 2]) / (v[vj + 2] - v[vi + 2]) + v[vi + 0]))
			    c = !c;
		    ed[j] = dtDistancePtSegSqr2D(pt, ptStart, v, vj, v, vi, ref et[j]);
	    }
	    return c;
    }

    public static void projectPoly(float[] axis, float[] poly, int npoly,
						    ref float rmin, ref float rmax)
    {
	    rmin = rmax = dtVdot2D(axis, poly);
	    for (int i = 1; i < npoly; ++i)
	    {
		    float d = dtVdot2D(axis,0, poly,i*3);
		    rmin = Math.Min(rmin, d);
		    rmax = Math.Max(rmax, d);
	    }
    }

    public static bool overlapRange(float amin,float amax,
						     float bmin, float bmax,
						     float eps)
    {
	    return ((amin+eps) > bmax || (amax-eps) < bmin) ? false : true;
    }

    /// Determines if the two convex polygons overlap on the xz-plane.
    ///  @param[in]		polya		Polygon A vertices.	[(x, y, z) * @p npolya]
    ///  @param[in]		npolya		The number of vertices in polygon A.
    ///  @param[in]		polyb		Polygon B vertices.	[(x, y, z) * @p npolyb]
    ///  @param[in]		npolyb		The number of vertices in polygon B.
    /// @return True if the two polygons overlap.
    /// @par
    ///
    /// All vertices are projected onto the xz-plane, so the y-values are ignored.
    public static bool dtOverlapPolyPoly2D(float[] polya, int npolya,
						     float[] polyb, int npolyb)
    {
	    const float eps = 1e-4f;
	
	    for (int i = 0, j = npolya-1; i < npolya; j=i++)
	    {
            int vaStart = j*3;
            int vbStart = i*3;
		    float[] n = new float[] { polya[vbStart + 2]-polya[vaStart + 2], 0, -(polya[vbStart + 0]-polya[vaStart + 0]) };
		    float amin = 0.0f,amax = 0.0f,bmin = 0.0f,bmax = 0.0f;
		    projectPoly(n, polya, npolya,ref amin,ref amax);
		    projectPoly(n, polyb, npolyb,ref bmin,ref bmax);
		    if (!overlapRange(amin,amax, bmin,bmax, eps))
		    {
			    // Found separating axis
			    return false;
		    }
	    }
	    for (int i = 0, j = npolyb-1; i < npolyb; j=i++)
	    {
            int vaStart = j*3;
            int vbStart = i*3;
		    float[] n = new float[] { polyb[vbStart + 2]-polyb[vaStart + 2], 0, -(polyb[vbStart + 0]-polyb[vaStart + 0]) };
		    float amin = 0.0f,amax = 0.0f,bmin = 0.0f,bmax = 0.0f;
		    projectPoly(n, polya, npolya,ref amin,ref amax);
		    projectPoly(n, polyb, npolyb,ref bmin,ref bmax);
		    if (!overlapRange(amin,amax, bmin,bmax, eps))
		    {
			    // Found separating axis
			    return false;
		    }
	    }
	    return true;
    }

    // Returns a random point in a convex polygon.
    // Adapted from Graphics Gems article.
    public static void dtRandomPointInConvexPoly(float[] pts, int npts, float[] areas,
							       float s, float t, float[] _out)
    {
	    // Calc triangle araes
	    float areasum = 0.0f;
	    for (int i = 2; i < npts; i++) {
		    areas[i] = dtTriArea2D(pts,0 , pts,(i-1)*3, pts,i*3);
		    areasum += Math.Max(0.001f, areas[i]);
	    }
	    // Find sub triangle weighted by area.
	    float thr = s*areasum;
	    float acc = 0.0f;
	    float u = 0.0f;
	    int tri = 0;
	    for (int i = 2; i < npts; i++) {
		    float dacc = areas[i];
		    if (thr >= acc && thr < (acc+dacc))
		    {
			    u = (thr - acc) / dacc;
			    tri = i;
			    break;
		    }
		    acc += dacc;
	    }
	
	    float v = (float)Math.Sqrt(t);
	
	    float a = 1 - v;
	    float b = (1 - u) * v;
	    float c = u * v;
	    int paStart = 0;
        int pbStart = (tri-1)*3;
        int pcStart = tri*3;

	    _out[0] = a*pts[paStart + 0] + b*pts[pbStart + 0] + c*pts[pcStart + 0];
	    _out[1] = a*pts[paStart + 1] + b*pts[pbStart + 1] + c*pts[pcStart + 1];
	    _out[2] = a*pts[paStart + 2] + b*pts[pbStart + 2] + c*pts[pcStart + 2];
    }

    public static float vperpXZ(float[] a, float[] b) 
    { 
        return a[0]*b[2] - a[2]*b[0]; 
    }

    public static bool dtIntersectSegSeg2D(float[] ap, float[] aq,
						     float[] bp, float[] bq,
						     ref float s, ref float t)
    {
	    float[] u = new float[3];
        float[] v = new float[3];
        float[] w = new float[3];
	    dtVsub(u,aq,ap);
	    dtVsub(v,bq,bp);
	    dtVsub(w,ap,bp);
	    float d = vperpXZ(u,v);
	    if (Math.Abs(d) < 1e-6f) return false;
	    s = vperpXZ(v,w) / d;
	    t = vperpXZ(u,w) / d;
	    return true;
    }

    public static bool dtIntersectSegSeg2D(float[] ap, int apStart, float[] aq, int aqStart,
                             float[] bp, int bpStart, float[] bq, int bqStart,
                             ref float s, ref float t) {
        float[] u = new float[3];
        float[] v = new float[3];
        float[] w = new float[3];
        dtVsub(u, 0, aq, aqStart, ap, apStart);
        dtVsub(v, 0, bq, bqStart, bp, bpStart);
        dtVsub(w, 0, ap, apStart, bp, bpStart);
        float d = vperpXZ(u, v);
        if (Math.Abs(d) < 1e-6f) return false;
        s = vperpXZ(v, w) / d;
        t = vperpXZ(u, w) / d;
        return true;
    }

    /// Swaps the values of the two parameters.
    ///  @param[in,out]	a	Value A
    ///  @param[in,out]	b	Value B
    static void dtSwap<T>(ref T lhs, ref T rhs) {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    /// Returns the square of the value.
    ///  @param[in]		a	The value.
    ///  @return The square of the value.
    public static float dtSqr(float a) {
        return a * a;
    }
    public static int dtSqr(int a) {
        return a * a;
    }
    public static uint dtSqr(uint a) {
        return a * a;
    }
    public static byte dtSqr(byte a) {
        return (byte)(a * a);
    }

    /// Clamps the value to the specified range.
    ///  @param[in]		v	The value to clamp.
    ///  @param[in]		mn	The minimum permitted return value.
    ///  @param[in]		mx	The maximum permitted return value.
    ///  @return The value, clamped to the specified range.
    // C#: Originally a template function but operators and template types in c# are a no
    public static int dtClamp(int v, int mn, int mx) {
        return v < mn ? mn : (v > mx ? mx : v);
    }
    public static uint dtClamp(uint v, uint mn, uint mx) {
        return v < mn ? mn : (v > mx ? mx : v);
    }
    public static byte dtClamp(byte v, byte mn, byte mx) {
        return v < mn ? mn : (v > mx ? mx : v);
    }
    public static ushort dtClamp(ushort v, ushort mn, ushort mx) {
        return v < mn ? mn : (v > mx ? mx : v);
    }
    public static float dtClamp(float v, float mn, float mx) {
        return v < mn ? mn : (v > mx ? mx : v);
    }

    /// @}
    /// @name Vector helper functions.
    /// @{

    /// Derives the cross product of two vectors. (@p v1 x @p v2)
    ///  @param[out]	dest	The cross product. [(x, y, z)]
    ///  @param[in]		v1		A Vector [(x, y, z)]
    ///  @param[in]		v2		A vector [(x, y, z)]
    public static void dtVcross(float[] dest, float[] v1, float[] v2) {
        dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
        dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
        dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
    }
    /// Derives the dot product of two vectors. (@p v1 . @p v2)
    ///  @param[in]		v1	A Vector [(x, y, z)]
    ///  @param[in]		v2	A vector [(x, y, z)]
    /// @return The dot product.
    public static float dtVdot(float[] v1, float[] v2) {
        return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
    }
    public static float dtVdot(float[] v1, int v1Start, float[] v2, int v2Start) {
        return v1[v1Start + 0] * v2[v2Start + 0] + v1[v1Start + 1] * v2[v2Start + 1] + v1[v1Start + 2] * v2[v2Start + 2];
    }

    /// Performs a scaled vector addition. (@p v1 + (@p v2 * @p s))
    ///  @param[out]	dest	The result vector. [(x, y, z)]
    ///  @param[in]		v1		The base vector. [(x, y, z)]
    ///  @param[in]		v2		The vector to scale and add to @p v1. [(x, y, z)]
    ///  @param[in]		s		The amount to scale @p v2 by before adding to @p v1.
    public static void dtVmad(float[] dest, float[] v1, float[] v2, float s) {
        dest[0] = v1[0] + v2[0] * s;
        dest[1] = v1[1] + v2[1] * s;
        dest[2] = v1[2] + v2[2] * s;
    }

    /// Performs a linear interpolation between two vectors. (@p v1 toward @p v2)
    ///  @param[out]	dest	The result vector. [(x, y, x)]
    ///  @param[in]		v1		The starting vector.
    ///  @param[in]		v2		The destination vector.
    ///	 @param[in]		t		The interpolation factor. [Limits: 0 <= value <= 1.0]
    public static void dtVlerp(float[] dest, float[] v1, float[] v2, float t) {
        dest[0] = v1[0] + (v2[0] - v1[0]) * t;
        dest[1] = v1[1] + (v2[1] - v1[1]) * t;
        dest[2] = v1[2] + (v2[2] - v1[2]) * t;
    }
    public static void dtVlerp(float[] dest, int destStart, float[] v1, int v1Start, float[] v2, int v2Start, float t) {
        dest[destStart + 0] = v1[v1Start + 0] + (v2[v2Start + 0] - v1[v1Start + 0]) * t;
        dest[destStart + 1] = v1[v1Start + 1] + (v2[v2Start + 1] - v1[v1Start + 1]) * t;
        dest[destStart + 2] = v1[v1Start + 2] + (v2[v2Start + 2] - v1[v1Start + 2]) * t;
    }
    /// Performs a vector addition. (@p v1 + @p v2)
    ///  @param[out]	dest	The result vector. [(x, y, z)]
    ///  @param[in]		v1		The base vector. [(x, y, z)]
    ///  @param[in]		v2		The vector to add to @p v1. [(x, y, z)]
    public static void dtVadd(float[] dest, float[] v1, float[] v2) {
        dest[0] = v1[0] + v2[0];
        dest[1] = v1[1] + v2[1];
        dest[2] = v1[2] + v2[2];
    }
    public static void dtVadd(float[] dest, int destStart, float[] v1, int v1Start, float[] v2, int v2Start) {
        dest[destStart + 0] = v1[v1Start + 0] + v2[v2Start + 0];
        dest[destStart + 1] = v1[v1Start + 1] + v2[v2Start + 1];
        dest[destStart + 2] = v1[v1Start + 2] + v2[v2Start + 2];
    }

    /// Performs a vector subtraction. (@p v1 - @p v2)
    ///  @param[out]	dest	The result vector. [(x, y, z)]
    ///  @param[in]		v1		The base vector. [(x, y, z)]
    ///  @param[in]		v2		The vector to subtract from @p v1. [(x, y, z)]
    public static void dtVsub(float[] dest, float[] v1, float[] v2) {
        dest[0] = v1[0] - v2[0];
        dest[1] = v1[1] - v2[1];
        dest[2] = v1[2] - v2[2];
    }
    public static void dtVsub(float[] dest, int destStart, float[] v1, int v1Start, float[] v2, int v2Start) {
        dest[destStart + 0] = v1[v1Start + 0] - v2[v2Start + 0];
        dest[destStart + 1] = v1[v1Start + 1] - v2[v2Start + 1];
        dest[destStart + 2] = v1[v1Start + 2] - v2[v2Start + 2];
    }

    /// Scales the vector by the specified value. (@p v * @p t)
    ///  @param[out]	dest	The result vector. [(x, y, z)]
    ///  @param[in]		v		The vector to scale. [(x, y, z)]
    ///  @param[in]		t		The scaling factor.
    public static void dtVscale(float[] dest, float[] v, float t) {
        dest[0] = v[0] * t;
        dest[1] = v[1] * t;
        dest[2] = v[2] * t;
    }
    public static void dtVscale(float[] dest, int destStart, float[] v, int vStart, float t) {
        dest[destStart + 0] = v[vStart + 0] * t;
        dest[destStart + 1] = v[vStart + 1] * t;
        dest[destStart + 2] = v[vStart + 2] * t;
    }
    /// Selects the minimum value of each element from the specified vectors.
    ///  @param[in,out]	mn	A vector.  (Will be updated with the result.) [(x, y, z)]
    ///  @param[in]	v	A vector. [(x, y, z)]
    public static void dtVmin(float[] mn, float[] v) {
        mn[0] = Math.Min(mn[0], v[0]);
        mn[1] = Math.Min(mn[1], v[1]);
        mn[2] = Math.Min(mn[2], v[2]);
    }
    public static void dtVmin(float[] mn, int mnStart, float[] v, int vStart) {
        mn[mnStart + 0] = Math.Min(mn[mnStart + 0], v[vStart + 0]);
        mn[mnStart + 1] = Math.Min(mn[mnStart + 1], v[vStart + 1]);
        mn[mnStart + 2] = Math.Min(mn[mnStart + 2], v[vStart + 2]);
    }

    /// Selects the maximum value of each element from the specified vectors.
    ///  @param[in,out]	mx	A vector.  (Will be updated with the result.) [(x, y, z)]
    ///  @param[in]		v	A vector. [(x, y, z)]
    public static void dtVmax(float[] mx, float[] v) {
        mx[0] = Math.Max(mx[0], v[0]);
        mx[1] = Math.Max(mx[1], v[1]);
        mx[2] = Math.Max(mx[2], v[2]);
    }
    public static void dtVmax(float[] mx, int mxStart, float[] v, int vStart) {
        mx[mxStart + 0] = Math.Max(mx[mxStart + 0], v[vStart + 0]);
        mx[mxStart + 1] = Math.Max(mx[mxStart + 1], v[vStart + 1]);
        mx[mxStart + 2] = Math.Max(mx[mxStart + 2], v[vStart + 2]);
    }

    /// Sets the vector elements to the specified values.
    ///  @param[out]	dest	The result vector. [(x, y, z)]
    ///  @param[in]		x		The x-value of the vector.
    ///  @param[in]		y		The y-value of the vector.
    ///  @param[in]		z		The z-value of the vector.
    public static void dtVset(float[] dest, float x, float y, float z) {
        dest[0] = x; dest[1] = y; dest[2] = z;
    }
    public static void dtVset(float[] dest, int destStart, float x, float y, float z) {
        dest[destStart + 0] = x; dest[destStart + 1] = y; dest[destStart + 2] = z;
    }

    /// Performs a vector copy.
    ///  @param[out]	dest	The result. [(x, y, z)]
    ///  @param[in]		a		The vector to copy. [(x, y, z)]
    public static void dtVcopy(float[] dest, float[] a) {
        dest[0] = a[0];
        dest[1] = a[1];
        dest[2] = a[2];
    }
    public static void dtVcopy(float[] dest, int destStart, float[] a, int aStart) {
        dest[destStart + 0] = a[aStart + 0];
        dest[destStart + 1] = a[aStart + 1];
        dest[destStart + 2] = a[aStart + 2];
    }
    /// Derives the scalar length of the vector.
    ///  @param[in]		v The vector. [(x, y, z)]
    /// @return The scalar length of the vector.
    public static float dtVlen(float[] v) {
        return (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
    }
    public static float dtVlen(float[] v, int vStart) {
        return (float)Math.Sqrt(v[0 + vStart] * v[0 + vStart] + v[1 + vStart] * v[1 + vStart] + v[2 + vStart] * v[2 + vStart]);
    }

    /// Derives the square of the scalar length of the vector. (len * len)
    ///  @param[in]		v The vector. [(x, y, z)]
    /// @return The square of the scalar length of the vector.
    public static float dtVlenSqr(float[] v) {
        return v[0] * v[0] + v[1] * v[1] + v[2] * v[2];
    }
    public static float dtVlenSqr(float[] v, int vStart) {
        return v[0 + vStart] * v[0 + vStart] + v[1 + vStart] * v[1 + vStart] + v[2 + vStart] * v[2 + vStart];
    }

    /// Returns the distance between two points.
    ///  @param[in]		v1	A point. [(x, y, z)]
    ///  @param[in]		v2	A point. [(x, y, z)]
    /// @return The distance between the two points.
    public static float dtVdist(float[] v1, float[] v2) {
        float dx = v2[0] - v1[0];
        float dy = v2[1] - v1[1];
        float dz = v2[2] - v1[2];
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
    public static float dtVdist(float[] v1, int v1Start, float[] v2, int v2Start) {
        float dx = v2[v2Start + 0] - v1[v1Start + 0];
        float dy = v2[v2Start + 1] - v1[v1Start + 1];
        float dz = v2[v2Start + 2] - v1[v1Start + 2];
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// Returns the square of the distance between two points.
    ///  @param[in]		v1	A point. [(x, y, z)]
    ///  @param[in]		v2	A point. [(x, y, z)]
    /// @return The square of the distance between the two points.
    public static float dtVdistSqr(float[] v1, float[] v2) {
        float dx = v2[0] - v1[0];
        float dy = v2[1] - v1[1];
        float dz = v2[2] - v1[2];
        return dx * dx + dy * dy + dz * dz;
    }
    public static float dtVdistSqr(float[] v1, int v1Start, float[] v2, int v2Start) {
        float dx = v2[v2Start + 0] - v1[v1Start + 0];
        float dy = v2[v2Start + 1] - v1[v1Start + 1];
        float dz = v2[v2Start + 2] - v1[v1Start + 2];
        return dx * dx + dy * dy + dz * dz;
    }

    /// Derives the distance between the specified points on the xz-plane.
    ///  @param[in]		v1	A point. [(x, y, z)]
    ///  @param[in]		v2	A point. [(x, y, z)]
    /// @return The distance between the point on the xz-plane.
    ///
    /// The vectors are projected onto the xz-plane, so the y-values are ignored.
    public static float dtVdist2D(float[] v1, float[] v2) {
        float dx = v2[0] - v1[0];
        float dz = v2[2] - v1[2];
        return (float)Math.Sqrt(dx * dx + dz * dz);
    }
    public static float dtVdist2D(float[] v1, int v1Start, float[] v2, int v2Start) {
        float dx = v2[v2Start + 0] - v1[v1Start + 0];
        float dz = v2[v2Start + 2] - v1[v1Start + 2];
        return (float)Math.Sqrt(dx * dx + dz * dz);
    }
    /// Derives the square of the distance between the specified points on the xz-plane.
    ///  @param[in]		v1	A point. [(x, y, z)]
    ///  @param[in]		v2	A point. [(x, y, z)]
    /// @return The square of the distance between the point on the xz-plane.
    public static float dtVdist2DSqr(float[] v1, float[] v2) {
        float dx = v2[0] - v1[0];
        float dz = v2[2] - v1[2];
        return dx * dx + dz * dz;
    }
    public static float dtVdist2DSqr(float[] v1, int v1Start, float[] v2, int v2Start) {
        float dx = v2[v2Start + 0] - v1[v1Start + 0];
        float dz = v2[v2Start + 2] - v1[v1Start + 2];
        return dx * dx + dz * dz;
    }
    /// Normalizes the vector.
    ///  @param[in,out]	v	The vector to normalize. [(x, y, z)]
    public static void dtVnormalize(float[] v) {
        float d = 1.0f / (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        v[0] *= d;
        v[1] *= d;
        v[2] *= d;
    }
    public static void dtVnormalize(float[] v, int vStart) {
        float d = 1.0f / (float)Math.Sqrt(v[vStart + 0] * v[vStart + 0] + v[vStart + 1] * v[vStart + 1] + v[vStart + 2] * v[vStart + 2]);
        v[vStart + 0] *= d;
        v[vStart + 1] *= d;
        v[vStart + 2] *= d;
    }

    /// Performs a 'sloppy' colocation check of the specified points.
    ///  @param[in]		p0	A point. [(x, y, z)]
    ///  @param[in]		p1	A point. [(x, y, z)]
    /// @return True if the points are considered to be at the same location.
    ///
    /// Basically, this function will return true if the specified points are 
    /// close enough to eachother to be considered colocated.
    public static bool dtVequal(float[] p0, float[] p1) {
        const float thrSqrt = (1.0f / 16384.0f);
        const float thr = thrSqrt * thrSqrt;
        float d = dtVdistSqr(p0, p1);
        return d < thr;
    }
    public static bool dtVequal(float[] p0, int p0Start, float[] p1, int p1Start) {
        const float thrSqrt = (1.0f / 16384.0f);
        const float thr = thrSqrt * thrSqrt;
        float d = dtVdistSqr(p0, p0Start, p1, p1Start);
        return d < thr;
    }
    /// Derives the dot product of two vectors on the xz-plane. (@p u . @p v)
    ///  @param[in]		u		A vector [(x, y, z)]
    ///  @param[in]		v		A vector [(x, y, z)]
    /// @return The dot product on the xz-plane.
    ///
    /// The vectors are projected onto the xz-plane, so the y-values are ignored.
    public static float dtVdot2D(float[] u, float[] v) {
        return u[0] * v[0] + u[2] * v[2];
    }
    public static float dtVdot2D(float[] u, int uStart, float[] v, int vStart) {
        return u[uStart + 0] * v[vStart + 0] + u[uStart + 2] * v[vStart + 2];
    }

    /// Derives the xz-plane 2D perp product of the two vectors. (uz*vx - ux*vz)
    ///  @param[in]		u		The LHV vector [(x, y, z)]
    ///  @param[in]		v		The RHV vector [(x, y, z)]
    /// @return The dot product on the xz-plane.
    ///
    /// The vectors are projected onto the xz-plane, so the y-values are ignored.
    public static float dtVperp2D(float[] u, float[] v) {
        return u[2] * v[0] - u[0] * v[2];
    }
    public static float dtVperp2D(float[] u, int uStart, float[] v, int vStart) {
        return u[uStart + 2] * v[vStart + 0] - u[uStart + 0] * v[vStart + 2];
    }
    /// @}
    /// @name Computational geometry helper functions.
    /// @{

    /**

    @fn float dtTriArea2D(const float* a, const float* b, const float* c)
    @par

    The vertices are projected onto the xz-plane, so the y-values are ignored.

    This is a low cost function than can be used for various purposes.  Its main purpose
    is for point/line relationship testing.

    In all cases: A value of zero indicates that all vertices are collinear or represent the same point.
    (On the xz-plane.)

    When used for point/line relationship tests, AB usually represents a line against which
    the C point is to be tested.  In this case:

    A positive value indicates that point C is to the left of line AB, looking from A toward B.<br/>
    A negative value indicates that point C is to the right of lineAB, looking from A toward B.

    When used for evaluating a triangle:

    The absolute value of the return value is two times the area of the triangle when it is
    projected onto the xz-plane.

    A positive return value indicates:

    <ul>
    <li>The vertices are wrapped in the normal Detour wrap direction.</li>
    <li>The triangle's 3D face normal is in the general up direction.</li>
    </ul>

    A negative return value indicates:

    <ul>
    <li>The vertices are reverse wrapped. (Wrapped opposite the normal Detour wrap direction.)</li>
    <li>The triangle's 3D face normal is in the general down direction.</li>
    </ul>

    */

    /// Derives the signed xz-plane area of the triangle ABC, or the relationship of line AB to point C.
    ///  @param[in]		a		Vertex A. [(x, y, z)]
    ///  @param[in]		b		Vertex B. [(x, y, z)]
    ///  @param[in]		c		Vertex C. [(x, y, z)]
    /// @return The signed xz-plane area of the triangle.
    public static float dtTriArea2D(float[] a, float[] b, float[] c) {
        float abx = b[0] - a[0];
        float abz = b[2] - a[2];
        float acx = c[0] - a[0];
        float acz = c[2] - a[2];
        return acx * abz - abx * acz;
    }
    public static float dtTriArea2D(float[] a, int aStart, float[] b, int bStart, float[] c, int cStart) {
        float abx = b[bStart + 0] - a[aStart + 0];
        float abz = b[bStart + 2] - a[aStart + 2];
        float acx = c[cStart + 0] - a[aStart + 0];
        float acz = c[cStart + 2] - a[aStart + 2];
        return acx * abz - abx * acz;
    }
    /// Determines if two axis-aligned bounding boxes overlap.
    ///  @param[in]		amin	Minimum bounds of box A. [(x, y, z)]
    ///  @param[in]		amax	Maximum bounds of box A. [(x, y, z)]
    ///  @param[in]		bmin	Minimum bounds of box B. [(x, y, z)]
    ///  @param[in]		bmax	Maximum bounds of box B. [(x, y, z)]
    /// @return True if the two AABB's overlap.
    /// @see dtOverlapBounds
    public static bool dtOverlapQuantBounds(ushort[] amin, ushort[] amax,
                                     ushort[] bmin, ushort[] bmax) {
        bool overlap = true;
        overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
        overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
        overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
        return overlap;
    }

    /// Determines if two axis-aligned bounding boxes overlap.
    ///  @param[in]		amin	Minimum bounds of box A. [(x, y, z)]
    ///  @param[in]		amax	Maximum bounds of box A. [(x, y, z)]
    ///  @param[in]		bmin	Minimum bounds of box B. [(x, y, z)]
    ///  @param[in]		bmax	Maximum bounds of box B. [(x, y, z)]
    /// @return True if the two AABB's overlap.
    /// @see dtOverlapQuantBounds
    public static bool dtOverlapBounds(float[] amin, float[] amax,
                                float[] bmin, float[] bmax) {
        bool overlap = true;
        overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
        overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
        overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
        return overlap;
    }

    /// @}
    /// @name Miscellanious functions.
    /// @{

    public static uint dtNextPow2(uint v) {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;
        return v;
    }


    public static int dtIlog2(uint v) {
        //C# not happy with shifting uints
        int r;
        int shift;
        r = ((v > 0xffff) ? 1 : 0) << 4; v >>= r;
        shift = ((v > 0xff) ? 1 : 0) << 3; v >>= shift; r |= shift;
        shift = ((v > 0xf) ? 1 : 0) << 2; v >>= shift; r |= shift;
        shift = ((v > 0x3) ? 1 : 0) << 1; v >>= shift; r |= shift;
        r |= ((int)v >> 1);
        return r;
    }

    public static int dtAlign4(int x) {
        return (x + 3) & ~3;
    }

    public static int dtOppositeTile(int side) {
        return (side + 4) & 0x7;
    }

    public static void dtSwapEndian(ref ushort v) {
        byte[] bytes = BitConverter.GetBytes(v);
        System.Array.Reverse(bytes);
        v = BitConverter.ToUInt16(bytes, 0);
    }

    public static void dtSwapEndian(ref short v) {
        byte[] bytes = BitConverter.GetBytes(v);
        System.Array.Reverse(bytes);
        v = BitConverter.ToInt16(bytes, 0);
    }

    public static void dtSwapEndian(ref uint v) {
        byte[] bytes = BitConverter.GetBytes(v);
        System.Array.Reverse(bytes);
        v = BitConverter.ToUInt32(bytes, 0);
    }

    public static void dtSwapEndian(ref int v) {
        byte[] bytes = BitConverter.GetBytes(v);
        System.Array.Reverse(bytes);
        v = BitConverter.ToInt32(bytes, 0);
    }

    public static void dtSwapEndian(ref float v) {
        byte[] bytes = BitConverter.GetBytes(v);
        System.Array.Reverse(bytes);
        v = BitConverter.ToSingle(bytes, 0);
    }


    public static void dtcsArrayItemsCreate<T>(T[] array) where T : class, new() {
        for (int i = 0; i < array.Length; ++i) {
            array[i] = new T();
        }
    }
}