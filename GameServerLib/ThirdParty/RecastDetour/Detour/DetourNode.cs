using System;
using System.Diagnostics;
using dtStatus = System.UInt32;
using dtNodeIndex = System.UInt16;
#if DT_POLYREF64
using dtPolyRef = System.UInt64;
//using dtTileRef = System.UInt64;
#else
using dtPolyRef = System.UInt32;
//using dtTileRef = System.UInt32;
#endif

#if DT_POLYREF64
// From Thomas Wang, https://gist.github.com/badboy/6267743
inline uint dtHashRef(dtPolyRef a)
{
	a = (~a) + (a << 18); // a = (a << 18) - a - 1;
	a = a ^ (a >> 31);
	a = a * 21; // a = (a + (a << 2)) + (a << 4);
	a = a ^ (a >> 11);
	a = a + (a << 6);
	a = a ^ (a >> 22);
	return (uint)a;
}
#else
public partial class Detour{
    public static uint dtHashRef(dtPolyRef a)
    {
	    a += ~(a<<15);
	    a ^=  (a>>10);
	    a +=  (a<<3);
	    a ^=  (a>>6);
	    a += ~(a<<11);
	    a ^=  (a>>16);
	    return (uint)a;
    }
}
#endif

public partial class Detour{

    public enum dtNodeFlags {
        DT_NODE_OPEN = 0x01,
        DT_NODE_CLOSED = 0x02,
    };

    public const dtNodeIndex DT_NULL_IDX = dtNodeIndex.MaxValue; //(dtNodeIndex)~0;

    public class dtNode {
        public float[] pos = new float[3];				//< Position of the node.
        public float cost;					//< Cost from previous node to current node.
        public float total;				//< Cost up to the node.
        public uint pidx;// : 30;		//< Index to parent node.
        public byte flags;// : 2;		//< Node flags 0/open/closed.
        public dtPolyRef id;				    //< Polygon ref the node corresponds to.
        ///
        public static int getSizeOf() {
            //C# can't guess the sizeof of the float array, let's pretend
            return sizeof(float) * (3 + 1 + 1)
                + sizeof(uint)
                + sizeof(byte)
                + sizeof(dtPolyRef);
        }
        public void dtcsClearFlag(dtNodeFlags flag) {
            unchecked {
                flags &= (byte)(~flag);
            }
        }
        public void dtcsSetFlag(dtNodeFlags flag) {
            flags |= (byte)flag;
        }
        public bool dtcsTestFlag(dtNodeFlags flag) {
            return (flags & (byte)flag) != 0;
        }
    };


    public class dtNodePool{
        private dtNode[] m_nodes;
        private dtNodeIndex[] m_first;
        private dtNodeIndex[] m_next;
        private int m_maxNodes;
        private int m_hashSize;
        private int m_nodeCount;

//////////////////////////////////////////////////////////////////////////////////////////
        public dtNodePool(int maxNodes, int hashSize) 
	
        {
	        m_maxNodes = maxNodes;
	        m_hashSize = hashSize;

	        Debug.Assert(dtNextPow2((uint)m_hashSize) == (uint)m_hashSize);
	        Debug.Assert(m_maxNodes > 0);

	        m_nodes = new dtNode[m_maxNodes];
            dtcsArrayItemsCreate(m_nodes);
	        m_next = new dtNodeIndex[m_maxNodes];
	        m_first = new dtNodeIndex[hashSize];

	        Debug.Assert(m_nodes != null);
	        Debug.Assert(m_next != null);
	        Debug.Assert(m_first != null);

            for (int i=0;i<hashSize;++i){
                m_first[i] = DT_NULL_IDX; 
            }
            for (int i=0;i<m_maxNodes;++i){
				m_next[i] = DT_NULL_IDX; 
            } 
        }

        public void clear()
        {
            for (int i=0;i<m_hashSize;++i){
                m_first[i] = DT_NULL_IDX;
            }
	        m_nodeCount = 0;
        }

        public uint getNodeIdx(dtNode node) {
            if (node == null)
                return 0;

            return (uint)(System.Array.IndexOf(m_nodes, node)) + 1;
        }

