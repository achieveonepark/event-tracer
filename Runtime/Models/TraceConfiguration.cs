using System;

namespace Achieve.EventTracer
{
    [Serializable]
    public sealed class TraceConfiguration
    {
        public bool enabled = true;
        public bool captureCaller = true;
        public bool captureStackTrace = false;
        public int maxEvents = 10000;
        public int maxPayloadCharacters = 4096;
        public int maxStackTraceCharacters = 8192;
        public int maxBufferCharacters = 4 * 1024 * 1024;

        public TraceConfiguration Copy() => (TraceConfiguration)MemberwiseClone();

        public void Validate()
        {
            if (maxEvents < 1 || maxEvents > 100000)
                throw new ArgumentOutOfRangeException(nameof(maxEvents), "Event limit must be 1–100,000.");
            if (maxPayloadCharacters < 128 || maxPayloadCharacters > 65536)
                throw new ArgumentOutOfRangeException(nameof(maxPayloadCharacters), "Payload limit must be 128–65,536 characters.");
            if (maxStackTraceCharacters < 128 || maxStackTraceCharacters > 65536)
                throw new ArgumentOutOfRangeException(nameof(maxStackTraceCharacters), "Stack trace limit must be 128–65,536 characters.");
            if (maxBufferCharacters < maxPayloadCharacters || maxBufferCharacters > 64 * 1024 * 1024)
                throw new ArgumentOutOfRangeException(nameof(maxBufferCharacters), "Buffer budget must fit one payload and be at most 64M characters.");
        }
    }
}
