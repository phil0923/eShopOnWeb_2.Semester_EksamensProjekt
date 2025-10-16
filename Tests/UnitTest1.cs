using FluentAssertions;

namespace Tests
{
	public class UnitTest1
	{
		[Fact]
		public void Test1()
		{
			var a = 5;
			var b = 3;

			var result = a + b;


			result.Should().Be(8);


		}
	}
}
