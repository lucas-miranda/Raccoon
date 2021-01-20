include rules.mk

# options: q[uiet], m[inimal], n[ormal], d[etailed] and diag[nostic]
VERBOSE := quiet

#BIN_PATH := Raccoon/bin/Debug
SHADERS_FOLDER := Raccoon/Graphics/Shaders/Stock/HLSL
SHADERS_TARGET_FOLDER := Raccoon/

build:
> @echo "Building..."
> msbuild /maxCpuCount /verbosity:$(VERBOSE) Raccoon.sln
> @echo "Done!"
.PHONY: build

shaders:
> make -C $(SHADERS_FOLDER) TARGET_PATH="$(shell pwd)/$(SHADERS_TARGET_FOLDER)"
.PHONY: shaders

restore:
> @echo "Restoring Nuget Packages..."
> msbuild /restore /maxCpuCount /verbosity:$(VERBOSE) Raccoon.sln
> @echo "Done!"
.PHONY: restore

clean:
> @echo "Cleaning intermediate files..."
> rm -rdf Raccoon/obj/
> @echo "Done!"
.PHONY: clean
