using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Achieve.EventTracer
{
    public static class EventTrace
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public const bool IsSupported = true;
#else
        public const bool IsSupported = false;
#endif
        private static int mainThreadId = Environment.CurrentManagedThreadId;
        private static TraceConfiguration configuration = new TraceConfiguration();
        private static TraceBuffer buffer = new TraceBuffer(configuration.maxEvents, configuration.maxBufferCharacters);
        private static TraceSession session = new TraceSession("Editor session", "{}");
        private static long nextSequence;
        private static long startedTimestamp = Stopwatch.GetTimestamp();
        private static bool capturingPayload;
        public static bool IsEnabled => IsSupported && configuration.enabled;
        public static TraceConfiguration Configuration => configuration.Copy();
        public static TraceSession Session => session;
        public static int Count => buffer.Count;
        public static long DroppedCount => buffer.DroppedCount;
        public static int BufferedCharacters => buffer.CharacterCount;
        public static long Version { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntime()
        {
            mainThreadId = Environment.CurrentManagedThreadId;
            StartSession("Play session");
        }

        public static void Configure(TraceConfiguration options)
        {
            AssertMainThread();
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();
            configuration = options.Copy();
            buffer.Resize(configuration.maxEvents, configuration.maxBufferCharacters);
            Version++;
        }

        public static TraceSession StartSession(string name, object metadata = null)
        {
            AssertMainThread();
            session = new TraceSession(Limit(string.IsNullOrWhiteSpace(name) ? "Trace session" : name, 256),
                PayloadSnapshot.Serialize(metadata, configuration.maxPayloadCharacters));
            nextSequence = 0;
            startedTimestamp = Stopwatch.GetTimestamp();
            buffer.Clear();
            Version++;
            return session;
        }

        /// <summary>Records on the Unity main thread. parentId is explicitly propagated by the caller.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TraceId Record(string eventName, object data = null, UnityEngine.Object source = null,
            TraceId parentId = default, string category = null, TraceLevel level = TraceLevel.Info,
            [CallerFilePath] string callerFile = "", [CallerMemberName] string callerMember = "",
            [CallerLineNumber] int callerLine = 0)
        {
            if (!IsEnabled) return default;
            AssertMainThread();
            if (capturingPayload) return default; // A DTO getter must not recursively trace itself.
            // Capture before reading payload getters; omit this recorder's own frame.
            var stackTrace = configuration.captureStackTrace
                ? Limit(new StackTrace(1, true).ToString(), configuration.maxStackTraceCharacters)
                : string.Empty;
            var name = Limit(string.IsNullOrWhiteSpace(eventName) ? "Unnamed event" : eventName, 256);
            var eventCategory = category;
            if (string.IsNullOrWhiteSpace(eventCategory))
            {
                var dot = name.IndexOf('.');
                eventCategory = dot > 0 ? name.Substring(0, dot) : "General";
            }
            string payload;
            capturingPayload = true;
            try { payload = PayloadSnapshot.Serialize(data, configuration.maxPayloadCharacters); }
            finally { capturingPayload = false; }
            var id = new TraceId(session.Id, ++nextSequence);
            if (parentId.SessionId == id.SessionId && parentId.Sequence >= id.Sequence) parentId = default;
            var record = new TraceRecord(id, parentId, name, Limit(eventCategory, 128), level,
                (Stopwatch.GetTimestamp() - startedTimestamp) / (double)Stopwatch.Frequency,
                Time.frameCount, payload, source,
                configuration.captureCaller ? Limit(callerFile, 1024) : string.Empty,
                configuration.captureCaller ? Limit(callerMember, 256) : string.Empty,
                configuration.captureCaller ? callerLine : 0, stackTrace);
            buffer.Add(record);
            Version++;
            return id;
        }

        public static TraceRecord[] GetSnapshot()
        {
            AssertMainThread();
            return buffer.Snapshot();
        }

        public static TraceCapture Capture()
        {
            AssertMainThread();
            return new TraceCapture(session, buffer.Snapshot(), buffer.DroppedCount);
        }

        public static void Clear()
        {
            AssertMainThread();
            buffer.Clear();
            // Keep sequence IDs unique within the session, even after Clear.
            Version++;
        }

        private static void AssertMainThread()
        {
            if (Environment.CurrentManagedThreadId != mainThreadId)
                throw new InvalidOperationException("EventTrace must be called on the Unity main thread. Queue worker events to the main thread.");
        }

        private static string Limit(string value, int max) => string.IsNullOrEmpty(value)
            ? string.Empty : value.Length <= max ? value : value.Substring(0, max);
    }
}
