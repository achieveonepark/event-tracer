# Changelog

## 1.0.0

- Prepare the first stable OpenUPM release with MIT license and package metadata.
- Make English the default documentation language, with linked Korean guides.
- Default the editor UI to English while preserving saved language choices.
- Add optional recording-time call stacks, a per-event character limit, and buffer budget accounting.
- Show call stacks in the event browser and preserve them in .etr exports and imports.
- Keep older captures and editor preferences compatible with the new stack trace settings.

## 0.1.0

- Independent UPM package with explicit session-aware parent IDs.
- Immutable payload snapshots and count/character bounded ring buffer.
- UI Toolkit event browser with search, filters, cause/effect navigation, and source/caller links.
- Detached JSON export/import with validation.
- Native Unity editor styling and .etr importer with UI Toolkit capture inspector.
- Shop demo and Editor behavior tests.
