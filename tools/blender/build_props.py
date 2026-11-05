"""
Turns the studio's bought-in models (Poly Haven, CC0, fetched by tools/fetch_models.mjs; Sketchfab, CC BY, committed in
tools/blender/assets/sketchfab) into one FBX per prop in unity/Assets/Game/Art/Models/Props, with Unity ready textures
in unity/Assets/Game/Art/Textures/Props/<Prop> and a manifest, Props.json, that StudioProps.cs builds the materials from
(docs/21_staging.md, "Built set"). build_studio.py places the props with Prop_<Name>_<n> markers.

Run headless from the repo root (Windows or macOS):
    blender --background --python tools/blender/build_props.py -- [Name ...] [--sheet sheet.png]

Every prop comes out the same way: one mesh, real scale in metres, facing the audience (-y in the design frame), its
origin at the bottom centre, turned half a turn so the FBX lands in Unity as designed (build_studio.py, to_unity_frame).
Its material slots are MX_<Prop>_<k>. Each material's textures are written as:
    _base    base colour (sRGB; alpha kept when the material is cut out or blended)
    _normal  OpenGL normal map (what glTF and Unity both use)
    _mask    URP Lit's metallic map: R metallic, G occlusion, A smoothness (1 - roughness), factors applied; at most
             1024 px, since it carries less detail than the colour and normal maps
    _emit    emission colour
"""

import json
import math
import os
import sys

import bpy
import numpy as np
from mathutils import Matrix, Vector

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ASSETS = os.path.join(REPO, "tools", "blender", "assets")
MODEL_OUT = os.path.join(REPO, "unity", "Assets", "Game", "Art", "Models", "Props")
TEX_OUT = os.path.join(REPO, "unity", "Assets", "Game", "Art", "Textures", "Props")
MANIFEST = os.path.join(MODEL_OUT, "Props.json")

# name: (source file under assets/, yaw fix in degrees so the front faces -y, max triangles, max texture size,
#        tints {material name part: [r, g, b]} multiplied into the base colour in Unity)
PROPS = {
    # Guest area
    "LongSofa": ("sketchfab/LongSofa.glb", -90, 110000, 2048, {"leather": [0.2, 0.3, 0.24]}),
    "ArmChair": ("polyhaven/ArmChair_01/ArmChair_01.gltf", 0, 40000, 2048, {"armchair": [0.78, 0.5, 0.36]}),
    "ModernArmChair": ("polyhaven/modern_arm_chair_01/modern_arm_chair_01.gltf", 0, 40000, 2048, {}),
    "CoffeeTable": ("polyhaven/modern_coffee_table_01/modern_coffee_table_01.gltf", 90, 40000, 2048, {}),
    "SideTable": ("polyhaven/side_table_01/side_table_01.gltf", 0, 20000, 1024, {}),
    "Nightstand": ("polyhaven/ClassicNightstand_01/ClassicNightstand_01.gltf", 0, 20000, 1024, {}),
    "ThrowPillows": ("polyhaven/throw_pillows_01/throw_pillows_01.gltf", 0, 20000, 1024, {}),
    "Ottoman": ("polyhaven/Ottoman_01/Ottoman_01.gltf", 0, 20000, 1024, {}),
    "WickerBasket": ("polyhaven/wicker_basket_01/wicker_basket_01.gltf", 0, 20000, 1024, {}),
    "FloorLamp": ("sketchfab/FloorLamp.glb", 0, 20000, 1024, {}),
    "Chandelier": ("polyhaven/Chandelier_01/Chandelier_01.gltf", 0, 40000, 1024, {}),
    "OilLamp": ("polyhaven/vintage_oil_lamp/vintage_oil_lamp.gltf", 0, 20000, 1024, {}),
    "Candles": ("polyhaven/brass_candleholders/brass_candleholders.gltf", 0, 45000, 1024, {}),
    "Candlestick": ("polyhaven/wooden_candlestick/wooden_candlestick.gltf", 0, 8000, 512, {}),
    "PlantTall": ("polyhaven/potted_plant_01/potted_plant_01.gltf", 0, 100000, 2048, {}),
    "PlantWide": ("polyhaven/potted_plant_02/potted_plant_02.gltf", 0, 75000, 2048, {}),
    "PlantSmall": ("polyhaven/potted_plant_04/potted_plant_04.gltf", 0, 8000, 1024, {}),
    "Encyclopedia": ("polyhaven/book_encyclopedia_set_01/book_encyclopedia_set_01.gltf", 0, 30000, 1024, {}),
    "MantelClock": ("polyhaven/mantel_clock_01/mantel_clock_01.gltf", 0, 20000, 1024, {}),
    "PictureFrame1": ("polyhaven/fancy_picture_frame_01/fancy_picture_frame_01.gltf", 0, 5000, 1024, {}),
    "PictureFrame2": ("polyhaven/fancy_picture_frame_02/fancy_picture_frame_02.gltf", 0, 20000, 1024, {}),
    "StandingFrame": ("polyhaven/standing_picture_frame_01/standing_picture_frame_01.gltf", 0, 5000, 512, {}),
    "VaseWhite": ("polyhaven/ceramic_vase_01/ceramic_vase_01.gltf", 0, 5000, 512, {}),
    "VaseBrass": ("polyhaven/brass_vase_01/brass_vase_01.gltf", 0, 12000, 512, {}),
    "VaseAntique": ("polyhaven/antique_ceramic_vase_01/antique_ceramic_vase_01.gltf", 0, 10000, 512, {}),
    "ChessSet": ("polyhaven/chess_set/chess_set.gltf", 0, 40000, 1024, {}),
    "WineBottles": ("polyhaven/wine_bottles_01/wine_bottles_01.gltf", 0, 20000, 1024, {}),
    "Bulb": ("polyhaven/lightbulb_01/lightbulb_01.gltf", 0, 1500, 256, {}),
    "Ukulele": ("polyhaven/Ukulele_01/Ukulele_01.gltf", 0, 9000, 1024, {}),
    # Band
    "DrumKit": ("sketchfab/DrumKit.glb", 0, 140000, 2048, {"": [0.72, 0.7, 0.66]}),
    "Guitar": ("sketchfab/Guitar.glb", 0, 45000, 1024, {}),
    "Bass": ("sketchfab/Bass.glb", 0, 26400, 1024, {}),
    "Amp": ("sketchfab/Amp.glb", -90, 5000, 1024, {}),
    "Piano": ("sketchfab/Piano.glb", 0, 5000, 2048, {}),
}

