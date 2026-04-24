"""Helpers for running NX Open Python scripts in batch mode."""

import os

import NXOpen


def get_target_part_path():
    target = os.environ.get("NX_TARGET_PRT", "").strip()
    if not target:
        return None
    return os.path.abspath(target)


def should_save_changes():
    return os.environ.get("NX_SAVE_CHANGES", "").strip() == "1"


def open_target_part(session, listing_window=None):
    target_path = get_target_part_path()
    if not target_path:
        return session.Parts.Work, None

    if listing_window is not None:
        listing_window.WriteLine(f"Opening target part: {target_path}")

    part, load_status = session.Parts.OpenDisplay(target_path)
    session.Parts.SetWork(part)

    if listing_window is not None and load_status is not None:
        listing_window.WriteLine("Target part opened.")

    return session.Parts.Work, target_path
