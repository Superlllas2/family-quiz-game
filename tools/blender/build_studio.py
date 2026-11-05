"""
Builds the late night studio set (docs/21_staging.md, "Game night 1 version"; decisions 0015, 0016) and exports
unity/Assets/Game/Art/Models/Studio.fbx. Original design (D49, D93): no real show's set pieces.

Run headless from the repo root (Windows or macOS):
    blender --background --python tools/blender/build_studio.py -- unity/Assets/Game/Art/Models/Studio.fbx [--plan plan.png]

Design space: metres, x to the right as seen from the audience, y away from the audience (the walls are at +y),
z up. Unity receives the same layout as x right, y up, z away from the audience (the export turns the set 180 degrees
about z, because the FBX exporter maps Blender (x, y, z) to Unity (-x, z, -y)).

The walls form a bay (ZONES below): the guests and the desk along the window wall, a 45 degree turn to the curtain
wall, another to the band wall. Each zone is modelled straight in its own design frame and turned into place at the
end, so every number inside a build_* function is in that zone's frame.

Blender only models. Lighting is done and baked in Unity (0015). Empties mark where Unity puts things:
    Seat_1 .. Seat_8   guest seats in fill order (two middle sofa seats, two outer sofa seats, inner armchairs,
                       outer armchairs), at the top of the seat cushion, facing the audience
    Host_konstantin, Host_ilja   host places behind the desk
    LP_<kind>_<n>      practical lights (pendant, floor lamp, desk lamp, sconce, stage lamp, shelf, window)
    LT_Board           what the stage lamps aim at
    LK_<name>, LKT_<name>     Unity lights (keys, fill, uplights, washes) and what each aims at
    CAM_<name>, CAMT_<name>   camera shots (StudioRig) and what each looks at
    Zone_<name>        each zone's design origin and turn, for Unity's light probe grids
Objects named Dyn_* move at runtime (the curtain halves); everything else is static and lightmapped.
Material slot names (M_*) are matched by name in Unity.
"""

import math
import random
import sys

import bmesh
import bpy
from mathutils import Matrix, Vector

random.seed(7)

# ------------------------------------------------------------------------------------------------ helpers

COLLECTION = None

# Zones: each part of the set is modelled in its own straight design frame and then turned into place, so the walls
# form a bay around the cameras (guests and desk along the windows, a 45 degree turn to the curtain wall, another to
# the band). A zone is (pivot in its design frame, where that pivot lands, turn about z in degrees).
CURRENT_ZONE = "World"
OBJ_ZONE = {}
UV_FRAME = {}


def set_zone(name):
    global CURRENT_ZONE
    CURRENT_ZONE = name


def material(name):
    m = bpy.data.materials.get(name)
    if m is None:
        m = bpy.data.materials.new(name)
    return m


