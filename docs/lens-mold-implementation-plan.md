# Lens Mold Implementation Plan

## Goal

Build a practical NX automation workflow for lens mold development so the process can move from requirement input to surface creation, CAM path generation, and shop-floor validation.

## Target scope

- aspheric lens mold surface modeling
- freeform lens mold surface modeling
- mold insert preparation
- NX CAM tool path generation
- iterative correction after production testing

## Delivery strategy

### Phase 1: Data definition and validation

- define the part naming, unit system, and output folder rules
- define parameter schema for aspheric and freeform surfaces
- define machining assumptions such as blank size, tool families, tolerance, and surface finish targets

### Phase 2: Surface construction foundation

- generate aspheric meridian section data from formula parameters
- create NX point sets and reference geometry from the parameter file
- upgrade the point set into spline and surface creation
- create mold base reference datums and coordinate systems

### Phase 3: Mold part structure

- create insert blank
- trim or combine optical surface with mold body
- create alignment and process reference geometry
- prepare manufacturable part structure

### Phase 4: CAM automation

- define MCS, blank, and part geometry
- define tool libraries for roughing, semi-finishing, and finishing
- create operation templates for optical surface machining
- generate tool paths and export NC output

### Phase 5: Closed-loop adjustment

- compare test results with target surface quality
- adjust geometric parameters and CAM settings
- rerun the workflow with revised parameters

## Recommended first production milestone

The first realistic milestone is:

1. generate an aspheric meridian from parameter input
2. create NX geometry for that meridian
3. build an initial revolved or fitted optical surface
4. prepare one repeatable CAM template for finishing

## Current implementation status

- command line NX automation is working
- batch processing of `.prt` files is working
- point and datum plane creation are verified
- next direct step is parameter-driven optical geometry generation

## Risks to manage early

- optical formula interpretation must match design intent exactly
- unit mistakes will immediately corrupt mold dimensions
- CAM automation depends on NX CAM licensing and stable operation templates
- freeform surfaces need a clear input representation such as grid sag data, point cloud, or polynomial terms

## Information still needed from production intent

- optical surface type: rotational asphere, toric, or freeform
- unit system: mm or inch
- target material and machine type
- roughing and finishing strategy
- tool library constraints
- required surface roughness and tolerance
