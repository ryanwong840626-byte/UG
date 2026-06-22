# UG Project Workspace

This repository is the working space for UG/NX related notes, scripts, experiments, and project context.

## Goal

Use this repository to keep project memory in one place so future development can continue smoothly.

## Recommended layout

- `docs/`: project background, requirements, setup notes, and progress
- `src/`: scripts, sample code, or automation tools
- `assets/`: screenshots, reference images, and sample files

## Working method

1. Put new project materials into the proper folder.
2. Update the files in `docs/` when requirements or decisions change.
3. Commit important milestones so the history stays readable.

## Current repository

- GitHub: `https://github.com/ryanwong840626-byte/UG`
- Main branch: `main`

## Current lens mold work

- NX command-line automation is available through `run_journal.exe`.
- Lens mold asphere point and spline generation are scaffolded.
- Single-station NC preprocessing and a first UG collision preview Journal are available:
  - `src/python/nc_single_station.py`
  - `src/tools/prepare_single_station_samples.ps1`
  - `src/journals/single_station_collision_model_v6.vb`
  - `docs/single-station-collision-workflow.md`
