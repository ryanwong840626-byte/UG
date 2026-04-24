# Lens Mold Requirements Template

Fill this file for each mold or product family we automate.

## Basic info

- Project name:
- Customer or internal code:
- Part family:
- Units: `mm` or `inch`

## Optical surface type

- `rotational asphere`
- `toric`
- `freeform`

## Surface definition

### For rotational asphere

- base radius:
- conic constant:
- clear aperture diameter:
- semi diameter:
- center thickness or insert reference thickness:
- asphere coefficients:

### For freeform

- source format: `grid sag` / `point cloud` / `polynomial`
- source file path:
- sampling resolution:
- trimming boundary:

## Mold structure

- insert overall size:
- reference origin:
- molding side orientation:
- shrinkage or compensation rule:
- holder or fixture constraints:

## CAM requirements

- machine:
- controller:
- tool types:
- spindle speed range:
- feed strategy:
- roughing method:
- finishing method:
- tool path tolerance:
- stepover:
- scallop target:

## Output requirements

- NX part path:
- NC output format:
- setup sheet needed: `yes/no`
- inspection data export needed: `yes/no`

## Validation plan

- test cut material:
- acceptance criteria:
- expected failure modes:
- adjustment loop owner:
