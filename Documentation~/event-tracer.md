# Event Tracer API

[한국어](event-tracer.ko.md) · [README](../README.md)

## Editor browser

Open **Tools → Achieve → Event Tracer** to inspect recorded events.

- Search event names and payloads; filter by category, severity, and source name.
- Browse a virtualized list with elapsed time, frame number, and cause/effect indentation.
- Navigate to causes and effects, inspect payload snapshots and call stacks, select living source objects, and open the calling code.
- **Record** enables or disables collection. **Pause view** freezes the view while collection continues.
- **New session** starts a new session and clears the buffer. **Clear** empties the buffer while preserving the session and sequence counter.
- **Export / Import** saves or reads `.etr` JSON captures. Older `.json` captures are also supported. Importing never replaces the live recorder.
- **Capture limits** sets event count, payload size, buffer character budget, caller location capture, stack capture, and per-stack character limit. Click **Apply** to save changes.

Selecting an `.etr` file in `Assets` shows a read-only UI Toolkit inspector. Choose **Open in Event Tracer** to browse the entire capture. The **한국어 / English** preference is shared with Play Bookmarks; event names, source names, payloads, search inputs, and call stacks are preserved as recorded.

Import **Shop Demo** from Package Manager and run **Tools → Achieve → Samples → Create Shop Trace Demo**. The intentional duplicate gold charge appears as two effects of the same purchase event.

## Recording

```csharp
TraceId Record(string eventName, object data = null, UnityEngine.Object source = null,
    TraceId parentId = default, string category = null, TraceLevel level = TraceLevel.Info,
    string callerFile = "", string callerMember = "", int callerLine = 0);
```

C# caller attributes supply the calling file, member, and line automatically. The default category is the portion of the event name before its first `.`; names without a separator use `General`.

`TraceRecord` exposes read-only properties. `GetSnapshot()` returns an array in buffer order. Source objects are held through weak references; destroyed objects and imported captures cannot be selected. Runtime object IDs are not persistent identifiers across sessions.

| API | Behavior |
| --- | --- |
| `EventTrace.Configure(options)` | Validates and copies settings; resizing retains the newest records |
| `EventTrace.StartSession(name, metadata)` | Creates a session ID, snapshots metadata, and clears the buffer |
| `EventTrace.Record(...)` | Stores a payload snapshot, caller location, explicit parent ID, and optional call stack |
| `EventTrace.GetSnapshot()` | Returns the current immutable records in a new array |
| `EventTrace.Capture()` | Returns a detached capture of the session, records, and dropped count |
| `EventTrace.Clear()` | Clears the buffer without resetting the session ID or sequence counter |
| `TraceCapture.ToJson()` | Exports format version 1 JSON |
| `TraceCapture.FromJson(json)` | Validates the format, IDs, sequence, and parent links, then returns a detached capture |

Recording, configuration, and capture operations require the Unity main thread. Editor settings are stored in EditorPrefs and reapplied when entering Play mode.

Collection works in the Editor and Development Builds. Regular release builds return an invalid `TraceId`. Guard expensive DTO creation with `if (EventTrace.IsEnabled)` because arguments are evaluated before calling `Record`. Queue worker-thread data to your game's main thread.

Add recording points to game code or event-bus adapters explicitly. The package does not intercept every C# event. Pass the returned session-aware `TraceId` as `parentId` to asynchronous and coroutine follow-up work. Causality is never inferred from timestamps alone.

## Recording-time call stacks

Caller location and call stack capture are independent. Caller location is enabled by default and stores one file/member/line. Stack capture is disabled by default because walking and formatting the stack costs time and allocations.

```csharp
var options = EventTrace.Configuration;
options.captureStackTrace = true;
options.maxStackTraceCharacters = 8192;
EventTrace.Configure(options);

EventTrace.Record("Combat.Hit", new { damage = 25 });
string callStack = EventTrace.GetSnapshot()[0].StackTrace;
```

The stack is captured inside `Record`, before payload getters are evaluated, and omits the recorder's own frame. It is a snapshot of the synchronous managed call chain at that moment. File names and line numbers are included when debug symbols are available; optimized or IL2CPP builds can provide less detail. Async/coroutine continuations do not recreate earlier execution stacks. Continue passing `parentId` to connect related work.

`maxStackTraceCharacters` defaults to 8,192 and accepts 128–65,536. Longer stacks are truncated. Stored stack characters count toward `maxBufferCharacters`; a record larger than the entire buffer budget is dropped. The limit bounds retained stack text, not the temporary allocation used to build the stack string.

The browser's **CALL STACK** field is read-only and supports copying text. Enabling stack capture only affects future events. Events recorded with capture disabled, and older captures without a `stackTrace` field, expose `StackTrace` as an empty string.

## Capacity and snapshots

| Setting | Default | Allowed values |
| --- | --- | --- |
| `enabled` | `true` | Enable or disable recording |
| `captureCaller` | `true` | Capture caller file/member/line |
| `captureStackTrace` | `false` | Capture recording-time call stacks |
| `maxEvents` | `10000` | 1–100,000 |
| `maxPayloadCharacters` | `4096` | 128–65,536 |
| `maxStackTraceCharacters` | `8192` | 128–65,536 |
| `maxBufferCharacters` | `4 * 1024 * 1024` | At least one maximum-sized payload, up to 64M characters |

The buffer evicts the oldest records when either the count or character budget is exceeded. `DroppedCount` includes records discarded by either limit. The character budget limits managed string lengths rather than exact process memory. Record objects, arrays, rendering, stack formatting, and JSON export need additional memory.

Payload snapshots copy public fields and readable public properties, with a default depth limit of 4 and at most 32 collection/object entries. Unity objects store only name, ID, and type. Oversized payloads use valid JSON: `{"$truncated":true}`.

Strings are JSON-escaped; numbers use invariant culture; enums use names; DateTime uses ISO format. Cycles become `<cycle>`, excessive depth becomes `<max-depth>`, and unreadable properties become `<unreadable: ...>`. Recursive `Record` calls from payload getters are ignored. Getter execution time is not bounded, so use small data objects.

If a parent is evicted or belongs to another session, the UI indicates this rather than linking to an unrelated event with the same sequence number.

## Storage

```csharp
var capture = EventTrace.Capture();
var json = capture.ToJson();
var restored = TraceCapture.FromJson(json);
```

JSON contains session ID/name/start UTC/metadata; event and parent IDs; elapsed time/frame; category/severity; payload; source name/object ID; caller location; and the optional stack trace. Unity object references are not exported. Adding the optional stack field preserves format version 1 compatibility: older files still import.

The Editor uses `.etr` by default. `TraceCaptureImporter` imports files in `Assets` as `TraceCaptureAsset` objects. Its inspector displays a summary and preview and can open the full capture in the browser. Runtime `ToJson`/`FromJson` methods do not depend on the file extension.

Imports accept at most 16M characters and 100,000 records; the browser also checks a 16MB file-size limit before reading. Large exports may exceed these limits, so reduce capture limits or split captures.

## Testing

Install Unity Test Framework in a consuming project and add `"testables": ["com.achive.event-tracer"]` to `Packages/manifest.json`. Run the package's EditMode tests in Test Runner. The runtime package has no dependency on Test Framework or Play Bookmarks.
