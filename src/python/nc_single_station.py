"""Utilities for single-station lens mold NC preprocessing.

This module converts an old roughing NC program into a single-station
tool-center sample table. Some legacy programs use an X offset around the
fixture-center distance; for the real machine here, that offset represents
a second pass on the same workpiece, not a second physical station.
"""

from __future__ import annotations

import argparse
import csv
import json
import re
from dataclasses import asdict, dataclass
from pathlib import Path


AXIS_RE = re.compile(r"([XZC])\s*([-+]?\d+(?:\.\d+)?)", re.IGNORECASE)


@dataclass
class ToolPoint:
    index: int
    raw_x: float
    local_x: float
    z: float
    c_unwrapped: float
    pass_id: int


def parse_axis_words(line: str) -> dict[str, float]:
    return {axis.upper(): float(value) for axis, value in AXIS_RE.findall(line)}


def unwrap_c(previous: float | None, current_raw: float) -> float:
    if previous is None:
        return current_raw

    previous_mod = previous % 360.0
    delta = current_raw - previous_mod
    while delta > 180.0:
        delta -= 360.0
    while delta <= -180.0:
        delta += 360.0
    return previous + delta


def nc_to_single_station_points(
    nc_path: Path,
    station_x_offset: float = 197.0,
    x_split_threshold: float = 120.0,
    sample_every: int = 80,
) -> list[ToolPoint]:
    points: list[ToolPoint] = []
    last_c: float | None = None
    sampled_index = 0

    with nc_path.open("r", encoding="utf-8", errors="ignore") as file:
        for line_index, line in enumerate(file):
            words = parse_axis_words(line)
            if "X" not in words or "Z" not in words:
                continue

            raw_x = words["X"]
            z = words["Z"]
            raw_c = words.get("C", last_c if last_c is not None else 0.0)
            c_unwrapped = unwrap_c(last_c, raw_c)
            last_c = c_unwrapped

            pass_id = 2 if raw_x >= x_split_threshold else 1
            local_x = raw_x - station_x_offset if pass_id == 2 else raw_x

            if sampled_index % sample_every == 0:
                points.append(
                    ToolPoint(
                        index=line_index,
                        raw_x=raw_x,
                        local_x=local_x,
                        z=z,
                        c_unwrapped=c_unwrapped,
                        pass_id=pass_id,
                    )
                )
            sampled_index += 1

    return points


def write_points_csv(points: list[ToolPoint], csv_path: Path) -> None:
    csv_path.parent.mkdir(parents=True, exist_ok=True)
    with csv_path.open("w", newline="", encoding="utf-8") as file:
        writer = csv.DictWriter(
            file,
            fieldnames=["index", "raw_X", "local_X", "Z", "C_unwrapped", "pass_id"],
        )
        writer.writeheader()
        for point in points:
            writer.writerow(
                {
                    "index": point.index,
                    "raw_X": point.raw_x,
                    "local_X": point.local_x,
                    "Z": point.z,
                    "C_unwrapped": point.c_unwrapped,
                    "pass_id": point.pass_id,
                }
            )


def summarize(points: list[ToolPoint]) -> dict[str, object]:
    if not points:
        return {"samples": 0}

    return {
        "samples": len(points),
        "raw_x_min": min(point.raw_x for point in points),
        "raw_x_max": max(point.raw_x for point in points),
        "local_x_min": min(point.local_x for point in points),
        "local_x_max": max(point.local_x for point in points),
        "z_min": min(point.z for point in points),
        "z_max": max(point.z for point in points),
        "pass_counts": {
            "1": sum(1 for point in points if point.pass_id == 1),
            "2": sum(1 for point in points if point.pass_id == 2),
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Convert NC to single-station tool samples.")
    parser.add_argument("--nc", required=True, type=Path, help="Input NC file.")
    parser.add_argument("--csv", required=True, type=Path, help="Output sample CSV.")
    parser.add_argument("--summary", type=Path, help="Optional JSON summary output.")
    parser.add_argument("--station-x-offset", type=float, default=197.0)
    parser.add_argument("--x-split-threshold", type=float, default=120.0)
    parser.add_argument("--sample-every", type=int, default=80)
    args = parser.parse_args()

    points = nc_to_single_station_points(
        args.nc,
        station_x_offset=args.station_x_offset,
        x_split_threshold=args.x_split_threshold,
        sample_every=args.sample_every,
    )
    write_points_csv(points, args.csv)
    summary = summarize(points)

    if args.summary:
        args.summary.parent.mkdir(parents=True, exist_ok=True)
        args.summary.write_text(json.dumps(summary, indent=2), encoding="utf-8")

    print(json.dumps(summary, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
