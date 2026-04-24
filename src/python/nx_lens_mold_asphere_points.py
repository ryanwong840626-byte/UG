"""Create sampled asphere meridian points in NX from a JSON parameter file."""

import json
import os

import NXOpen

from lens_mold_math import generate_asphere_profile
from nx_batch_utils import open_target_part, should_save_changes


def write_line(listing_window, text):
    listing_window.WriteLine(str(text))


def load_config():
    config_path = os.environ.get("NX_LENS_CONFIG", "").strip()
    if not config_path:
        raise ValueError("NX_LENS_CONFIG is not set")
    with open(config_path, "r", encoding="utf-8") as handle:
        return json.load(handle), os.path.abspath(config_path)


def main():
    session = NXOpen.Session.GetSession()
    ui = NXOpen.UI.GetUI()
    listing_window = session.ListingWindow
    listing_window.Open()

    write_line(listing_window, "=== NX Lens Mold Asphere Points ===")

    config, config_path = load_config()
    write_line(listing_window, f"Config: {config_path}")

    work_part, opened_path = open_target_part(session, listing_window)
    if opened_path:
        write_line(listing_window, f"Opened in batch mode: {opened_path}")

    if work_part is None:
        ui.NXMessageBox.Show(
            "NX Lens Mold",
            NXOpen.NXMessageBox.DialogType.Warning,
            "No work part is loaded. Provide a target .prt file.",
        )
        return

    asphere = config["asphere"]
    profile_points = generate_asphere_profile(
        radius_of_curvature=float(asphere["radius_of_curvature"]),
        conic_constant=float(asphere["conic_constant"]),
        semi_diameter=float(asphere["semi_diameter"]),
        sample_count=int(asphere["sample_count"]),
        coefficients=asphere.get("coefficients", {}),
    )

    mark_id = session.SetUndoMark(
        NXOpen.Session.MarkVisibility.Visible,
        "Lens Mold Asphere Points",
    )

    created_count = 0
    for x_value, y_value, z_value in profile_points:
        point = work_part.Points.CreatePoint(NXOpen.Point3d(x_value, y_value, z_value))
        point.SetVisibility(NXOpen.SmartObject.VisibilityOption.Visible)
        created_count += 1

    write_line(listing_window, f"Profile points created: {created_count}")
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
        "NX Lens Mold",
        NXOpen.NXMessageBox.DialogType.Information,
        f"Asphere profile point generation completed: {created_count} points.",
    )


def get_unload_option(_):
    return NXOpen.Session.LibraryUnloadOption.Immediately


if __name__ == "__main__":
    main()
