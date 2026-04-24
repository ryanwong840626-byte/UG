"""NX Open Python example: report session and current work part details.

Run this inside Siemens NX as a Python journal/script. It writes basic
information to the Listing Window so we can verify the NX Open environment is
working before building more advanced automation.
"""

import NXOpen

from nx_batch_utils import get_target_part_path, open_target_part


def write_line(listing_window, text):
    listing_window.WriteLine(str(text))


def get_part_label(part):
    if part is None:
        return "<none>"

    part_name = part.Name if getattr(part, "Name", None) else "<unnamed>"
    full_path = part.FullPath if getattr(part, "FullPath", None) else "<unsaved>"
    return f"{part_name} | {full_path}"


def main():
    session = NXOpen.Session.GetSession()
    ui = NXOpen.UI.GetUI()

    mark_id = session.SetUndoMark(
        NXOpen.Session.MarkVisibility.Visible,
        "NX Session Report",
    )

    listing_window = session.ListingWindow
    listing_window.Open()

    write_line(listing_window, "=== NX Session Report ===")
    target_path = get_target_part_path()
    if target_path:
        write_line(listing_window, f"Requested target part: {target_path}")

    work_part, opened_path = open_target_part(session, listing_window)
    display_part = session.Parts.Display

    if opened_path:
        write_line(listing_window, f"Opened in batch mode: {opened_path}")

    write_line(listing_window, f"Work part: {get_part_label(work_part)}")
    write_line(listing_window, f"Display part: {get_part_label(display_part)}")

    if work_part is None:
        write_line(listing_window, "No work part is currently loaded.")
        ui.NXMessageBox.Show(
            "NX Python Example",
            NXOpen.NXMessageBox.DialogType.Warning,
            "Script ran, but no work part is loaded. Pass NX_TARGET_PRT or open a part in NX.",
        )
        return

    units = "Metric" if work_part.PartUnits == NXOpen.Part.Units.Millimeters else "Inch"
    write_line(listing_window, f"Units: {units}")
    write_line(listing_window, f"Undo mark id: {mark_id}")

    ui.NXMessageBox.Show(
        "NX Python Example",
        NXOpen.NXMessageBox.DialogType.Information,
        "NX session report completed. Check the Listing Window for details.",
    )


def get_unload_option(_):
    return NXOpen.Session.LibraryUnloadOption.Immediately


if __name__ == "__main__":
    main()