class Piece:
    """Geometry gathered into one mesh object with several materials."""

    def __init__(self, name, bevel=0.0, segments=2, smooth=True, subsurf=0):
        self.name = name
        self.subsurf = subsurf
        self.bm = bmesh.new()
        self.mats = []
        self.bevel = bevel
        self.segments = segments
        self.smooth = smooth

    def _mat_index(self, mat):
        if mat not in self.mats:
            self.mats.append(mat)
        return self.mats.index(mat)

    def _tag(self, faces, mat, smooth=None):
        index = self._mat_index(mat)
        for f in faces:
            f.material_index = index
            f.smooth = self.smooth if smooth is None else smooth

    def box(self, x0, x1, y0, y1, z0, z1, mat, rot_z=0.0, pivot=None):
        """An axis aligned box from its two corners, optionally turned about z around pivot (default its centre)."""
        res = bmesh.ops.create_cube(self.bm, size=1.0)
        verts = res["verts"]
        sx, sy, sz = x1 - x0, y1 - y0, z1 - z0
        cx, cy, cz = (x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2
        bmesh.ops.scale(self.bm, vec=Vector((sx, sy, sz)), verts=verts)
        bmesh.ops.translate(self.bm, vec=Vector((cx, cy, cz)), verts=verts)
        if rot_z:
            p = Vector(pivot) if pivot is not None else Vector((cx, cy, cz))
            bmesh.ops.rotate(self.bm, cent=p, matrix=Matrix.Rotation(rot_z, 3, "Z"), verts=verts)
        self._tag({f for v in verts for f in v.link_faces}, mat)
        return verts

    def cyl(self, center, radius, depth, mat, axis="z", segments=24, radius2=None, rot=None, caps=True):
        res = bmesh.ops.create_cone(self.bm, cap_ends=caps, cap_tris=False, segments=segments, radius1=radius,
                                    radius2=radius if radius2 is None else radius2, depth=depth)
        verts = res["verts"]
        if axis == "x":
            bmesh.ops.rotate(self.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(math.pi / 2, 3, "Y"), verts=verts)
        elif axis == "y":
            bmesh.ops.rotate(self.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(math.pi / 2, 3, "X"), verts=verts)
        if rot is not None:
            bmesh.ops.rotate(self.bm, cent=(0, 0, 0), matrix=rot, verts=verts)
        bmesh.ops.translate(self.bm, vec=Vector(center), verts=verts)
        self._tag({f for v in verts for f in v.link_faces}, mat)
        return verts

    def sphere(self, center, radius, mat, scale=(1, 1, 1), subdiv=2):
        res = bmesh.ops.create_icosphere(self.bm, subdivisions=subdiv, radius=radius)
        verts = res["verts"]
        bmesh.ops.scale(self.bm, vec=Vector(scale), verts=verts)
        bmesh.ops.translate(self.bm, vec=Vector(center), verts=verts)
        self._tag({f for v in verts for f in v.link_faces}, mat)
        return verts

    def prism(self, poly, z0, z1, mat):
        """A vertical prism over a 2D polygon (counter clockwise seen from above), from z0 to z1."""
        bottom = [self.bm.verts.new((x, y, z0)) for x, y in poly]
        top = [self.bm.verts.new((x, y, z1)) for x, y in poly]
        n = len(poly)
        faces = [self.bm.faces.new(top), self.bm.faces.new(list(reversed(bottom)))]
        for i in range(n):
            j = (i + 1) % n
            faces.append(self.bm.faces.new((bottom[i], bottom[j], top[j], top[i])))
        self._tag(faces, mat)
        return bottom + top

    def grid(self, points, nu, nv, mat):
        """A quad grid from a (nu+1) x (nv+1) list of points, row by row."""
        vs = [self.bm.verts.new(p) for p in points]
        faces = []
        for j in range(nv):
            for i in range(nu):
                a = j * (nu + 1) + i
                faces.append(self.bm.faces.new((vs[a], vs[a + 1], vs[a + nu + 2], vs[a + nu + 1])))
        self._tag(faces, mat)
        return vs

    def build(self, origin=(0, 0, 0)):
        if not self.subsurf:
            # Separate boxes of a soft piece stay separate shells, or subdivision pinches where corners meet.
            bmesh.ops.remove_doubles(self.bm, verts=self.bm.verts, dist=1e-5)
        bmesh.ops.translate(self.bm, vec=-Vector(origin), verts=self.bm.verts)
        mesh = bpy.data.meshes.new(self.name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(self.name, mesh)
        COLLECTION.objects.link(obj)
        OBJ_ZONE[obj.name] = CURRENT_ZONE
        obj.location = Vector(origin)
        for m in self.mats:
            mesh.materials.append(material(m))
        if self.bevel > 0:
            mod = obj.modifiers.new("Bevel", "BEVEL")
            mod.width = self.bevel
            mod.segments = self.segments
            mod.limit_method = "ANGLE"
            mod.angle_limit = math.radians(40)
            mod.harden_normals = True
            mod.use_clamp_overlap = True
            if self.subsurf:
                mod.harden_normals = False
        if self.subsurf:
            # Soft upholstery: a wide bevel rounded off by one subdivision level.
            sub = obj.modifiers.new("Soft", "SUBSURF")
            sub.levels = self.subsurf
            sub.render_levels = self.subsurf
            for f in mesh.polygons:
                f.use_smooth = True
        return obj


def sag(piece, verts, depth):
    """Presses the top face of a cushion box down towards its front edge (the side at lower y), by up to depth."""
    top = max(v.co.z for v in verts)
    tops = [v for v in verts if v.co.z >= top - 1e-6]
    y0 = min(v.co.y for v in tops)
    y1 = max(v.co.y for v in tops)
    for v in tops:
        v.co.z -= depth * (1.0 - (v.co.y - y0) / max(1e-6, y1 - y0))


def mug(piece, x, y, z, mat, yaw=0.0):
    """A mug standing at height z, with coffee in it and its handle turned yaw radians from +x."""
    piece.cyl((x, y, z + 0.05), 0.042, 0.1, mat, segments=16)
    piece.cyl((x, y, z + 0.1005), 0.036, 0.001, "M_Soil", segments=16)
    v = piece.cyl((0, 0, 0), 0.026, 0.012, mat, axis="y", segments=10, caps=False)
    bmesh.ops.translate(piece.bm, vec=(0.05, 0, 0), verts=v)
    bmesh.ops.rotate(piece.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(yaw, 3, "Z"), verts=v)
    bmesh.ops.translate(piece.bm, vec=(x, y, z + 0.055), verts=v)


def empty(name, loc, yaw=0.0):
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = "ARROWS"
    obj.location = Vector(loc)
    obj.rotation_euler = (0, 0, yaw)
    COLLECTION.objects.link(obj)
    OBJ_ZONE[obj.name] = CURRENT_ZONE
    return obj


def aimed(name, at, target):
    """A light or camera marker, PREFIX_Name, and the point it looks at, PREFIXT_Name (LK_Fill and LKT_Fill)."""
    prefix, rest = name.split("_", 1)
    empty(name, at)
    empty(prefix + "T_" + rest, target)


def pool_can(piece, at, target):
    """A lamp can hung from the ceiling on a drop pipe at at, pointing at target."""
    a, t = Vector(at), Vector(target)
    d = (t - a).normalized()
    rod(piece, (a.x, a.y, a.z + 0.2), (a.x, a.y, WALL_TOP), 0.018, "M_MetalBlack", 8)
    rot = Vector((0, 0, 1)).rotation_difference(d).to_matrix()
    v = piece.cyl((0, 0, 0), 0.14, 0.36, "M_MetalBlack", segments=18)
    bmesh.ops.rotate(piece.bm, cent=(0, 0, 0), matrix=rot, verts=v)
    bmesh.ops.translate(piece.bm, vec=a - d * 0.1, verts=v)
    v = piece.cyl((0, 0, 0), 0.115, 0.01, "M_CanLens", segments=18)
    bmesh.ops.translate(piece.bm, vec=(0, 0, 0.18), verts=v)
    bmesh.ops.rotate(piece.bm, cent=(0, 0, 0), matrix=rot, verts=v)
    bmesh.ops.translate(piece.bm, vec=a - d * 0.1, verts=v)


def beam(name, at, target, end_radius, start_radius=0.09, length=None, segments=28):
    """A visible shaft of light in the haze (Unity's LightBeam shader, M_Beam): an open cone from at towards target,
    length metres long (to the target by default), its UVs' v running 0 at the lamp to 1 at the far end."""
    a, b = Vector(at), Vector(target)
    axis = (b - a).normalized()
    length = length or (b - a).length
    rot = Vector((0, 0, 1)).rotation_difference(axis).to_matrix()
    bm = bmesh.new()
    uv = bm.loops.layers.uv.new()
    rings = []
    for z, r in ((0.0, start_radius), (length, end_radius)):
        rings.append([bm.verts.new(a + rot @ Vector((r * math.cos(2 * math.pi * i / segments),
                                                        r * math.sin(2 * math.pi * i / segments), z)))
                      for i in range(segments)])
    for i in range(segments):
        j = (i + 1) % segments
        f = bm.faces.new((rings[0][i], rings[0][j], rings[1][j], rings[1][i]))
        for loop, (u, v) in zip(f.loops, ((i / segments, 0), ((i + 1) / segments, 0), ((i + 1) / segments, 1), (i / segments, 1))):
            loop[uv].uv = (u, v)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    mesh.materials.append(material("M_Beam"))
    for p in mesh.polygons:
        p.use_smooth = True
    obj = bpy.data.objects.new(name, mesh)
    COLLECTION.objects.link(obj)
    OBJ_ZONE[obj.name] = CURRENT_ZONE
    return obj


PROP_COUNT = {}


def prop(name, x, y, z, yaw=0.0, scale=1.0, tilt=0.0):
    """Marks where Unity puts a bought-in prop (tools/blender/build_props.py, StudioProps.cs): Prop_<Name>_<n> at the
    prop's bottom centre, its front turned yaw degrees from the audience (positive faces +x), tipped back tilt degrees."""
    n = PROP_COUNT.get(name, 0) + 1
    PROP_COUNT[name] = n
    obj = empty("Prop_%s_%d" % (name, n), (x, y, z), math.radians(yaw))
    obj.rotation_euler = (math.radians(tilt), 0.0, math.radians(yaw))   # XYZ: tipped about its own x, then turned
    obj.scale = (scale, scale, scale)
    return obj


def turn_objects(names, pivot, degrees):
    """Turns already built objects (meshes and markers) about z around a design point of their zone, as a group."""
    m = (Matrix.Translation((pivot[0], pivot[1], 0)) @ Matrix.Rotation(math.radians(degrees), 4, "Z")
         @ Matrix.Translation((-pivot[0], -pivot[1], 0)))
    for name in names:
        obj = bpy.data.objects[name]
        obj.matrix_world = m @ obj.matrix_basis
        # UVs stay in the unturned frame, so a rug's pattern and the wood grain turn with the piece.
        UV_FRAME[name] = UV_FRAME.get(name, Matrix.Identity(4)) @ m.inverted()


def inset_polygon(poly, d):
    """A convex counter clockwise polygon moved in by d on every side (negative d grows it)."""
    lines = []
    n = len(poly)
    for i in range(n):
        (ax, ay), (bx, by) = poly[i], poly[(i + 1) % n]
        ex, ey = bx - ax, by - ay
        length = math.hypot(ex, ey)
        nx, ny = -ey / length, ex / length          # the inside is on the left of each edge
        lines.append(((ax + nx * d, ay + ny * d), (ex, ey)))
    out = []
    for i in range(n):
        (p, r), (q, s) = lines[i - 1], lines[i]
        cross = r[0] * s[1] - r[1] * s[0]
        if abs(cross) < 1e-9:
            out.append(q)
            continue
        t = ((q[0] - p[0]) * s[1] - (q[1] - p[1]) * s[0]) / cross
        out.append((p[0] + r[0] * t, p[1] + r[1] * t))
    return out


def edge_strips(piece, poly, edges, z0, z1, mat, out=0.03):
    """Thin glowing strips standing just outside the given edges (indices into poly) of a riser."""
    n = len(poly)
    grown = inset_polygon(poly, -out)
    for i in edges:
        a, b = poly[i], poly[(i + 1) % n]
        ga, gb = grown[i], grown[(i + 1) % n]
        piece.prism([a, ga, gb, b][::-1] if _area([a, ga, gb, b]) < 0 else [a, ga, gb, b], z0, z1, mat)


def _area(poly):
    return 0.5 * sum(poly[i][0] * poly[(i + 1) % len(poly)][1] - poly[(i + 1) % len(poly)][0] * poly[i][1]
                     for i in range(len(poly)))


def arc_points(x0, x1, y_mid, y_end, n=24):
    """A gentle convex arc from x0 to x1: y_end at both ends, y_mid (nearer the audience) in the middle, as a polyline
    from left to right (the front edge of a counter clockwise polygon)."""
    pts = []
    cx, half = (x0 + x1) / 2, (x1 - x0) / 2
    for i in range(n + 1):
        x = x0 + (x1 - x0) * i / n
        u = (x - cx) / half
        pts.append((x, y_mid + (y_end - y_mid) * u * u))
    return pts


def built_since(before):
    """Names of the objects built since the snapshot before (a set of names)."""
    return [o.name for o in COLLECTION.objects if o.name not in before]


def box_uvs(obj):
    """Box projected UVs in metres (1 UV unit = 1 m), from world coordinates, so textures tile at real scale."""
    if obj.type != "MESH":
        return
    mesh = obj.data
    bm = bmesh.new()
    bm.from_mesh(mesh)
    uv = bm.loops.layers.uv.verify()
    mw = UV_FRAME.get(obj.name, Matrix.Identity(4)) @ obj.matrix_world
    for f in bm.faces:
        n = (mw.to_3x3() @ f.normal).normalized()
        ax, ay, az = abs(n.x), abs(n.y), abs(n.z)
        for loop in f.loops:
            p = mw @ loop.vert.co
            if az >= ax and az >= ay:
                loop[uv].uv = (p.x, p.y)
            elif ax >= ay:
                loop[uv].uv = (p.y, p.z)
            else:
                loop[uv].uv = (p.x, p.z)
    bm.to_mesh(mesh)
    bm.free()


# ------------------------------------------------------------------------------------------------ layout numbers

PLATFORM_TOP = 0.18           # the guest and desk riser
STAGE_TOP = 0.45              # the curtain stage floor
WALL_Y = 6.5                  # back of the stage box (stage zone)
WALL_TOP = 6.0                # height of every wall: lower than a real studio, so the room feels cosy
GUEST_WALL_Y = 5.5            # front face of the window wall (guest zone)
GUEST_X = (-10.0, 3.85)       # the window wall's ends (guest zone): it stops just past the desk, so the stage is beside the hosts
BAND_WALL_Y = 6.0             # front face of the band wall (band zone)
BAND_X = (7.17, 14.4)         # the band wall's ends (band zone): long, for a big band stage
CENTER_X = (1.1, 10.6)        # the curtain wall's ends; its front face is the proscenium's (stage zone = world)
TAN_HALF = math.tan(math.radians(22.5))   # a riser's end on the corner bisector moves this far along the wall per metre of depth
CORNER_L = (CENTER_X[0], 3.7)
CORNER_R = (CENTER_X[1], 3.7)
# Every zone's wall end lands on a corner of the curtain wall. A zone's riser stops before the corner by about 0.41 of
# its depth (tan 22.5 degrees), so neighbouring zones never overlap.
ZONES = {
    "World": ((0.0, 0.0), (0.0, 0.0), 0.0),
    "Stage": ((0.0, 0.0), (0.0, 0.0), 0.0),
    "Guests": ((GUEST_X[1], GUEST_WALL_Y), CORNER_L, 45.0),
    "Band": ((BAND_X[0], BAND_WALL_Y), CORNER_R, -45.0),
}
WINDOW_X = (-8.6, -1.0)
WINDOW_Z = (1.05, 4.7)
OPENING_X = (3.4, 7.6)        # the proscenium opening
OPENING_SPRING = 3.3          # where the arch starts
ARCH_RISE = 1.1
PROSC_Y = (3.7, 4.05)
CURTAIN_Y = 4.3
CURTAIN_TOP = 4.55
# 16:9 like the TV, so the Stage shot can fill the screen with it and the question overlay sits inside (D92).
BOARD = dict(x0=3.5, x1=7.5, y=5.9, z0=0.95, z1=3.2)
DESK_CX, DESK_Y = 1.25, 2.55  # desk centre (x) and front face (y), before the desk turns
DESK_TURN = -9.0              # degrees the desk group turns towards the guests (build_desk)
FLOOR_RUG = dict(x0=2.85, x1=7.95, y0=-4.6, y1=-0.4)   # world frame; clear of the guest riser, the stage steps and the band stage
# The curtain stage's apron in front of the arch: its front is a gentle arc, nearest the audience in the middle; its
# ends run along the two corner bisectors, so it fills the curtain wall from the desk to the band.
APRON_FRONT = (1.25, 1.75)    # y of the apron front at the middle and at the ends
STEPS_X = (4.3, 6.7)          # the steps down from the middle of the apron
BAND_DECK_TOP = 0.4           # the band stage
DRUM_RISER_TOP = 0.72         # the drum riser on the band stage, one step higher


# ------------------------------------------------------------------------------------------------ the room

def wall_trim(piece, x0, x1, y0, battens, top=WALL_TOP, wainscot=True):
    """Skirting, a dado rail, a cornice, vertical battens and framed wainscot panels on a wall face at y0."""
    piece.box(x0, x1, y0 - 0.06, y0, 0.0, 0.14, "M_WoodDark")            # skirting
    piece.box(x0, x1, y0 - 0.05, y0, 0.95, 1.02, "M_WoodDark")           # dado rail
    piece.box(x0, x1, y0 - 0.09, y0, top - 0.22, top - 0.12, "M_WoodDark")   # cornice
    piece.box(x0, x1, y0 - 0.05, y0, top - 0.32, top - 0.22, "M_WoodDark")
    for x in battens:
        piece.box(x - 0.04, x + 0.04, y0 - 0.03, y0, 1.02, top - 0.32, "M_WoodDark")
    if wainscot:
        n = max(1, int(round((x1 - x0) / 1.1)))
        w = (x1 - x0) / n
        for i in range(n):
            a, b = x0 + i * w + 0.12, x0 + (i + 1) * w - 0.12
            if b - a < 0.2:
                continue
            for z0, z1 in [(0.26, 0.3), (0.79, 0.83)]:
                piece.box(a, b, y0 - 0.025, y0, z0, z1, "M_WoodDark")
            piece.box(a, a + 0.04, y0 - 0.025, y0, 0.3, 0.79, "M_WoodDark")
            piece.box(b - 0.04, b, y0 - 0.025, y0, 0.3, 0.79, "M_WoodDark")


def panel_frames(piece, x0, x1, y0, z0, z1, width=1.15):
    """Tall raised panels between x0 and x1 on a wall face at y0: a proud moulded frame around a slightly raised field."""
    n = max(1, int(round((x1 - x0) / width)))
    w = (x1 - x0) / n
    f, inset = 0.05, 0.07
    for i in range(n):
        a, b = x0 + i * w + 0.05, x0 + (i + 1) * w - 0.05
        if b - a < 0.3:
            continue
        piece.box(a, b, y0 - 0.03, y0, z1 - f, z1, "M_WoodDark")
        piece.box(a, b, y0 - 0.03, y0, z0, z0 + f, "M_WoodDark")
        piece.box(a, a + f, y0 - 0.03, y0, z0 + f, z1 - f, "M_WoodDark")
        piece.box(b - f, b, y0 - 0.03, y0, z0 + f, z1 - f, "M_WoodDark")
        piece.box(a + f + inset, b - f - inset, y0 - 0.014, y0, z0 + f + inset, z1 - f - inset, "M_WoodPanel")


def rod(piece, a, b, radius, mat, segments=6):
    """A thin cylinder from point a to point b."""
    a, b = Vector(a), Vector(b)
    d = b - a
    v = piece.cyl((0, 0, 0), radius, d.length, mat, segments=segments)
    rot = Vector((0, 0, 1)).rotation_difference(d.normalized()).to_matrix()
    bmesh.ops.rotate(piece.bm, cent=(0, 0, 0), matrix=rot, verts=v)
    bmesh.ops.translate(piece.bm, vec=(a + b) / 2, verts=v)


def box_truss(piece, a, b, size=0.3, pitch=0.5):
    """A square aluminium box truss from a to b (horizontal): four chords and zig-zag braces on each side."""
    a, b = Vector(a), Vector(b)
    axis = (b - a).normalized()
    side = Vector((-axis.y, axis.x, 0)).normalized() * (size / 2)
    up = Vector((0, 0, size / 2))
    corners = [side + up, -side + up, -side - up, side - up]
    for c in corners:
        rod(piece, a + c, b + c, 0.024, "M_Truss", 8)
    n = max(1, int((b - a).length / pitch))
    step = (b - a) / n
    for i in range(n):
        p0, p1 = a + step * i, a + step * (i + 1)
        for k in range(4):
            c0, c1 = corners[k], corners[(k + 1) % 4]
            if i % 2 == 0:
                rod(piece, p0 + c0, p1 + c1, 0.011, "M_Truss", 5)
            else:
                rod(piece, p0 + c1, p1 + c0, 0.011, "M_Truss", 5)


def room_side(p, margin=0.0):
    """True when the design point p is inside the bay, at least margin from every wall."""
    s = math.sqrt(0.5)
    gx, gy = CORNER_L
    bx, by = CORNER_R
    return ((p[0] - gx) * s - (p[1] - gy) * s >= margin and gy - p[1] >= margin
            and -(p[0] - bx) * s - (p[1] - by) * s >= margin)


def clip_to_room(a, b, margin):
    """The part of the straight run a..b inside the bay, or None."""
    inside = [i / 200 for i in range(201) if room_side((a[0] + (b[0] - a[0]) * i / 200, a[1] + (b[1] - a[1]) * i / 200), margin)]
    if not inside:
        return None
    t0, t1 = inside[0], inside[-1]
    return ((a[0] + (b[0] - a[0]) * t0, a[1] + (b[1] - a[1]) * t0), (a[0] + (b[0] - a[0]) * t1, a[1] + (b[1] - a[1]) * t1))


def build_room():
    set_zone("World")
    floor = Piece("Floor", smooth=False)
    floor.box(-14.0, 19.0, -24.0, 8.0, -0.05, 0.0, "M_FloorWood")
    floor.build()
    # A dark ceiling over everything, so no shot sees the city over the top of the walls.
    ceiling = Piece("Ceiling", smooth=False)
    ceiling.box(-14.0, 19.0, -24.0, 8.0, WALL_TOP, WALL_TOP + 0.05, "M_BackCloth")
    ceiling.build()

    # Pilasters on the two corners of the bay, on the bisector, so the turn reads as a design, not a seam.
    posts = Piece("CornerPosts", bevel=0.01)
    for (cx, cy), yaw in ((CORNER_L, math.radians(22.5)), (CORNER_R, math.radians(-22.5))):
        posts.box(cx - 0.26, cx + 0.26, cy - 0.12, cy + 0.3, 0.0, WALL_TOP, "M_WoodDark", yaw, (cx, cy, 0))
    posts.build()

    # Ceiling trusses with lamp cans, kept inside the bay: a TV studio above a living room.
    truss = Piece("Truss", bevel=0.0)
    tz = WALL_TOP - 0.45
    cans = []
    for y in [-4.0, -1.0, 2.0]:
        run = clip_to_room((-16.0, y), (18.0, y), 0.35)
        if run:
            (xa, _), (xb, _) = run
            box_truss(truss, (xa, y, tz + 0.15), (xb, y, tz + 0.15))
            cans += [(x, y) for x in [xa + (xb - xa) * k / 4 for k in (0.5, 1.5, 2.5, 3.5)]]
    for x in [-6.0, -2.0, 2.0, 6.0, 10.0]:
        run = clip_to_room((x, -9.0), (x, 3.5), 0.35)
        if run:
            (_, ya), (_, yb) = run
            box_truss(truss, (x, ya, tz + 0.45), (x, yb, tz + 0.45))
    for x, y in cans:
        truss.cyl((x, y, tz - 0.25), 0.17, 0.42, "M_MetalBlack", segments=16)
        truss.cyl((x, y, tz - 0.465), 0.13, 0.01, "M_CanLens", segments=16)          # the lit lens, seen from below
        truss.box(x - 0.02, x + 0.02, y - 0.02, y + 0.02, tz - 0.05, tz, "M_MetalBlack")
    truss.build()

    # Soft fill from the audience side, like the big soft lights of a TV studio (a Unity area light).
    aimed("LK_Fill", (2.5, -14.0, 4.8), (2.5, 0.0, 1.4))
    # Soft warm pools on the open floor from the truss, so the middle of the studio is not one dark plane.
    # Each hangs from the ceiling on a drop pipe, and its shaft shows in the haze.
    cans = Piece("PoolCans", bevel=0.004)
    for i, (px, py) in enumerate([(3.2, -0.6), (6.6, -2.6), (0.6, -4.4)]):
        at = (px - 0.6, py - 1.2, WALL_TOP - 0.7)
        aimed("LK_FloorPool_%d" % (i + 1), at, (px, py, 0.0))
        pool_can(cans, at, (px, py, 0.0))
        beam("BeamFloorPool%d" % (i + 1), at, (px, py, 0.0), 1.0)
    cans.build()
    # The establishing shot looks into the bay from the front, high, and comes down (Unity lerps between the two).
    aimed("CAM_EstablishStart", (1.8, -21.5, 5.55), (2.4, 0.0, 1.3))   # below the ceiling (WALL_TOP)
    aimed("CAM_EstablishEnd", (1.9, -18.6, 4.1), (2.4, 0.0, 1.1))

    build_guest_wall()
    build_curtain_wall()
    build_band_wall()


def build_guest_wall():
    """The window wall behind the guests and the desk (guest zone)."""
    set_zone("Guests")
    wall = Piece("GuestWall", bevel=0.01)
    t = 0.3
    y0, y1 = GUEST_WALL_Y, GUEST_WALL_Y + t
    x0, x1 = GUEST_X
    top = WALL_TOP
    wx0, wx1 = WINDOW_X
    wz0, wz1 = WINDOW_Z
    wall.box(x0, wx0, y0, y1, 0, top, "M_WoodPanel")
    wall.box(wx1, x1, y0, y1, 0, top, "M_WoodPanel")
    wall.box(wx0, wx1, y0, y1, 0, wz0, "M_WoodPanel")
    wall.box(wx0, wx1, y0, y1, wz1, top, "M_WoodPanel")
    wall_trim(wall, x0, x1, y0, [wx0 - 0.35, wx1 + 0.35], top)
    wall.box(x0 - 0.3, x0, y0 - 0.12, y1, 0, top, "M_WoodDark")        # end pilaster
    wall.build()

    # Window: a moulded casing on the room face, a deep reveal (the wall's own thickness), a deep sill board, and
    # the steel frame and mullions set back in the reveal, with glass that catches the room's reflections.
    win = Piece("Window", bevel=0.008)
    c = 0.16
    win.box(wx0 - c, wx1 + c, y0 - 0.05, y0, wz1, wz1 + c, "M_WoodDark")                 # casing
    win.box(wx0 - c, wx0, y0 - 0.05, y0, wz0 - 0.06, wz1, "M_WoodDark")
    win.box(wx1, wx1 + c, y0 - 0.05, y0, wz0 - 0.06, wz1, "M_WoodDark")
    win.box(wx0 - c - 0.04, wx1 + c + 0.04, y0 - 0.07, y0 - 0.05, wz1 + c - 0.03, wz1 + c, "M_WoodDark")
    win.box(wx0 - 0.06, wx1 + 0.06, y0 - 0.16, y0 + 0.2, wz0 - 0.05, wz0, "M_WoodDark")    # sill board
    win.box(wx0, wx1, y0 - 0.1, y0 - 0.04, wz0 - 0.16, wz0 - 0.05, "M_WoodDark")          # apron under it
    fy0, fy1 = y0 + 0.17, y0 + 0.25
    win.box(wx0, wx1, fy0, fy1, wz0, wz0 + 0.06, "M_MetalBlack")
    win.box(wx0, wx1, fy0, fy1, wz1 - 0.06, wz1, "M_MetalBlack")
    win.box(wx0, wx0 + 0.06, fy0, fy1, wz0, wz1, "M_MetalBlack")
    win.box(wx1 - 0.06, wx1, fy0, fy1, wz0, wz1, "M_MetalBlack")
    n = 6
    for i in range(1, n):
        x = wx0 + (wx1 - wx0) * i / n
        win.box(x - 0.03, x + 0.03, fy0, fy1, wz0, wz1, "M_MetalBlack")
    for z in [wz0 + (wz1 - wz0) * 0.36, wz0 + (wz1 - wz0) * 0.7]:
        win.box(wx0, wx1, fy0 + 0.01, fy1 - 0.01, z - 0.022, z + 0.022, "M_MetalBlack")
    win.build()
    glass = Piece("GlassWindow", smooth=False)
    glass.box(wx0 + 0.03, wx1 - 0.03, fy0 + 0.035, fy0 + 0.04, wz0 + 0.03, wz1 - 0.03, "M_Glass")
    glass.build()
    panels = Piece("GuestWallPanels", bevel=0.004)
    panel_frames(panels, x0 + 0.12, wx0 - c - 0.25, y0, 1.25, top - 0.5)
    panels.build()
    empty("LP_Window_1", ((wx0 + wx1) / 2, GUEST_WALL_Y + 0.4, (wz0 + wz1) / 2))

    build_city()

    # Uplights grazing the wall beside the window.
    for i, x in enumerate([WINDOW_X[0] - 0.35, WINDOW_X[1] + 0.38]):
        aimed("LK_Uplight_G%d" % (i + 1), (x, GUEST_WALL_Y - 0.3, 0.25), (x, GUEST_WALL_Y + 0.05, 5.0))


# The old 34 m picture 10.5 m behind the wall, scaled up 28.6 times about the window so it looks the same, 300 m away.
CITY_BACKDROP = dict(x0=-613.0, x1=359.0, y=GUEST_WALL_Y + 300.0, z0=-137.0, z1=349.0)


def build_city():
    """The invented evening city behind the window (guest zone): the far skyline picture on a big backdrop, and three
    rows of lit blocks in front of it at 130 to 250 m (far enough that their windows are as fine as the skyline's), so the view shifts with the camera. The studio is high up: the
    blocks stand on ground far below the window. No real skyline (D93)."""
    b = CITY_BACKDROP
    city = Piece("CityBackdrop", smooth=False)
    city.box(b["x0"], b["x1"], b["y"], b["y"] + 0.1, b["z0"], b["z1"], "M_City")
    city.build()
    rnd = random.Random(5)
    for row, (depth, mat) in enumerate([(130.0, "M_CityBuilding"), (190.0, "M_CityBuilding"), (250.0, "M_CityBuildingFar")]):
        blocks = Piece("CityBlocks%d" % (row + 1), smooth=False)
        span = 30.0 + depth * 1.3
        x = WINDOW_X[0] - span
        while x < WINDOW_X[1] + span:
            w = rnd.uniform(18.0, 40.0) + row * 8
            d = rnd.uniform(14.0, 26.0)
            # Low, so the far skyline keeps the top of the window; the odd tower breaks the line.
            top = rnd.uniform(-0.05, 0.05) * depth + (rnd.random() < 0.12) * rnd.uniform(0.04, 0.09) * depth
            y0 = GUEST_WALL_Y + depth + rnd.uniform(-8.0, 8.0)
            blocks.box(x, x + w, y0, y0 + d, -200.0, top, mat)
            if rnd.random() < 0.35:
                # A setback tower on top.
                inset = w * rnd.uniform(0.2, 0.35)
                blocks.box(x + inset, x + w - inset, y0 + 2, y0 + d - 2, top, top + rnd.uniform(6, 16), mat)
            x += w + rnd.uniform(3.0, 10.0)
        blocks.build()


def fit_uvs(obj, x0, x1, z0, z1):
    """UVs 0..1 across a backdrop facing the room, instead of the metre box projection."""
    mesh = obj.data
    bm = bmesh.new()
    bm.from_mesh(mesh)
    uv = bm.loops.layers.uv.verify()
    for f in bm.faces:
        for loop in f.loops:
            p = obj.matrix_world @ loop.vert.co
            loop[uv].uv = ((p.x - x0) / (x1 - x0), (p.z - z0) / (z1 - z0))
    bm.to_mesh(mesh)
    bm.free()


def build_curtain_wall():
    """The wall around the proscenium, between the two corners (stage zone, which is the world frame)."""
    set_zone("Stage")
    wall = Piece("CurtainWall", bevel=0.01)
    y0, y1 = PROSC_Y
    panels = Piece("CurtainWallPanels", bevel=0.004)
    for a, b in [(CENTER_X[0], 2.55), (8.45, CENTER_X[1])]:
        wall.box(a, b, y0, y1, 0, WALL_TOP, "M_WoodPanel")
        wall_trim(wall, a, b, y0, [], WALL_TOP)
        panel_frames(panels, a + 0.2, b - 0.1, y0, 1.25, WALL_TOP - 0.5)
    wall.box(2.55, 8.45, y0 - 0.09, y0, WALL_TOP - 0.22, WALL_TOP - 0.12, "M_WoodDark")
    wall.build()
    panels.build()
    # Uplights stand on the apron, grazing the walls beside the arch.
    for i, x in enumerate([1.85, 9.55]):
        aimed("LK_Uplight_C%d" % (i + 1), (x, y0 - 0.3, STAGE_TOP + 0.08), (x, y0 + 0.05, 5.0))


def build_band_wall():
    set_zone("Band")
    wall = Piece("BandWall", bevel=0.01)
    y0, y1 = BAND_WALL_Y, BAND_WALL_Y + 0.3
    x0, x1 = BAND_X
    wall.box(x0, x1, y0, y1, 0, WALL_TOP, "M_WoodPanel")
    wall_trim(wall, x0, x1, y0, [], WALL_TOP)
    wall.box(x1, x1 + 0.3, y0 - 0.12, y1, 0, WALL_TOP, "M_WoodDark")    # end pilaster
    wall.build()
    panels = Piece("BandWallPanels", bevel=0.004)
    panel_frames(panels, x0 + 0.25, x1 - 0.1, y0, 1.25, WALL_TOP - 0.5)
    panels.build()
    aimed("LK_Uplight_B1", (x1 - 0.45, y0 - 0.3, BAND_DECK_TOP + 0.08), (x1 - 0.45, y0 + 0.05, 5.0))


# ------------------------------------------------------------------------------------------------ guest area

def build_guest_area():
    set_zone("Guests")
    # The riser under the guests and the desk, with a warm light strip along its front edge.
    # Its end by the stage runs along the corner bisector, so the riser and the stage's apron meet edge to edge.
    riser = Piece("GuestRiser", bevel=0.02)
    rx_front = GUEST_X[1] - 0.06 - TAN_HALF * (GUEST_WALL_Y - 2.0)
    riser_poly = [(-9.95, 2.0), (rx_front, 2.0), (GUEST_X[1] - 0.06, GUEST_WALL_Y), (-9.95, GUEST_WALL_Y)]
    riser.prism(riser_poly, 0.0, PLATFORM_TOP, "M_WoodDark")
    riser.build()
    strip = Piece("GuestRiserStrip", smooth=False)
    edge_strips(strip, riser_poly, [0, 1], 0.05, 0.11, "M_GlowStrip")
    strip.build()

    # Keys and cameras for the guests and hosts (Unity reads these markers; docs/21_staging.md, shot table).
    aimed("LK_GuestKey", (-4.0, -2.2, 5.5), (-4.6, 3.7, 0.7))
    # A cool rim from above the window side, so the guests stand out from the wall; and a warm pool on the floor in
    # front of the riser, so the bottom of the Wide shot is not one bare plane.
    aimed("LK_RimGuests", (-4.6, 5.25, 4.9), (-4.6, 2.9, 0.7))
    aimed("LK_FloorPool_4", (-4.2, -0.6, WALL_TOP - 0.7), (-4.8, 0.9, 0.0))
    cans = Piece("PoolCansGuests", bevel=0.004)
    pool_can(cans, (-4.2, -0.6, WALL_TOP - 0.7), (-4.8, 0.9, 0.0))
    cans.build()
    beam("BeamFloorPool4", (-4.2, -0.6, WALL_TOP - 0.7), (-4.8, 0.9, 0.0), 1.0)
    # Far enough back that every answer card fits, from the outer left armchair to the desk.
    aimed("CAM_Wide", (-3.35, -7.6, 2.55), (-3.35, 3.6, 1.3))
    aimed("CAM_Seats", (-4.6, -5.6, 1.45), (-4.6, 3.6, 1.45))

    rug = Piece("Rug", bevel=0.004)
    rug.box(-9.0, -0.9, 2.1, 4.9, PLATFORM_TOP, PLATFORM_TOP + 0.012, "M_Rug")
    rug.build()

    seat_z = PLATFORM_TOP + 0.012
    # The sofa: a long tufted sofa re-tinted the muted dark green of D93, deep, so the guests sit well back from the
    # coffee table (a bought-in model, tools/blender/build_props.py; its seat is 0.55 m up, its seats 0.74 m apart).
    sofa_cx, sofa_front = -4.65, 3.1
    prop("LongSofa", sofa_cx, sofa_front + 0.75, seat_z)
    sofa_seats = [sofa_cx + dx for dx in (-1.12, -0.38, 0.38, 1.12)]
    sofa_y, sofa_top = sofa_front + 0.5, seat_z + 0.55

    # Armchairs: two on each side in an arc that comes forward at the ends, turned in towards the sofa so the guests
    # sit as one conversation group (a positive turn faces +x). The outer pair are classic upholstered tub chairs, the
    # inner pair leather and wood, so they are not a matched set; the right pair turns more, towards the desk.
    arm_specs = [(-8.5, 2.82, 21, "ArmChair", 0.77, 0.5), (-7.38, 3.08, 11, "ModernArmChair", 0.99, 0.55),
                 (-1.85, 3.1, -13, "ModernArmChair", 0.99, 0.55), (-0.75, 2.86, -24, "ArmChair", 0.77, 0.5)]
    seat_points = []
    for cx, cy, yaw, model, depth, seat in arm_specs:
        prop(model, cx, cy + depth / 2, seat_z, yaw)
        seat_points.append((cx, cy + 0.34, math.radians(yaw), seat_z + seat))

    # Seat markers in fill order (docs/21_staging.md, "Seats").
    order = [
        (sofa_seats[1], sofa_y, 0.0, sofa_top), (sofa_seats[2], sofa_y, 0.0, sofa_top),
        (sofa_seats[0], sofa_y, 0.0, sofa_top), (sofa_seats[3], sofa_y, 0.0, sofa_top),
        seat_points[1], seat_points[2],
        seat_points[0], seat_points[3],
    ]
    for i, (x, y, yaw, z) in enumerate(order):
        empty("Seat_%d" % (i + 1), (x, y, z), yaw)

    # A low coffee table in front of the sofa, below the name tags, set a few degrees off square as if someone nudged
    # it: a chess game left half played, a candle and a mug on a coaster.
    before = {o.name for o in COLLECTION.objects}
    tcx, tcy = sofa_cx, 2.45
    tz = seat_z + 0.39
    prop("CoffeeTable", tcx, tcy, seat_z)
    prop("ChessSet", tcx - 0.28, tcy, tz, 12)
    prop("Candlestick", tcx + 0.3, tcy + 0.1, tz)
    light_marker("Candle", (tcx + 0.3, tcy + 0.1, tz + 0.26))
    deco = Piece("CoffeeTableProps", bevel=0.003)
    deco.cyl((tcx + 0.42, tcy - 0.12, tz + 0.004), 0.045, 0.008, "M_Cork", segments=16)
    mug(deco, tcx + 0.42, tcy - 0.12, tz + 0.008, "M_MugB", 2.3)
    deco.build()
    turn_objects(built_since(before), (tcx, tcy), -3.0)

    # Side tables at the sofa's arms, set back between the sofa and the armchairs, each with a glowing oil lamp.
    prop("Nightstand", -6.72, 4.25, seat_z)
    prop("OilLamp", -6.78, 4.22, seat_z + 0.7)
    prop("StandingFrame", -6.55, 4.3, seat_z + 0.7, -20)
    empty("LP_TableLamp_1", (-6.78, 4.22, seat_z + 0.7 + 0.52))
    prop("SideTable", -2.62, 4.25, seat_z, 7)
    prop("OilLamp", -2.7, 4.25, seat_z + 0.55)
    prop("VaseWhite", -2.45, 4.32, seat_z + 0.55)
    empty("LP_TableLamp_3", (-2.7, 4.25, seat_z + 0.55 + 0.52))

    # Floor lamps behind the outer armchairs, a big plant in the far corner of the window wall.
    for i, (x, y) in enumerate([(-9.2, 4.3), (-0.2, 4.55)]):
        prop("FloorLamp", x, y, PLATFORM_TOP)
        empty("LP_FloorLamp_%d" % (i + 1), (x, y, PLATFORM_TOP + 1.38))
    prop("PlantWide", -9.62, 5.05, PLATFORM_TOP, 30)

    # A chandelier with little fabric shades over the coffee table, on a rod from the ceiling.
    chz = 2.75
    prop("Chandelier", sofa_cx, 3.55, chz)
    rod_piece = Piece("ChandelierRod", bevel=0.0)
    rod_piece.cyl((sofa_cx, 3.55, (chz + 0.8 + WALL_TOP) / 2), 0.008, WALL_TOP - chz - 0.8, "M_MetalBrass", segments=8)
    rod_piece.cyl((sofa_cx, 3.55, WALL_TOP - 0.03), 0.07, 0.05, "M_MetalBrass", segments=16)
    rod_piece.build()
    empty("LP_Pendant_1", (sofa_cx, 3.55, chz + 0.35))

    # Candles on the window sill: a brass candelabra set at the left end, two wooden candlesticks at the right.
    sill = WINDOW_Z[0]
    prop("Candles", -7.75, GUEST_WALL_Y + 0.02, sill, 0)
    light_marker("Candle", (-7.75, GUEST_WALL_Y - 0.02, sill + 0.7))
    for i, x in enumerate([-1.75, -1.5]):
        prop("Candlestick", x, GUEST_WALL_Y + 0.01 - 0.05 * i, sill, 0, 1.0 - 0.18 * i)
    light_marker("Candle", (-1.62, GUEST_WALL_Y, sill + 0.26))


# ------------------------------------------------------------------------------------------------ the desk

def build_desk():
    set_zone("Guests")
    before = {o.name for o in COLLECTION.objects}
    # The hosts' key and solo shots are built with the desk, so they turn with it.
    aimed("LK_HostKey", (1.0, 0.2, 5.0), (1.25, 3.9, 1.0))
    aimed("LK_RimHosts", (1.25, 5.15, 4.3), (1.25, 3.3, 1.2))
    aimed("CAM_HostSoloKonstantin", (0.55, 0.5, 1.62), (0.7, 3.85, 1.32))
    aimed("CAM_HostSoloIlja", (1.95, 0.5, 1.62), (1.8, 3.85, 1.32))
    z0 = PLATFORM_TOP + 0.01
    x0, x1 = DESK_CX - 1.25, DESK_CX + 1.25
    top_z = z0 + 1.0
    front = DESK_Y
    back = DESK_Y + 0.85

    desk = Piece("Desk", bevel=0.012)
    desk.box(x0 - 0.05, x1 + 0.05, front - 0.06, back, top_z, top_z + 0.06, "M_WoodWarm")        # top
    desk.box(x0, x1, front + 0.06, back - 0.05, z0, top_z, "M_WoodDark")                           # body
    desk.box(x0 - 0.02, x1 + 0.02, front + 0.02, back - 0.02, z0, z0 + 0.06, "M_WoodDark")         # plinth
    desk.build()

    # Ribbed front and ends: half round flutes, the warm strip under the top washes over them.
    flutes = Piece("DeskFlutes", bevel=0.0)
    pitch = 0.05
    n = int((x1 - x0) / pitch)
    h = top_z - (z0 + 0.06) - 0.06
    zc = z0 + 0.06 + h / 2
    for i in range(n):
        x = x0 + pitch * (i + 0.5)
        flutes.cyl((x, front + 0.06, zc), 0.022, h, "M_WoodWarm", segments=10)
    for side_x in [x0 - 0.0, x1 + 0.0]:
        for j in range(int((back - front - 0.2) / pitch)):
            y = front + 0.12 + pitch * j
            flutes.cyl((side_x, y, zc), 0.022, h, "M_WoodWarm", segments=10)
    flutes.build()

    strip = Piece("DeskStrip", smooth=False)
    strip.box(x0, x1, front + 0.0, front + 0.03, top_z - 0.018, top_z - 0.004, "M_GlowStrip")
    strip.build()
    empty("LP_DeskStrip_1", (DESK_CX, front - 0.1, top_z - 0.08))

    # Desk lamp and two mugs.
    acc = Piece("DeskProps", bevel=0.004)
    # A brass banker's lamp with a green glass shade, tipped towards the desk top.
    lx, ly = x1 - 0.32, back - 0.24
    acc.cyl((lx, ly, top_z + 0.075), 0.085, 0.03, "M_MetalBrass", segments=20)
    acc.cyl((lx, ly, top_z + 0.24), 0.011, 0.3, "M_MetalBrass", segments=8)
    v = acc.cyl((0, 0, 0), 0.07, 0.34, "M_LampShadeGreen", axis="x", segments=20)
    bmesh.ops.scale(acc.bm, vec=(1.0, 1.15, 0.7), verts=v)
    bmesh.ops.rotate(acc.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(0.25, 3, "X"), verts=v)
    bmesh.ops.translate(acc.bm, vec=(lx, ly - 0.05, top_z + 0.42), verts=v)
    # Two different mugs, their handles turned anyhow.
    mug(acc, DESK_CX - 0.55, front + 0.25, top_z + 0.06, "M_MugA", 2.6)
    mug(acc, DESK_CX + 0.35, front + 0.22, top_z + 0.06, "M_MugB", 0.4)
    acc.box(DESK_CX - 0.2, DESK_CX + 0.15, front + 0.2, front + 0.45, top_z + 0.06, top_z + 0.068, "M_Paper")
    # A small stack of cue cards in front of each host, squared up badly, and a pen.
    for cx_, cy_, rot in ((DESK_CX - 0.78, front + 0.36, 0.12), (DESK_CX + 0.62, front + 0.32, -0.2)):
        for k in range(4):
            z = top_z + 0.06 + k * 0.0025
            acc.box(cx_ - 0.1, cx_ + 0.1, cy_ - 0.065, cy_ + 0.065, z, z + 0.002, "M_CueCard", rot + 0.06 * (k % 2) - 0.03 * k)
    acc.cyl((DESK_CX + 0.6, front + 0.47, top_z + 0.066), 0.0045, 0.14, "M_MetalBlack", axis="x", segments=6)
    # A retro desk microphone between the hosts.
    mx, my = DESK_CX, front + 0.32
    acc.cyl((mx, my, top_z + 0.075), 0.07, 0.025, "M_MetalBlack", segments=20)
    acc.cyl((mx, my, top_z + 0.17), 0.008, 0.17, "M_Chrome", segments=8)
    v = acc.cyl((0, 0, 0), 0.032, 0.09, "M_Chrome", segments=16, axis="y")
    bmesh.ops.rotate(acc.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(-0.5, 3, "X"), verts=v)
    bmesh.ops.translate(acc.bm, vec=(mx, my - 0.02, top_z + 0.27), verts=v)
    acc.build()
    empty("LP_DeskLamp_1", (lx, ly - 0.05, top_z + 0.36))

    # Host chairs behind the desk (the cube hosts sit on them).
    chairs = Piece("HostChairs", bevel=0.02)
    for i, x in enumerate([DESK_CX - 0.55, DESK_CX + 0.55]):
        cy = back + 0.45
        chairs.box(x - 0.3, x + 0.3, cy - 0.28, cy + 0.28, z0 + 0.42, z0 + 0.52, "M_Leather")
        chairs.box(x - 0.3, x + 0.3, cy + 0.2, cy + 0.3, z0 + 0.52, z0 + 1.25, "M_Leather")
        chairs.cyl((x, cy, z0 + 0.22), 0.035, 0.4, "M_MetalBlack", segments=10)
        chairs.cyl((x, cy, z0 + 0.03), 0.28, 0.04, "M_MetalBlack", segments=5)
    chairs.build()
    empty("Host_konstantin", (DESK_CX - 0.55, back + 0.4, z0 + 0.52), 0.0)
    empty("Host_ilja", (DESK_CX + 0.55, back + 0.4, z0 + 0.52), 0.0)
    # The desk, its rug, the hosts and their shots turn a little towards the guests, so the hosts and the sofa read as
    # one conversation (a negative turn faces the desk front towards -x, the guests).
    prop("PlantSmall", DESK_CX - 1.0, front + 0.62, top_z + 0.06, 0)
    rug2 = Piece("RugDesk", bevel=0.004)
    rug2.box(-0.15, 2.45, 2.3, 4.6, PLATFORM_TOP, PLATFORM_TOP + 0.01, "M_Rug2")
    rug2.build()
    turn_objects(built_since(before), (DESK_CX, (front + back) / 2), DESK_TURN)

    # Bookshelf behind the hosts: the cosy backdrop of the host solo shot.
    shelf = Piece("HostShelf", bevel=0.006)
    sx0, sx1 = DESK_CX - 1.5, DESK_CX + 1.5
    sy0, sy1 = GUEST_WALL_Y - 0.42, GUEST_WALL_Y - 0.02
    shelf.box(sx0, sx0 + 0.05, sy0, sy1, 0, 3.6, "M_WoodDark")
    shelf.box(sx1 - 0.05, sx1, sy0, sy1, 0, 3.6, "M_WoodDark")
    shelf.box((sx0 + sx1) / 2 - 0.025, (sx0 + sx1) / 2 + 0.025, sy0, sy1, 0, 3.6, "M_WoodDark")
    shelf.box(sx0, sx1, sy1 - 0.03, sy1, 0, 3.6, "M_WoodDark")
    levels = [0.0, 0.75, 1.5, 2.25, 2.95, 3.6]
    for z in levels:
        shelf.box(sx0, sx1, sy0, sy1, z, z + 0.04, "M_WoodDark")
    shelf.build()
    # Bought-in pieces among the books: an encyclopedia set, a mantel clock, vases and a small plant, each in a gap
    # the books leave for it (level, x0, x1 in the shelf's frame).
    sy_back = sy1 - 0.1
    shelf_props = [(1, "Encyclopedia", sx0 + 0.62, 0.0, 0.62), (1, "VaseAntique", sx1 - 0.35, 0.0, 0.32),
                   (2, "MantelClock", sx1 - 0.55, 0.0, 0.42), (2, "VaseWhite", sx0 + 0.3, 0.0, 0.26),
                   (3, "Encyclopedia", sx1 - 0.65, -6.0, 0.62), (3, "PlantSmall", sx0 + 0.3, 0.0, 0.24),
                   (4, "PlantSmall", sx1 - 0.35, 30.0, 0.24), (4, "VaseBrass", sx0 + 0.45, 0.0, 0.26)]
    skips = []
    for level, name, x, yaw, width in shelf_props:
        prop(name, x, sy_back - (0.05 if name == "Encyclopedia" else 0.1), levels[level] + 0.04, yaw)
        skips.append((level, x - width / 2 - 0.03, x + width / 2 + 0.03))
    fill_books("HostShelfBooks", sx0 + 0.07, sx1 - 0.07, sy0 + 0.04, sy1 - 0.06, levels[:-1], skip_mid=(sx0 + sx1) / 2,
               skips=skips)
    prop("VaseBrass", sx1 - 0.4, sy_back - 0.08, levels[-1] + 0.04)
    for i, z in enumerate([0.79, 2.29]):
        empty("LP_Shelf_%d" % (i + 1), ((sx0 + sx1) / 2, sy0 + 0.1, levels[[1, 3][i]] + 0.62))


def fill_books(name, x0, x1, y0, y1, levels, skip_mid=None, skips=()):
    books = Piece(name, bevel=0.003)
    mats = ["M_BookA", "M_BookB", "M_BookC", "M_BookD", "M_BookE"]
    for li, z in enumerate(levels):
        x = x0
        while x < x1 - 0.05:
            if skip_mid is not None and abs(x - skip_mid) < 0.06:
                x += 0.08
                continue
            gap = next((b for level, a, b in skips if level == li and a - 0.05 <= x < b), None)
            if gap is not None:
                x = gap
                continue
            if random.random() < 0.12:
                # A gap with a small object instead of books.
                if random.random() < 0.5:
                    books.cyl((x + 0.08, (y0 + y1) / 2, z + 0.04 + 0.09), 0.06, 0.18, "M_Ceramic", segments=14)
                x += 0.22
                continue
            run = random.randint(4, 11)
            for _ in range(run):
                wb = random.uniform(0.025, 0.055)
                hb = random.uniform(0.2, 0.32)
                db = random.uniform(0.17, (y1 - y0))
                if x + wb > x1 or (skip_mid is not None and abs(x + wb - skip_mid) < 0.05) \
                        or any(level == li and a <= x + wb and x < b for level, a, b in skips):
                    break
                books.box(x, x + wb, y1 - db, y1, z + 0.04, z + 0.04 + hb, random.choice(mats))
                if wb > 0.03 and random.random() < 0.45:
                    # Gold tooled bands across the spine.
                    for fz in (0.12, 0.84):
                        bz = z + 0.04 + hb * fz
                        books.box(x + 0.002, x + wb - 0.002, y1 - db - 0.0015, y1 - db + 0.002, bz, bz + 0.007, "M_MetalBrass")
                x += wb + 0.003
            if random.random() < 0.35 and x < x1 - 0.3:
                # A leaning or stacked group.
                for k in range(random.randint(2, 4)):
                    books.box(x + 0.02, x + 0.26, y1 - 0.2, y1, z + 0.04 + k * 0.04, z + 0.08 + k * 0.04, random.choice(mats))
                x += 0.3
            x += random.uniform(0.02, 0.12)
    books.build()


# ------------------------------------------------------------------------------------------------ the curtain stage

def build_stage():
    set_zone("Stage")
    aimed("LK_CurtainWash", (5.5, 0.2, WALL_TOP - 0.4), (5.5, 4.3, 2.4))
    beam("BeamCurtainWash", (5.5, 0.2, WALL_TOP - 0.4), (5.5, 4.3, 2.4), 1.1, 0.14)
    aimed("CAM_Curtain", (5.5, -4.0, 2.3), (5.5, 4.0, 2.55))
    aimed("CAM_Stage", (5.5, -2.0, 2.075), (5.5, 5.9, 2.075))
    aimed("CAM_StagePushIn", (5.5, -1.65, 2.075), (5.5, 5.9, 2.075))
    stage = Piece("Stage", bevel=0.015)
    stage.box(2.75, 8.25, PROSC_Y[0], WALL_Y, 0.0, STAGE_TOP, "M_WoodDark")             # behind the arch
    stage.box(2.85, 8.15, PROSC_Y[0], WALL_Y, STAGE_TOP - 0.005, STAGE_TOP, "M_StageFloor")
    apron = apron_polygon()
    stage.prism(apron, 0.0, STAGE_TOP, "M_WoodDark")
    stage.prism(inset_polygon(apron, 0.1), STAGE_TOP - 0.005, STAGE_TOP + 0.004, "M_StageFloor")
    # Steps down from the middle of the apron to the floor where the winner walks.
    sx0, sx1 = STEPS_X
    front = APRON_FRONT[0]
    rise = STAGE_TOP / 3
    for k in range(2):
        stage.box(sx0 + 0.15 * (1 - k), sx1 - 0.15 * (1 - k), front - 0.32 * (2 - k), front + 0.05, 0.0, rise * (k + 1), "M_WoodDark")
        stage.box(sx0 + 0.15 * (1 - k) + 0.05, sx1 - 0.15 * (1 - k) - 0.05, front - 0.32 * (2 - k) + 0.04, front - 0.32 * (1 - k),
                  rise * (k + 1) - 0.004, rise * (k + 1) + 0.003, "M_StageFloor")
    stage.build()
    strip = Piece("StageStrip", smooth=False)
    edge_strips(strip, apron, [i for i in range(len(apron)) if i != len(apron) - 2], 0.2, 0.26, "M_GlowStrip")
    strip.build()

    # Proscenium: a wooden front with an arched opening (original shape: no bulbs around it, D93).
    plate = Piece("Proscenium", bevel=0.0, smooth=False)
    plate.box(2.55, 8.45, PROSC_Y[0], PROSC_Y[1], 0.0, WALL_TOP, "M_WoodPanel")
    plate_obj = plate.build()
    cut = Piece("ProsceniumCut", smooth=False)
    cx = (OPENING_X[0] + OPENING_X[1]) / 2
    cut.box(OPENING_X[0], OPENING_X[1], PROSC_Y[0] - 0.5, PROSC_Y[1] + 0.5, STAGE_TOP, OPENING_SPRING, "M_WoodPanel")
    v = cut.cyl((0, 0, 0), (OPENING_X[1] - OPENING_X[0]) / 2, 1.4, "M_WoodPanel", axis="y", segments=64)
    bmesh.ops.scale(cut.bm, vec=Vector((1, 1, ARCH_RISE / ((OPENING_X[1] - OPENING_X[0]) / 2))), verts=v)
    bmesh.ops.translate(cut.bm, vec=Vector((cx, (PROSC_Y[0] + PROSC_Y[1]) / 2, OPENING_SPRING)), verts=v)
    cut_obj = cut.build()
    mod = plate_obj.modifiers.new("Arch", "BOOLEAN")
    mod.operation = "DIFFERENCE"
    mod.solver = "EXACT"
    mod.use_self = True
    mod.object = cut_obj
    bpy.context.view_layer.objects.active = plate_obj
    bpy.ops.object.modifier_apply(modifier="Arch")
    bpy.data.objects.remove(cut_obj, do_unlink=True)
    bev = plate_obj.modifiers.new("Bevel", "BEVEL")
    bev.width = 0.02
    bev.segments = 2
    bev.limit_method = "ANGLE"
    bev.harden_normals = True
    for f in plate_obj.data.polygons:
        f.use_smooth = True

    # A moulding around the arch: a band of small boxes along the curve.
    mould = Piece("ProsceniumTrim", bevel=0.006)
    rx = (OPENING_X[1] - OPENING_X[0]) / 2
    pts = []
    for i in range(41):
        a = math.pi * i / 40
        pts.append((cx + (rx + 0.06) * math.cos(a), OPENING_SPRING + (ARCH_RISE + 0.06) * math.sin(a)))
    for (xa, za), (xb, zb) in zip(pts, pts[1:]):
        mx, mz = (xa + xb) / 2, (za + zb) / 2
        length = math.hypot(xb - xa, zb - za) + 0.01
        ang = math.atan2(zb - za, xb - xa)
        v = mould.box(mx - length / 2, mx + length / 2, PROSC_Y[0] - 0.05, PROSC_Y[0], mz - 0.06, mz + 0.06, "M_MetalBrass")
        bmesh.ops.rotate(mould.bm, cent=(mx, 0, mz), matrix=Matrix.Rotation(-ang, 3, "Y"), verts=v)
    for x in [OPENING_X[0] - 0.06, OPENING_X[1] + 0.06]:
        mould.box(x - 0.06, x + 0.06, PROSC_Y[0] - 0.05, PROSC_Y[0], STAGE_TOP, OPENING_SPRING, "M_MetalBrass")
    mould.build()

    # The question board behind the curtain (D92): plain, lit by the stage lamps, a thin wooden frame.
    b = BOARD
    board = Piece("QuestionBoard", bevel=0.01)
    board.box(b["x0"], b["x1"], b["y"], b["y"] + 0.06, b["z0"], b["z1"], "M_Board")
    board.build()
    frame = Piece("QuestionBoardFrame", bevel=0.008)
    fw = 0.07
    fy0, fy1 = b["y"] - 0.04, b["y"] + 0.05
    frame.box(b["x0"] - fw, b["x1"] + fw, fy0, fy1, b["z0"] - fw, b["z0"], "M_WoodDark")
    frame.box(b["x0"] - fw, b["x1"] + fw, fy0, fy1, b["z1"], b["z1"] + fw, "M_WoodDark")
    frame.box(b["x0"] - fw, b["x0"], fy0, fy1, b["z0"], b["z1"], "M_WoodDark")
    frame.box(b["x1"], b["x1"] + fw, fy0, fy1, b["z0"], b["z1"], "M_WoodDark")
    # Two posts holding it, and a dark back wall of the stage.
    for x in [b["x0"] + 0.4, b["x1"] - 0.4]:
        frame.box(x - 0.04, x + 0.04, b["y"] + 0.06, b["y"] + 0.12, STAGE_TOP, b["z0"], "M_WoodDark")
    frame.build()
    back = Piece("StageBackCloth", smooth=False)
    back.box(2.7, 8.3, WALL_Y - 0.06, WALL_Y - 0.02, STAGE_TOP, 5.6, "M_BackCloth")
    back.build()
    empty("LT_Board", ((b["x0"] + b["x1"]) / 2, b["y"], (b["z0"] + b["z1"]) / 2))

    # The curtain: two pleated halves of deep green velvet (Dyn_: they gather towards the sides at runtime).
    half = (OPENING_X[1] - OPENING_X[0]) / 2 + 0.2
    for side, (edge, sign) in (("L", (OPENING_X[0] - 0.15, 1)), ("R", (OPENING_X[1] + 0.15, -1))):
        build_curtain_half("Dyn_Curtain" + side, edge, sign, half + 0.15)
    # A fixed pleated valance across the top, behind the arch.
    val = Piece("CurtainValance")
    nu = 120
    pts = []
    for zz in [CURTAIN_TOP + 0.1, CURTAIN_TOP + 0.9]:
        for i in range(nu + 1):
            x = OPENING_X[0] - 0.3 + (OPENING_X[1] - OPENING_X[0] + 0.6) * i / nu
            pts.append((x, CURTAIN_Y - 0.12 + 0.04 * math.sin(x * 2 * math.pi / 0.2), zz))
    val.grid(pts, nu, 1, "M_Velvet")
    vobj = val.build()
    sol = vobj.modifiers.new("Thick", "SOLIDIFY")
    sol.thickness = 0.03

    # Stage lamps on stands at both front corners, aimed at the board.
    for i, (x, y) in enumerate([(3.25, 2.2), (7.75, 2.2)]):   # on the apron, in far enough that the beams clear the arch
        build_stage_lamp("StageLamp%d" % (i + 1), x, y, i + 1, STAGE_TOP)


def apron_polygon():
    """The apron in front of the arch (stage zone), counter clockwise: the front arc from left to right, then the two
    back corners at the curtain wall. Its ends follow the corner bisectors, a little inside them."""
    y_back = PROSC_Y[0]
    mid, end = APRON_FRONT

    def left(y):
        return CORNER_L[0] + 0.08 + TAN_HALF * (y_back - y)

    def right(y):
        return CORNER_R[0] - 0.08 - TAN_HALF * (y_back - y)

    return arc_points(left(end), right(end), mid, end) + [(right(y_back), y_back), (left(y_back), y_back)]


def build_curtain_half(name, edge_x, sign, width):
    """A pleated curtain half whose origin is its outer edge, so scaling x gathers it to the side."""
    cur = Piece(name)
    nu, nv = 200, 18
    pts = []
    z0, z1 = STAGE_TOP + 0.01, CURTAIN_TOP + 0.5
    rnd = random.Random(11 if sign > 0 else 23)
    # Irregular folds: the spacing wanders, the folds are tight and shallow under the heading and open up towards the
    # hem, like heavy velvet hanging from a gathered track; the hem lifts a little on each fold's crest.
    wobble = [(rnd.uniform(1.2, 2.2), rnd.uniform(0, 6.3)), (rnd.uniform(3.5, 5.5), rnd.uniform(0, 6.3))]
    for j in range(nv + 1):
        t = j / nv                        # 0 at the hem, 1 at the top
        z_base = z0 + (z1 - z0) * t
        period = 0.27 - 0.06 * t          # metres per fold: heavy cloth makes fewer, deeper folds
        amp = 0.095 - 0.045 * t
        for i in range(nu + 1):
            u = i / nu
            d = width * u
            phase = 2 * math.pi * (d / period + 0.18 * math.sin(d * wobble[0][0] + wobble[0][1])
                                   + 0.07 * math.sin(d * wobble[1][0] + wobble[1][1]))
            s = math.sin(phase)
            shaped = math.copysign(abs(s) ** 0.75, s)
            sway = 0.025 * (1 - t) * math.sin(d * 1.1 + 0.7)   # the whole curtain hangs a touch unevenly
            depth = amp * shaped + sway
            # The two halves meet flat where they overlap at the middle, the right one a little in front.
            edge = min(1.0, (width - d) / 0.3)
            depth = depth * edge + (0.0 if sign > 0 else -0.03) * (1 - edge)
            z = z_base + (0.012 * (1 + s) if j == 0 else 0.0)
            pts.append((edge_x + sign * d, CURTAIN_Y + depth, z))
    cur.grid(pts, nu, nv, "M_Velvet")
    obj = cur.build(origin=(edge_x, CURTAIN_Y, z0))
    sol = obj.modifiers.new("Thick", "SOLIDIFY")
    sol.thickness = 0.028                 # heavy lined velvet
    return obj


def build_stage_lamp(name, x, y, index, floor_z=0.0):
    lamp = Piece(name, bevel=0.004)
    for k in range(3):
        a = 2 * math.pi * k / 3 + 0.4
        fx, fy = x + 0.45 * math.cos(a), y + 0.45 * math.sin(a)
        mx, my = (x + fx) / 2, (y + fy) / 2
        length = math.hypot(0.45, 0.9)
        v = lamp.cyl((0, 0, 0), 0.015, length, "M_MetalBlack", segments=8)
        tilt = math.atan2(0.45, 0.9)
        bmesh.ops.rotate(lamp.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(tilt, 3, "Y"), verts=v)
        bmesh.ops.rotate(lamp.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(a + math.pi, 3, "Z"), verts=v)
        bmesh.ops.translate(lamp.bm, vec=Vector((mx, my, floor_z + 0.45)), verts=v)
    lamp.cyl((x, y, floor_z + 1.3), 0.022, 1.7, "M_MetalBlack", segments=10)
    head_z = floor_z + 2.15
    target = Vector(((BOARD["x0"] + BOARD["x1"]) / 2, BOARD["y"], (BOARD["z0"] + BOARD["z1"]) / 2))
    pos = Vector((x, y, head_z))
    d = (target - pos).normalized()
    rot = Vector((0, 0, 1)).rotation_difference(d).to_matrix()
    v = lamp.cyl((0, 0, 0), 0.17, 0.4, "M_MetalBlack", segments=20)
    bmesh.ops.rotate(lamp.bm, cent=(0, 0, 0), matrix=rot, verts=v)
    bmesh.ops.translate(lamp.bm, vec=pos, verts=v)
    v = lamp.cyl((0, 0, 0), 0.14, 0.02, "M_Bulb", segments=20)
    bmesh.ops.translate(lamp.bm, vec=Vector((0, 0, 0.2)), verts=v)
    bmesh.ops.rotate(lamp.bm, cent=(0, 0, 0), matrix=rot, verts=v)
    bmesh.ops.translate(lamp.bm, vec=pos, verts=v)
    lamp.build()
    empty("LP_StageLamp_%d" % index, tuple(pos + d * 0.25))
    # Its shaft through the haze, to the closed curtain.
    beam("BeamStageLamp%d" % index, tuple(pos + d * 0.2), tuple(target), 0.55, 0.13, (CURTAIN_Y - pos.y - 0.05) / d.y)


# ------------------------------------------------------------------------------------------------ band riser (no band yet)

def build_band():
    set_zone("Band")
    aimed("LK_BandTop", (11.3, 3.6, WALL_TOP - 0.4), (11.3, 4.4, 0.4))
    beam("BeamBandTop", (11.3, 3.6, WALL_TOP - 0.4), (11.3, 4.4, 0.4), 1.2, 0.14)
    aimed("LK_BandFront", (11.3, -0.6, 4.4), (11.3, 4.3, 0.9))
    aimed("CAM_BandPreview", (11.3, -3.4, 2.2), (11.3, 4.0, 1.0))   # only for StudioCapture (no Band shot yet, D96)
    # The band stage: a big deck along the band wall with an angled front, its left end on the corner bisector (it
    # meets the curtain stage's apron there), and a drum riser one step up at the back.
    wy = BAND_WALL_Y
    deck = [(BAND_X[0] + 0.08 + TAN_HALF * 4.1, 1.9), (10.0, 1.35), (12.3, 1.35), (BAND_X[1] - 0.35, 3.0),
            (BAND_X[1] - 0.35, wy), (BAND_X[0] + 0.08, wy)]
    drums = [(10.45, 3.85), (12.15, 3.85), (12.5, 4.2), (12.5, wy - 0.02), (10.1, wy - 0.02), (10.1, 4.2)]
    riser = Piece("BandRiser", bevel=0.015)
    riser.prism(deck, 0.0, BAND_DECK_TOP, "M_WoodDark")
    riser.prism(inset_polygon(deck, 0.1), BAND_DECK_TOP - 0.005, BAND_DECK_TOP + 0.004, "M_StageFloor")
    riser.prism(drums, BAND_DECK_TOP, DRUM_RISER_TOP, "M_WoodDark")
    riser.prism(inset_polygon(drums, 0.08), DRUM_RISER_TOP - 0.005, DRUM_RISER_TOP + 0.004, "M_StageFloor")
    riser.build()
    strip = Piece("BandRiserStrip", smooth=False)
    edge_strips(strip, deck, [0, 1, 2, 5], 0.16, 0.22, "M_GlowStrip")
    edge_strips(strip, drums, [5, 0, 1], BAND_DECK_TOP + 0.14, BAND_DECK_TOP + 0.19, "M_GlowStrip")
    strip.build()
    # The instruments are bought-in models (tools/blender/build_props.py): a drum kit with its stool on the riser, an
    # upright piano against the wall with a bench, a guitar on its stand and a bass on a stand by two combo amps.
    bx, by = 11.3, 5.0
    rug = Piece("BandRug", bevel=0.004)
    rug.box(bx - 0.95, bx + 0.95, by - 0.95, by + 0.9, DRUM_RISER_TOP, DRUM_RISER_TOP + 0.01, "M_BandRug")
    rug.build()
    prop("DrumKit", bx, by - 0.05, DRUM_RISER_TOP + 0.01)
    top = BAND_DECK_TOP
    prop("Piano", 9.05, wy - 0.3, top)
    bench = Piece("PianoBench", bevel=0.01)
    pbx, pby = 9.05, wy - 0.95
    bench.box(pbx - 0.42, pbx + 0.42, pby - 0.17, pby + 0.17, top + 0.44, top + 0.5, "M_Leather")
    bench.box(pbx - 0.44, pbx + 0.44, pby - 0.19, pby + 0.19, top + 0.38, top + 0.44, "M_WoodWarm")
    for lx in (pbx - 0.38, pbx + 0.38):
        for ly in (pby - 0.14, pby + 0.14):
            bench.cyl((lx, ly, top + 0.19), 0.022, 0.38, "M_WoodWarm", segments=10, radius2=0.017)
    bench.build()
    prop("Ukulele", 10.1, wy - 0.03, 1.72, 8)
    prop("Amp", 12.95, wy - 0.22, top)
    prop("Amp", 13.66, wy - 0.24, top, -6)
    prop("Guitar", 12.55, 4.9, top, -18)
    prop("Bass", 13.55, 4.62, top + 0.16, -10, 1.0, -14)

    gear = Piece("BandGear", bevel=0.004)
    # A stand for the bass: two front legs to a cradle under the body, a back leg up to the neck.
    sx, sy = 13.55, 4.62
    rod(gear, (sx - 0.14, sy - 0.12, top), (sx - 0.08, sy - 0.04, top + 0.2), 0.008, "M_MetalBlack")
    rod(gear, (sx + 0.14, sy - 0.12, top), (sx + 0.08, sy - 0.04, top + 0.2), 0.008, "M_MetalBlack")
    rod(gear, (sx - 0.12, sy - 0.05, top + 0.2), (sx + 0.12, sy - 0.05, top + 0.2), 0.009, "M_MetalBlack")
    rod(gear, (sx, sy + 0.32, top), (sx, sy + 0.17, top + 0.86), 0.008, "M_MetalBlack")
    # A music stand for the bass player (its desk faces the player, so the audience sees its back).
    msx, msy = 12.95, 3.75
    gear.cyl((msx, msy, top + 0.55), 0.008, 1.1, "M_MetalBlack", segments=8)
    for k in range(3):
        a = 2 * math.pi * k / 3 + 1.0
        rod(gear, (msx, msy, top + 0.25), (msx + 0.22 * math.cos(a), msy + 0.22 * math.sin(a), top), 0.007, "M_MetalBlack")
    v = gear.box(msx - 0.24, msx + 0.24, msy - 0.006, msy + 0.006, -0.17, 0.17, "M_MetalBlack")
    bmesh.ops.rotate(gear.bm, cent=(msx, msy, 0), matrix=Matrix.Rotation(-0.35, 3, "X"), verts=v)   # desk tips back
    bmesh.ops.translate(gear.bm, vec=(0, 0, top + 1.2), verts=v)
    v = gear.box(msx - 0.2, msx + 0.2, msy + 0.008, msy + 0.012, -0.14, 0.14, "M_Paper")           # sheet music
    bmesh.ops.rotate(gear.bm, cent=(msx, msy, 0), matrix=Matrix.Rotation(-0.35, 3, "X"), verts=v)
    bmesh.ops.translate(gear.bm, vec=(0, 0, top + 1.2), verts=v)
    # A small lamp left on the guitar amp, and a vocal mic at the front of the stage.
    alx, aly, alz = 12.95, wy - 0.22, top + 0.47
    gear.cyl((alx, aly, alz + 0.08), 0.045, 0.16, "M_Ceramic", segments=14)
    gear.cyl((alx, aly, alz + 0.24), 0.12, 0.15, "M_LampShade", segments=20, radius2=0.085, caps=False)
    mx, my = 11.1, 2.3
    gear.cyl((mx, my, top + 0.7), 0.01, 1.4, "M_Chrome", segments=8)
    gear.cyl((mx, my, top + 0.02), 0.14, 0.03, "M_MetalBlack", segments=16)
    gear.cyl((mx, my - 0.05, top + 1.43), 0.025, 0.12, "M_MetalBlack", segments=12, axis="y")
    gear.build()
    empty("LP_TableLamp_2", (alx, aly, alz + 0.22))
    # A lamp on the piano.
    prop("OilLamp", 9.6, wy - 0.25, top + 1.15)
    empty("LP_TableLamp_4", (9.6, wy - 0.25, top + 1.15 + 0.52))


def build_guitar(name, x, y, floor_z, body_mat, scale):
    """An electric guitar (or, longer, a bass) leaning back on an A-frame stand, facing the audience."""
    g = Piece(name, bevel=0.003)
    s = scale
    verts = []
    # Body: two overlapping flattened discs (lower and upper bout) and a waist, facing -y.
    verts += g.cyl((0, 0, 0.0), 0.19 * s ** 0.5, 0.045, body_mat, axis="y", segments=28)
    verts += g.cyl((0.01, 0, 0.21 * s ** 0.5), 0.145 * s ** 0.5, 0.045, body_mat, axis="y", segments=24)
    verts += g.box(-0.06, 0.07, -0.028, 0.028, -0.02, 0.2, "M_KeysWhite")                   # pickguard
    neck = 0.5 * s
    verts += g.box(-0.025, 0.025, -0.012, 0.018, 0.3, 0.3 + neck, "M_WoodDark")
    verts += g.box(-0.04, 0.045, -0.01, 0.016, 0.3 + neck, 0.3 + neck + 0.17, "M_WoodDark")  # headstock
    for k in range(3 if s < 1.1 else 2):
        verts += g.cyl((0.0, -0.03, 0.02 + k * 0.07), 0.012, 0.008, "M_Chrome", axis="y", segments=10)  # knobs
    # Lean back 14 degrees, then stand on the stand's cradle.
    lean = Matrix.Rotation(math.radians(-14), 3, "X")
    vs = list({v for v in verts})
    bmesh.ops.rotate(g.bm, cent=(0, 0, 0), matrix=lean, verts=vs)
    bmesh.ops.translate(g.bm, vec=Vector((x, y, floor_z + 0.33)), verts=vs)
    # The stand: two front legs to a cradle, a back leg and a neck rest.
    rod(g, (x - 0.14, y - 0.12, floor_z), (x - 0.08, y - 0.04, floor_z + 0.2), 0.008, "M_MetalBlack")
    rod(g, (x + 0.14, y - 0.12, floor_z), (x + 0.08, y - 0.04, floor_z + 0.2), 0.008, "M_MetalBlack")
    rod(g, (x - 0.12, y - 0.05, floor_z + 0.2), (x + 0.12, y - 0.05, floor_z + 0.2), 0.009, "M_MetalBlack")
    rod(g, (x, y + 0.3, floor_z), (x, y + 0.12 * s, floor_z + 0.3 + 0.5 * s), 0.008, "M_MetalBlack")
    g.build()


# ------------------------------------------------------------------------------------------------ dressing

def build_dressing():
    set_zone("Guests")
    # Bought-in plants and pictures (tools/blender/build_props.py): a tall plant in the corner beside the host shelf,
    # one on the apron by the corner, one on the band stage; framed paintings on the walls beside the arch and the band.
    prop("PlantTall", 3.22, GUEST_WALL_Y - 0.45, PLATFORM_TOP, 15)
    set_zone("Stage")
    prop("PlantWide", 1.85, PROSC_Y[0] - 0.42, STAGE_TOP, -20)
    prop("PictureFrame2", 1.85, PROSC_Y[0] - 0.05, 1.75)    # origin at the bottom centre: half its depth off the wall
    prop("PictureFrame1", 9.55, PROSC_Y[0] - 0.012, 2.15)
    set_zone("Band")
    prop("PlantTall", 7.95, BAND_WALL_Y - 0.4, BAND_DECK_TOP, -40, 0.85)
    prop("PictureFrame1", 11.3, BAND_WALL_Y - 0.015, 2.55, 0, 1.25)

    # A big flat rug on the open floor in front of the stage: the third zone of the bay (guests, desk, the floor where
    # the winner walks), so the middle of the studio is no longer one bare plane. It stays flat: nothing on the floor
    # blocks a shot or the overlays drawn over it. Its pattern is mapped once over it (StudioMaterials, M_Rug3).
    set_zone("World")
    rug = Piece("FloorRug", bevel=0.004)
    rug.box(FLOOR_RUG["x0"], FLOOR_RUG["x1"], FLOOR_RUG["y0"], FLOOR_RUG["y1"], 0.0, 0.012, "M_Rug3")
    rug.build()

    # Lit church candles in clusters: beside the stage steps, at the front corner of the guest riser and of the band
    # stage.
    set_zone("Stage")
    wax = Piece("CandlesStage", bevel=0.0)
    candle_cluster(wax, STEPS_X[0] - 0.42, APRON_FRONT[0] - 0.22, 0.0, 3, 6)
    candle_cluster(wax, STEPS_X[1] + 0.42, APRON_FRONT[0] - 0.22, 0.0, 4, 5)
    wax.build()
    set_zone("Guests")
    wax = Piece("CandlesGuests", bevel=0.0)
    candle_cluster(wax, -9.55, 2.35, PLATFORM_TOP, 5, 4, 0.12)
    wax.build()
    set_zone("Band")
    wax = Piece("CandlesBand", bevel=0.0)
    candle_cluster(wax, 9.35, 2.1, BAND_DECK_TOP, 6, 5)
    wax.build()

    # Festoon strings: one over the window, framing the city; one along the band wall with two swags out over the band
    # stage to the ceiling.
    set_zone("Guests")
    string = Piece("FestoonGuests", bevel=0.0)
    festoon(string, (WINDOW_X[0] - 0.55, GUEST_WALL_Y - 0.2, 5.0), (WINDOW_X[1] + 0.45, GUEST_WALL_Y - 0.2, 5.0), 0.38)
    string.build()
    set_zone("Band")
    string = Piece("FestoonBand", bevel=0.0)
    festoon(string, (BAND_X[0] + 0.5, BAND_WALL_Y - 0.15, 4.75), (BAND_X[1] - 0.45, BAND_WALL_Y - 0.15, 4.75), 0.5)
    festoon(string, (9.1, BAND_WALL_Y - 0.15, 4.9), (10.2, 1.7, WALL_TOP - 0.3), 0.85)
    festoon(string, (13.1, BAND_WALL_Y - 0.15, 4.9), (12.2, 1.7, WALL_TOP - 0.3), 0.85)
    string.build()

    # The working floor of a TV studio: cables taped down or snaking off to where the cameras would be, and spike marks
    # where the hosts and the winner stand. Flat on the floor, so nothing blocks a shot.
    set_zone("Guests")
    floor = Piece("FloorCables", bevel=0.0)
    cable(floor, [(-10.4, -2.6), (-9.6, -0.9), (-9.1, 0.6), (-8.75, 1.6), (-8.6, 1.97)])
    cable(floor, [(3.0, -1.0), (1.2, -1.6), (-0.8, -1.95), (-2.6, -2.9), (-3.4, -4.2)], 0.013)
    cable(floor, [(-5.2, 1.98), (-5.5, 1.2), (-6.4, 0.3), (-7.9, -0.6), (-9.5, -1.4), (-11.0, -1.7)], 0.008)
    for x, y, yaw, mat in [(-6.2, 1.35, 0.1, "M_TapeWhite"), (-3.1, 1.3, -0.05, "M_TapeWhite"), (-4.65, 0.75, 0.0, "M_TapeYellow"),
                           (0.9, 1.1, 0.3, "M_TapeYellow")]:
        tape_mark(floor, x, y, yaw, mat)
    floor.build()
    set_zone("Stage")
    floor = Piece("FloorCablesStage", bevel=0.0)
    cable(floor, [(3.25, 2.25), (3.0, 1.5), (2.75, 0.5), (2.1, -0.8), (1.0, -2.2), (0.2, -3.6)], 0.012, z=0.0)
    cable(floor, [(7.75, 2.25), (8.2, 1.4), (8.9, 0.2), (9.2, -1.6), (9.0, -3.4)], 0.012, z=0.0)
    for x, y, yaw, mat in [(5.5, 0.15, 0.0, "M_TapeYellow"), (4.6, -0.6, 0.2, "M_TapeWhite"), (6.4, -0.7, -0.25, "M_TapeWhite")]:
        tape_mark(floor, x, y, yaw, mat, 0.14)
    floor.build()

    # Wall sconces beside the window, on both sides of the proscenium and on the band wall.
    for zone, name, specs in (("Guests", "GuestSconces", [(-9.2, GUEST_WALL_Y, 2.75), (-0.5, GUEST_WALL_Y, 2.75)]),
                              ("Stage", "Sconces", [(1.85, PROSC_Y[0], 3.6), (9.55, PROSC_Y[0], 3.6)]),
                              ("Band", "BandSconce", [(8.4, BAND_WALL_Y, 3.3), (13.6, BAND_WALL_Y, 3.3)])):
        set_zone(zone)
        sconce = Piece(name, bevel=0.004)
        for k, (x, y, z) in enumerate(specs):
            y -= 0.03
            sconce.box(x - 0.12, x + 0.12, y - 0.04, y, z - 0.2, z + 0.1, "M_MetalBrass")
            sconce.cyl((x, y - 0.18, z + 0.1), 0.11, 0.2, "M_LampShade", segments=20, radius2=0.08, caps=False)
            empty("LP_Sconce_%s%d" % (zone[0], k + 1), (x, y - 0.18, z + 0.08))
        sconce.build()


def cable(piece, points, radius=0.011, mat="M_Cable", z=0.0):
    """A cable lying on the floor through the given (x, y) points: a smooth Catmull-Rom curve swept as a thin tube."""
    pts = [Vector((x, y, z + radius)) for x, y in points]
    curve = []
    for i in range(len(pts) - 1):
        p0, p1, p2, p3 = pts[max(0, i - 1)], pts[i], pts[i + 1], pts[min(len(pts) - 1, i + 2)]
        steps = max(2, int((p2 - p1).length / 0.12))
        for k in range(steps):
            t = k / steps
            curve.append(0.5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t
                                + (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t))
    curve.append(pts[-1])
    for a, b in zip(curve, curve[1:]):
        rod(piece, a, b, radius, mat, 6)
        piece.sphere(b, radius, mat, subdiv=1)     # rounds the joints


def tape_mark(piece, x, y, yaw, mat="M_TapeWhite", size=0.12):
    """A spike mark of gaffer tape on the floor: a small cross where something stands for the show."""
    for a in (0.0, math.pi / 2):
        piece.box(x - size / 2, x + size / 2, y - 0.012, y + 0.012, 0.0005, 0.0015, mat, yaw + a, (x, y, 0))


LIGHT_COUNT = {}


def light_marker(kind, at):
    """An LP_<kind>_<n> practical light marker (StudioBuilder.AddLights), numbered per kind."""
    n = LIGHT_COUNT.get(kind, 0) + 1
    LIGHT_COUNT[kind] = n
    return empty("LP_%s_%d" % (kind, n), at)


def festoon(piece, a, b, sag, spacing=0.42, light_every=4):
    """A festoon string from a to b sagging by sag in the middle: a thin cable with a bulb (the bought-in light bulb,
    hung upside down) every spacing metres and a warm baked light every light_every bulbs."""
    a, b = Vector(a), Vector(b)
    n = max(2, int((b - a).length / spacing))
    pts = []
    for i in range(n + 1):
        t = i / n
        p = a + (b - a) * t
        p.z -= sag * 4 * t * (1 - t)
        pts.append(p)
    for p, q in zip(pts, pts[1:]):
        rod(piece, p, q, 0.004, "M_MetalBlack", 4)
    for i, p in enumerate(pts[1:-1], 1):
        prop("Bulb", p.x, p.y, p.z - 0.025, 0.0, 1.0, 180.0)
        if i % light_every == light_every // 2:
            light_marker("Festoon", (p.x, p.y, p.z - 0.09))


def candle_cluster(piece, x, y, z, seed, count=5, spread=0.16):
    """A cluster of church candles of different heights, lit, standing at z; one warm light over the cluster."""
    rnd = random.Random(seed)
    top = z
    for k in range(count):
        a = 2 * math.pi * k / count + rnd.uniform(-0.4, 0.4)
        r = 0.0 if k == 0 else spread * rnd.uniform(0.6, 1.0)
        cx, cy = x + r * math.cos(a), y + r * math.sin(a)
        radius = rnd.uniform(0.035, 0.06)
        h = rnd.uniform(0.1, 0.32) if k else 0.36
        piece.cyl((cx, cy, z + h / 2), radius, h, "M_Wax", segments=18)
        piece.cyl((cx, cy, z + h + 0.008), 0.0016, 0.016, "M_Soil", segments=4)                  # wick
        piece.sphere((cx, cy, z + h + 0.03), 0.011, "M_Flame", scale=(1, 1, 2.2), subdiv=1)
        top = max(top, z + h + 0.03)
    light_marker("Candle", (x, y, top + 0.04))


def plant(name, x, y, z, s):
    p = Piece(name, bevel=0.0)
    p.cyl((x, y, z + 0.22 * s), 0.2 * s, 0.44 * s, "M_Pot", segments=20, radius2=0.16 * s)
    p.cyl((x, y, z + 0.43 * s), 0.18 * s, 0.02 * s, "M_Soil", segments=20)
    # Stems fanning out of the pot, each carrying thin pointed leaves that arch outwards and droop at the tips.
    rnd = random.Random(sum(ord(c) for c in name))   # stable: str hash() changes per run
    base = Vector((x, y, z + 0.42 * s))
    for k in range(int(15 * max(s, 0.5))):
        a = rnd.uniform(0, 2 * math.pi)
        tilt = rnd.uniform(0.15, 0.6)
        length = rnd.uniform(0.6, 1.25) * s
        d = Vector((math.sin(tilt) * math.cos(a), math.sin(tilt) * math.sin(a), math.cos(tilt)))
        tip = base + d * length
        rod(p, base, tip, 0.007 * s, "M_Stem", 5)
        for j in range(rnd.randint(5, 8)):
            t = rnd.uniform(0.35, 1.0)
            at = base + d * length * t
            side = a + rnd.choice((-1, 1)) * rnd.uniform(0.6, 1.4)
            out = Vector((math.cos(side), math.sin(side), 0.0))
            leaf_len = rnd.uniform(0.2, 0.36) * s
            droop = rnd.uniform(-0.6, 0.15)
            ld = (out + Vector((0, 0, droop))).normalized()
            centre = at + ld * leaf_len * 0.5
            v = p.sphere((0, 0, 0), 1.0, "M_Leaf", scale=(0.085 * s, leaf_len * 0.5, 0.006 * s), subdiv=1)
            # Long axis (local y) along the leaf's direction, flat face roughly up, a little twist.
            rot = Vector((0, 1, 0)).rotation_difference(ld).to_matrix() @ Matrix.Rotation(rnd.uniform(-0.5, 0.5), 3, "Y")
            bmesh.ops.rotate(p.bm, cent=(0, 0, 0), matrix=rot, verts=v)
            bmesh.ops.translate(p.bm, vec=centre, verts=v)
    p.build()


# ------------------------------------------------------------------------------------------------ export

def zone_matrix(name):
    (lx, ly), (wx, wy), yaw = ZONES[name]
    return (Matrix.Translation((wx, wy, 0)) @ Matrix.Rotation(math.radians(yaw), 4, "Z")
            @ Matrix.Translation((-lx, -ly, 0)))


def to_zone_frames():
    """Moves every object from its zone's design frame into place (an object transform, so Dyn_ pieces keep their local
    axes), then leaves one Zone_<name> marker per zone at its design origin for Unity's probe grids."""
    for obj in list(COLLECTION.objects):
        obj.matrix_world = zone_matrix(OBJ_ZONE.get(obj.name, "World")) @ obj.matrix_world
    set_zone("World")
    for name, (_, _, yaw) in ZONES.items():
        empty("Zone_" + name, tuple(zone_matrix(name) @ Vector((0, 0, 0))), math.radians(yaw))


def render_plan(path):
    """A top down picture of the layout with every marker drawn, to check the bay without opening Unity."""
    scene = bpy.context.scene
    cam_data = bpy.data.cameras.new("PlanCam")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = 40.0
    cam = bpy.data.objects.new("PlanCam", cam_data)
    scene.collection.objects.link(cam)
    cam.location = (3.0, -4.0, 30.0)
    scene.camera = cam
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "RANDOM"
    scene.render.resolution_x, scene.render.resolution_y = 1600, 1300
    scene.render.filepath = path
    marks = []
    for obj in list(COLLECTION.objects):
        if obj.type == "EMPTY" and obj.name.split("_")[0] in ("CAM", "LK", "Seat", "Host"):
            bm = bmesh.new()
            bmesh.ops.create_cone(bm, cap_ends=True, segments=12, radius1=0.18, radius2=0.18, depth=0.05)
            mesh = bpy.data.meshes.new("mark")
            bm.to_mesh(mesh)
            bm.free()
            m = bpy.data.objects.new("mark_" + obj.name, mesh)
            m.location = obj.matrix_world.translation + Vector((0, 0, 8.0))
            col = material("Mark_" + obj.name.split("_")[0])
            col.diffuse_color = {"CAM": (0.1, 0.4, 1, 1), "LK": (1, 0.8, 0.1, 1)}.get(obj.name.split("_")[0], (1, 0.1, 0.1, 1))
            mesh.materials.append(col)
            scene.collection.objects.link(m)
            marks.append(m)
    hidden = [o for o in COLLECTION.objects if o.type == "MESH" and o.name in ("CityBackdrop", "Ceiling")
              or o.name.startswith("Truss") or o.name.startswith("CityBlocks")]
    for o in hidden:
        o.hide_render = True
    bpy.ops.render.render(write_still=True)
    for o in hidden:
        o.hide_render = False
    for m in marks:
        bpy.data.objects.remove(m, do_unlink=True)
    bpy.data.objects.remove(cam, do_unlink=True)
    print("STUDIO PLAN written to " + path)


def to_unity_frame():
    """Turns everything 180 degrees about z so the exporter's axis change lands the layout as designed in Unity."""
    turn = Matrix.Rotation(math.pi, 4, "Z")
    for obj in list(COLLECTION.objects):
        if obj.type == "MESH":
            # Bake the turn into the mesh about the object's own origin, and move the origin.
            obj.data.transform(Matrix.Rotation(math.pi, 4, "Z"))
            obj.location = turn @ obj.location
        else:
            obj.location = turn @ obj.location
            obj.rotation_euler.z += math.pi


def main():
    global COLLECTION
    out = sys.argv[sys.argv.index("--") + 1]
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    COLLECTION = scene.collection

    build_room()
    build_guest_area()
    build_desk()
    build_stage()
    build_band()
    build_dressing()

    # UVs come from the design frames, so wood and rugs tile the same on a turned wall. matrix_world of a new object
    # is only filled in by a scene update.
    bpy.context.view_layer.update()
    for obj in COLLECTION.objects:
        if not obj.name.startswith("Beam"):     # beams keep their own UVs (v along the shaft)
            box_uvs(obj)
    b = CITY_BACKDROP
    fit_uvs(bpy.data.objects["CityBackdrop"], b["x0"], b["x1"], b["z0"], b["z1"])
    to_zone_frames()
    args = sys.argv[sys.argv.index("--") + 1:]
    if "--plan" in args:
        render_plan(args[args.index("--plan") + 1])
    to_unity_frame()

    bpy.ops.export_scene.fbx(
        filepath=out,
        use_selection=False,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        object_types={"MESH", "EMPTY"},
        use_mesh_modifiers=True,
        mesh_smooth_type="OFF",
        use_tspace=False,
        add_leaf_bones=False,
        bake_anim=False,
    )
    meshes = sum(1 for o in COLLECTION.objects if o.type == "MESH")
    tris = 0
    deps = bpy.context.evaluated_depsgraph_get()
    for o in COLLECTION.objects:
        if o.type == "MESH":
            ev = o.evaluated_get(deps)
            tris += sum(len(p.vertices) - 2 for p in ev.data.polygons)
    print("STUDIO EXPORT %s: %d meshes, %d empties, about %d triangles" % (
        out, meshes, len(COLLECTION.objects) - meshes, tris))


main()
