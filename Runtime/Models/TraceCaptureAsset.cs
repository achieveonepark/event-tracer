using System;
using UnityEngine;

namespace Achieve.EventTracer
{
    /// <summary>The imported main object of an .etr capture file.</summary>
    public sealed class TraceCaptureAsset : ScriptableObject
    {
        [SerializeField] private TraceCapture capture;
        public TraceCapture Capture => capture;
        public void Initialize(TraceCapture value) => capture = value ?? throw new ArgumentNullException(nameof(value));
    }
}
