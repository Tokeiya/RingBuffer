namespace Tokeiya3.Collections.Generics.RingBuffer;

internal struct PivotController
{
	private static readonly int[] SizeTable =
	{
		1,
		2,
		4,
		8,
		16,
		32,
		64,
		128,
		256,
		512,
		1024,
		2048,
		4096,
		8192,
		16384,
		32768,
		65536,
		131072,
		262144,
		524288,
		1048576,
		2097152,
		4194304,
		8388608,
		16777216,
		33554432,
		67108864,
		134217728,
		268435456,
		536870912,
		1073741824
	};

	private static int AdjustCapacitySize(int capacitySize)
	{
		if (capacitySize <= 0)
			throw new ArgumentOutOfRangeException($"{nameof(capacitySize)} can't accept less than 1.");

		if (capacitySize > RingBuffer.MaxCapacity)
			throw new ArgumentOutOfRangeException($"{nameof(capacitySize)} can't accept greater than 1073741824.");

		foreach (var candidate in SizeTable)
			if (candidate >= capacitySize)
				return candidate;

		throw new InvalidOperationException("Detect inconsistency.");
	}

	private readonly int _mask;
	private int _pivot;

	public PivotController(int capacitySize)
	{
		Capacity = AdjustCapacitySize(capacitySize);
		_pivot = 0;
		Count = 0;
		Version = 0;
		_mask = Capacity - 1;
	}

	public int Capacity { get; }
	public int Count { get; private set; }

	public (int current, int recent) Advance()
	{
		++Version;
		var recent = _pivot;

		if (Capacity == Count)
		{
			var tmp = _pivot + 1;
			tmp &= _mask;
			_pivot = tmp;
		}
		else
		{
			++Count;
		}

		return (_pivot, recent);
	}

	public void Retreat()
	{
		++Version;
		--Count;
	}

	public void InclementCount()
	{
		++Count;
	}

	public void InclementVersion()
	{
		++Version;
	}

	public int Version { get; private set; }

	public void Clear()
	{
		++Version;
		_pivot = 0;
		Count = 0;
	}

	public int Convert(int index)
	{
		if (index < 0) throw new IndexOutOfRangeException("Index is negative.");
		if (index >= Count) throw new IndexOutOfRangeException("Index is over.");

		return (index + _pivot) & _mask;
	}
}