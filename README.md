## AceLand Event Driven

[![Open Runtime, Paid Editor Tools](https://img.shields.io/badge/Open%20Runtime,%20Paid%20Editor%20Tools-C24B42)](https://docs.parsue.io/aceland-unity-packages/open-and-paid/open-core-and-paid-editor-tools) [![CoreCLR Ready](https://img.shields.io/badge/CoreCLR-Ready-4A5BC2)]()   
[![Sponsor](https://img.shields.io/badge/Sponsor-%E2%9D%A4-db61a2?logo=githubsponsors&logoColor=white)](https://github.com/sponsors/parsue) [![Discord](https://img.shields.io/badge/Discord-Join-5865F2?logo=discord&logoColor=white)](https://discord.gg/XsCYGnYzuc) [![Docs](https://img.shields.io/badge/Docs-GitBook-3884FF?logo=gitbook&logoColor=white)](https://docs.parsue.io/aceland-unity-packages/core-packages/event-driven)

Type-safe EventBus + reactive Signals for modern Unity. **Talk in types, not in strings.**

AceLand Event Driven is a **zero-dependency Core package** that gives you two independent,
decoupled messaging tools:

- **Event Bus** — a strongly-typed, global publish/subscribe bus where **an event is just an
  interface**. Registration is produced by a **Roslyn Source Generator at compile time**, so
  startup does zero reflection and zero assembly scanning — friendly to Unity 7 / CoreCLR and
  the "no domain reload" workflow.
- **Event Signal** — lightweight, id-based reactive values you can create anywhere at runtime,
  then hand out as narrow "capability" views (listen-only, trigger-only, read-only, linker).

It also ships an **Editor Event Monitor** window so you can watch events, listeners, payloads
and raise history live while you play.

## Why Use It
- **Strongly typed, no magic strings.** Events are interfaces; the compiler catches mistakes.
- **Zero reflection at startup.** A Source Generator writes the registration code at compile
  time — no `AppDomain` scan, no `ReflectionTypeLoadException`, no domain-reload reliance.
- **Zero dependency.** The runtime assembly references nothing else.
- **Sticky events built in.** Cache the last raise and let late subscribers catch up with
  `KickStart()`.
- **Least-privilege by design.** Signals expose narrow capability interfaces.

## Why It's Different

Most Unity event systems make you choose between convenience and safety. AceLand Event Driven refuses that trade-off:

| | Typical string/enum event bus | Reflection-scanning frameworks | **AceLand Event Driven** |
| --- | --- | --- | --- |
| Type safety | ❌ magic strings, runtime failures | ⚠️ attributes, discovered late | ✅ **interfaces, compile-time checked** |
| Startup cost | cheap but fragile | ❌ `AppDomain` scan + reflection | ✅ **zero reflection, source-generated** |
| Domain-reload / CoreCLR | relies on reload | ❌ `ReflectionTypeLoadException` risk | ✅ **no reload reliance, Unity 7 ready** |
| Late subscribers | manual bookkeeping | varies | ✅ **built-in sticky cache + `KickStart()`** |
| Dependencies | varies | often heavy | ✅ **zero dependency** |

Because registration is generated at **compile time**, the cost you'd normally pay at startup simply isn't there — and mistakes surface in your IDE, not in a play-test.

> For the full API, Event Monitor walkthrough and best practices, see the package README and the official documentation.

## Package Info
| | |
| --- | --- |
| display name | AceLand Event Driven |
| package name | com.aceland.eventdriven |
| namespace | AceLand.EventDriven |
| git repository | [https://github.com/parsue/com.aceland.eventdriven.git](https://github.com/parsue/com.aceland.eventdriven.git) |
| dependencies | none (zero-dependency Core package) |

## Code Samples

### Event Bus

Define an event — an event is just an interface:

```csharp
using AceLand.EventDriven.Bus;

// IEvent<T> = one payload; the single method returns void and takes exactly T.
[EventDescription("Player Scored")]   // optional: shown in the Event Monitor
public interface IPlayerScored : IEvent<int>
{
    void OnPlayerScored(int score);
}
```

Listen — implement the interface and subscribe:

```csharp
using AceLand.EventDriven.Bus;
using UnityEngine;

public class ScoreBoard : MonoBehaviour, IPlayerScored
{
    private void OnEnable()  => EventBus.Event(this).Listen();
    private void OnDisable() => EventBus.Event(this).Unlisten();

    // Called automatically whenever IPlayerScored is raised.
    public void OnPlayerScored(int score) =>
        Debug.Log($"Score is now {score}");
}
```

Raise — publish, optionally caching the last value for late subscribers:

```csharp
using AceLand.EventDriven.Bus;

public class Game
{
    public void AddPoint(int score)
    {
        // Raise without cache:
        EventBus.Event<IPlayerScored>()
            .WithData(score)
            .Raise();

        // Or raise AND remember the last value (sticky):
        EventBus.Event<IPlayerScored>()
            .WithData(score)
            .WithCache()
            .Raise();
    }
}
```

Let a late subscriber immediately receive the last cached value:

```csharp
EventBus.Event(this).Listen().KickStart().Done();
```

### Event Signal

Create, read and react to id-based reactive values:

```csharp
using AceLand.EventDriven.EventSignal;

// Create (or get) a signal by id.
var health = Signal<int>.GetOrCreate("player.health");
health.Value = 100;

// React to changes.
health.AddListener(v => Debug.Log($"Health = {v}"));
health.Value = 80;   // listener fires with 80
```

Hand out only the capability a consumer needs:

```csharp
using AceLand.EventDriven.EventSignal;

var signal = Signal<int>.GetOrCreate("player.health");

IReadonlySignal<int> readOnly = signal.AsReadonly();   // can read + listen
ISignalListener<int> listener = signal.AsListener();   // can only listen
ISignalTrigger<int>  trigger  = signal.AsTrigger();    // can only set/trigger
```

## Documents
We use GitBook as the public documentation for our packages.

> Visit our [GitBook](https://docs.parsue.io/aceland-unity-packages/core-packages/event-driven)

Please visit our GitBook for details.