# Props whose glowing part shares a material with the rest: faces above a height (metres, from the prop's bottom) get
# a copy of their material named <material>_<part>, so Unity can make only that part glow (StudioProps.cs, Glows).
SPLITS = {
    "FloorLamp": ("shade", 1.2),
}


# ------------------------------------------------------------------------------------------------ node graph

def linked_from(socket):
    return socket.links[0].from_socket if socket.is_linked else None


def trace(socket, depth=0):
    """Follows a BSDF input upstream to an image. Returns (image, channel, factor, ops) or None. channel is 'Color',
    'Alpha', 'Red', 'Green' or 'Blue'; factor is a constant multiplied in on the way (glTF factors); ops lists the
    math operations met (a LESS_THAN marks an alpha cut out)."""
    src = linked_from(socket)
    if src is None or depth > 8:
        return None
    node = src.node
    if node.type == "TEX_IMAGE":
        return (node.image, "Alpha" if src.name == "Alpha" else "Color", 1.0, []) if node.image else None
    if node.type in ("SEPARATE_COLOR", "SEPRGB", "SEPARATE_RGB"):
        hit = trace(node.inputs[0], depth + 1)
        if hit:
            return (hit[0], {"R": "Red", "G": "Green", "B": "Blue"}.get(src.name, src.name), hit[2], hit[3])
        return None
    if node.type == "MATH":
        factor, found = 1.0, None
        for inp in node.inputs[:2]:
            if inp.is_linked and found is None:
                found = trace(inp, depth + 1)
            elif not inp.is_linked and node.operation == "MULTIPLY":
                factor *= inp.default_value
        if found:
            return (found[0], found[1], found[2] * factor, found[3] + [(node.operation, node.inputs[1].default_value)])
        return None
    if node.type == "NORMAL_MAP":
        return trace(node.inputs["Color"], depth + 1)
    for inp in node.inputs:
        if inp.is_linked and inp.type in ("RGBA", "VALUE", "VECTOR"):
            hit = trace(inp, depth + 1)
            if hit:
                return hit
    return None


