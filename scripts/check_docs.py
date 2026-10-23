"""Check local Markdown links and extraction destinations using only the Python standard library."""
from pathlib import Path
import json
import re

root = Path(__file__).resolve().parent.parent
failures = []
for path in root.rglob("*.md"):
    if any(part in {"bin", "obj", ".git", "artifacts"} for part in path.parts):
        continue
    for target in re.findall(r"\]\(([^)]+)\)", path.read_text(encoding="utf-8")):
        if "://" in target or target.startswith(("#", "mailto:")):
            continue
        target = target.split("#", 1)[0]
        if target and not (path.parent / target).exists():
            failures.append(f"{path.relative_to(root)}: missing link {target}")
for item in json.loads((root / "docs/extraction-manifest.json").read_text())["files"]:
    if not (root / item["destination"]).is_file():
        failures.append(f"Missing extraction destination: {item['destination']}")
if failures:
    raise SystemExit("\n".join(failures))
print("Markdown links and extraction destinations are valid.")
