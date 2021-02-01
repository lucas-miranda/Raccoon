include ../../rules.mk

# build options
TARGET_ARCH := x64

# folders
DEBUG_BIN_PATH := bin/Debug
RELEASE_BIN_PATH := bin/Release
NUGET_FOLDER := $(HOME)/.nuget/packages

# dependencies
FNA_OUTPUT_DIR := ../FNA/bin/Debug
SHARPFONT_OUTPUT_DIR := ../SharpFont/Source/SharpFont/obj/Debug
NEWTONSOFT_JSON_OUTPUT_DIR := $(NUGET_FOLDER)/newtonsoft.json/12.0.2/lib/net45
NUNIT_OUTPUT_DIR := $(NUGET_FOLDER)/nunit/3.12.0/lib/net45

FILES := \
	Audio/Music.cs \
	Audio/SoundEffect.cs \
	Core/Camera.cs \
	Core/Components/Alarm.cs \
	Core/Components/Component.cs \
	Core/Components/CoroutineHandler.cs \
	Core/Components/CoroutinesHandler.cs \
	Core/Components/Movement/BasicMovement.cs \
	Core/Components/Movement/Movement.cs \
	Core/Components/Movement/PlatformerMovement.cs \
	Core/Components/Physics/Body.cs \
	Core/Components/StateMachine/State.cs \
	Core/Components/StateMachine/StateMachine.cs \
	Core/Coroutines/Coroutine.cs \
	Core/Coroutines/Coroutines.cs \
	Core/Coroutines/Instructions/CoroutineInstruction.cs \
	Core/Coroutines/Instructions/ParallelizeCoroutine.cs \
	Core/Debug/Console.cs \
	Core/Debug/Debug.cs \
	Core/Definitions/Comparison.cs \
	Core/Definitions/Direction.cs \
	Core/Definitions/Extensions.cs \
	Core/Definitions/ImageFlip.cs \
	Core/Definitions/ResizeMode.cs \
	Core/Entity.cs \
	Core/Game.cs \
	Core/IAsset.cs \
	Core/IDebugRenderable.cs \
	Core/IExtendedRenderable.cs \
	Core/IExtendedUpdatable.cs \
	Core/IRenderable.cs \
	Core/ISceneObject.cs \
	Core/IUpdatable.cs \
	Core/Input/Axis.cs \
	Core/Input/Button.cs \
	Core/Input/Controller.cs \
	Core/Input/GamePadIndex.cs \
	Core/Input/GamePadThumbStick.cs \
	Core/Input/GamePadTriggerButton.cs \
	Core/Input/Input.cs \
	Core/Input/Key.cs \
	Core/Input/Trigger.cs \
	Core/Input/XboxController.cs \
	Core/Physics/CollisionInfo.cs \
	Core/Physics/CollisionList.cs \
	Core/Physics/Contact.cs \
	Core/Physics/ContactList.cs \
	Core/Physics/IMaterial.cs \
	Core/Physics/InternalCollisionInfo.cs \
	Core/Physics/Physics.Box.cs \
	Core/Physics/Physics.Circle.cs \
	Core/Physics/Physics.Grid.cs \
	Core/Physics/Physics.Helper.cs \
	Core/Physics/Physics.Polygon.cs \
	Core/Physics/Physics.cs \
	Core/Physics/Shapes/BoxShape.cs \
	Core/Physics/Shapes/CircleShape.cs \
	Core/Physics/Shapes/GridShape.cs \
	Core/Physics/Shapes/IShape.cs \
	Core/Physics/Shapes/PolygonShape.cs \
	Core/Physics/StandardMaterial.cs \
	Core/Physics/Tools/SAT.cs \
	Core/Scene.cs \
	Core/Transform.cs \
	Core/XNAGameWrapper.cs \
	Graphics/Atlas/Atlas.cs \
	Graphics/Atlas/AtlasAnimation.cs \
	Graphics/Atlas/AtlasAnimationFrame.cs \
	Graphics/Atlas/AtlasSubTexture.cs \
	Graphics/Atlas/Exceptions/AtlasMismatchSubTextureTypeException.cs \
	Graphics/Atlas/Exceptions/AtlasSubTextureNotFoundException.cs \
	Graphics/Atlas/Processors/AsepriteAtlasProcessor.cs \
	Graphics/Atlas/Processors/IAtlasProcessor.cs \
	Graphics/Atlas/Processors/RavenAtlasProcessor.cs \
	Graphics/Batches/BatchMode.cs \
	Graphics/Batches/IBatchItem.cs \
	Graphics/Batches/MixedBatch.cs \
	Graphics/Batches/PrimitiveBatch.cs \
	Graphics/Batches/PrimitiveBatchItem.cs \
	Graphics/Batches/SpriteBatch.cs \
	Graphics/Batches/SpriteBatchItem.cs \
	Graphics/Color.cs \
	Graphics/Colors.cs \
	Graphics/Drawables/Animation.Track.cs \
	Graphics/Drawables/Animation.cs \
	Graphics/Drawables/Canvas.cs \
	Graphics/Drawables/FrameSet.cs \
	Graphics/Drawables/Graphic.cs \
	Graphics/Drawables/Grid.cs \
	Graphics/Drawables/Image.cs \
	Graphics/Drawables/PrimitiveGraphic.cs \
	Graphics/Drawables/Primitives/AnnulusPrimitive.cs \
	Graphics/Drawables/Primitives/CirclePrimitive.cs \
	Graphics/Drawables/Primitives/LinePrimitive.cs \
	Graphics/Drawables/Primitives/PolygonPrimitive.cs \
	Graphics/Drawables/Primitives/RectanglePrimitive.cs \
	Graphics/Drawables/Quad.cs \
	Graphics/Drawables/ScreenEffects/FadeTarget.cs \
	Graphics/Drawables/ScreenEffects/GraphicScreenFadeEffect.cs \
	Graphics/Drawables/ScreenEffects/ScreenEffect.cs \
	Graphics/Drawables/ScreenEffects/ScreenFadeEffect.cs \
	Graphics/Drawables/ScreenEffects/ScreenFlashEffect.cs \
	Graphics/Drawables/ScreenEffects/SquareTransition.cs \
	Graphics/Drawables/Strip.cs \
	Graphics/Drawables/Text.RenderInfo.cs \
	Graphics/Drawables/Text.cs \
	Graphics/Drawables/TileMap.cs \
	Graphics/Font.cs \
	Graphics/Fonts/FontFaceRenderMap.cs \
	Graphics/Fonts/FontFormat.cs \
	Graphics/Fonts/FontService.cs \
	Graphics/Renderer.cs \
	Graphics/Shader.cs \
	Graphics/Shaders/Attributes/ShaderParameterAttribute.cs \
	Graphics/Shaders/BasicShader.cs \
	Graphics/Shaders/IShaderDepthWrite.cs \
	Graphics/Shaders/IShaderParameters.cs \
	Graphics/Shaders/IShaderTexture.cs \
	Graphics/Shaders/IShaderTransform.cs \
	Graphics/Shaders/IShaderVertexColor.cs \
	Graphics/Texture.cs \
	Logger/ILoggerListener.cs \
	Logger/Listeners/StdOutputLoggerListener.cs \
	Logger/Listeners/TextWriterLoggerListener.cs \
	Logger/Logger.cs \
	Logger/Tokens/CategoryLoggerToken.cs \
	Logger/Tokens/HeaderLoggerToken.cs \
	Logger/Tokens/LoggerToken.cs \
	Logger/Tokens/MessageLoggerTokenTree.cs \
	Logger/Tokens/SubjectsLoggerToken.cs \
	Logger/Tokens/TextLoggerToken.cs \
	Logger/Tokens/TimestampLoggerToken.cs \
	Math/BezierCurve.cs \
	Math/Circle.cs \
	Math/Line.cs \
	Math/Polygon.cs \
	Math/Rectangle.cs \
	Math/Size.cs \
	Math/Triangle.cs \
	Math/Vector2.cs \
	Properties/AssemblyInfo.cs \
	Resource.Designer.cs \
	Test/Extensions/EnumTest.cs \
	Test/Math/CircleTest.cs \
	Test/Util/HelperTest.cs \
	Util/Bit.cs \
	Util/BitTag.cs \
	Util/Collections/BinaryHeap.cs \
	Util/Collections/Locker.cs \
	Util/Collections/PriorityQueue.cs \
	Util/Collections/ReadOnlyCollection.cs \
	Util/Collections/ReadOnlyList.cs \
	Util/Graphics/EmissionOptions.cs \
	Util/Graphics/Particle.cs \
	Util/Graphics/ParticleEmitter.cs \
	Util/Helper.cs \
	Util/Math.cs \
	Util/Random.cs \
	Util/Range.cs \
	Util/Routines.cs \
	Util/Simplify.cs \
	Util/Tiled/Layers/ITiledLayer.cs \
	Util/Tiled/Layers/TiledImageLayer.cs \
	Util/Tiled/Layers/TiledObjectLayer.cs \
	Util/Tiled/Layers/TiledTile.cs \
	Util/Tiled/Layers/TiledTileLayer.cs \
	Util/Tiled/TiledData.cs \
	Util/Tiled/TiledImage.cs \
	Util/Tiled/TiledMap.cs \
	Util/Tiled/TiledObject.cs \
	Util/Tiled/TiledObjectGroup.cs \
	Util/Tiled/TiledProperty.cs \
	Util/Tiled/Tileset/TiledAnimation.cs \
	Util/Tiled/Tileset/TiledTerrainType.cs \
	Util/Tiled/Tileset/TiledTileset.cs \
	Util/Tiled/Tileset/TiledTilesetTile.cs \
	Util/Time.cs \
	Util/Tween/Ease.cs \
	Util/Tween/Lerper.cs \
	Util/Tween/Tween.cs \
	Util/Tween/Tweener.cs

