# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2022-11-01

### Added

+ Semantic parsing of command line. This uses language specifiction 2.0 to provide
  more information to support enhanced suggestions.
+ Improved compliance and understanding of Posix command line specification.
+ Add `git` completions for porcelain commands.
+ Add `mamba` command completions.

### Changed

+ Switch from XML to Parquet file format for language definition files.
+ Implement version 2.0 of language definition sepcification.

### Conda language specification changes
+ Add `--solver` keep `--experimental-solver` deprecated though still an option. 
+ Add `conda notices` (conda version 4.14.0)
+ Add `conda rename` (conda version 4.14.0)
+ Add undocumented `conda init` flags (conda issue #11574)

## [0.1.1] - 2022-09-16

### Changed

+ Improved documentation for PowerShell Gallery.

## [0.1.0] - 2022-09-16

### Added

+ Initial release.
+ New text completions: `conda`.

### Changed

+ Nothing
