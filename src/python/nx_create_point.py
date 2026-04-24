"""NX Open Python example: create a visible point in the current work part.

Run this inside Siemens NX as a Python journal/script. The example creates a
point at a fixed coordinate so we can validate basic geometry creation from
NX Open Python.
"""

import NXOpen


POINT_X = 10.0
POINT_Y = 20.0
POINT_Z = 30.0


def write_line(listing_window, text):
    listing_window.WriteLine(str(text))


def main():
    session = NXOpen.Session.GetSession()
    ui = NXOpen.UI.GetUI()
    work_part = session.Parts.Work

    listing_window = session.ListingWindow
    listing_window.Open()
    write_line(listing_window, "=== NX Create Point Example ===")

    if work_part is None:
        ui.NXMessageBox.Show(
            "NX Python Example",
            NXOpen.NXMessageBox.DialogType.Warning,
            "No work part is loaded. Open or create a part first.",
        )
        write_line(listing_window, "No work part is loaded.")
        return

    mark_id = session.SetUndoMark(
        NXOpen.Session.MarkVisibility.Visible,
        "Create Point Example",
    )

    point_coordinates = NXOpen.Point3d(POINT_X, POINT_Y, POINT_Z)
    point = work_part.Points.CreatePoint(point_coordinates)
    point.SetVisibility(NXOpen.SmartObject.VisibilityOption.Visible)

    write_line(listing_window, f"Created point at: ({POINT_X}, {POINT_Y}, {POINT_Z})")
    write_line(listing_window, f"Point tag: {point.Tag}")
    write_line(listing_window, f"Undo mark id: {mark_id}")

    ui.NXMessageBox.Show(
        "NX Python Example",
        NXOpen.NXMessageBox.DialogType.Information,
        "Point created successfully. Check the Listing Window for details.",
    )


def get_unload_option(_):
    return NXOpen.Session.LibraryUnloadOption.Immediately


if __name__ == "__main__":
    main()
