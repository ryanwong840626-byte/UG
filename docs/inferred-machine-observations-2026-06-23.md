# Inferred Machine Observations - 2026-06-23

## Source material

The current inference is based on user-supplied machine photos, fixture photos,
caliper photos, and short videos from 2026-06-23.

## Corrected machine topology

The physical machine should be modeled as:

- one fixture/chuck
- one workpiece/lens mold
- one grinding head
- multiple tool/pass positions on the same workpiece, not two physical stations

## Important coordinate correction

The sampled NC local X maximum is about `49.94 mm`. Based on the caliper photos
and lens/mold scale, this value is more likely a diameter coordinate than a
radius coordinate.

Therefore the collision preview should test both interpretations:

- radius mode: tool radial position = `local_X`
- diameter mode: tool radial position = `local_X / 2`

For the current machine photos, diameter mode is the more plausible assumption.

## First estimated geometry

These values are approximate and must be calibrated before any production
approval:

- lens/mold nominal diameter: about `50.8 mm`
- lens/mold modeling radius: `25.4 mm`
- blue chuck/holder outer radius: about `36 mm`
- holder/fixture rear support radius: about `30 mm`
- grinding wheel radius: about `32 mm`
- grinding wheel thickness: about `6 mm`
- tool shank radius: about `6 mm`
- grinding head angle: still uncalibrated; photos show an inclined spindle

## Current modeling decision

Generate a new UG preview that treats NC X as diameter and maps it to local
radius by dividing by two. Keep all outputs marked as simulation-only until
real tool/fixture dimensions are measured and verified.
