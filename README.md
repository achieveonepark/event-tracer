# Event Tracer

[![openupm](https://img.shields.io/npm/v/com.achive.event-tracer?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.achive.event-tracer/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.achive.event-tracer)](https://openupm.com/packages/com.achive.event-tracer/)

[한국어](README.ko.md)

A Unity tool for understanding what happened in your game and why. Record events with their data, follow related events, inspect call stacks, and save a session to revisit or share.

## Installation

Requires **Unity 2022.3 or later**. Recording works in the Editor and Development Builds.

### OpenUPM

With the [OpenUPM CLI](https://openupm.com/docs/getting-started-cli.html) installed, run this from your Unity project folder:

```sh
openupm add com.achive.event-tracer
```

To install through the Unity Editor:

1. Open **Edit → Project Settings → Package Manager** and add a scoped registry:

   | Field | Value |
   | --- | --- |
   | Name | `OpenUPM` |
   | URL | `https://package.openupm.com` |
   | Scope | `com.achive.event-tracer` |

2. Click **Apply**, then open **Window → Package Manager**.
3. Choose **+ → Add package by name**, enter `com.achive.event-tracer`, and click **Add**.

See the [OpenUPM installation guide](https://openupm.com/docs/getting-started.html) for more help.

### Git URL

Alternatively, in Package Manager choose **+ → Add package from git URL** and paste:

```text
https://github.com/achieveonepark/event-tracer.git
```

## Quick start

Open **Tools → Achieve → Event Tracer**. Start a session at the beginning of the scenario you want to inspect, then record the actions you care about:

```csharp
using Achieve.EventTracer;

EventTrace.StartSession("Shop test");
var purchase = EventTrace.Record("Shop.Purchase", new { price = 100 });
EventTrace.Record("Wallet.Spend", new { amount = 100 }, parentId: purchase);
```

Call these methods on the Unity main thread. Pass a recorded event's ID as `parentId` to link a later event to its cause. Add `source: this` when recording from a `MonoBehaviour` to let the browser select that object.

Enter Play mode and select an event in the browser. Its details show the recorded data, cause, effects, source object, and calling code. Use search and the category, severity, or source filters to narrow the list.

The UI defaults to English. Use the language menu to switch to **한국어**; your choice is remembered.

## Try the demo

1. Select **Event Tracer** in Package Manager and import **Shop Demo** from its **Samples** section.
2. Run **Tools → Achieve → Samples → Create Shop Trace Demo**.
3. Enter Play mode and click **Buy FireSword** in the Game view.
4. In Event Tracer, select `Shop.Purchase`. Two `Wallet.Spend` events appear as effects of that purchase: the demo intentionally charges twice.
5. Turn off **Intentional duplicate charge** and buy again to compare with a single charge.

Use **Reset shop / new session** in the Game view to start over.

## Browser controls

| Control | Use it to |
| --- | --- |
| **Record** | Turn event recording on or off |
| **Pause view** | Keep the displayed events still while recording continues |
| **New session** | Start a fresh session and empty the event list |
| **Clear** | Empty the list while staying in the same session |
| **Back to live** | Return from an imported capture or paused view |
| **Capture limits → Apply** | Set recording limits and enable caller or stack capture |

## Call stacks

Enable **Capture limits → Capture stack trace**, then click **Apply** before reproducing the issue. Select a new event to read or copy its **CALL STACK**.

Stack recording is off by default because it adds overhead. File names and line numbers depend on available debug symbols. For events separated by async work or coroutines, pass `parentId` to preserve the cause/effect link.

## Save and reopen a session

Click **Export** to save the displayed session as an `.etr` file, including event data, relationships, and recorded call stacks. Export captures you want to keep before starting a new session.

Click **Import** to reopen a capture without changing live recording. You can also select an `.etr` file in the Project window and choose **Open in Event Tracer**. Use **Back to live** to return to the current session.

## Troubleshooting

- **No events?** Check **Record**, clear any search and filters, and confirm the game calls `EventTrace.Record`. Recording runs in the Editor or a Development Build.
- **No call stack?** Enable stack capture, click **Apply**, and reproduce the action again. Existing events keep the information recorded at the time.
- **Missing older events or parents?** The buffer removes old events when its limits are reached. Increase the limits under **Capture limits** or export shorter sessions.
- **Cannot select the source object?** Supply `source` when recording. Selection is available while that object is alive; saved captures keep its name rather than a live reference.

See the [usage and API guide](Documentation~/event-tracer.md) for configuration examples and capture limits.

## License

[MIT](LICENSE.md).
