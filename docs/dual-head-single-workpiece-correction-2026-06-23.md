# Dual-Head Single-Workpiece Correction - 2026-06-23

## Corrected machine interpretation

The transparent part is the mold/workpiece. Its current diameter is `80 mm`.

The machine should be modeled as:

- one workpiece/mold
- one chuck/fixture holding that workpiece
- two grinding heads
- two sequential operations:
  - pass 1: rough grinding / milling-grinding
  - pass 2: finish grinding

The NC split that was previously interpreted as station 1 / station 2 is now
treated as head/pass 1 and head/pass 2 on the same workpiece.

## Superseded assumptions

The previous `single_station_collision_model_v7_diameter_x.vb` tested an
`X-as-diameter` interpretation based on partial photo evidence. That assumption
is now superseded for the main validation path because the workpiece diameter
has been clarified as `80 mm`.

Keep v7 only as a historical comparison model.

## Current v8 direction

The next UG/NX preview should use:

- workpiece radius: `40 mm`
- single fixture/chuck
- two different tool/head colors for rough and finish passes
- path split names: `pass_id=1` rough head, `pass_id=2` finish head

The exact physical offsets between the two grinding heads are still
uncalibrated. Until measured, v8 is still a geometry-orientation preview, not a
final production collision proof.
