import { cp, mkdir, rm } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const projectRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const leafletDist = resolve(projectRoot, "node_modules/leaflet/dist");
const destination = resolve(projectRoot, "wwwroot/lib/leaflet");

await rm(destination, { recursive: true, force: true });
await mkdir(destination, { recursive: true });
await Promise.all([
  cp(resolve(leafletDist, "leaflet.css"), resolve(destination, "leaflet.css")),
  cp(resolve(leafletDist, "leaflet.js"), resolve(destination, "leaflet.js")),
  cp(resolve(leafletDist, "images"), resolve(destination, "images"), {
    recursive: true
  })
]);
