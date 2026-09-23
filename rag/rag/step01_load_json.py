"""STEP 1 - Load the JSON datasets.

Read and validate all three ShilpoHub files up front, so a missing file, a
syntax error or a wrong structure fails here - with the file name - instead of
as a KeyError three steps later.

Output: {filename: parsed JSON}, keyed by file name (e.g. "craft.json").
"""

import json
from pathlib import Path
from typing import Any, Dict, Iterable, Optional

import config

# The list each file must expose. Everything else in a file is optional.
_REQUIRED_LIST = {
    "craft.json": "crafts",
    "craftDetails.json": "crafts",
    "GEO.json": "crafts",
}


def load_json(data_dir=None, filenames: Iterable[str] = config.DATASET_FILES) -> Dict[str, Any]:
    base = Path(data_dir or config.DATA_DIR).expanduser().resolve()
    if not base.is_dir():
        raise FileNotFoundError(f"Dataset folder not found: {base}")

    datasets: Dict[str, Any] = {}
    for name in filenames:
        path = base / name
        if not path.is_file():
            raise FileNotFoundError(f"Dataset file not found: {path}")
        if path.stat().st_size == 0:
            raise ValueError(f"Dataset file is empty: {name}")

        try:
            # utf-8-sig tolerates a BOM that Windows editors like to add.
            data = json.loads(path.read_text(encoding="utf-8-sig"))
        except json.JSONDecodeError as exc:
            raise ValueError(f"{name} is not valid JSON (line {exc.lineno}, column {exc.colno}): {exc.msg}") from exc

        if not isinstance(data, dict):
            raise ValueError(f"{name}: expected a JSON object at the top level, got {type(data).__name__}.")

        key = _REQUIRED_LIST.get(name)
        if key and (not isinstance(data.get(key), list) or not data[key]):
            raise ValueError(f"{name}: expected a non-empty '{key}' list.")

        datasets[name] = data
        detail = f"{len(data[key])} {key}" if key else f"{len(data)} top-level keys"
        print(f"[1] Loaded     : {name} ({path.stat().st_size / 1024:.1f} KB, {detail})")

    return datasets
