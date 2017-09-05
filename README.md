# Cake.MarkdownToPdf
[![Build status](https://ci.appveyor.com/api/projects/status/gopqcygjgfumot9c?svg=true)](https://ci.appveyor.com/project/twenzel/cake-markdowntopdf) [![NuGet Version](http://img.shields.io/nuget/v/Cake.MarkdownToPdf.svg?style=flat)](https://www.nuget.org/packages/Cake.MarkdownToPdf/) [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

Cake addin to convert markdown files to pdf. This addin uses [FSharp.Markdown.Pdf](https://github.com/theburningmonk/FSharp.Markdown.Pdf).

## Usage

Directly in your build script via a Cake `#addin` directive:

```csharp
#addin "Cake.MarkdownToPdf"

Task("Convert")
  .Does(() => {        
    MarkdownFileToPdf("readme.md", "output.pdf");
	
	// or markdown text
	MarkdownToPdf("Some markdown formated text...", "output.pdf");
});
```
