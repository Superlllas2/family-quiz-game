// Downloads the photo scanned CC0 texture sets of the 3D studio from Poly Haven (polyhaven.com, every asset CC0)
// into unity/Assets/Game/Art/Textures/Scanned and writes the license log next to them (docs/21_staging.md,
// "Game night 1 version"). Run from the repo root on Windows or macOS:  node tools/fetch_textures.mjs [--force]
// Files that already exist are kept unless --force is given. Needs Node 18 or later (global fetch).

import { mkdir, writeFile, access } from "node:fs/promises";
import { join } from "node:path";

const OUT = "unity/Assets/Game/Art/Textures/Scanned";
const API = "https://api.polyhaven.com";
const HEADERS = { "User-Agent": "family-quiz-studio-build" };

// id: Poly Haven asset; res: download size (2k for big surfaces close to the camera, 1k for the rest);
// use: the studio material it feeds (StudioMaterials.cs).
export const SETS = [
  { id: "wood_floor_worn", res: "2k", use: "M_FloorWood" },
  { id: "dark_wooden_planks", res: "1k", use: "M_StageFloor" },
  { id: "dark_wood", res: "2k", use: "M_WoodPanel" },
  { id: "walnut_veneer", res: "1k", use: "M_WoodDark" },
  { id: "rosewood_veneer1", res: "1k", use: "M_WoodWarm" },
  { id: "poly_wool_herringbone", res: "2k", use: "M_FabricSofa" },
  { id: "fabric_leather_02", res: "1k", use: "M_FabricChair" },
  { id: "velour_velvet", res: "2k", use: "M_Velvet, M_BackCloth" },
  { id: "brown_leather", res: "1k", use: "M_Leather" },
  { id: "dirty_carpet", res: "1k", use: "M_Rug, M_Rug2 (surface only, the pattern stays procedural)" },
];

// Poly Haven map name -> our file suffix. nor_gl is the OpenGL normal map Unity expects.
const MAPS = { Diffuse: "diff", nor_gl: "nor", Rough: "rough" };

async function exists(path) {
  try {
    await access(path);
    return true;
  } catch {
    return false;
  }
}

async function json(url) {
  const r = await fetch(url, { headers: HEADERS });
  if (!r.ok) throw new Error(url + " -> " + r.status);
  return r.json();
}

async function main() {
  const force = process.argv.includes("--force");
  await mkdir(OUT, { recursive: true });
  const rows = [];
  for (const set of SETS) {
    const [files, info] = await Promise.all([json(`${API}/files/${set.id}`), json(`${API}/info/${set.id}`)]);
    for (const [map, suffix] of Object.entries(MAPS)) {
      const entry = files[map]?.[set.res]?.jpg ?? files[map]?.[set.res]?.png;
      if (!entry) throw new Error(`${set.id} has no ${map} at ${set.res}`);
      const ext = entry.url.endsWith(".png") ? "png" : "jpg";
      const file = join(OUT, `${set.id}_${suffix}.${ext}`);
      if (!force && (await exists(file))) continue;
      const r = await fetch(entry.url, { headers: HEADERS });
      if (!r.ok) throw new Error(entry.url + " -> " + r.status);
      await writeFile(file, Buffer.from(await r.arrayBuffer()));
      console.log("fetched", file);
    }
    const authors = Object.keys(info.authors ?? {}).join(", ");
    const size = info.dimensions ? `${(info.dimensions[0] / 1000).toFixed(2)} m` : "?";
    rows.push(`| ${set.id}_{diff,nor,rough} | ${info.name} | ${set.res} | ${size} | ${set.use} | https://polyhaven.com/a/${set.id} | ${authors} | CC0 |`);
  }
  const log = [
    "# Scanned textures: license log",
    "",
    "Written by `tools/fetch_textures.mjs`. Every file is from Poly Haven under CC0 (public domain, no attribution",
    "required, commercial use allowed: https://polyhaven.com/license), so the game can be sold (D1, the rule in",
    "`docs/12_audio.md`). The authors are credited here anyway.",
    "",
    "| Files | Name | Resolution | Covers | Used by | Source | Author | License |",
    "| --- | --- | --- | --- | --- | --- | --- | --- |",
    ...rows,
    "",
  ].join("\n");
  await writeFile(join(OUT, "LICENSES.md"), log);
  console.log("wrote", join(OUT, "LICENSES.md"));
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