        public dtNode getNodeAtIdx(uint idx) {
            if (idx == 0)
                return null;
            return m_nodes[idx - 1];
        }

        public int getMemUsed() {
            return
                sizeof(int) * 3 +
                dtNode.getSizeOf() * m_maxNodes +
                sizeof(dtNodeIndex) * m_maxNodes +
                sizeof(dtNodeIndex) * m_hashSize;
        }

        public int getMaxNodes() {
            return m_maxNodes;
        }

        public int getHashSize() {
            return m_hashSize;
        }
        public dtNodeIndex getFirst(int bucket) {
            return m_first[bucket];
        }
        public dtNodeIndex getNext(int i) {
            return m_next[i];
        }

        public dtNode findNode(dtPolyRef id)
        {
	        uint bucket = (uint)(dtHashRef(id) & (m_hashSize-1));
	        dtNodeIndex i = m_first[bucket];
	        while (i != DT_NULL_IDX)
	        {
		        if (m_nodes[i].id == id)
			        return m_nodes[i];
		        i = m_next[i];
	        }
	        return null;
        } 

        public dtNode getNode(dtPolyRef id)
        {
	        uint bucket = (uint)(dtHashRef(id) & (m_hashSize-1));
	        dtNodeIndex i = m_first[bucket];
	        dtNode node = null;
	        while (i != DT_NULL_IDX)
	        {
		        if (m_nodes[i].id == id)
			        return m_nodes[i];
		        i = m_next[i];
	        }
	
	        if (m_nodeCount >= m_maxNodes)
		        return null;
	
	        i = (dtNodeIndex)m_nodeCount;
	        m_nodeCount++;
	
	        // Init node
	        node = m_nodes[i];
	        node.pidx = 0;
	        node.cost = 0;
	        node.total = 0;
	        node.id = id;
	        node.flags = 0;
	
	        m_next[i] = m_first[bucket];
	        m_first[bucket] = i;
	
	        return node;
        }
    }


//////////////////////////////////////////////////////////////////////////////////////////
    public class dtNodeQueue{
        private dtNode[] m_heap;
        private int m_capacity;
        private int m_size;

        public dtNodeQueue(int n)
        {
	        m_capacity = n;
	        Debug.Assert(m_capacity > 0);
	
	        m_heap = new dtNode[m_capacity+1];//(dtNode**)dtAlloc(sizeof(dtNode*)*(m_capacity+1), DT_ALLOC_PERM);
	        Debug.Assert(m_heap != null);
        }

        public void clear() {
            m_size = 0;
        }

        public dtNode top() {
            return m_heap[0];
        }

        public dtNode pop() {
            dtNode result = m_heap[0];
            m_size--;
            trickleDown(0, m_heap[m_size]);
            return result;
        }

        public void push(dtNode node) {
            m_size++;
            bubbleUp(m_size - 1, node);
        }

        public void modify(dtNode node) {
            for (int i = 0; i < m_size; ++i) {
                if (m_heap[i] == node) {
                    bubbleUp(i, node);
                    return;
                }
            }
        }

        public bool empty() {
            return m_size == 0;
        }

        public int getMemUsed() {
            return sizeof(int) * 2 +
            dtNode.getSizeOf() * (m_capacity + 1);
        }

        public int getCapacity() {
            return m_capacity;
        }


        public void bubbleUp(int i, dtNode node)
        {
	        int parent = (i-1)/2;
	        // note: (index > 0) means there is a parent
	        while ((i > 0) && (m_heap[parent].total > node.total))
	        {
		        m_heap[i] = m_heap[parent];
		        i = parent;
		        parent = (i-1)/2;
	        }
	        m_heap[i] = node;
        }

        public void trickleDown(int i, dtNode node)
        {
	        int child = (i*2)+1;
	        while (child < m_size)
	        {
		        if (((child+1) < m_size) && 
			        (m_heap[child].total > m_heap[child+1].total))
		        {
			        child++;
		        }
		        m_heap[i] = m_heap[child];
		        i = child;
		        child = (i*2)+1;
	        }
	        bubbleUp(i, node);
        }
    }
}
