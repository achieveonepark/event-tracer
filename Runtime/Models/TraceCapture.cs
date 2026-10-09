using System;
using System.Collections.Generic;
using UnityEngine;

namespace Achieve.EventTracer
{
    [Serializable]
    public sealed class TraceCapture
    {
        [SerializeField] private int formatVersion = 1;
        [SerializeField] private TraceSession session;
        [SerializeField] private TraceRecord[] records;
        [SerializeField] private long droppedCount;
        public TraceSession Session => session;
        public IReadOnlyList<TraceRecord> Records => Array.AsReadOnly(records);
        public long DroppedCount => droppedCount;

        internal TraceCapture(TraceSession traceSession, TraceRecord[] snapshot, long dropped)
        {
            session = traceSession;
            records = snapshot;
            droppedCount = dropped;
        }

        public string ToJson(bool prettyPrint = true) => JsonUtility.ToJson(this, prettyPrint);

        /// <summary>Returns a detached capture; importing never changes the live recorder.</summary>
        public static TraceCapture FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Length > 16 * 1024 * 1024)
                throw new ArgumentException("Capture is empty or exceeds 16M characters.", nameof(json));
            TraceCapture capture;
            try { capture = JsonUtility.FromJson<TraceCapture>(json); }
            catch (Exception exception) { throw new ArgumentException("Capture JSON is invalid.", nameof(json), exception); }
            if (capture == null || capture.formatVersion != 1 || capture.session == null ||
                string.IsNullOrEmpty(capture.session.Id) || capture.records == null ||
                capture.records.Length > 100000 || capture.droppedCount < 0)
                throw new ArgumentException("Unsupported or incomplete capture.", nameof(json));

            long previous = 0;
            foreach (var record in capture.records)
            {
                if (record == null || record.Id.SessionId != capture.session.Id || record.Id.Sequence <= previous ||
                    string.IsNullOrWhiteSpace(record.EventName) || string.IsNullOrEmpty(record.PayloadJson) ||
                    record.ElapsedSeconds < 0 || double.IsNaN(record.ElapsedSeconds) || double.IsInfinity(record.ElapsedSeconds) ||
                    !Enum.IsDefined(typeof(TraceLevel), record.Level) || record.ParentId.Sequence < 0 ||
                    record.ParentId.SessionId == record.Id.SessionId && record.ParentId.Sequence >= record.Id.Sequence)
                    throw new ArgumentException("Capture contains invalid event records.", nameof(json));
                previous = record.Id.Sequence;
            }
            return capture;
        }
    }
}
