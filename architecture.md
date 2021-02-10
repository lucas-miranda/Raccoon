# Architecture

This document describes how Raccoon works at high-level and how it's organized.
If you feel curious or want to know how it works and what it can do without going through hundred lines of code "pseudo-distributed", this is the right place.

## Bird's Eye View

The main goal of Raccoon is to act as a supportive layer to use FNA/XNA, providing helpers to anything related to 2D game making (3D is not planned).
It's relation with FNA/XNA is just as to be able to draw, access raw inputs and audio, taking advantage of it's very nice cross-platform support.
Raccoon is highly tight coupled to XNA api and only XNA compatible api can be used to replace it's backend (such as MonoGame).

Entry point is at `Game`, it should be created and it's where all the game `Scene` should be registered and any rendering pre-settings should be done.
Calling `Game.Start()` effectively runs everything until running ends.

## Code Map

Every directory will be explained, it's job, interactions with another ones and why it belongs there.
All folders listed are subfolders at `Raccoon/`

### `Audio/`

Has music and sound effect assets, with it's respective playback methods.

### `Core/`

Game core functionalities, entity-component related, transform, camera, scene, physics, input, debug, coroutines, enum definitions and extensions
Base interfaces belongs there also, such as: renderable, updatable, asset, scene object and so on.

### `Graphics/`

Anything related to graphics or how graphics should be renderer is here.
Drawables (animation, image, primitives, text etc) or graphic assets (atlases, fonts, shaders).

* `Renderer.cs` is where you should see to fine-tune rendering parameters as it draws **any** graphic and you need it to be able to render to screen.
* `Drawables/Graphic.cs` is a base class to anything that will render to screen.
* `Shaders/Stock/HLSL/` holds all provided shaders, it's helpers and structures, it even has a `Makefile` to build anything `.fx` at that folder to `TARGET_PATH`.

### `Logger/`

It's a basic logger system, supporting multiple listeners.
Messages are turned into tokens and each listener handles it at it's own way.

### `Math/`

Math related geometry objects are here.

### `Util/`

Several kinds of utilities and helpers.
There is some collections implemented, particle emission, tween support, random generator, math functions collection, coroutine helper routines, enum bit tag handler, bit operations, polygon simplification.
