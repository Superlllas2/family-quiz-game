"""
Builds the late night studio set (docs/21_staging.md, "Game night 1 version"; decisions 0015, 0016) and exports
unity/Assets/Game/Art/Models/Studio.fbx. Original design (D49, D93): no real show's set pieces.

Run headless from the repo root (Windows or macOS):
    blender --background --python tools/blender/build_studio.py -- unity/Assets/Game/Art/Models/Studio.fbx

Design space: metres, x to the right as seen from the audience, y away from the audience (the back wall is at +y),
z up. Unity receives the same layout as x right, y up, z away from the audience (the export turns the set 180 degrees
about z, because the FBX exporter maps Blender (x, y, z) to Unity (-x, z, -y)).

Blender only models. Lighting is done and baked in Unity (0015). Empties mark where Unity puts things:
    Seat_1 .. Seat_8   guest seats in fill order (two middle sofa seats, two outer sofa seats, inner armchairs,
                       outer armchairs), at the top of the seat cushion, facing the audience
    Host_konstantin, Host_ilja   host places behind the desk
    LP_<kind>_<n>      practical lights (pendant, floor lamp, desk lamp, sconce, stage lamp, shelf, window)
    LT_Board           what the stage lamps aim at
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


def material(name):
    m = bpy.data.materials.get(name)
    if m is None:
        m = bpy.data.materials.new(name)
    return m


class Piece:
    """Geometry gathered into one mesh object with several materials."""

    def __init__(self, name, bevel=0.0, segments=2, smooth=True):
        self.name = name
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
        bmesh.ops.remove_doubles(self.bm, verts=self.bm.verts, dist=1e-5)
        bmesh.ops.translate(self.bm, vec=-Vector(origin), verts=self.bm.verts)
        mesh = bpy.data.meshes.new(self.name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(self.name, mesh)
        COLLECTION.objects.link(obj)
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
        return obj


def empty(name, loc, yaw=0.0):
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = "ARROWS"
    obj.location = Vector(loc)
    obj.rotation_euler = (0, 0, yaw)
    COLLECTION.objects.link(obj)
    return obj


def box_uvs(obj):
    """Box projected UVs in metres (1 UV unit = 1 m), from world coordinates, so textures tile at real scale."""
    if obj.type != "MESH":
        return
    mesh = obj.data
    bm = bmesh.new()
    bm.from_mesh(mesh)
    uv = bm.loops.layers.uv.verify()
    mw = obj.matrix_world
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
WALL_Y = 6.5                  # front face of the back wall
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
DESK_CX, DESK_Y = 1.25, 2.55  # desk centre (x) and front face (y)


# ------------------------------------------------------------------------------------------------ the room

def build_room():
    floor = Piece("Floor", smooth=False)
    floor.box(-12.5, 12.5, -9.0, WALL_Y + 0.05, -0.05, 0.0, "M_FloorWood")
    floor.build()

    # Back wall with the big window behind the guest area.
    wall = Piece("BackWall", bevel=0.01)
    t = 0.3
    y0, y1 = WALL_Y, WALL_Y + t
    top = 7.0
    wx0, wx1 = WINDOW_X
    wz0, wz1 = WINDOW_Z
    wall.box(-12.5, wx0, y0, y1, 0, top, "M_WoodPanel")
    wall.box(wx1, 12.5, y0, y1, 0, top, "M_WoodPanel")
    wall.box(wx0, wx1, y0, y1, 0, wz0, "M_WoodPanel")
    wall.box(wx0, wx1, y0, y1, wz1, top, "M_WoodPanel")
    # Panel trim: a dado rail and vertical battens across the solid wall.
    wall.box(-12.5, 12.5, y0 - 0.04, y0, 0.95, 1.02, "M_WoodDark")
    wall.box(-12.5, 12.5, y0 - 0.06, y0, 0.0, 0.14, "M_WoodDark")   # skirting
    for x in [-12.0, -10.8, -9.6, -0.2, 2.4, 8.6, 9.8, 11.0, 12.2]:
        wall.box(x - 0.04, x + 0.04, y0 - 0.03, y0, 1.02, top, "M_WoodDark")
    wall.build()

    # Window frame and mullions.
    win = Piece("Window", bevel=0.008)
    fy0, fy1 = WALL_Y - 0.05, WALL_Y + 0.12
    win.box(wx0 - 0.12, wx1 + 0.12, fy0, fy1, wz0 - 0.12, wz0, "M_WoodDark")      # sill
    win.box(wx0 - 0.12, wx1 + 0.12, fy0, fy1, wz1, wz1 + 0.12, "M_WoodDark")
    win.box(wx0 - 0.12, wx0, fy0, fy1, wz0, wz1, "M_WoodDark")
    win.box(wx1, wx1 + 0.12, fy0, fy1, wz0, wz1, "M_WoodDark")
    n = 6
    for i in range(1, n):
        x = wx0 + (wx1 - wx0) * i / n
        win.box(x - 0.035, x + 0.035, fy0 + 0.04, fy1, wz0, wz1, "M_MetalBlack")
    for z in [wz0 + (wz1 - wz0) * 0.36, wz0 + (wz1 - wz0) * 0.7]:
        win.box(wx0, wx1, fy0 + 0.04, fy1, z - 0.03, z + 0.03, "M_MetalBlack")
    win.build()
    empty("LP_Window_1", ((wx0 + wx1) / 2, WALL_Y + 0.4, (wz0 + wz1) / 2))

    # The invented evening city far behind the window: a lit backdrop.
    city = Piece("CityBackdrop", smooth=False)
    city.box(-26.0, 8.0, 17.0, 17.05, -2.0, 15.0, "M_City")
    city.build()

    # Side walls, so the wide shots end in wood, not in black.
    side = Piece("SideWalls", bevel=0.01)
    side.box(-12.8, -12.5, -6.0, WALL_Y + 0.3, 0, 7.0, "M_WoodPanel")
    side.box(12.5, 12.8, -6.0, WALL_Y + 0.3, 0, 7.0, "M_WoodPanel")
    side.build()

    # Ceiling trusses with lamp cans: a TV studio above a living room.
    truss = Piece("Truss", bevel=0.005)
    for y in [-1.5, 1.5, 4.5]:
        truss.box(-12.4, 12.4, y - 0.12, y + 0.12, 7.2, 7.45, "M_Truss")
    for x in [-9.0, -3.0, 3.0, 9.0]:
        truss.box(x - 0.12, x + 0.12, -1.6, 4.6, 7.45, 7.6, "M_Truss")
    for i, (x, y) in enumerate([(-7.5, -1.5), (-4.0, -1.5), (-0.5, -1.5), (4.0, -1.5), (7.5, -1.5),
                                (-6.0, 1.5), (1.0, 1.5), (5.5, 1.5), (10.0, 1.5)]):
        truss.cyl((x, y, 6.95), 0.17, 0.42, "M_MetalBlack", segments=16)
        truss.box(x - 0.02, x + 0.02, y - 0.02, y + 0.02, 7.15, 7.2, "M_MetalBlack")
    truss.build()


# ------------------------------------------------------------------------------------------------ guest area

def build_guest_area():
    # The riser under the guests and the desk, with a warm light strip along its front edge.
    riser = Piece("GuestRiser", bevel=0.02)
    riser.box(-9.4, 2.9, 1.2, 5.2, 0.0, PLATFORM_TOP, "M_WoodDark")
    riser.build()
    strip = Piece("GuestRiserStrip", smooth=False)
    strip.box(-9.35, 2.85, 1.17, 1.2, 0.05, 0.11, "M_GlowStrip")
    strip.build()

    rug = Piece("Rug", bevel=0.004)
    rug.box(-9.0, -0.9, 2.1, 4.9, PLATFORM_TOP, PLATFORM_TOP + 0.012, "M_Rug")
    rug.build()
    rug2 = Piece("RugDesk", bevel=0.004)
    rug2.box(-0.2, 2.7, 2.2, 4.6, PLATFORM_TOP, PLATFORM_TOP + 0.01, "M_Rug2")
    rug2.build()

    seat_z = PLATFORM_TOP + 0.012
    # Sofa: four seats, muted dark green fabric (D93) so the guests' colours stand out.
    sofa_x0, sofa_x1 = -6.25, -3.05
    sy0, sy1 = 3.25, 4.2
    sofa = Piece("Sofa", bevel=0.035, segments=3)
    legs = Piece("SofaLegs", bevel=0.006)
    sofa.box(sofa_x0, sofa_x1, sy0, sy1, seat_z + 0.12, seat_z + 0.36, "M_FabricSofa")            # base
    sofa.box(sofa_x0, sofa_x0 + 0.2, sy0, sy1, seat_z + 0.12, seat_z + 0.62, "M_FabricSofa")      # arms
    sofa.box(sofa_x1 - 0.2, sofa_x1, sy0, sy1, seat_z + 0.12, seat_z + 0.62, "M_FabricSofa")
    sofa.box(sofa_x0 + 0.2, sofa_x1 - 0.2, sy1 - 0.22, sy1, seat_z + 0.36, seat_z + 0.86, "M_FabricSofa")  # back
    inner0, inner1 = sofa_x0 + 0.2, sofa_x1 - 0.2
    w = (inner1 - inner0) / 4
    for i in range(4):
        a, b = inner0 + i * w + 0.01, inner0 + (i + 1) * w - 0.01
        sofa.box(a, b, sy0 + 0.02, sy1 - 0.22, seat_z + 0.36, seat_z + 0.5, "M_FabricSofa")         # seat cushion
        sofa.box(a, b, sy1 - 0.38, sy1 - 0.22, seat_z + 0.5, seat_z + 0.84, "M_FabricSofa")         # back cushion
    for x in [sofa_x0 + 0.08, sofa_x1 - 0.08]:
        for y in [sy0 + 0.08, sy1 - 0.08]:
            legs.cyl((x, y, seat_z + 0.06), 0.03, 0.12, "M_WoodWarm", segments=12, radius2=0.022)
    sofa.build()
    legs.build()
    sofa_seats = [inner0 + (i + 0.5) * w for i in range(4)]

    # Armchairs: two on each side in a shallow arc, turned a little towards the middle.
    chairs = Piece("Armchairs", bevel=0.03, segments=3)
    chair_legs = Piece("ArmchairLegs", bevel=0.005)
    arm_specs = [(-8.25, 3.05, math.radians(-14)), (-7.15, 3.18, math.radians(-8)),
                 (-2.05, 3.18, math.radians(8)), (-0.95, 3.05, math.radians(14))]
    seat_points = []
    for cx, cy, yaw in arm_specs:
        p = (cx, cy + 0.45, 0)
        cw, cd = 0.88, 0.9
        x0, x1, y0, y1 = cx - cw / 2, cx + cw / 2, cy, cy + cd
        chairs.box(x0, x1, y0, y1, seat_z + 0.14, seat_z + 0.38, "M_FabricChair", yaw, p)
        chairs.box(x0, x0 + 0.13, y0, y1, seat_z + 0.14, seat_z + 0.62, "M_FabricChair", yaw, p)
        chairs.box(x1 - 0.13, x1, y0, y1, seat_z + 0.14, seat_z + 0.62, "M_FabricChair", yaw, p)
        chairs.box(x0 + 0.13, x1 - 0.13, y1 - 0.18, y1, seat_z + 0.38, seat_z + 0.9, "M_FabricChair", yaw, p)
        chairs.box(x0 + 0.14, x1 - 0.14, y0 + 0.02, y1 - 0.18, seat_z + 0.38, seat_z + 0.52, "M_FabricChair", yaw, p)
        for lx in [x0 + 0.07, x1 - 0.07]:
            for ly in [y0 + 0.07, y1 - 0.07]:
                v = chair_legs.cyl((lx, ly, seat_z + 0.07), 0.025, 0.14, "M_WoodWarm", segments=10, radius2=0.018)
                bmesh.ops.rotate(chair_legs.bm, cent=p, matrix=Matrix.Rotation(yaw, 3, "Z"), verts=v)
        seat_points.append((cx, cy + 0.36, yaw))
    chairs.build()
    chair_legs.build()

    # Seat markers in fill order (docs/21_staging.md, "Seats").
    top = seat_z + 0.52
    sofa_y = sy0 + 0.33
    order = [
        (sofa_seats[1], sofa_y, 0.0), (sofa_seats[2], sofa_y, 0.0),
        (sofa_seats[0], sofa_y, 0.0), (sofa_seats[3], sofa_y, 0.0),
        seat_points[1], seat_points[2],
        seat_points[0], seat_points[3],
    ]
    for i, (x, y, yaw) in enumerate(order):
        empty("Seat_%d" % (i + 1), (x, y, top), yaw)

    # Floor lamp beside the outer left armchair, pendants over the sofa.
    lamp = Piece("FloorLamp", bevel=0.004)
    lamp.cyl((-9.55, 4.6, PLATFORM_TOP + 0.015), 0.17, 0.03, "M_MetalBrass", segments=24)
    lamp.cyl((-9.55, 4.6, PLATFORM_TOP + 0.8), 0.015, 1.55, "M_MetalBrass", segments=10)
    lamp.cyl((-9.55, 4.6, PLATFORM_TOP + 1.68), 0.28, 0.34, "M_LampShade", segments=32, radius2=0.2, caps=False)
    lamp.build()
    empty("LP_FloorLamp_1", (-9.55, 4.6, PLATFORM_TOP + 1.62))

    pend = Piece("Pendants", bevel=0.003)
    for i, x in enumerate([-5.45, -3.85]):
        pend.cyl((x, 3.75, 5.2), 0.006, 3.6, "M_MetalBlack", segments=6)
        pend.cyl((x, 3.75, 3.28), 0.3, 0.26, "M_MetalBrass", segments=32, radius2=0.05, caps=False)
        pend.sphere((x, 3.75, 3.18), 0.06, "M_Bulb")
        empty("LP_Pendant_%d" % (i + 1), (x, 3.75, 3.12))
    pend.build()

    # Side table between the left armchairs and the sofa, with a small lamp.
    table = Piece("SideTable", bevel=0.008)
    table.cyl((-6.65, 3.9, PLATFORM_TOP + 0.52), 0.22, 0.035, "M_WoodWarm", segments=28)
    table.cyl((-6.65, 3.9, PLATFORM_TOP + 0.26), 0.03, 0.5, "M_WoodWarm", segments=10)
    table.cyl((-6.65, 3.9, PLATFORM_TOP + 0.04), 0.16, 0.03, "M_WoodWarm", segments=24)
    table.cyl((-6.65, 3.9, PLATFORM_TOP + 0.62), 0.05, 0.16, "M_Ceramic", segments=16)
    table.cyl((-6.65, 3.9, PLATFORM_TOP + 0.8), 0.14, 0.18, "M_LampShade", segments=24, radius2=0.1, caps=False)
    table.build()
    empty("LP_TableLamp_1", (-6.65, 3.9, PLATFORM_TOP + 0.78))


# ------------------------------------------------------------------------------------------------ the desk

def build_desk():
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
    lx, ly = x1 - 0.3, back - 0.25
    acc.cyl((lx, ly, top_z + 0.08), 0.1, 0.03, "M_MetalBrass", segments=20)
    acc.cyl((lx, ly, top_z + 0.25), 0.012, 0.32, "M_MetalBrass", segments=8)
    acc.cyl((lx, ly - 0.05, top_z + 0.43), 0.17, 0.12, "M_LampShadeGreen", segments=24, radius2=0.08, caps=False)
    for mx in [DESK_CX - 0.55, DESK_CX + 0.35]:
        acc.cyl((mx, front + 0.25, top_z + 0.11), 0.045, 0.1, "M_Ceramic", segments=16)
    acc.box(DESK_CX - 0.2, DESK_CX + 0.15, front + 0.2, front + 0.45, top_z + 0.06, top_z + 0.068, "M_Paper")
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

    # Bookshelf behind the hosts: the cosy backdrop of the host solo shot.
    shelf = Piece("HostShelf", bevel=0.006)
    sx0, sx1 = DESK_CX - 1.5, DESK_CX + 1.5
    sy0, sy1 = WALL_Y - 0.42, WALL_Y - 0.02
    shelf.box(sx0, sx0 + 0.05, sy0, sy1, 0, 3.6, "M_WoodDark")
    shelf.box(sx1 - 0.05, sx1, sy0, sy1, 0, 3.6, "M_WoodDark")
    shelf.box((sx0 + sx1) / 2 - 0.025, (sx0 + sx1) / 2 + 0.025, sy0, sy1, 0, 3.6, "M_WoodDark")
    shelf.box(sx0, sx1, sy1 - 0.03, sy1, 0, 3.6, "M_WoodDark")
    levels = [0.0, 0.75, 1.5, 2.25, 2.95, 3.6]
    for z in levels:
        shelf.box(sx0, sx1, sy0, sy1, z, z + 0.04, "M_WoodDark")
    shelf.build()
    fill_books("HostShelfBooks", sx0 + 0.07, sx1 - 0.07, sy0 + 0.04, sy1 - 0.06, levels[:-1], skip_mid=(sx0 + sx1) / 2)
    for i, z in enumerate([0.79, 2.29]):
        empty("LP_Shelf_%d" % (i + 1), ((sx0 + sx1) / 2, sy0 + 0.1, levels[[1, 3][i]] + 0.62))


def fill_books(name, x0, x1, y0, y1, levels, skip_mid=None):
    books = Piece(name, bevel=0.003)
    mats = ["M_BookA", "M_BookB", "M_BookC", "M_BookD", "M_BookE"]
    for li, z in enumerate(levels):
        x = x0
        while x < x1 - 0.05:
            if skip_mid is not None and abs(x - skip_mid) < 0.06:
                x += 0.08
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
                if x + wb > x1 or (skip_mid is not None and abs(x + wb - skip_mid) < 0.05):
                    break
                books.box(x, x + wb, y1 - db, y1, z + 0.04, z + 0.04 + hb, random.choice(mats))
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
    stage = Piece("Stage", bevel=0.015)
    stage.box(2.75, 8.25, 3.05, WALL_Y, 0.0, STAGE_TOP, "M_WoodDark")
    stage.box(2.85, 8.15, 3.1, WALL_Y, STAGE_TOP - 0.005, STAGE_TOP, "M_StageFloor")
    stage.build()
    strip = Piece("StageStrip", smooth=False)
    strip.box(2.8, 8.2, 3.02, 3.05, 0.2, 0.26, "M_GlowStrip")
    strip.build()

    # Proscenium: a wooden front with an arched opening (original shape: no bulbs around it, D93).
    plate = Piece("Proscenium", bevel=0.0, smooth=False)
    plate.box(2.55, 8.45, PROSC_Y[0], PROSC_Y[1], 0.0, 5.6, "M_WoodPanel")
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
    sol.thickness = 0.015

    # Stage lamps on stands at both front corners, aimed at the board.
    for i, (x, y) in enumerate([(2.35, 2.6), (8.65, 2.6)]):
        build_stage_lamp("StageLamp%d" % (i + 1), x, y, i + 1)


def build_curtain_half(name, edge_x, sign, width):
    """A pleated curtain half whose origin is its outer edge, so scaling x gathers it to the side."""
    cur = Piece(name)
    nu, nv = 90, 8
    pts = []
    z0, z1 = STAGE_TOP + 0.01, CURTAIN_TOP + 0.5
    for j in range(nv + 1):
        z = z0 + (z1 - z0) * j / nv
        hem = 0.015 * math.sin(j * 1.3) if j == 0 else 0.0
        for i in range(nu + 1):
            u = i / nu
            x = edge_x + sign * width * u
            depth = 0.055 * math.sin(u * width * 2 * math.pi / 0.21) + hem
            pts.append((x, CURTAIN_Y + depth, z))
    cur.grid(pts, nu, nv, "M_Velvet")
    obj = cur.build(origin=(edge_x, CURTAIN_Y, z0))
    sol = obj.modifiers.new("Thick", "SOLIDIFY")
    sol.thickness = 0.012
    return obj


def build_stage_lamp(name, x, y, index):
    lamp = Piece(name, bevel=0.004)
    floor_z = 0.0
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
    lamp.cyl((x, y, 1.4), 0.022, 1.9, "M_MetalBlack", segments=10)
    head_z = 2.4
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


# ------------------------------------------------------------------------------------------------ band riser (no band yet)

def build_band():
    riser = Piece("BandRiser", bevel=0.015)
    riser.box(8.85, 11.9, 2.4, 5.9, 0.0, 0.4, "M_WoodDark")
    riser.build()
    strip = Piece("BandRiserStrip", smooth=False)
    strip.box(8.9, 11.85, 2.37, 2.4, 0.16, 0.22, "M_GlowStrip")
    strip.box(8.82, 8.85, 2.45, 5.85, 0.16, 0.22, "M_GlowStrip")
    strip.build()
    top = 0.4
    kit = Piece("DrumKit", bevel=0.004)
    bx, by = 10.75, 4.7
    kit.cyl((bx, by, top + 0.3), 0.3, 0.42, "M_DrumShell", axis="y", segments=32)
    kit.cyl((bx, by - 0.22, top + 0.3), 0.27, 0.01, "M_DrumHead", axis="y", segments=32)
    kit.cyl((bx - 0.45, by - 0.25, top + 0.62), 0.18, 0.16, "M_DrumShell", segments=24)   # snare
    kit.cyl((bx - 0.45, by - 0.25, top + 0.71), 0.17, 0.01, "M_DrumHead", segments=24)
    kit.cyl((bx - 0.45, by - 0.25, top + 0.3), 0.012, 0.6, "M_Chrome", segments=8)
    for tx in [bx - 0.18, bx + 0.18]:
        kit.cyl((tx, by - 0.05, top + 0.78), 0.14, 0.16, "M_DrumShell", segments=24)
        kit.cyl((tx, by - 0.05, top + 0.865), 0.13, 0.01, "M_DrumHead", segments=24)
    kit.cyl((bx + 0.55, by - 0.15, top + 0.32), 0.22, 0.45, "M_DrumShell", segments=24)    # floor tom
    kit.cyl((bx + 0.55, by - 0.15, top + 0.55), 0.21, 0.01, "M_DrumHead", segments=24)
    for cxp, cyp, h in [(bx - 0.75, by - 0.05, 1.05), (bx + 0.7, by + 0.25, 1.25), (bx - 0.35, by + 0.3, 1.3)]:
        kit.cyl((cxp, cyp, top + h / 2), 0.01, h, "M_Chrome", segments=8)
        kit.cyl((cxp, cyp, top + h), 0.24, 0.006, "M_Cymbal", segments=32, radius2=0.2)
    kit.cyl((bx, by + 0.45, top + 0.25), 0.18, 0.06, "M_Leather", segments=20)             # stool
    kit.cyl((bx, by + 0.45, top + 0.12), 0.02, 0.24, "M_Chrome", segments=8)
    kit.build()

    keys = Piece("Keyboard", bevel=0.006)
    kx, ky = 9.55, 3.4
    keys.box(kx - 0.6, kx + 0.6, ky - 0.18, ky + 0.18, top + 0.85, top + 0.95, "M_MetalBlack")
    keys.box(kx - 0.55, kx + 0.55, ky - 0.17, ky + 0.02, top + 0.95, top + 0.965, "M_KeysWhite")
    for s in [-1, 1]:
        v = keys.box(kx - 0.02, kx + 0.02, ky - 0.25, ky + 0.25, top, top + 0.88, "M_MetalBlack")
        bmesh.ops.rotate(keys.bm, cent=(kx, ky, top + 0.44), matrix=Matrix.Rotation(s * 0.5, 3, "X"), verts=v)
    keys.build()

    amp = Piece("Amps", bevel=0.012)
    amp.box(11.05, 11.65, 2.75, 3.15, top, top + 0.62, "M_Amp")
    amp.box(11.1, 11.6, 2.73, 2.75, top + 0.06, top + 0.5, "M_AmpCloth")
    amp.box(9.05, 9.5, 5.2, 5.55, top, top + 0.5, "M_Amp")
    amp.cyl((10.1, 2.95, top + 0.7), 0.01, 1.4, "M_Chrome", segments=8)                     # mic stand
    amp.cyl((10.1, 2.95, top + 0.02), 0.14, 0.03, "M_MetalBlack", segments=16)
    amp.cyl((10.1, 2.9, top + 1.43), 0.025, 0.12, "M_MetalBlack", segments=12, axis="y")
    amp.build()


# ------------------------------------------------------------------------------------------------ dressing

def build_dressing():
    # Tall bookshelf at the far left, by the window.
    shelf = Piece("LeftShelf", bevel=0.006)
    sx0, sx1 = -12.3, -9.3
    sy0, sy1 = WALL_Y - 0.45, WALL_Y - 0.02
    for x in [sx0, (sx0 + sx1) / 2 - 0.025, sx1 - 0.05]:
        shelf.box(x, x + 0.05, sy0, sy1, 0, 4.4, "M_WoodDark")
    shelf.box(sx0, sx1, sy1 - 0.03, sy1, 0, 4.4, "M_WoodDark")
    levels = [0.0, 0.8, 1.6, 2.4, 3.2, 4.0]
    for z in levels:
        shelf.box(sx0, sx1, sy0, sy1, z, z + 0.04, "M_WoodDark")
    shelf.build()
    fill_books("LeftShelfBooks", sx0 + 0.07, sx1 - 0.07, sy0 + 0.04, sy1 - 0.06, levels[:-1], skip_mid=(sx0 + sx1) / 2)
    empty("LP_Shelf_3", ((sx0 + sx1) / 2, sy0 + 0.1, 2.25))

    # Plants: a big one by the window, one by the stage, small ones on the shelves.
    plant("PlantBig", -9.7, 5.55, PLATFORM_TOP, 1.0)
    plant("PlantStage", 2.25, 5.6, PLATFORM_TOP, 0.85)
    plant("PlantShelf1", -11.6, WALL_Y - 0.25, 3.24, 0.32)
    plant("PlantShelf2", DESK_CX + 1.1, WALL_Y - 0.25, 2.99, 0.3)
    plant("PlantBand", 11.7, 5.6, 0.4, 0.7)

    # Wall sconces between the stage and the band, and at the far left.
    sconce = Piece("Sconces", bevel=0.004)
    for i, (x, z) in enumerate([(-12.45, 3.3), (8.65, 3.6), (12.45, 3.3)]):
        y = WALL_Y - 0.03 if abs(x) < 12 else 2.0
        if abs(x) >= 12:
            sconce.box(x - 0.03 * (1 if x > 0 else -1) - 0.03, x + 0.03, y - 0.12, y + 0.12, z - 0.2, z + 0.1, "M_MetalBrass")
            sconce.cyl((x - 0.18 * (1 if x > 0 else -1), y, z + 0.1), 0.11, 0.2, "M_LampShade", segments=20, radius2=0.08, caps=False)
            empty("LP_Sconce_%d" % (i + 1), (x - 0.18 * (1 if x > 0 else -1), y, z + 0.08))
        else:
            sconce.box(x - 0.12, x + 0.12, y - 0.04, y, z - 0.2, z + 0.1, "M_MetalBrass")
            sconce.cyl((x, y - 0.18, z + 0.1), 0.11, 0.2, "M_LampShade", segments=20, radius2=0.08, caps=False)
            empty("LP_Sconce_%d" % (i + 1), (x, y - 0.18, z + 0.08))
    sconce.build()


def plant(name, x, y, z, s):
    p = Piece(name, bevel=0.0)
    p.cyl((x, y, z + 0.22 * s), 0.2 * s, 0.44 * s, "M_Pot", segments=20, radius2=0.16 * s)
    p.cyl((x, y, z + 0.43 * s), 0.18 * s, 0.02 * s, "M_Soil", segments=20)
    for k in range(int(26 * max(s, 0.5))):
        a = random.uniform(0, 2 * math.pi)
        r = random.uniform(0.0, 0.32) * s
        h = random.uniform(0.55, 1.4) * s
        p.sphere((x + r * math.cos(a), y + r * math.sin(a), z + h), random.uniform(0.09, 0.16) * s, "M_Leaf",
                 scale=(1.0, 0.45, 1.6), subdiv=1)
    for k in range(5):
        a = random.uniform(0, 2 * math.pi)
        v = p.cyl((0, 0, 0), 0.008 * s, 0.9 * s, "M_Stem", segments=6)
        bmesh.ops.rotate(p.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(random.uniform(0.1, 0.35), 3, "X"), verts=v)
        bmesh.ops.rotate(p.bm, cent=(0, 0, 0), matrix=Matrix.Rotation(a, 3, "Z"), verts=v)
        bmesh.ops.translate(p.bm, vec=Vector((x, y, z + 0.85 * s)), verts=v)
    p.build()


# ------------------------------------------------------------------------------------------------ export

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

    for obj in COLLECTION.objects:
        box_uvs(obj)
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