RESOURCES_DIR := ./
RESOURCES_NAMESPACE := Raccoon.Resource
RESOURCES := \
	-resource:$(RESOURCES_DIR)/basicShader.fxc,$(RESOURCES_NAMESPACE).BasicShader \
	-resource:$(RESOURCES_DIR)/04b03.ttf,$(RESOURCES_NAMESPACE)._04b03

debug:
> mkdir -p $(DEBUG_BIN_PATH)
# fna
> make -s -C ../FNA debug
> cp -R $(FNA_OUTPUT_DIR)/. $(DEBUG_BIN_PATH)
# sharpfont
> cp -R $(SHARPFONT_OUTPUT_DIR)/. $(DEBUG_BIN_PATH)
# newtonsoft.json
> cp -R $(NEWTONSOFT_JSON_OUTPUT_DIR)/. $(DEBUG_BIN_PATH)
# nunit
> cp -R $(NUNIT_OUTPUT_DIR)/. $(DEBUG_BIN_PATH)
# compile code
> @echo "Compiling Raccoon code..."
> @mcs \
	/unsafe \
	-debug \
	-out:$(DEBUG_BIN_PATH)/Raccoon.dll \
	-sdk:4.7.1 \
	-langversion:7.1 \
	-define:DEBUG \
	-target:library \
	-platform:$(TARGET_ARCH) \
	-r:$(DEBUG_BIN_PATH)/FNA.dll \
	-r:$(DEBUG_BIN_PATH)/SharpFont.dll \
	-r:$(DEBUG_BIN_PATH)/Newtonsoft.Json.dll \
	-r:$(DEBUG_BIN_PATH)/nunit.framework.dll \
	$(FILES) \
	$(RESOURCES)
.PHONY: debug

clean-debug:
> rm -rdf $(DEBUG_BIN_PATH)
.PHONY: clean-debug
