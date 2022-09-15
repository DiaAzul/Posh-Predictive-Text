import glob
import os
import sys
import subprocess
from pathlib import Path
from shutil import copy2

# Configuration file for the Sphinx documentation builder.
#
# For the full list of built-in configuration values, see the documentation:
# https://www.sphinx-doc.org/en/master/usage/configuration.html

# -- Path setup --------------------------------------------------------------

# If extensions (or modules to document with autodoc) are in another directory,
# add these directories to sys.path here. If the directory is relative to the
# documentation root, use os.path.abspath to make it absolute, like shown here.
#
sys.path.insert(0, str(Path(str(Path(__file__).parent.parent)).resolve()))
sys.setrecursionlimit(1500)

# -- Copy assets from outside docs to temporary assets folder -----------------

asset_list = [
    "CHANGELOG.md",
    "Assets/PoshPredictiveText Plain.svg",
    "Assets/PoshPredictiveText Plain.png",
    "Assets/PoshPredictiveText Banner.png",
    "Assets/PoshPredictiveText Clear Banner Plain.svg",
]
basedir = Path(__file__).parent.parent.parent
print(f"Using base directory {basedir}")

target_directory = basedir / "docs" / "source" / "assets"
if not target_directory.exists():
    target_directory.mkdir()

for asset in asset_list:
    source_file = basedir / asset
    filename = os.path.basename(source_file)
    destination_file = target_directory / filename
    copy2(source_file, destination_file)

pshelp_dir = basedir / "PowerShellHelpDocs"
pshelp_files = glob.glob(str(pshelp_dir) + "/*.md")
print(f"Copying PowerShell help files from {pshelp_dir}")

pshelp_target_directory = basedir / "docs" / "source" / "pshelp"
if not pshelp_target_directory.exists():
    pshelp_target_directory.mkdir()

for pshelp_file in pshelp_files:
    filename = os.path.basename(pshelp_file)
    destination_file = pshelp_target_directory / filename
    copy2(pshelp_file, destination_file)
    print(destination_file)

# Get version of Doxygen and save it as a tuple
doxygen_test = subprocess.run(["doxygen", "--version"], capture_output=True)
if doxygen_test.returncode < 0:
    raise RuntimeError(
        "doxygen --version reported the following error:\n\t"
        + str(doxygen_test.stderr, encoding="utf-8")
    )
doxygen_version = tuple(
    int(x) for x in str(doxygen_test.stdout, encoding="utf-8").split()[0].split(".")
)
print("Using Doxygen v%d.%d.%d" % doxygen_version)

# Run Doxygen and capture output in source/xml
print("Starting Doxygen.")

doxygen_target_directory = basedir / "docs" / "source" / "xml"
if not doxygen_target_directory.exists():
    doxygen_target_directory.mkdir()
doxygen_test = subprocess.run(["doxygen", "doxygen.conf"], capture_output=True)
if doxygen_test.returncode != 0:
    raise RuntimeError(
        "doxygen failed to compile with following error:\n\t"
        + str(doxygen_test.stderr, encoding="utf-8")
    )
print("Doxygen completed.")

# -- Project information -----------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#project-information

project = "Posh Predictive Text"
copyright = "2022, Tanzo Creative Ltd."
author = "David Plummer"
release = "0.1.5"

# -- General configuration ---------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#general-configuration

extensions = [
    "breathe",
    "sphinx_csharp",
    "sphinx_rtd_theme",
    "myst_parser",
]

# Breathe Configuration

breath_project_poshPredictiveText = basedir / "docs" / "source" / "xml"
print(f"Breath project {breath_project_poshPredictiveText}")

breathe_projects = {"PoshPredictiveText": breath_project_poshPredictiveText}
breathe_default_project = "PoshPredictiveText"

# Breathe c-sharp settings
sphinx_csharp_multi_language = True


# Add any paths that contain templates here, relative to this directory.
templates_path = ["_templates"]

# List of patterns, relative to source directory, that match files and
# directories to ignore when looking for source files.
# This pattern also affects html_static_path and html_extra_path.
exclude_patterns = ["_build", "Thumbs.db", ".DS_Store"]


# -- Options for HTML output -------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#options-for-html-output

# The theme to use for HTML and HTML Help pages.  See the documentation for
# a list of builtin themes.
#
html_theme = "sphinx_rtd_theme"
html_theme_path = [
    "_themes",
]

html_logo = "./assets/PoshPredictiveText Clear Banner Plain.svg"
html_theme_options = {
    "logo_only": True,
    "display_version": False,
}

# Add any paths that contain custom static files (such as style sheets) here,
# relative to this directory. They are copied after the builtin static files,
# so a file named "default.css" will overwrite the builtin "default.css".
html_static_path = ["_static"]
