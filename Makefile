include rules.mk

# options: q[uiet], m[inimal], n[ormal], d[etailed] and diag[nostic]
VERBOSE := quiet

# mcs or msbuild
BUILD_TYPE := mcs

# shaders build options
SHADERS_FOLDER := Raccoon/Graphics/Shaders/Stock/HLSL
SHADERS_TARGET_FOLDER := Raccoon

ifeq ($(BUILD_TYPE), mcs)

build:
> @echo "Building with mcs..."
> make -C Raccoon/
> @echo "Done!"
.PHONY: build

else ifeq ($(BUILD_TYPE), msbuild)

build:
> @echo "Building with msbuild..."
> msbuild /maxCpuCount /verbosity:$(VERBOSE) Raccoon.sln
> @echo "Done!"
.PHONY: build

else
$(error Build type '$(BUILD_TYPE)' isn't valid. Accepted values are: 'mcs' or 'msbuild')
endif

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
