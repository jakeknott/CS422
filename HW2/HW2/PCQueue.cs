using System;

namespace CS422
{
	public class Node
	{
		public int value; 
		public Node next;

		public Node ()
		{
			value = 0;
			next = null;
		}

		public Node (int val)
		{
			value = val;
			next = null;
		}
	}

	public class PCQueue
	{
		private Node _dummyNode;
		Node _startNode;
		Node _endNode;

		public PCQueue ()
		{
			_dummyNode = new Node ();
			_startNode = _dummyNode;
			_endNode = _dummyNode;
		}

		public void Enqueue(int dataValue)
		{
			// Since the start is always a dummy, we do not need to check if it is null.
			// Just make a new node and add it to the end of the list.
			_endNode.next = new Node (dataValue);
			_endNode = _endNode.next;
		}

		public bool Dequeue(ref int out_value)
		{
			// Since the start is always a dummy, check if there is another node after
			if (_startNode.next != null)
			{
				// If there is another node, then move the front node forward one and 
				// set the out value to the start's next (which is the first item in the list).
				out_value = _startNode.next.value;

				// start's next's next is either a node (the case where there are multiple nodes in the list)
				// Or start's next's next is null (based on node constructer) (the case where there was only the one 
				// node in the list. 
				_startNode.next = _startNode.next.next;

				return true;
			}

			// If there is only the start node, no node after, then return false
			// since the start node is always the dummy node. 
			return false;
		}

	}
}