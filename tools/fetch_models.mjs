// Downloads the photo scanned CC0 models of the 3D studio from Poly Haven (polyhaven.com, every asset CC0) as glTF
// with 2k textures into tools/blender/assets/polyhaven/<id>/, where tools/blender/build_props.py turns them into the
// studio's prop models (docs/21_staging.md, "Built set"). The Sketchfab models (CC BY) are committed by hand in
// tools/blender/assets/sketchfab/ with their own license log. Run from the repo root on Windows or macOS:
//     node tools/fetch_models.mjs [--force]
// Files that already exist are kept unless --force is given. Needs Node 18 or later (global fetch).

import { mkdir, writeFile, access } from "node:fs/promises";
import { join, dirname } from "node:path";

const OUT = "tools/blender/assets/polyhaven";
const API = "https://api.polyhaven.com";
const HEADERS = { "User-Agent": "family-quiz-studio-build" };

// id: Poly Haven asset; res: texture size; use: where the set uses it (build_studio.py, PROPS in build_props.py).
export const MODELS = [
  { id: "ArmChair_01", res: "2k", use: "outer guest armchairs (re-tinted)" },
  { id: "modern_arm_chair_01", res: "2k", use: "inner guest armchairs" },
  { id: "modern_coffee_table_01", res: "2k", use: "coffee table" },
  { id: "side_table_01", res: "2k", use: "side table by the sofa" },
  { id: "ClassicNightstand_01", res: "2k", use: "side table by the sofa" },
  { id: "throw_pillows_01", res: "2k", use: "sofa pillows" },
  { id: "Chandelier_01", res: "2k", use: "chandelier over the sofa" },
  { id: "vintage_oil_lamp", res: "2k", use: "table lamp" },
  { id: "brass_candleholders", res: "2k", use: "candles on the window sill" },
  { id: "wooden_candlestick", res: "1k", use: "candles" },
  { id: "potted_plant_01", res: "2k", use: "big plants" },
  { id: "potted_plant_02", res: "2k", use: "plants" },
  { id: "potted_plant_04", res: "1k", use: "small plants on shelves and the desk" },
  { id: "book_encyclopedia_set_01", res: "2k", use: "host shelf" },
  { id: "mantel_clock_01", res: "1k", use: "host shelf" },
  { id: "fancy_picture_frame_01", res: "2k", use: "walls" },
  { id: "fancy_picture_frame_02", res: "2k", use: "walls" },
  { id: "standing_picture_frame_01", res: "1k", use: "side table" },
  { id: "ceramic_vase_01", res: "1k", use: "shelves" },
  { id: "brass_vase_01", res: "1k", use: "shelves" },
  { id: "antique_ceramic_vase_01", res: "1k", use: "shelves" },
  { id: "chess_set", res: "1k", use: "coffee table" },
  { id: "wine_bottles_01", res: "1k", use: "side table" },
  { id: "wicker_basket_01", res: "1k", use: "beside the sofa" },
  { id: "lightbulb_01", res: "1k", use: "festoon bulbs over the band stage" },
  { id: "Ukulele_01", res: "1k", use: "band wall" },
  { id: "Ottoman_01", res: "2k", use: "guest area" },
];

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

async function download(url, file, force) {
  if (!force && (await exists(file))) return;
  await mkdir(dirname(file), { recursive: true });
  const r = await fetch(url, { headers: HEADERS });
  if (!r.ok) throw new Error(url + " -> " + r.status);
  await writeFile(file, Buffer.from(await r.arrayBuffer()));
  console.log("fetched", file);
}

async function main() {
  const force = process.argv.includes("--force");
  const only = process.argv.slice(2).filter((a) => !a.startsWith("--"));
  const rows = [];
  for (const model of MODELS) {
    const [files, info] = await Promise.all([json(`${API}/files/${model.id}`), json(`${API}/info/${model.id}`)]);
    if (only.length === 0 || only.includes(model.id)) {
      const gltf = files.gltf?.[model.res]?.gltf;
      if (!gltf) throw new Error(`${model.id} has no glTF at ${model.res}`);
      const folder = join(OUT, model.id);
      await download(gltf.url, join(folder, `${model.id}.gltf`), force);
      for (const [path, entry] of Object.entries(gltf.include ?? {})) await download(entry.url, join(folder, path), force);
    }
    const authors = Object.keys(info.authors ?? {}).join(", ");
    rows.push(`| ${model.id} | ${info.name} | ${model.res} | ${model.use} | https://polyhaven.com/a/${model.id} | ${authors} | CC0 |`);
  }
  const log = [
    "# Poly Haven models: license log",
    "",
    "Written by `tools/fetch_models.mjs`. Every model is from Poly Haven under CC0 (public domain, no attribution",
    "required, commercial use allowed: https://polyhaven.com/license), so the game can be sold (D1, the rule in",
    "`docs/12_audio.md`). The authors are credited here anyway.",
    "",
    "| Model | Name | Textures | Used for | Source | Author | License |",
    "| --- | --- | --- | --- | --- | --- | --- |",
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
