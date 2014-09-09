using NUnit.Framework;
using System;

namespace Try
{
	[TestFixture ()]
	public class MondayTest
	{
		[Test ()]
		public void TestCase ()
		{
			if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
				throw new Exception ("Its monday :(");

		}
	}
}

