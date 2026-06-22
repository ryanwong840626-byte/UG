# NX Python Examples

## Current example

- `nx_session_report.py`: opens the NX Listing Window and reports the current session, work part, display part, units, and body count
- `nx_create_point.py`: creates a visible point in the current work part at a fixed coordinate
- `nx_create_datum_plane.py`: creates a fixed datum plane at the part origin
- `nx_lens_mold_asphere_points.py`: reads an asphere JSON config and creates sampled meridian points in NX
- `nx_lens_mold_asphere_spline.py`: reads an asphere JSON config, creates reference points, and attempts to build a meridian spline
- `nc_single_station.py`: converts a legacy NC file into single-station local tool samples for UG collision preview

## How to run

1. Open Siemens NX.
2. Load or create a part.
3. Run the script from the NX journal or automation entry that supports Python.
4. Check the Listing Window and message box for the result.

## Optional launcher scripts

- `../tools/run_nx_session_report.ps1`
- `../tools/run_nx_create_point.ps1`
- `../tools/run_nx_create_datum_plane.ps1`
- `../tools/run_nx_batch_update.ps1`
- `../tools/run_nx_lens_mold_asphere_points.ps1`
- `../tools/run_nx_lens_mold_asphere_spline.ps1`
- `../tools/prepare_single_station_samples.ps1`

These launchers call `run_journal.exe` directly from PowerShell so you can test examples with fewer manual steps.

## Batch mode examples

Run the session report against a specific part:

`powershell -ExecutionPolicy Bypass -File .\run_nx_session_report.ps1 -PartPath C:\path\to\model1.prt`

Create a point and save the changed part:

`powershell -ExecutionPolicy Bypass -File .\run_nx_create_point.ps1 -PartPath C:\path\to\model1.prt -SaveChanges`

Create a datum plane and save the changed part:

`powershell -ExecutionPolicy Bypass -File .\run_nx_create_datum_plane.ps1 -PartPath C:\path\to\model1.prt -SaveChanges`

Batch process all `.prt` files in a folder:

`powershell -ExecutionPolicy Bypass -File .\run_nx_batch_update.ps1 -FolderPath C:\path\to\parts -Operation point -SaveChanges`

Generate asphere meridian sample points from a config file:

`powershell -ExecutionPolicy Bypass -File .\run_nx_lens_mold_asphere_points.ps1 -PartPath C:\path\to\lens.prt -ConfigPath ..\configs\lens_mold_asphere_sample.json -SaveChanges`

Generate an asphere meridian spline from a config file:

`powershell -ExecutionPolicy Bypass -File .\run_nx_lens_mold_asphere_spline.ps1 -PartPath C:\path\to\lens.prt -ConfigPath ..\configs\lens_mold_asphere_sample.json -SaveChanges`

Prepare single-station tool samples from the current roughing NC:

`powershell -ExecutionPolicy Bypass -File .\prepare_single_station_samples.ps1 -NcPath C:\path\to\data_30F.nc`

## Why this is the first example

This script is intentionally simple. It verifies that:

- NX Open Python is available
- the script can access the current NX session
- the script can read part information
- output can be shown inside NX

## Next examples to add

- export part metadata
- batch-read part files
- parameterize geometry values from command line input
- build spline and surface features from lens configuration input
- upgrade the single-station collision preview into an NC-driven clearance check
