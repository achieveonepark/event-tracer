using System;

namespace Achieve.EventTracer
{
    public readonly struct TraceId : IEquatable<TraceId>
    {
        public string SessionId { get; }
        public long Sequence { get; }
        public bool IsValid => !string.IsNullOrEmpty(SessionId) && Sequence > 0;

        public TraceId(string sessionId, long sequence)
        {
            SessionId = sessionId;
            Sequence = sequence;
        }

        public bool Equals(TraceId other) => SessionId == other.SessionId && Sequence == other.Sequence;
        public override bool Equals(object obj) => obj is TraceId other && Equals(other);
        public override int GetHashCode() => unchecked(((SessionId?.GetHashCode() ?? 0) * 397) ^ Sequence.GetHashCode());
        public override string ToString() => IsValid ? $"{SessionId}:{Sequence}" : "none";
        public static bool operator ==(TraceId left, TraceId right) => left.Equals(right);
        public static bool operator !=(TraceId left, TraceId right) => !left.Equals(right);
    }

    public enum TraceLevel { Info, Warning, Error }
}
