using System.Collections;

namespace Tokeiya3.Collections.Generics.RingBuffer;

public abstract class RingBuffer
{
	public const int MaxCapacity = 1073741824;
}

public class RingBuffer<T> : RingBuffer, IList<T>
{
	private readonly T[] _storage;
	private PivotController _pivot;

	public RingBuffer(int capacitySize)
	{
		_pivot = new PivotController(capacitySize);
		_storage = new T[_pivot.Capacity];
	}

	public int Capacity => _storage.Length;

	public IEnumerator<T> GetEnumerator()
	{
		var current = _pivot.Version;

		for (var i = 0; i < Count; i++)
		{
			if (_pivot.Version != current) throw new InvalidOperationException("Collection is modified.");
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}


	public void Add(T item)
	{
		if (Capacity == Count)
		{
			var idx = _pivot.Advance().recent;
			_storage[idx] = item;
		}
		else
		{
			_storage[_pivot.Count] = item;
			_pivot.Advance();
		}
	}

	public void Clear()
	{
		_pivot.Clear();
	}

	public bool Contains(T item)
	{
		for (var i = 0; i < Count; i++)
			if (EqualityComparer<T>.Default.Equals(this[i], item))
				return true;

		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
		if (Count > array.Length - arrayIndex) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

		for (var i = 0; i < Count; i++) array[i + arrayIndex] = this[i];
	}


	public bool Remove(T item)
	{
		var index = IndexOf(item);
		if (index == -1) return false;

		RemoveAt(index);
		return true;
	}

	public int Count => _pivot.Count;


	public bool IsReadOnly => false;


	public int IndexOf(T item)
	{
		for (var i = 0; i < Count; i++)
			if (EqualityComparer<T>.Default.Equals(this[i], item))
				return i;

		return -1;
	}

	public void Insert(int index, T item)
	{
		if (index is 0 && Count < Capacity)
		{
			_pivot.Advance();
			var cnt = Count - 1;

			for (var i = Count - 1; i > 0; i--) this[i] = this[i - 1];
			this[0] = item;
			return;
		}

		if (index >= Count || index < 0) throw new ArgumentOutOfRangeException(nameof(index));


		if (index is 0)
		{
			this[0] = item;
			_pivot.InclementVersion();
		}
		else if (Count < Capacity)
		{
			_pivot.InclementCount();
			_pivot.InclementVersion();

			for (var i = Count - 1; i > index; i--) this[i] = this[i - 1];

			this[index] = item;
		}
		else
		{
			_pivot.Advance();

			for (var i = Count - 1; i > index; i--) this[i] = this[i - 1];

			this[index] = item;
		}
	}

	public void RemoveAt(int index)
	{
		if (index >= Count || index < 0) throw new ArgumentOutOfRangeException(nameof(index));

		var cnt = Count - 1;
		for (var i = index; i < cnt; i++) this[i] = this[i + 1];

		_pivot.Retreat();
	}

	public T this[int index]
	{
		get
		{
			var idx = _pivot.Convert(index);
			return _storage[idx];
		}
		set
		{
			var idx = _pivot.Convert(index);
			_storage[idx] = value;
		}
	}

	public bool AddExchange(T item, out T value)
	{
		if (Capacity == Count)
		{
			var idx = _pivot.Advance().recent;
			value = _storage[idx];
			_storage[idx] = item;
			return true;
		}

		_storage[_pivot.Count] = item;
		_pivot.Advance();
		value = item;
		return false;
	}
}