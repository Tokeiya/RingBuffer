using System;
using ChainingAssertion;
using Xunit;

namespace Tokeiya3.Collections.Generics.RingBuffer.Tests;

public class PivotControllerTests
{
	[Fact]
	public void InitTest()
	{
		var target = new PivotController(4);
		target.Capacity.Is(4);
		target.Count.Is(0);
		target.Version.Is(0);

		target = new PivotController(100);
		target.Capacity.Is(128);
		target.Count.Is(0);
		target.Version.Is(0);
	}

	[Fact]
	public void InvalidInitTest()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new PivotController(0));
		Assert.Throws<ArgumentOutOfRangeException>(() => new PivotController(-1));
		Assert.Throws<ArgumentOutOfRangeException>(() => new PivotController(RingBuffer.MaxCapacity + 1));
	}

	[Fact]
	public void AdvanceTest()
	{
		var target = new PivotController(4);

		for (var i = 0; i < 4; i++)
		{
			target.Count.Is(i);
			target.Version.Is(i);

			var (current, _) = target.Advance();
			current.Is(0);

			target.Count.Is(i + 1);
			target.Version.Is(i + 1);
		}

		target.Count.Is(4);

		for (var i = 0; i < 10; i++)
		{
			target.Count.Is(4);
			target.Version.Is(4 + i);

			var (current, recent) = target.Advance();
			current.Is((i + 1) & 3);
			recent.Is(i & 3);

			target.Count.Is(4);
			target.Version.Is(5 + i);
		}
	}

	[Fact]
	public void ClearTest()
	{
		var target = new PivotController(4);

		for (var i = 0; i < 10; i++) target.Advance();

		target.Count.Is(4);
		target.Version.Is(10);

		target.Clear();
		target.Count.Is(0);
		target.Version.Is(11);
	}

	[Fact]
	public void ConvertTest()
	{
		var target = new PivotController(4);

		void AreEqual(params int[] expected)
		{
			for (var i = 0; i < expected.Length; i++) target.Convert(i).Is(expected[i]);
		}

		target.Advance();
		AreEqual(0);

		target.Advance();
		AreEqual(0, 1);

		target.Advance();
		AreEqual(0, 1, 2);

		target.Advance();
		AreEqual(0, 1, 2, 3);

		target.Advance();
		AreEqual(1, 2, 3, 0);

		target.Advance();
		AreEqual(2, 3, 0, 1);

		target.Advance();
		AreEqual(3, 0, 1, 2);

		target.Advance();
		AreEqual(0, 1, 2, 3);
	}

	[Fact]
	public void InvalidConvertTest()
	{
		var target = new PivotController(4);
		Assert.Throws<IndexOutOfRangeException>(() => target.Convert(0));
		Assert.Throws<IndexOutOfRangeException>(() => target.Convert(-1));

		target.Advance();
		Assert.Throws<IndexOutOfRangeException>(() => target.Convert(1));

		target.Advance();
		Assert.Throws<IndexOutOfRangeException>(() => target.Convert(2));

		target.Advance();
		Assert.Throws<IndexOutOfRangeException>(() => target.Convert(3));

		target.Advance();
		Assert.Throws<IndexOutOfRangeException>(() => target.Convert(4));

		target.Advance();
		Assert.Throws<IndexOutOfRangeException>(() => target.Convert(4));
	}
}