# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed


### Fixed

### Security

## [0.0.1] - 2024-08-30
### Added
- Initial release of project


## [0.1.0] - 2024-09-15
### Added
- Custom UI integration to interact as an admin
- Teleport Panel
- Debug Panel
- Items Panel

## [0.2.0] - 2024-09-16
### Added
- UI Panels for construct and elements

### Changed
- Removed unrelevant items from dropdown and moved them to the respective UI panels

### Security
- Fixed a bug showing the options to a non admin user


## [0.2.1] - 2024-09-17

### Added
- config option to allow access to different user roles set in BO

### Changed
- Reworked the c# mod to allow the config option to work

## [0.2.2] - 2024-09-19

### Added
- dedicated log file to debug the UI interactions withouth having to look at grains_dev.log
- more error messages that are send to the client

### Changed
- Replaced all log refferences to use custom logging

### Fixed
- Bug that was caused by leftover code , the roles from BO couldnt open the hud due to a check on only admin was left over somewhere down the line. is adjusted now.
- Bug that would show the failover textbox when copying text to clipboard from the debug panel. is fixed now.
- Bug preventing to use tp command on own player character. 
- Bug preventing to use ::pos location strings to teleport to custom locations

## [0.2.30] - 2024-09-21

### Changed
- replaced the github repo to a dedicated one


## [0.2.32] - 2024-09-21

### Changed
- Input for volume items uses a calculation so we can request properly 1000 liters of fuel

### Fixed
- UI bug that would change default market drop downs due to using the same class

## [0.2.33] - 2024-09-27

### Added
- blueprint management : ability to add blueprints trough a weblink , make them magic and give them to players or add them to inventory


## [0.2.34] - 2024-09-27

### Fixed
- Pagination on blueprints page

## [0.2.35] - 2024-10-02

### Added
- remove DRM protection on elements (hovercraft seat controller for example)

### Changed
- Added fallback and catch exeptions for teleportation locations

## [0.2.36] - 2024-10-26

### Added
- version in the c# code, it will log the version in the logs

### Fixed
- Bug in construct panel when the construct was owned by a organization
- Bug that would spawn items in linked container instead of nanopack

## [0.2.37] - 2024-10-27

### Added
- Inactivity Manager with configurable options in the config file (optional settings , by default its turned off)


