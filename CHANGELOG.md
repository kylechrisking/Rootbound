# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2024-03-21
### Added
- Quest System
  - Multiple objective types support
  - Daily and weekly quest generation
  - Quest chains and storylines
  - Milestone rewards system
  - Progress tracking and visualization
- Critter System
  - Diverse critter types with unique behaviors
  - Time and season-based spawning system
  - Interest-based interaction mechanics
  - Catching mechanics with tool requirements
  - Population management and rarity system
- Museum System
  - Multiple themed wings with unlock progression
  - Grid-based exhibit placement
  - Theme bonuses and collection rewards
  - Interactive exhibit displays
  - Visitor pathfinding with A* algorithm
  - Museum completion tracking
- Core Systems
  - Achievement system with progress tracking
  - Tutorial system with contextual triggers
  - Notification system for game events
  - Save/Load system using JSON serialization

### Changed
- Migrated UI system to Unity UI Toolkit
- Enhanced inventory system for museum exhibits
- Improved notification system architecture
- Refined achievement system integration

### Fixed
- UI layout issues in museum grid display
- Edge cases in museum visitor pathfinding
- Quest progress persistence bugs
- Exhibit placement validation errors

### Security
- Added input validation for save data loading
- Implemented error handling for file operations

## [0.1.0] - 2024-03-20
### Added
- Initial project setup
- Basic game architecture
- Core systems foundation

[Unreleased]: https://github.com/kylechrisking/Rootbound/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/kylechrisking/Rootbound/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/kylechrisking/Rootbound/releases/tag/v0.1.0 