def occlusion_image(mat):
    """glTF's occlusion goes into a 'glTF Material Output' group node; the importer wires it there."""
    for node in mat.node_tree.nodes:
        if node.type == "GROUP" and node.node_tree and "glTF" in node.node_tree.name:
            for inp in node.inputs:
                if inp.name.lower().startswith("occlusion"):
                    return trace(inp)
    return None


# ------------------------------------------------------------------------------------------------ images

def pixels(img, size):
    """The image as float RGBA rows (bottom row first), at most size on its longer side."""
    img = img.copy()
    w, h = img.size
    if max(w, h) > size:
        s = size / max(w, h)
        img.scale(max(1, int(w * s)), max(1, int(h * s)))
        w, h = img.size
    px = np.empty(w * h * 4, dtype=np.float32)
    img.pixels.foreach_get(px)
    bpy.data.images.remove(img)
    return px.reshape(h, w, 4)


def write(px, path, alpha):
    h, w = px.shape[:2]
    img = bpy.data.images.new(os.path.basename(path), w, h, alpha=alpha, float_buffer=False)
    img.colorspace_settings.name = "Non-Color"   # write the values as they are
    img.pixels.foreach_set(px.astype(np.float32).ravel())
    img.filepath_raw = path
    img.file_format = "PNG" if path.endswith(".png") else "JPEG"
    scene = bpy.context.scene
    scene.render.image_settings.quality = 92
    img.save(filepath=path, quality=92) if not path.endswith(".png") else img.save(filepath=path)
    bpy.data.images.remove(img)


def export_material(prop, k, mat, size, tints, out_dir):
    bsdf = next((n for n in mat.node_tree.nodes if n.type == "BSDF_PRINCIPLED"), None) if mat and mat.node_tree else None
    slot = "MX_%s_%d" % (prop, k)
    entry = {"slot": slot, "source": mat.name if mat else "", "baseColor": [1, 1, 1, 1], "metallic": 0.0,
             "smoothness": 0.4, "alpha": "opaque", "cutoff": 0.5, "normalScale": 1.0, "emission": [0, 0, 0]}
    if bsdf is None:
        return entry
    base_in, alpha_in = bsdf.inputs["Base Color"], bsdf.inputs["Alpha"]
    base = trace(base_in)
    if not base_in.is_linked:
        entry["baseColor"] = list(base_in.default_value)
    for part, tint in tints.items():
        if part.lower() in mat.name.lower():
            entry["baseColor"] = [tint[0], tint[1], tint[2], 1.0]
    alpha = trace(alpha_in)
    if alpha:
        cut = [v for op, v in alpha[3] if op == "LESS_THAN"]
        entry["alpha"] = "mask" if cut or getattr(mat, "surface_render_method", "") == "DITHERED" else "blend"
        entry["cutoff"] = float(cut[0]) if cut else 0.5
    elif alpha_in.default_value < 0.99:
        entry["alpha"] = "blend"
        entry["baseColor"][3] = alpha_in.default_value
    if base:
        px = pixels(base[0], size)
        keep_alpha = entry["alpha"] != "opaque"
        path = os.path.join(out_dir, "%s_base.%s" % (slot, "png" if keep_alpha else "jpg"))
        if not keep_alpha:
            px[..., 3] = 1.0
        write(px, path, keep_alpha)
        entry["base"] = os.path.basename(path)

    normal_in = bsdf.inputs["Normal"]
    normal = trace(normal_in)
    if normal:
        nm = next((n for n in mat.node_tree.nodes if n.type == "NORMAL_MAP"), None)
        entry["normalScale"] = nm.inputs["Strength"].default_value if nm else 1.0
        px = pixels(normal[0], size)
        px[..., 3] = 1.0
        path = os.path.join(out_dir, "%s_normal.jpg" % slot)
        write(px, path, False)
        entry["normal"] = os.path.basename(path)

    rough = trace(bsdf.inputs["Roughness"])
    metal = trace(bsdf.inputs["Metallic"])
    occ = occlusion_image(mat)
    rough_v = bsdf.inputs["Roughness"].default_value
    metal_v = bsdf.inputs["Metallic"].default_value
    entry["smoothness"] = 1.0 - rough_v
    entry["metallic"] = metal_v
    if rough or metal or occ:
        ref = (rough or metal or occ)[0]
        size = min(size, 1024)
        ref_px = pixels(ref, size)
        h, w = ref_px.shape[:2]

        def channel(hit, default):
            if not hit:
                return np.full((h, w), default, dtype=np.float32)
            p = pixels(hit[0], size)
            if p.shape[:2] != (h, w):
                img = bpy.data.images.new("tmp", p.shape[1], p.shape[0], float_buffer=True)
                img.pixels.foreach_set(p.ravel())
                img.scale(w, h)
                p = np.empty(w * h * 4, dtype=np.float32)
                img.pixels.foreach_get(p)
                bpy.data.images.remove(img)
                p = p.reshape(h, w, 4)
            c = {"Red": 0, "Green": 1, "Blue": 2, "Alpha": 3}.get(hit[1], 0)
            return p[..., c] * hit[2]

        mask = np.zeros((h, w, 4), dtype=np.float32)
        mask[..., 0] = channel(metal, metal_v)
        mask[..., 1] = channel(occ, 1.0)
        mask[..., 3] = 1.0 - channel(rough, rough_v)
        path = os.path.join(out_dir, "%s_mask.png" % slot)
        write(np.clip(mask, 0, 1), path, True)
        entry["mask"] = os.path.basename(path)
        entry["smoothness"] = 1.0
        entry["metallic"] = 1.0

    emit_in = bsdf.inputs.get("Emission Color")
    strength = bsdf.inputs.get("Emission Strength")
    emit = trace(emit_in) if emit_in else None
    if emit:
        px = pixels(emit[0], size)
        px[..., 3] = 1.0
        path = os.path.join(out_dir, "%s_emit.jpg" % slot)
        write(px, path, False)
        entry["emit"] = os.path.basename(path)
        entry["emission"] = [strength.default_value if strength else 1.0] * 3
    elif emit_in and strength and strength.default_value > 0:
        c = emit_in.default_value
        if max(c[0], c[1], c[2]) > 0.001:
            entry["emission"] = [c[0] * strength.default_value, c[1] * strength.default_value, c[2] * strength.default_value]
    return entry


