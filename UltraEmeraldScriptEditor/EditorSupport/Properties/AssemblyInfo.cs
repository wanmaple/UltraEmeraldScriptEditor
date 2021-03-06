﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("EditorSupport")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Lilith Games")]
[assembly: AssemblyProduct("EditorSupport")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8f29407d-f5a7-40d8-a20b-8f8dc303b5ff")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
// 加上这个才能编译类库中的Generic.xaml文件
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

// 给类库注册命名空间
[assembly: XmlnsDefinition("wanmaple:editor", "EditorSupport")]
[assembly: XmlnsDefinition("wanmaple:editor", "EditorSupport.Document")]
[assembly: XmlnsDefinition("wanmaple:editor", "EditorSupport.CodeCompletion")]
[assembly: XmlnsDefinition("wanmaple:editor", "EditorSupport.Editing")]
[assembly: XmlnsDefinition("wanmaple:editor", "EditorSupport.Rendering")]
[assembly: XmlnsDefinition("wanmaple:editor", "EditorSupport.Rendering.Renderers")]
