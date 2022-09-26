# q[uiet], m[inimal], n[ormal], d[etailed] or diag[nostic]
export verbose := "quiet"

# msbuild or mcs
export build_type := "msbuild"

export shaders_dirpath := "Raccoon/Graphics/Shaders/Stock/HLSL"
export shaders_target_dirpath := "Raccoon"

build:
    #!/bin/zsh
    set -euo pipefail

    case "$build_type" in
        msbuild)
            just _build_msbuild
            ;;
        mcs)
            just _build_mcs
            ;;
        *)
            echo "Unsupported build type '$build_type'. Accepted values: msbuild or mcs"
            ;;
    esac

    print -P "%F{blue}->%f Done!"

_build_msbuild:
    #!/bin/zsh
    set -euo pipefail

    print -P "%F{magenta}->%f %Bmsbuild%b"
    msbuild /maxCpuCount /verbosity:{{verbose}} Raccoon.sln

_build_mcs:
    #!/bin/zsh
    set -euo pipefail

    print -P "%F{magenta}->%f %Bmcs%b"
    make -C Raccoon/

shaders:
    #!/bin/zsh
    set -euo pipefail

    print -P "%F{magenta}->%f %BShaders%b"
    make -C $shaders_dirpath TARGET_PATH="$(pwd)/$shaders_target_dirpath"
    #just $shaders_dirpath target_dirpath="$(pwd)/$shaders_target_dirpath"
    print -P "%F{blue}->%f Done!"

restore:
    #!/bin/zsh
    set -euo pipefail

    print -P "%F{magenta}->%f %BRestore%b"
    msbuild /restore /maxCpuCount /verbosity:{{verbose}} Raccoon.sln
    print -P "%F{blue}->%f Done!"

clean:
    #!/bin/zsh
    set -euo pipefail

    print -P "%F{magenta}->%f %BClean%b"
    rm -rdf Raccoon/obj/
    print -P "%F{blue}->%f Done!"
