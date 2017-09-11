# Cake.MarkdownToPdf
[![Build status](https://ci.appveyor.com/api/projects/status/gopqcygjgfumot9c?svg=true)](https://ci.appveyor.com/project/twenzel/cake-markdowntopdf) [![NuGet Version](http://img.shields.io/nuget/v/Cake.MarkdownToPdf.svg?style=flat)](https://www.nuget.org/packages/Cake.MarkdownToPdf/) [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

Cake addin to convert markdown files to pdf. This addin uses [Markdig](https://github.com/lunet-io/markdig) markdown processor.

## Usage

Directly in your build script via a Cake `#addin` directive:

```csharp
#addin "Cake.MarkdownToPdf"

Task("Convert")
  .Does(() => {        
    MarkdownFileToPdf("readme.md", "output.pdf");
    
    // or markdown text
    MarkdownToPdf("Some markdown formated text...", "output.pdf");
    
    // or using settings
    MarkdownFileToPdf("readme.md", "output.pdf", settings => {
        settings.Theme = Themes.Github;
        settings.UseAdvancedMarkdownTables(); // or UseAdvancedMarkdownExtensions();
        
        // accessing the internal markdig markdown pipeline
        settings.MarkdownPipeline.UseGridTables();
    });
});
```

### Themes
Use can use the build-in "Default" or "Github" theme or define any css style on your own


```csharp
Task("Convert")
  .Does(() => {        
    // custom css
    MarkdownFileToPdf("readme.md", "output.pdf", settings => {
        settings.CssFile = "path to my css";
        
        // optional
        settings.HtmlTemplateFile = "path to my custom html file"; // using {$html} placeholder
    });
});
```

### PDF settings
Settings such as orientation, margins etc can be set through the PdfSettings:
```csharp
Task("Convert")
  .Does(() => {        
  
    MarkdownFileToPdf("readme.md", "output.pdf", settings => {
        settings.Pdf.PageSize = PdfPageSize.Letter;
    });
});
```