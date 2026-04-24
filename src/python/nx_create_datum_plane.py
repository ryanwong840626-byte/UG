"""NX Open Python example: create a fixed datum plane in the current work part."""

import NXOpen

from nx_batch_utils import open_target_part, should_save_changes


ORIGIN_X = 0.0
ORIGIN_Y = 0.0
ORIGIN_Z = 0.0


def write_line(listing_window, text):
    listing_window.WriteLine(str(text))


def create_identity_matrix():
    matrix = NXOpen.Matrix3x3()
    matrix.Xx = 1.0
    matrix.Xy = 0.0
    matrix.Xz = 0.0
    matrix.Yx = 0.0
    matrix.Yy = 1.0
    matrix.Yz = 0.0
    matrix.Zx = 0.0
    matrix.Zy = 0.0
    matrix.Zz = 1.0
    return matrix


def main():
    session = NXOpen.Session.GetSession()
    ui = NXOpen.UI.GetUI()

    listing_window = session.ListingWindow
    listing_window.Open()
    write_line(listing_window, "=== NX Create Datum Plane Example ===")

    work_part, opened_path = open_target_part(session, listing_window)
    if opened_path:
        write_line(listing_window, f"Opened in batch mode: {opened_path}")

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
        "Create Datum Plane Example",
    )

    origin = NXOpen.Point3d(ORIGIN_X, ORIGIN_Y, ORIGIN_Z)
    orientation = create_identity_matrix()
    datum_plane = work_part.Datums.CreateFixedDatumPlane(origin, orientation)

    write_line(listing_window, f"Created datum plane at origin: ({ORIGIN_X}, {ORIGIN_Y}, {ORIGIN_Z})")
    write_line(listing_window, f"Datum plane tag: {datum_plane.Tag}")
    write_line(listing_window, f"Undo mark id: {mark_id}")

    if should_save_changes():
        save_status = work_part.Save(
            NXOpen.BasePart.SaveComponents.FalseValue,
            NXOpen.BasePart.CloseAfterSave.FalseValue,
        )
        write_line(listing_window, f"Save completed: {save_status is not None}")
    else:
        write_line(listing_window, "Save skipped. Set NX_SAVE_CHANGES=1 to save changes.")

    ui.NXMessageBox.Show(
        "NX Python Example",
        NXOpen.NXMessageBox.DialogType.Information,
        "Datum plane created successfully. Check the Listing Window for details.",
    )


def get_unload_option(_):
    return NXOpen.Session.LibraryUnloadOption.Immediately


if __name__ == "__main__":
    main()
