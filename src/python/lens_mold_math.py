"""Math helpers for lens mold parameter processing."""

from math import sqrt


def asphere_sag(radius_of_curvature, conic_constant, radial_distance, coefficients=None):
    """Return sag value for a rotational asphere."""
    if radius_of_curvature == 0:
        raise ValueError("radius_of_curvature must not be 0")

    coefficients = coefficients or {}
    curvature = 1.0 / radius_of_curvature
    r2 = radial_distance * radial_distance
    radicand = 1.0 - (1.0 + conic_constant) * (curvature * curvature) * r2

    if radicand < 0:
        raise ValueError(
            f"invalid asphere parameters at radial distance {radial_distance}: radicand {radicand}"
        )

    base = (curvature * r2) / (1.0 + sqrt(radicand))
    extra = 0.0

    for order_text, coefficient in coefficients.items():
        order = int(order_text)
        extra += float(coefficient) * (radial_distance ** order)

    return base + extra


def generate_asphere_profile(radius_of_curvature, conic_constant, semi_diameter, sample_count, coefficients=None):
    """Generate X/Z profile points for a rotational asphere meridian."""
    if sample_count < 2:
        raise ValueError("sample_count must be at least 2")
    if semi_diameter <= 0:
        raise ValueError("semi_diameter must be greater than 0")

    step = (2.0 * semi_diameter) / (sample_count - 1)
    points = []

    for index in range(sample_count):
        x = -semi_diameter + index * step
        r = abs(x)
        z = asphere_sag(radius_of_curvature, conic_constant, r, coefficients)
        points.append((x, 0.0, z))

    return points
