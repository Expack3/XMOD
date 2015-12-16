# XMOD
Originally built for as a capstone project for the Davenport Technology Class of 2014, XMOD (XACT sound Module) is a sound module (MOD)-style player utilizing the custom-built .XMOD format for XNA and MonoGame-based projects.

## Features
* Directly utilizes the XACT sound engine - no need to pipe in external sound sources!
* Actual code uses ~624KB of memory
* It Just Works - give it what it needs, and it gets to work, no questions asked.

## Requirements
* The latest version of DirectX 9.
* Visual Studio 2010 or later.
* [XNA 4.0](http://www.microsoft.com/en-us/download/details.aspx?id=20914)

## Directory Structure
* **XNA**: This contains the source code for the original XNA implementation of XMOD.
* (coming soon) **MonoGame**: When added, this will contain the source code for the MonoGame implementation of XMOD.
* **Format documentation**: This contains the RTF-formatted documentation for how the XMOD format works.

## For MonoGame Users
Conversion to the latest version of MonoGame is extremely simple - just replace the XNA usings with the MonoGame ones, recompile the sound assets, and you're good to go!

## FAQ
### Why a custom format?
Because this was originally part of a collaborative capstone project piece, meaning it had to be "unique, novel, and solve a practical problem". Having a MOD-style player utilizing XACT met all three criteria. Otherwise, a third-party solution would've been used, such as the excellent [libopenmpt](http://buildbot.openmpt.org/builds/latest-unpacked/libopenmpt-docs/docs/index.htm).
### Why should I use an XACT-based MOD format over [insert-format-here]?
Because you don't need to integrate another third-party solution - you'll have all the basic capabilites of the Impulse Tracker (IT) MOD format AND have guaranteed compilation for an XBOX360 compile target (not like the latter even matters anymore).
### Why should I use XMOD?
Because, as of this writing, it's the only open-source, XACT-based solution for MOD-style music playback.
