using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChainingAssertion;
using Xunit;

namespace Tokeiya3.Collections.Generics.RingBuffer.Tests;

public static class Extensions
{
	public static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> source)
	{
		var idx = -1;

		foreach (var elem in source) yield return (elem, ++idx);
	}

	public static void AreEqual(this RingBuffer<long> source, IEnumerable<long> expected)
	{
		var expectedArray = expected.ToArray();

		source.Count.Is(expectedArray.Length);

		foreach (var (act, i) in source.WithIndex())
		{
			act.Is(expectedArray[i]);
			act.Is(source[i]);
		}
	}

	public static void AreEqual(this RingBuffer<long> source, params long[] expected)
	{
		source.AreEqual((IEnumerable<long>)expected);
	}
}

public class RingBufferTests
{
	[Fact]
	public void MaxCapacityTest()
	{
		RingBuffer.MaxCapacity.Is(1073741824);
	}

	[Fact]
	public void InitTest()
	{
		var target = new RingBuffer<int>(3);
		target.Capacity.Is(4);
		target.Count.Is(0);

		target = new RingBuffer<int>(8);
		target.Capacity.Is(8);
		target.Count.Is(0);
	}

	[Fact]
	public void InvalidInitialTest()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(0));
		Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(-1));

		Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(1073741825));
	}

	[Fact]
	public void GetEnumeratorTest()
	{
		var target = new RingBuffer<long>(4);


		target.AreEqual(Array.Empty<long>());

		var list = new List<long>();

		for (var i = 0; i < 4; i++)
		{
			target.Add(i);
			list.Add(i);
			target.AreEqual(list.ToArray());
		}

		target.Add(4);
		target.AreEqual(1, 2, 3, 4);

		target.Add(5);
		target.AreEqual(2, 3, 4, 5);
	}

	[Fact]
	public void GetNonGenericEnumeratorTest()
	{
		var target = new RingBuffer<long>(4);


		void AreEqual(params long[] expected)
		{
			target.Count.Is(expected.Length);

			var enumerator = ((IEnumerable)target).GetEnumerator();

			var i = -1;

			while (enumerator.MoveNext()) enumerator.Current.Is(expected[++i]);
		}

		AreEqual(Array.Empty<long>());

		var list = new List<long>();

		for (var i = 0; i < 4; i++)
		{
			target.Add(i);
			list.Add(i);
			AreEqual(list.ToArray());
		}

		target.Add(4);
		AreEqual(1, 2, 3, 4);

		target.Add(5);
		AreEqual(2, 3, 4, 5);
	}

	[Fact]
	public void AddTest()
	{
		var target = new RingBuffer<long>(4);
		var list = new LinkedList<long>();

		for (var i = 0; i < 4; i++)
		{
			target.Add(i);
			target.Capacity.Is(4);
			target.Count.Is(i + 1);

			list.AddLast(i);
			target.AreEqual(list);
		}

		target.Add(10);
		target.AreEqual(1, 2, 3, 10);
		target.Capacity.Is(4);
		target.Count.Is(4);

		target.Add(100);
		target.AreEqual(2, 3, 10, 100);
		target.Capacity.Is(4);
		target.Count.Is(4);

		target.Add(1000);
		target.AreEqual(3, 10, 100, 1000);
		target.Capacity.Is(4);
		target.Count.Is(4);

		target.Add(1001);
		target.AreEqual(10, 100, 1000, 1001);
		target.Capacity.Is(4);
		target.Count.Is(4);

		target.Add(1002);
		target.AreEqual(100, 1000, 1001, 1002);
		target.Capacity.Is(4);
		target.Count.Is(4);
	}

	[Fact]
	public void AddExchangeTest()
	{
		var target = new RingBuffer<long>(4);

		for (var i = 0; i < 4; i++)
		{
			target.AddExchange(i, out var v).IsFalse();
			v.Is(i);
		}

		for (var i = 4; i < 100; i++)
		{
			target.AddExchange(i, out var v).IsTrue();
			v.Is(i - 4);
		}
	}

	[Fact]
	public void ClearTest()
	{
		var target = new RingBuffer<long>(4);

		for (var i = 0; i < 4; i++) target.Add(i);

		target.AreEqual(0, 1, 2, 3);

		target.Clear();
		target.Count.Is(0);
		target.Capacity.Is(4);
	}

	[Fact]
	public void ContainsTest()
	{
		var target = new RingBuffer<long>(4);

		for (var i = 0; i < 4; i++)
		{
			target.Contains(i).IsFalse();
			target.Add(i);
			target.Contains(i).IsTrue();
		}

		target.Contains(100).IsFalse();
	}

	[Fact]
	public void CopyToTest()
	{
		var target = new RingBuffer<long>(4);


		for (var i = 0; i < 4; i++) target.Add(i + 1);

		var array = new long[5];

		void AreEqual(params long[] expected)
		{
			expected.Length.Is(5);

			for (var i = 0; i < 5; i++) array[i].Is(expected[i]);
		}

		target.CopyTo(array, 0);

		AreEqual(1, 2, 3, 4, 0);

		for (var i = 0; i < array.Length; i++) array[i] = 0;

		target.CopyTo(array, 1);
		AreEqual(0, 1, 2, 3, 4);
	}

	[Fact]
	public void RemoveTest()
	{
		var target = new RingBuffer<long>(4);

		for (var i = 0; i < 4; i++) target.Add(i + 1);

		target.Remove(1).IsTrue();
		target[0].Is(2);
		target.Count.Is(3);

		target.Remove(2).IsTrue();
		target[1].Is(4);
		target.Count.Is(2);

		target.Remove(0).IsFalse();
	}

	[Fact]
	public void RemoveAtTest()
	{
		var target = new RingBuffer<long>(4);
		Assert.Throws<ArgumentOutOfRangeException>(() => target.RemoveAt(0));
		Assert.Throws<ArgumentOutOfRangeException>(() => target.RemoveAt(-1));

		for (var i = 0; i < 4; i++) target.Add(i + 1);

		target.RemoveAt(1);
		target.RemoveAt(1);
		target.AreEqual(1, 4);

		target.Add(10);
		target.AreEqual(1, 4, 10);

		target.RemoveAt(2);
		target.AreEqual(1, 4);

		Assert.Throws<ArgumentOutOfRangeException>(() => target.RemoveAt(3));
		Assert.Throws<ArgumentOutOfRangeException>(() => target.RemoveAt(-1));
	}


	[Fact]
	public void CountTest()
	{
		var target = new RingBuffer<long>(4);
		target.Count.Is(0);

		for (var i = 0; i < 4; i++)
		{
			target.Count.Is(i);
			target.Add(i);
			target.Count.Is(i + 1);
		}

		for (var i = 0; i < 4; i++)
		{
			target.Count.Is(4);
			target.Add(i);
		}
	}

	[Fact]
	public void IsReadOnlyTest()
	{
		var target = new RingBuffer<long>(4);
		target.IsReadOnly.IsFalse();
	}

	[Fact]
	public void IndexOfTest()
	{
		var target = new RingBuffer<long>(4);

		for (var i = 0; i < 4; i++) target.Add(i + 1);

		target.IndexOf(0).Is(-1);

		for (var i = 0; i < 4; i++) target.IndexOf(i + 1).Is(i);
	}

	[Fact]
	public void InsertTest()
	{
		var target = new RingBuffer<long>(4);

		Assert.Throws<ArgumentOutOfRangeException>(() => target.Insert(1, 42));
		target.Count.Is(0);

		Assert.Throws<ArgumentOutOfRangeException>(() => target.Insert(-1, 42));
		target.Count.Is(0);

		for (var i = 0; i < 4; i++) target.Add(i);

		target.Insert(1, 42);
		target.AreEqual(1, 42, 2, 3);

		target.Insert(0, 43);
		target.AreEqual(43, 42, 2, 3);

		target.Insert(2, 44);
		target.AreEqual(42, 2, 44, 3);
	}

	[Fact]
	public void NonFilledInsertTest()
	{
		var target = new RingBuffer<long>(4);
		target.Insert(0, 42);
		target.Count.Is(1);
		target[0].Is(42);

		target.Insert(0, 1);
		target.Count.Is(2);
		target.AreEqual(1, 42);

		target.Insert(1, 10);
		target.Count.Is(3);
		target.AreEqual(1, 10, 42);

		target.Insert(2, 100);
		target.Count.Is(4);
		target.AreEqual(1, 10, 100, 42);

		target = new RingBuffer<long>(4);
		for (var i = 0; i < 3; i++) target.Add(i);

		target.Insert(0, 42);
		target.AreEqual(42, 0, 1, 2);

		target = new RingBuffer<long>(4);
		for (var i = 0; i < 3; i++) target.Add(i);

		target.Insert(1, 42);
		target.AreEqual(0, 42, 1, 2);
	}

	[Fact]
	public void InsertAndRemoveTest()
	{
		var target = new RingBuffer<long>(4);

		for (var i = 0; i < 4; i++) target.Add(i);

		target.RemoveAt(1);
		target.Insert(0, 42);

		target.AreEqual(42, 0, 2, 3);
	}

	[Fact]
	public void IndexerTest()
	{
		var target = new RingBuffer<long>(4);

		Assert.Throws<IndexOutOfRangeException>(() => target[0] = 100);

		for (var i = 0; i < 4; i++) target.Add(i);

		for (var i = 0; i < 4; i++)
		{
			target[i].Is(i);
			target[i] += 10;
			target[i].Is(i + 10);
		}

		for (var i = 0; i < 4; i++) target.Add(i + 10);

		for (var i = 0; i < 4; i++)
		{
			target[i].Is(i + 10);
			target[i] += 10;
			target[i].Is(i + 20);
		}
	}
}