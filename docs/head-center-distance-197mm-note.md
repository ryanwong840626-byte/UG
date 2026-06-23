# Head Center Distance Note

## Confirmed dimension

The physical center-to-center distance between the two grinding heads is:

`197 mm`

## Meaning for NC interpretation

The large X offset in the legacy NC should be interpreted as the distance
between the rough grinding head and the finish grinding head.

This confirms the current corrected model:

- one mold/workpiece
- one fixture/chuck
- two physical grinding heads
- head 1: rough grinding / milling-grinding
- head 2: finish grinding
- head center distance: `197 mm`

## Coordinate implication

For pass 2, the raw NC X value includes the second-head offset. A local
workpiece coordinate can be recovered by:

`local_X = raw_X - 197.0`

This matches the existing single-station preprocessing logic, but the name
should be understood as a two-head offset rather than a two-station offset.
