using System;
using UnityEngine;

namespace Achieve.EventTracer
{
    [Serializable]
    public sealed class TraceSession
    {
        [SerializeField] private string id;
        [SerializeField] private string name;
        [SerializeField] private string startedUtc;
        [SerializeField] private string metadataJson;
        public string Id => id;
        public string Name => name;
        public string StartedUtc => startedUtc;
        public string MetadataJson => metadataJson;

        internal TraceSession(string sessionName, string metadata)
        {
            id = Guid.NewGuid().ToString("N");
            name = sessionName;
            startedUtc = DateTime.UtcNow.ToString("O");
            metadataJson = metadata;
        }
    }
}
