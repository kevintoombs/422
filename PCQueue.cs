using System;

namespace CS422
{
    public class PCQueue
    {
        Node D = new Node();
        Node F, B;

        public PCQueue()
        {
            D.Next = D;
            F = B = D;
        }

        public void Enqueue(int dataValue) {
            Node n = new Node(dataValue);
            B = B.Next = n;
        }

        public bool Dequeue(ref int out_val) {
            if (object.ReferenceEquals(B, D)) return false;
            if (object.ReferenceEquals(F, D)) F = F.Next;
            if (object.ReferenceEquals(F, B))
            {
                out_val = F.Data;
                F = B = D;
                return true;
            }
            else
            {
                out_val = F.Data;
                F = F.Next;
                return true;
            }
        }

        private class Node
        {
            public Node Next;
            public int Data;
            public Node() { }
            public Node(int o) { Data = o; }
            public Node(int o, Node next) { Data = o; Next = next; }
        }
    }
}
