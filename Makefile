include rules.mk

# options: q[uiet], m[inimal], n[ormal], d[etailed] and diag[nostic]
VERBOSE := quiet

#BIN_PATH := Raccoon/bin/Debug
SHADERS_FOLDER := Raccoon/Graphics/Shaders/Stock/HLSL
SHADERS_TARGET_FOLDER := Raccoon/

FILES := \
	Raccoon/Audio/Music.cs \
	Raccoon/Audio/SoundEffect.cs \
	Raccoon/Core/Camera.cs \
	Raccoon/Core/Components/Alarm.cs \
	Raccoon/Core/Components/Component.cs \
	Raccoon/Core/Components/CoroutineHandler.cs \
	Raccoon/Core/Components/CoroutinesHandler.cs \
	Raccoon/Core/Components/Movement/BasicMovement.cs \
	Raccoon/Core/Components/Movement/Movement.cs \
	Raccoon/Core/Components/Movement/PlatformerMovement.cs \
	Raccoon/Core/Components/Physics/Body.cs \
	Raccoon/Core/Components/StateMachine/State.cs \
	Raccoon/Core/Components/StateMachine/StateMachine.cs \
	Raccoon/Core/Coroutines/Coroutine.cs \
	Raccoon/Core/Coroutines/Coroutines.cs \
	Raccoon/Core/Coroutines/Instructions/CoroutineInstruction.cs \
	Raccoon/Core/Coroutines/Instructions/ParallelizeCoroutine.cs \
	Raccoon/Core/Debug/Console.cs \
	Raccoon/Core/Debug/Debug.cs \
	Raccoon/Core/Definitions/Comparison.cs \
	Raccoon/Core/Definitions/Direction.cs \
	Raccoon/Core/Definitions/Extensions.cs \
	Raccoon/Core/Definitions/ImageFlip.cs \
	Raccoon/Core/Definitions/ResizeMode.cs \
	Raccoon/Core/Entity.cs \
	Raccoon/Core/Game.cs \
	Raccoon/Core/IAsset.cs \
	Raccoon/Core/IDebugRenderable.cs \
	Raccoon/Core/IExtendedRenderable.cs \
	Raccoon/Core/IExtendedUpdatable.cs \
	Raccoon/Core/IRenderable.cs \
	Raccoon/Core/ISceneObject.cs \
	Raccoon/Core/IUpdatable.cs \
	Raccoon/Core/Input/Axis.cs \
	Raccoon/Core/Input/Button.cs \
	Raccoon/Core/Input/Controller.cs \
	Raccoon/Core/Input/GamePadIndex.cs \
	Raccoon/Core/Input/GamePadThumbStick.cs \
	Raccoon/Core/Input/GamePadTriggerButton.cs \
	Raccoon/Core/Input/Input.cs \
	Raccoon/Core/Input/Key.cs \
	Raccoon/Core/Input/Trigger.cs \
	Raccoon/Core/Input/XboxController.cs \
	Raccoon/Core/Physics/CollisionInfo.cs \
	Raccoon/Core/Physics/CollisionList.cs \
	Raccoon/Core/Physics/Contact.cs \
	Raccoon/Core/Physics/ContactList.cs \
	Raccoon/Core/Physics/IMaterial.cs \
	Raccoon/Core/Physics/InternalCollisionInfo.cs \
	Raccoon/Core/Physics/Physics.Box.cs \
	Raccoon/Core/Physics/Physics.Circle.cs \
	Raccoon/Core/Physics/Physics.Grid.cs \
	Raccoon/Core/Physics/Physics.Helper.cs \
	Raccoon/Core/Physics/Physics.Polygon.cs \
	Raccoon/Core/Physics/Physics.cs \
	Raccoon/Core/Physics/Shapes/BoxShape.cs \
	Raccoon/Core/Physics/Shapes/CircleShape.cs \
	Raccoon/Core/Physics/Shapes/GridShape.cs \
	Raccoon/Core/Physics/Shapes/IShape.cs \
	Raccoon/Core/Physics/Shapes/PolygonShape.cs \
	Raccoon/Core/Physics/StandardMaterial.cs \
	Raccoon/Core/Physics/Tools/SAT.cs \
	Raccoon/Core/Scene.cs \
	Raccoon/Core/Transform.cs \
	Raccoon/Core/XNAGameWrapper.cs \
	Raccoon/Graphics/Atlas/Atlas.cs \
	Raccoon/Graphics/Atlas/AtlasAnimation.cs \
	Raccoon/Graphics/Atlas/AtlasAnimationFrame.cs \
	Raccoon/Graphics/Atlas/AtlasSubTexture.cs \
	Raccoon/Graphics/Atlas/Exceptions/AtlasMismatchSubTextureTypeException.cs \
	Raccoon/Graphics/Atlas/Exceptions/AtlasSubTextureNotFoundException.cs \
	Raccoon/Graphics/Atlas/Processors/AsepriteAtlasProcessor.cs \
	Raccoon/Graphics/Atlas/Processors/IAtlasProcessor.cs \
	Raccoon/Graphics/Atlas/Processors/RavenAtlasProcessor.cs \
	Raccoon/Graphics/Batches/BatchMode.cs \
	Raccoon/Graphics/Batches/IBatchItem.cs \
	Raccoon/Graphics/Batches/MixedBatch.cs \
	Raccoon/Graphics/Batches/PrimitiveBatch.cs \
	Raccoon/Graphics/Batches/PrimitiveBatchItem.cs \
	Raccoon/Graphics/Batches/SpriteBatch.cs \
	Raccoon/Graphics/Batches/SpriteBatchItem.cs \
	Raccoon/Graphics/Color.cs \
	Raccoon/Graphics/Colors.cs \
	Raccoon/Graphics/Drawables/Animation.Track.cs \
	Raccoon/Graphics/Drawables/Animation.cs \
	Raccoon/Graphics/Drawables/Canvas.cs \
	Raccoon/Graphics/Drawables/FrameSet.cs \
	Raccoon/Graphics/Drawables/Graphic.cs \
	Raccoon/Graphics/Drawables/Grid.cs \
	Raccoon/Graphics/Drawables/Image.cs \
	Raccoon/Graphics/Drawables/PrimitiveGraphic.cs \
	Raccoon/Graphics/Drawables/Primitives/AnnulusPrimitive.cs \
	Raccoon/Graphics/Drawables/Primitives/CirclePrimitive.cs \
	Raccoon/Graphics/Drawables/Primitives/LinePrimitive.cs \
	Raccoon/Graphics/Drawables/Primitives/PolygonPrimitive.cs \
	Raccoon/Graphics/Drawables/Primitives/RectanglePrimitive.cs \
	Raccoon/Graphics/Drawables/Quad.cs \
	Raccoon/Graphics/Drawables/ScreenEffects/FadeTarget.cs \
	Raccoon/Graphics/Drawables/ScreenEffects/GraphicScreenFadeEffect.cs \
	Raccoon/Graphics/Drawables/ScreenEffects/ScreenEffect.cs \
	Raccoon/Graphics/Drawables/ScreenEffects/ScreenFadeEffect.cs \
	Raccoon/Graphics/Drawables/ScreenEffects/ScreenFlashEffect.cs \
	Raccoon/Graphics/Drawables/ScreenEffects/SquareTransition.cs \
	Raccoon/Graphics/Drawables/Strip.cs \
	Raccoon/Graphics/Drawables/Text.RenderInfo.cs \
	Raccoon/Graphics/Drawables/Text.cs \
	Raccoon/Graphics/Drawables/TileMap.cs \
	Raccoon/Graphics/Font.cs \
	Raccoon/Graphics/Fonts/FontFaceRenderMap.cs \
	Raccoon/Graphics/Fonts/FontFormat.cs \
	Raccoon/Graphics/Fonts/FontService.cs \
	Raccoon/Graphics/Renderer.cs \
	Raccoon/Graphics/Shader.cs \
	Raccoon/Graphics/Shaders/Attributes/ShaderParameterAttribute.cs \
	Raccoon/Graphics/Shaders/BasicShader.cs \
	Raccoon/Graphics/Shaders/IShaderDepthWrite.cs \
	Raccoon/Graphics/Shaders/IShaderParameters.cs \
	Raccoon/Graphics/Shaders/IShaderTexture.cs \
	Raccoon/Graphics/Shaders/IShaderTransform.cs \
	Raccoon/Graphics/Shaders/IShaderVertexColor.cs \
	Raccoon/Graphics/Texture.cs \
	Raccoon/Logger/ILoggerListener.cs \
	Raccoon/Logger/Listeners/StdOutputLoggerListener.cs \
	Raccoon/Logger/Listeners/TextWriterLoggerListener.cs \
	Raccoon/Logger/Logger.cs \
	Raccoon/Logger/Tokens/CategoryLoggerToken.cs \
	Raccoon/Logger/Tokens/HeaderLoggerToken.cs \
	Raccoon/Logger/Tokens/LoggerToken.cs \
	Raccoon/Logger/Tokens/MessageLoggerTokenTree.cs \
	Raccoon/Logger/Tokens/SubjectsLoggerToken.cs \
	Raccoon/Logger/Tokens/TextLoggerToken.cs \
	Raccoon/Logger/Tokens/TimestampLoggerToken.cs \
	Raccoon/Math/BezierCurve.cs \
	Raccoon/Math/Circle.cs \
	Raccoon/Math/Line.cs \
	Raccoon/Math/Polygon.cs \
	Raccoon/Math/Rectangle.cs \
	Raccoon/Math/Size.cs \
	Raccoon/Math/Triangle.cs \
	Raccoon/Math/Vector2.cs \
	Raccoon/Properties/AssemblyInfo.cs \
	Raccoon/Resource.Designer.cs \
	Raccoon/Test/Extensions/EnumTest.cs \
	Raccoon/Test/Math/CircleTest.cs \
	Raccoon/Test/Util/HelperTest.cs \
	Raccoon/Util/Bit.cs \
	Raccoon/Util/BitTag.cs \
	Raccoon/Util/Collections/BinaryHeap.cs \
	Raccoon/Util/Collections/Locker.cs \
	Raccoon/Util/Collections/PriorityQueue.cs \
	Raccoon/Util/Collections/ReadOnlyCollection.cs \
	Raccoon/Util/Collections/ReadOnlyList.cs \
	Raccoon/Util/Graphics/EmissionOptions.cs \
	Raccoon/Util/Graphics/Particle.cs \
	Raccoon/Util/Graphics/ParticleEmitter.cs \
	Raccoon/Util/Helper.cs \
	Raccoon/Util/Math.cs \
	Raccoon/Util/Random.cs \
	Raccoon/Util/Range.cs \
	Raccoon/Util/Routines.cs \
	Raccoon/Util/Simplify.cs \
	Raccoon/Util/Tiled/Layers/ITiledLayer.cs \
	Raccoon/Util/Tiled/Layers/TiledImageLayer.cs \
	Raccoon/Util/Tiled/Layers/TiledObjectLayer.cs \
	Raccoon/Util/Tiled/Layers/TiledTile.cs \
	Raccoon/Util/Tiled/Layers/TiledTileLayer.cs \
	Raccoon/Util/Tiled/TiledData.cs \
	Raccoon/Util/Tiled/TiledImage.cs \
	Raccoon/Util/Tiled/TiledMap.cs \
	Raccoon/Util/Tiled/TiledObject.cs \
	Raccoon/Util/Tiled/TiledObjectGroup.cs \
	Raccoon/Util/Tiled/TiledProperty.cs \
	Raccoon/Util/Tiled/Tileset/TiledAnimation.cs \
	Raccoon/Util/Tiled/Tileset/TiledTerrainType.cs \
	Raccoon/Util/Tiled/Tileset/TiledTileset.cs \
	Raccoon/Util/Tiled/Tileset/TiledTilesetTile.cs \
	Raccoon/Util/Time.cs \
	Raccoon/Util/Tween/Ease.cs \
	Raccoon/Util/Tween/Lerper.cs \
	Raccoon/Util/Tween/Tween.cs \
	Raccoon/Util/Tween/Tweener.cs

