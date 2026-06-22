# Single-Station Collision Workflow

## Current machine assumption

The physical machine has one fixture, one workpiece, and one grinding head.
The legacy program name and the large X offset must not be interpreted as a
physical dual-station machine for collision validation.

## Coordinate interpretation

For the current `data_30F.nc` program:

- Raw X spans about `8.46 mm` to `239.94 mm`.
- The high-X section is converted back to local single-station radius by
  subtracting the current inferred fixture offset, `197.0 mm`.
- The resulting local workpiece radius spans about `0.37 mm` to `49.94 mm`.
- The two pass IDs in the sample table mean program passes on one fixture,
  not two machine stations.

## Generated assets

- `assets/samples/single_station_samples.csv`: sampled NC points converted to
  single-station local coordinates.
- `assets/samples/single_station_samples_summary.json`: quick sanity summary.
- `src/journals/single_station_collision_model_v6.vb`: UG/NX Journal that
  creates the current simplified single-station collision preview.

## Regenerate samples

From `src/tools`:

```powershell
.\prepare_single_station_samples.ps1 `
  -NcPath "C:\Users\RyanW\非球面\data_30F.nc" `
  -OutputCsv "..\..\assets\samples\single_station_samples.csv" `
  -SummaryPath "..\..\assets\samples\single_station_samples_summary.json"
```

## Validation status

This is a rough screening model only. It is useful for checking whether the
single-station coordinate interpretation is plausible and for building the
next UG automation step.

It is not yet sufficient to approve a compensated NC program for production.
Before any production trial, the model still needs the real machine dimensions:

- wheel diameter and thickness
- grinding spindle angle and shank direction
- fixture outer diameter and height
- lens blank diameter and clamping height
- machine travel limits and any guard/cover geometry

## Next target

Upgrade the static simplified preview into an NC-driven machine verification
model:

1. Keep one fixture/workpiece/head only.
2. Drive the tool center by the sampled NC path.
3. Use calibrated wheel and shank geometry.
4. Report minimum clearances and risky intervals before producing any
   compensation program for trial.
