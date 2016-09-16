Raccoon
==============

Raccoon is a framework focused in 2D game development made with [MonoGame](https://github.com/MonoGame/MonoGame).
It's originally designed for my personal use, but feel free to suggest or make a pull request anything you find relevant.

Dependencies
==============

- Install [MonoGame 3.5](http://www.monogame.net/2016/03/17/monogame-3-5/) (required to use *MonoGame Pipeline*)

Configuring a Visual Studio Project
=======================

*It's a bit annoying to set up right now, but I will make a template to fix it soon.*

1. Create a new Console Application
2. Add *Raccoon* project in the solution
3. Add reference to *Raccoon*
4. Open the *.csproj* from your project and add to the **first** `<PropertyGroup></PropertyGroup>`:

	`<MonoGamePlatform>WindowsGL</MonoGamePlatform>`

	*'WindowsGL' can be replaced for any supported platforms by MonoGame.*

Using MonoGame Content File
==============================

Open the *.csproj* from your project and add to `<Project></Project>`:

	<ItemGroup>
		<MonoGameContentReference Include="Content\Content.mgcb" />
	</ItemGroup>
		<Import Project="$(MSBuildExtensionsPath)\MonoGame\v3.0\MonoGame.Content.Builder.targets" />

*The 'Content' folder can be changed, but must match the Game.Instance.ContentDirectory value.*

License
=========
Raccoon is under [MIT License](/LICENSE).