RESOURCES_DIR := Raccoon/
RESOURCES_NAMESPACE := Raccoon.Resource
RESOURCES := \
	-resource:$(RESOURCES_DIR)/basicShader.fxc,$(RESOURCES_NAMESPACE).BasicShader \
	-resource:$(RESOURCES_DIR)/04b03.ttf,$(RESOURCES_NAMESPACE)._04b03

NUGET_FOLDER := $(HOME)/.nuget/packages
FNA_OUTPUT_DIR := FNA/bin/Debug

build:
> @echo "Building..."
> mkdir -p Raccoon/bin/Debug/
# fna
> cp $(FNA_OUTPUT_DIR)/FNA.dll $(FNA_OUTPUT_DIR)/FNA.dll.config $(FNA_OUTPUT_DIR)/FNA.pdb Raccoon/bin/Debug/
# sharpfont
> cp SharpFont/Source/SharpFont/obj/Debug/SharpFont.dll SharpFont/Source/SharpFont/obj/Debug/SharpFont.pdb Raccoon/bin/Debug/
# newtonsoft.json
> cp $(NUGET_FOLDER)/newtonsoft.json/12.0.2/lib/net45/Newtonsoft.Json.dll $(NUGET_FOLDER)/newtonsoft.json/12.0.2/lib/net45/Newtonsoft.Json.xml Raccoon/bin/Debug/
# nunit
> cp $(NUGET_FOLDER)/nunit/3.12.0/lib/net45/nunit.framework.dll $(NUGET_FOLDER)/nunit/3.12.0/lib/net45/nunit.framework.pdb $(NUGET_FOLDER)/nunit/3.12.0/lib/net45/nunit.framework.xml Raccoon/bin/Debug/
> mcs /unsafe -debug -out:Raccoon/bin/Debug/Raccoon.dll -sdk:4.7.1 -langversion:7.1 -define:DEBUG -target:library -platform:x64 -r:Raccoon/bin/Debug/FNA.dll -r:Raccoon/bin/Debug/SharpFont.dll -r:Raccoon/bin/Debug/Newtonsoft.Json.dll -r:Raccoon/bin/Debug/nunit.framework.dll $(FILES) $(RESOURCES)
> @echo "Done!"
.PHONY: build

#build:
#> @echo "Building..."
#> msbuild /maxCpuCount /verbosity:$(VERBOSE) Raccoon.sln
#> @echo "Done!"
#.PHONY: build

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
