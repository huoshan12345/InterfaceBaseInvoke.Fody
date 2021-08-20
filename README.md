# InterfaceBaseInvoke.Fody

[![Build](https://github.com/huoshan12345/InterfaceBaseInvoke.Fody/workflows/Build/badge.svg)](https://github.com/huoshan12345/InterfaceBaseInvoke.Fody/actions?query=workflow%3ABuild)
[![NuGet package](https://img.shields.io/nuget/v/InterfaceBaseInvoke.Fody.svg?logo=NuGet)](https://www.nuget.org/packages/InterfaceBaseInvoke.Fody)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/huoshan12345/InterfaceBaseInvoke.Fody/blob/master/LICENSE)  
![Icon](https://github.com/huoshan12345/InterfaceBaseInvoke.Fody/raw/master/icon.png)

This is an add-in for [Fody](https://github.com/Fody/Fody) which contains a workaround for `Interface Default Implementation Base Invocation`.  
The feature has not been implemented by C# team yet.  
The status for it can be seen in https://github.com/dotnet/csharplang/issues/2337 

---

 - [Installation](#installation)
 - [Usage](#usage)

---

## Installation

- Install the NuGet packages [`Fody`](https://www.nuget.org/packages/Fody) and [`InterfaceBaseInvoke.Fody`](https://www.nuget.org/packages/InterfaceBaseInvoke.Fody). Installing `Fody` explicitly is needed to enable weaving.

  ```
  PM> Install-Package Fody
  PM> Install-Package InterfaceBaseInvoke.Fody
  ```

- Add the `PrivateAssets="all"` metadata attribute to the `<PackageReference />` items of `Fody` and `InterfaceBaseInvoke.Fody` in your project file, so they won't be listed as dependencies.

- If you already have a `FodyWeavers.xml` file in the root directory of your project, add the `<InterfaceBaseInvoke />` tag there. This file will be created on the first build if it doesn't exist:

  ```XML
  <?xml version="1.0" encoding="utf-8" ?>
  <Weavers>
    <InterfaceBaseInvoke />
  </Weavers>
  ```

See [Fody usage](https://github.com/Fody/Home/blob/master/pages/usage.md) for general guidelines, and [Fody Configuration](https://github.com/Fody/Home/blob/master/pages/configuration.md) for additional options.

## Usage
Call the extension method `Base<T>` to cast an object to one of its interfaces, and then call a method or a property.  
Just like: 
- `var result = this.Base<Interface>().Method(1, "test");`
- `var value = this.Base<Interface>().Property`

## Simple example
```
public interface IService
{
    int Property => 5
    void Method() => Console.WriteLine("Calling...");
}

public class Service : IService
{
    public int Property => this.Base<IService>().Property + 1; // call the implementation of interface's property

    public void Method()
    {
        Console.WriteLine("Before call method");
        this.Base<IService>().Method(); // call the implementation of interface's method
        Console.WriteLine("After call method");
    }    
}
```

