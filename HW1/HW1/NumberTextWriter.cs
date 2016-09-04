using System;
using System.IO;

namespace CS422
{
	public class NumberedTextWriter : System.IO.TextWriter
	{
		private int _lineNumber;
		private TextWriter _writer; 

		public NumberedTextWriter(TextWriter wrapThis)
		{
			_lineNumber = 1;
			_writer = wrapThis;
		}

		public NumberedTextWriter(TextWriter wrapThis, int startingLineNumber)
		{
			_lineNumber = startingLineNumber;
			_writer = wrapThis;
		}

		public override void WriteLine (string value)
		{
			string newValue = _lineNumber + ": " + value;

			_writer.WriteLine (newValue);

			_lineNumber++;
		}

		public override System.Text.Encoding Encoding 
		{
			get { return _writer.Encoding; }
		}

	}
}

