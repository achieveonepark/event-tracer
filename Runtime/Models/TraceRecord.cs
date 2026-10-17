using System;
using UnityEngine;

namespace Achieve.EventTracer
{
    [Serializable]
    public sealed class TraceRecord
    {
        [SerializeField] private string sessionId;
        [SerializeField] private long sequence;
        [SerializeField] private string parentSessionId;
        [SerializeField] private long parentSequence;
        [SerializeField] private string eventName;
        [SerializeField] private string category;
        [SerializeField] private TraceLevel level;
        [SerializeField] private double elapsedSeconds;
        [SerializeField] private int frame;
        [SerializeField] private string payloadJson;
        [SerializeField] private string sourceName;
        [SerializeField] private string sourceObjectId;
        [SerializeField] private string callerFile;
        [SerializeField] private string callerMember;
        [SerializeField] private int callerLine;
        [SerializeField] private string stackTrace;
        [NonSerialized] private WeakReference<UnityEngine.Object> source;

        public TraceId Id => new TraceId(sessionId, sequence);
        public TraceId ParentId => new TraceId(parentSessionId, parentSequence);
        public string EventName => eventName;
        public string Category => category;
        public TraceLevel Level => level;
        public double ElapsedSeconds => elapsedSeconds;
        public int Frame => frame;
        public string PayloadJson => payloadJson;
        public string SourceName => sourceName;
        public string SourceObjectId => sourceObjectId;
        public string CallerFile => callerFile;
        public string CallerMember => callerMember;
        public int CallerLine => callerLine;
        public string StackTrace => stackTrace ?? string.Empty;
        public UnityEngine.Object SourceObject => source != null && source.TryGetTarget(out var target) && target != null
            ? target : null;

        internal int CharacterCost => (eventName?.Length ?? 0) + (category?.Length ?? 0) +
            (payloadJson?.Length ?? 0) + (sourceName?.Length ?? 0) + (callerFile?.Length ?? 0) +
            (callerMember?.Length ?? 0) + (stackTrace?.Length ?? 0) + 128;

        internal TraceRecord(TraceId id, TraceId parentId, string name, string eventCategory, TraceLevel eventLevel,
            double elapsed, int eventFrame, string payload, UnityEngine.Object sourceObject,
            string file, string member, int line, string callStack = "")
        {
            sessionId = id.SessionId;
            sequence = id.Sequence;
            parentSessionId = parentId.SessionId ?? string.Empty;
            parentSequence = parentId.Sequence;
            eventName = name;
            category = eventCategory;
            level = eventLevel;
            elapsedSeconds = elapsed;
            frame = eventFrame;
            payloadJson = payload;
            sourceName = sourceObject != null ? sourceObject.name : string.Empty;
            sourceObjectId = TraceObjectIdentity.Get(sourceObject);
            source = sourceObject != null ? new WeakReference<UnityEngine.Object>(sourceObject) : null;
            callerFile = file ?? string.Empty;
            callerMember = member ?? string.Empty;
            callerLine = line;
            stackTrace = callStack ?? string.Empty;
        }
    }
}