# ------------------------------------------------------------------------------------------------ meshes

def import_source(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=path)
    bpy.context.view_layer.update()
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not meshes:
        raise RuntimeError("no meshes in " + path)
    bpy.ops.object.select_all(action="DESELECT")
    for o in meshes:
        o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.object.make_single_user(object=True, obdata=True)
    for o in meshes:
        mw = o.matrix_world.copy()
        o.parent = None
        o.matrix_world = mw
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for o in list(bpy.context.scene.objects):
        if o.type != "MESH":
            bpy.data.objects.remove(o, do_unlink=True)
    if len(meshes) > 1:
        bpy.ops.object.join()
    obj = bpy.context.view_layer.objects.active
    return obj


def triangles(obj):
    return sum(len(p.vertices) - 2 for p in obj.data.polygons)


def normalise(obj, yaw, max_tris):
    if yaw:
        obj.data.transform(Matrix.Rotation(math.radians(yaw), 4, "Z"))
    tris = triangles(obj)
    if tris > max_tris:
        mod = obj.modifiers.new("Decimate", "DECIMATE")
        mod.decimate_type = "COLLAPSE"
        mod.ratio = max_tris / tris
        mod.use_collapse_triangulate = True
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=mod.name)
        # Collapsed faces break the imported custom normals: smooth by angle instead.
        if obj.data.has_custom_normals:
            bpy.ops.mesh.customdata_custom_splitnormals_clear()
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(35))
    # Origin at the bottom centre.
    xs = [v.co.x for v in obj.data.vertices]
    ys = [v.co.y for v in obj.data.vertices]
    zs = [v.co.z for v in obj.data.vertices]
    offset = Vector(((min(xs) + max(xs)) / 2, (min(ys) + max(ys)) / 2, min(zs)))
    obj.data.transform(Matrix.Translation(-offset))
    size = [max(xs) - min(xs), max(ys) - min(ys), max(zs) - min(zs)]
    return tris, triangles(obj), size


