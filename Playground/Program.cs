using Tokeiya3.Collections.Generics.RingBuffer;
using Tokeiya3.Collections.Generics.RingBuffer.Tests;

var target = new RingBuffer<long>(4);

void Dump()
{
	for (var i = 0; i < target.Count; i++) Console.WriteLine($"[{i}]={target[i]}");

	Console.WriteLine();
	Console.WriteLine();
}


target = new RingBuffer<long>(4);

for (var i = 0; i < 5; i++) target.Add(i);

Dump();

target.Insert(1, 42);
Dump();
target.AreEqual(2, 42, 3, 4);

target.Insert(0, 43);
Dump();
target.AreEqual(43, 42, 3, 4);

target.Insert(2, 44);
Dump();
target.AreEqual(42, 2, 44, 3);