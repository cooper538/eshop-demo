global using EShop.ArchitectureTests.TestBase;
#pragma warning disable IDE0005 // Using directive is unnecessary - false positive with partial classes (dotnet/roslyn#18348)
global using Microsoft.VisualStudio.TestTools.UnitTesting;
#pragma warning restore IDE0005
global using NetArchTest.Rules;

[assembly: DoNotParallelize] // Architecture tests should run sequentially