def export_fbx(obj, path):
    # Half a turn about z, as build_studio.py's to_unity_frame does, so the exporter's axis change lands it as designed.
    obj.data.transform(Matrix.Rotation(math.pi, 4, "Z"))
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z",
                             axis_up="Y", bake_space_transform=True, object_types={"MESH"}, use_mesh_modifiers=True,
                             mesh_smooth_type="OFF", use_tspace=False, add_leaf_bones=False, bake_anim=False,
                             path_mode="STRIP", embed_textures=False)
    obj.data.transform(Matrix.Rotation(math.pi, 4, "Z"))


def split(obj, part, height):
    """Moves the faces above height to a copy of their material named <material>_<part>."""
    mesh = obj.data
    copies = {}
    for poly in mesh.polygons:
        if poly.center.z <= height:
            continue
        index = poly.material_index
        if index not in copies:
            source = obj.material_slots[index].material
            copy = source.copy()
            copy.name = source.name + "_" + part
            mesh.materials.append(copy)
            copies[index] = len(mesh.materials) - 1
        poly.material_index = copies[index]


def build(name, manifest):
    src, yaw, max_tris, tex, tints = PROPS[name]
    obj = import_source(os.path.join(ASSETS, src))
    obj.name = name
    obj.data.name = name
    before, after, size = normalise(obj, yaw, max_tris)
    if name in SPLITS:
        split(obj, *SPLITS[name])
    out_dir = os.path.join(TEX_OUT, name)
    os.makedirs(out_dir, exist_ok=True)
    for f in os.listdir(out_dir):
        if not f.endswith(".meta"):
            os.remove(os.path.join(out_dir, f))
    entries = []
    # One slot per distinct material, in slot order; slots are renamed so Unity matches them by name.
    for k, slot in enumerate(obj.material_slots):
        mat = slot.material
        entries.append(export_material(name, k, mat, tex, tints, out_dir))
        if mat:
            mat.name = "MX_%s_%d" % (name, k)
    export_fbx(obj, os.path.join(MODEL_OUT, name + ".fbx"))
    manifest[name] = {"size": [round(s, 4) for s in size], "triangles": after, "materials": entries}
    print("PROP %s: %d -> %d triangles, %d materials, %.2f x %.2f x %.2f m" % (name, before, after, len(entries), *size))
    return obj


def render_sheet(names, path):
    """Front views of the props side by side, to check that every one faces the audience and stands on the floor."""
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    x = 0.0
    for name in names:
        bpy.ops.import_scene.fbx(filepath=os.path.join(MODEL_OUT, name + ".fbx"))
        objs = [o for o in bpy.context.selected_objects if o.type == "MESH"]
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        for o in objs:
            # Undo export_fbx's half turn, so the sheet shows the design frame (front towards -y).
            o.data.transform(Matrix.Rotation(math.pi, 4, "Z"))
            o.location.x += x
        w = max(o.dimensions.x for o in objs)
        for o in objs:
            o.location.x += w / 2
        x += w + 0.3
    cam_data = bpy.data.cameras.new("SheetCam")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = x + 0.5
    cam = bpy.data.objects.new("SheetCam", cam_data)
    scene.collection.objects.link(cam)
    height = (x + 0.5) * 500 / 2400
    cam.location = (x / 2, -20.0, height / 2 - 0.05)
    cam.rotation_euler = (math.pi / 2, 0, 0)
    scene.camera = cam
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "SINGLE"
    scene.render.resolution_x, scene.render.resolution_y = 2400, 500
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    print("SHEET written to " + path)


def main():
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    sheet = None
    if "--sheet" in args:
        i = args.index("--sheet")
        sheet = args[i + 1]
        args = args[:i] + args[i + 2:]
    names = args or list(PROPS)
    os.makedirs(MODEL_OUT, exist_ok=True)
    manifest = {}
    if os.path.exists(MANIFEST):
        with open(MANIFEST, encoding="utf-8") as f:
            data = json.load(f)
        manifest = {p["name"]: p for p in data["props"]} if "props" in data else {}
    for name in names:
        build(name, manifest)
    # A list, so Unity's JsonUtility can read it (StudioProps.cs).
    props = [dict(name=k, **{a: b for a, b in manifest[k].items() if a != "name"}) for k in PROPS if k in manifest]
    with open(MANIFEST, "w", encoding="utf-8", newline="\n") as f:
        json.dump({"props": props}, f, indent=1)
    print("PROPS manifest: %d props" % len(manifest))
    if sheet:
        render_sheet(names, sheet)


main()
