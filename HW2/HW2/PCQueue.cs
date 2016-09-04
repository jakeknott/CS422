using System;

namespace CS422
{
	public class Node
	{
		public int value; 
		public Node next;
	}

	public class PCQueue
	{
		private Node _dummyNode = new Node();
		Node _statNode = new Node();
		Node _endNode = new Node();

		public PCQueue ()
		{
			_statNode = _dummyNode;
			_endNode = _dummyNode;
		}

		public void Enqueue(int dataValue)
		{
			_endNode.next = newNode (dataValue);
			_endNode = _endNode.next;
		}

		public bool Dequeue(ref int out_value)
		{
			// front is never null since it will be a dummy node
			//if (object.ReferenceEquals(_statNode, _dummyNode))
			//{
				
				//if (front has a next (not null))
				//{
				//	Move front ahead
				//	Front = front.next;
				// decide if the old front is the back, then we need to set it to the 	dummy
				//}

			//}/

			if (!object.ReferenceEquals(_statNode, _endNode))
			{
				//
			}
			else 
			{
				
				// more than one item
				//Object o = front.data;
				//front = front.next;
				//Out_val = o;
				//Return true;
			}

			return false;
		}

		private Node newNode (int val)
		{
			Node returnNode = new Node ();
			returnNode.value = val;
			return returnNode;
		}
	}
}

