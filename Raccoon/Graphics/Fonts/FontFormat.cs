﻿#region MIT License
/*Copyright (c) 2016 Robert Rouhani <robert.rouhani@gmail.com>

SharpFont based on Tao.FreeType, Copyright (c) 2003-2007 Tao Framework Team

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/
#endregion

using System.Collections.Generic;

namespace Raccoon
{
	internal class FontFormat
	{
		/// <summary>
		/// Gets the name for the format.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the typical file extension for this format (lowercase).
		/// </summary>
		public string FileExtension { get; private set; }

		// ...

		public FontFormat(string name, string ext)
		{
			if (!ext.StartsWith(".")) ext = "." + ext;
			this.Name = name; this.FileExtension = ext;
		}

	}

	internal class FontFormatCollection : Dictionary<string, FontFormat>
	{

		public void Add(string name, string ext)
		{
			if (!ext.StartsWith(".")) ext = "." + ext;
			this.Add(ext, new FontFormat(name, ext));
		}

		public bool ContainsExt(string ext)
		{
			return this.ContainsKey(ext);
		}

	}

}
