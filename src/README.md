# Source Directory

Use this folder for scripts, examples, and tooling related to UG / NX workflows.

## Suggested subfolders

- `python/`: NX Open Python scripts or journal conversions
- `journals/`: UG/NX Journal files that are run inside NX when Python batch mode is not enough
- `dotnet/`: C# or VB.NET examples for NX Open
- `tools/`: helper scripts for setup, export, or automation

## Suggested rule

Keep each experiment small and add a short note explaining what it does.

## First usable example

- `python/nx_session_report.py`: first NX Open Python automation example for validating the session and current part context
- `python/nc_single_station.py`: converts legacy NC coordinates into single-station local tool samples
- `journals/single_station_collision_model_v6.vb`: creates the current simplified single-station UG collision preview
