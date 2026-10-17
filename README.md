# Event Tracer

[한국어](README.ko.md)

A Unity package for recording gameplay events, payload snapshots, explicit cause/effect relationships, and optional call stacks. Browse events in a UI Toolkit window and save sessions as `.etr` captures.

## Installation

Requires **Unity 2022.3 or later**. In Package Manager, choose **+ → Add package from git URL**:

```text
https://github.com/achieveonepark/event-tracer.git
```

The package ID is `com.achive.event-tracer` (the `achive` spelling is intentional). For OpenUPM registration and installation instructions, see [Publishing to OpenUPM](Documentation~/publishing.md).

## Usage

Open **Tools → Achieve → Event Tracer** and add recording points to your game code:

```csharp
using Achieve.EventTracer;

EventTrace.StartSession("Shop test");
var purchase = EventTrace.Record("Shop.Purchase", new { price = 100 });
EventTrace.Record("Wallet.Spend", new { amount = 100 }, parentId: purchase);
```

Call the API on the Unity main thread. Recording works in the Editor and Development Builds. The browser supports search, filters, cause/effect navigation, export/import, and **English / 한국어** display languages. The UI defaults to English and remembers your language selection.

## Call stacks

Enable **Capture limits → Capture stack trace → Apply** to capture the call stack when each new event is recorded. Select an event to view and copy its **CALL STACK**; stacks are preserved in exported `.etr` files. You can also configure this in code:

```csharp
var options = EventTrace.Configuration;
options.captureStackTrace = true;
options.maxStackTraceCharacters = 8192;
EventTrace.Configure(options);
```

Stack capture is off by default because it adds recording overhead. Each stack is truncated to the configured character limit and counts toward the buffer budget. File names and line numbers depend on available debug symbols. Async and coroutine continuations show their current execution stack; use `parentId` to connect events across those boundaries.

Import **Shop Demo** from Package Manager, then run **Tools → Achieve → Samples → Create Shop Trace Demo** to try an intentional duplicate charge.

See the [API and capture guide](Documentation~/event-tracer.md) for configuration, storage limits, and testing.

## AI skill

The skill source is in [`Skills~/achieve-event-tracer`](Skills~/achieve-event-tracer/SKILL.md). Unity Skill Manager collects it into the project's `.agents/skills` and `.claude/skills`. Invoke it with `$achieve-event-tracer` in Codex or `/achieve-event-tracer` in Claude Code.

## License

[MIT](LICENSE.md).
