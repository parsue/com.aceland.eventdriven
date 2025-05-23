﻿# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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
