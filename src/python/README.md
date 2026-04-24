# NX Python Examples

## Current example

- `nx_session_report.py`: opens the NX Listing Window and reports the current session, work part, display part, units, and body count
- `nx_create_point.py`: creates a visible point in the current work part at a fixed coordinate

## How to run

1. Open Siemens NX.
2. Load or create a part.
3. Run the script from the NX journal or automation entry that supports Python.
4. Check the Listing Window and message box for the result.

## Why this is the first example

This script is intentionally simple. It verifies that:

- NX Open Python is available
- the script can access the current NX session
- the script can read part information
- output can be shown inside NX

## Next examples to add

- create a datum feature
- export part metadata
- batch-read part files
