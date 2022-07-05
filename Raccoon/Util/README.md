## About

Set of utilities and helpers to diverse subjects.

## Overview

### `Collections/`

Contains some useful collections, that doesn't exists at `System.Collections`, and a reimplementation of `System.Collections.ObjectModel.ReadOnlyCollection` (for motivation disclaimer see [commit](https://github.com/lucas-miranda/Raccoon/commit/73d9f69f70b5a335223e19fbb74ef7200d8f3b1b)).

### `Graphics/`

Has graphic related utils. Such as a simple particle emitter.

### `Tiled/`

A very old Tiled format implementation.
It's no longer supported, as I don't use it a very long time.

### `Tween/`

Easing helpers, with a tween handler.
Can be used standalone to simple easing calculation.

### `Bit.cs`

Basic bit manipulation.

### `BitTag.cs`

Enum bit field manipulation, it provides very nice methods to verify values, iterators, convert methods.
Helps a lot to make things look better when dealing with bit fields constantly.

### `Helper.cs`

Set of small helper methods, each subject is separated at regions.

### `Math.cs`

Math constants, angle and number operations.

### `Random.cs`

Raccoon's random generation interface, has methods to generate from ranges, percentage, choosing from collections, random position at some geometries.
Underlying random generation method can be changed anytime, by default it uses `System.Random`, but anyone inheriting `System.Random` is allowed and encouraged.

### `Range.cs`

Closed range definition.
Has nice range clamping and testing methods.

### `Routines.cs`

Coroutine routines when dealing with some Raccoon types. Such as waiting an animation's track to end, wait for a tween to end it's execution, wait for a condition to be fulfilled.

### `Simplify.cs`

Aids in polygon simplification through specific constraints.

### `Time.cs`

Very simple time helper.
