# Changelog

All notable changes to this project will be documented in this file.

---

## [2.3.0] - 2025-12-13
### Removed
- all meta files

## [2.2.2] - 2025-12-10
### Fixed
- [Signal] GetOrCreate don't return correctly

## [2.2.1] - 2025-12-10
### Added
- [Signal] GetOrCreate
- [Signal] warning log on get signal fail with wrong type

## [2.2.0] - 2025-12-10
### Added
- [Signal] TryGetLinker and GetLinkerAsync for getting ISignalLinker
### Modified
- [Signal] Get changed to TryGet
- [Signal] GetAs changed to Get

## [2.1.8] - 2025-12-06
### Modified
- [package.json] add doc links

## [2.1.6] - 2025-12-03
### Modified
- [SignalLinker] move AdaptorOption to upper level of namespace
- [SignalAdaptor] with much handly add adaptor method now

## [2.1.5] - 2025-12-02
### Added
- [SignalLinker] a link up multiple Signal<T> for handle multiple signals condition
- [Signal] add bool Disposed in all signal types

## [2.1.4] - 2025-11-15
### Fixed
- [Dependency] com.aceland.taskutils version should be 2.0.3

## [2.1.3] - 2025-11-15
### Add
- [Dependency] Player Loop Hack
- [Project Settings] default signal trigger state for trigger once per frame, may changed in next frame
- [Signal] default trigger immediate
- [Signal Builder] WithTriggerOncePerFrame(PlayerLoopState) to trigger signal immediately
### Remove
- [Signal Builder] BuildReadonly will be removed, reasons:
- IReadonlySignal cannot be disposed
- signal permissions and conversion are completed

## [2.1.2] - 2025-11-15
### Fixed
- [Signal] Signal<T>.GetAsync now return Promise<ISignal<T>>
### Modified
- [EventBus] internal optimize
- [Signal.Builder] WithId can be ignore now, id will be a new guid

## [2.1.1] - 2025-11-14
### Modified
- [EventBus] optimizing

## [2.1.0] - 2025-11-14
### Modified
- [EventBus] simplify listener processes
- [EventBus] force single listener function in IEvent
- [EventBus] throw exception on wrong value type (or no value) on raising event

## [2.0.4] - 2025-10-11
### Added
- [Signal] add conversion between signal types, read docs
### Modified
- [Signal] arrange permissions signal types
- [Signal] protecting permissions of different types
 
## [2.0.3] - 2025-10-10
### Added
- [Signal] Signal Trigger, Signal Listener
### Modified
- [Signal] returning interface instead of concrete object
- [Signal] Get function refactoring to GetAs for different signal types
### Removed
- [Project Settings] Swap Signal option, not allow same id

## [2.0.2] - 2025-10-09
### Added
- [Signal] Trigger to ReadonlySignal<T>

## [2.0.1] - 2025-10-09
### Added
- [EventBus] clear cache function
- [Signal] Signal<T> to ReadonlySignal<T> extension

## [2.0.0] - 2025-10-06
### Added
- [Provider] Signal Prewarm Provider - build signals before scene load
- [Exception] new exception type for GetAsync of Signal
### Modified
- [Builder] Signal builders are reconstructed
### Removed
- [Builder] Listener in Signal Builder

---

## [1.0.20] - 2015-04-17
### Added
- [EventBus] add requirement on Event to be inherited by IEvent

## [1.0.19] - 2015-04-08
### Removed
- [Interface Binding] obsoleted, replaced by EventBus

## [1.0.18] - 2015-04-03
### Modified
- [EventBus] centralize methods chain in builders

## [1.0.17] - 2015-04-03
### Modified
- [EventBus] IEventListener and IEventRaiser is not required for user
- [EventBus] remove extension
- [EventBus] simplify methods chain, start with EventBus.Event<T>()

## [1.0.16] - 2015-04-03
### Modified
- [EventBus] Rename extension methods for object being both IEventListener and IEventRaiser 

## [1.0.15] - 2015-04-03
### Modified
- [EventBus] IEventData is not required for Event<TPayload>

## [1.0.14] - 2025-04-03
### Added
- [EventBus] created, replacement of Interface Binding
### Modified
- [Interface Binding] marked as obsoleted, will be replaced by EventBus
### Removed
- [Interface Mapping] obsoleted, replaced by EventBus

## [1.0.13] - 2025-02-11
### Modified
- [Interface Binding] IEnumerable<TInterface> will be returned by ListBindings() for Unity
- [Interface Binding] removed log and error throw when no binding of interface when calling ListBindings or ListBindingsAsync

## [1.0.12] - 2025-02-11
### Added
- [Interface Binding] multi interface binding and unbinding

## [1.0.11] - 2025-02-10
### Added
- [Interface Binding] Unbind<>() for remove binding 
### Fixed
- [Interface Binding] error on empty ListBindings
### Modified
- [Interface Binding] publish version
- [Interface Binding] output type of ListBindings to ReadOnlySpan<>

## [1.0.10] - 2025-02-10
### Added
- [Interface Binding] experimental level is released
## [1.0.9] - 2025-02-05
### Fixed
- [Interface Mapping] ignore abstract class

## [1.0.8] - 2025-1-25
### Modified
- [Editor] AceLand Project Setting as Tree structure

## [1.0.7] - 2024-12-31
### Modified
- Handling FindObjectsInactive on FindObjects in InterfaceMappings

## [1.0.6] - 2024-11-26
### Modified
- [Editor] Undo functional for Project Settings

## [1.0.4] - 2024-11-24
First public release. If you have an older version, please update or re-install.   
For detail please visit and bookmark our [GitBook](https://aceland-workshop.gitbook.io/aceland-unity-packages/)
