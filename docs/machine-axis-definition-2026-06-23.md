# Machine Axis Definition - 2026-06-23

## Confirmed axis logic

The machine has three active axis positions for the current rough/finish
workflow:

- `X`: fixture/workpiece horizontal traverse axis.
- `C`: workpiece rotation axis.
- `Z`: grinding head infeed/retract axis.

The grinding contact point is located on the horizontal line through the
workpiece center.

## Consequences for the simulation

The NC path must not be interpreted as multiple grinding heads or multiple
workpieces. The correct kinematic interpretation is:

1. The fixture and mold move horizontally along `X`.
2. The workpiece rotates around `C`.
3. Each grinding head feeds in/out along `Z`.
4. The two physical grinding heads have a confirmed center-to-center distance
   of `197 mm`.
5. The rough pass and finish pass are two head zones along the same X traverse.

## Modeling correction

The next UG preview should show:

- one fixture/mold assembly on an X slide
- two fixed grinding heads separated by `197 mm`
- X travel rail and two head center marks
- Z infeed direction for each head
- C rotation axis through the mold center
- path curves as workpiece-center travel / infeed information, not duplicated
  grinding head bodies

This axis definition supersedes earlier visual previews that treated NC samples
as multiple tool snapshots.
