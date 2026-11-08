#!/usr/bin/env python3
"""Install Graphify in a repository-local venv, register Codex, and enable native hooks."""
from __future__ import annotations

import argparse
import os
from pathlib import Path
import subprocess
import sys
import venv

ROOT = Path(__file__).resolve().parents[2]


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--no-hooks", action="store_true", help="Do not modify local Git hooks/config")
    arguments = parser.parse_args()
    environment = ROOT / ".venv-graphify"
    binary_directory = environment / ("Scripts" if os.name == "nt" else "bin")
    interpreter = binary_directory / ("python.exe" if os.name == "nt" else "python")
    if not interpreter.is_file():
        venv.EnvBuilder(with_pip=True).create(environment)
    subprocess.run([str(interpreter), "-m", "pip", "install", "-r",
                    str(ROOT / "Tools/Graphify/requirements-lock.txt")], cwd=ROOT, check=True)
    settings = os.environ.copy()
    settings.update({"GRAPHIFY_BACKEND": "none", "GRAPHIFY_QUERY_LOG_DISABLE": "1",
                     "GRAPHIFY_MAX_WORKERS": "1", "GRAPHIFY_OUT": "graphify-out"})
    settings["PATH"] = str(binary_directory) + os.pathsep + settings.get("PATH", "")
    subprocess.run([str(interpreter), "-m", "graphify", "install", "--project", "--platform", "codex"],
                   cwd=ROOT, env=settings, check=True)
    if not arguments.no_hooks:
        subprocess.run([str(interpreter), "-m", "graphify", "hook", "install"], cwd=ROOT, env=settings, check=True)
    subprocess.run([str(interpreter), str(ROOT / "Tools/Graphify/update.py")], cwd=ROOT, env=settings, check=True)
    print("Activate .venv-graphify to use the graphify command; restart Codex if needed to discover the project skill.")


if __name__ == "__main__":
    main()
