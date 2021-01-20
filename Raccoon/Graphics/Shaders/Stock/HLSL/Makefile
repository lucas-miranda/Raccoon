include ../../../../../../rules.mk

ifeq ($(origin TARGET_PATH), undefined)
	$(error Missing TARGET_PATH required argument)
endif

# wine prefix path where dx 2010 (with fxc) should be installed
WINE_PREFIX_NAME := x86
WINE_PREFIX_PATH := $(HOME)/.local/share/wineprefixes/$(WINE_PREFIX_NAME)

# fxc full path
FXC := $(WINE_PREFIX_PATH)/drive_c/Program Files/Microsoft DirectX SDK (June 2010)/Utilities/bin/x86/fxc.exe

build: $(TARGET_PATH)/ $(patsubst %.fx, $(TARGET_PATH)/%.fxc, $(wildcard *.fx))
.PHONY: build

$(TARGET_PATH)/:
> mkdir -p $(TARGET_PATH)

$(TARGET_PATH)/%.fxc: %.fx
> @echo "|- Compiling: $<"
> @echo "|  Output: $@"
> WINEPREFIX=$(WINE_PREFIX_PATH) wine "$(FXC)" /T fx_2_0 "$<" /Fo "$(shell realpath --relative-to=$(shell pwd) $@)"
> @echo "Done!"
