using System;

namespace Achieve.EventTracer
{
    internal sealed class TraceBuffer
    {
        private TraceRecord[] records;
        private int head;
        private int characterBudget;
        public int Count { get; private set; }
        public long DroppedCount { get; private set; }
        public int CharacterCount { get; private set; }

        public TraceBuffer(int capacity, int budget)
        {
            records = new TraceRecord[capacity];
            characterBudget = budget;
        }

        public void Add(TraceRecord record)
        {
            if (record.CharacterCost > characterBudget) { DroppedCount++; return; }
            while (Count > 0 && (Count == records.Length || CharacterCount + record.CharacterCost > characterBudget))
                DropOldest();
            records[(head + Count) % records.Length] = record;
            Count++;
            CharacterCount += record.CharacterCost;
        }

        public TraceRecord[] Snapshot()
        {
            var result = new TraceRecord[Count];
            for (var i = 0; i < Count; i++) result[i] = records[(head + i) % records.Length];
            return result;
        }

        public void Resize(int capacity, int budget)
        {
            var retained = Snapshot();
            var previousDropped = DroppedCount;
            records = new TraceRecord[capacity];
            characterBudget = budget;
            head = Count = CharacterCount = 0;
            DroppedCount = previousDropped;
            foreach (var record in retained) Add(record);
        }

        public void Clear()
        {
            Array.Clear(records, 0, records.Length);
            head = Count = CharacterCount = 0;
            DroppedCount = 0;
        }

        private void DropOldest()
        {
            CharacterCount -= records[head].CharacterCost;
            records[head] = null;
            head = (head + 1) % records.Length;
            Count--;
            DroppedCount++;
        }
    }
}
