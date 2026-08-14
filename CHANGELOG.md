# Changelog

All notable changes to this project are documented here.

## [Unreleased]

### Added

- `POST /api/todos/clear-completed` bulk endpoint
- Dockerfile + `.dockerignore` for container deployment
- GitHub Actions build workflow
- Server-side 120-character title validation

### Changed

- Clear-completed button now calls the bulk endpoint in a single request
- Docker image runs ASP.NET Core 10.0 on port 8080