﻿using AsmSpy.Core.TestLibrary;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Xunit;
using Xunit.Abstractions;

namespace AsmSpy.Core.Tests
{
    public class DependencyAnalyzerTests
    {
        private readonly ITestOutputHelper output;
        private readonly TestLogger logger;

        private readonly IEnumerable<FileInfo> filesToAnalyse;
        private readonly VisualizerOptions options = new VisualizerOptions();

        public DependencyAnalyzerTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            logger = new TestLogger(output);

            var thisAssembly = Assembly.GetExecutingAssembly();
            var testBinDirectory = Path.GetDirectoryName(thisAssembly.Location);
            output.WriteLine(testBinDirectory);

            filesToAnalyse = Directory.GetFiles(testBinDirectory, "*.dll").Select(x => new FileInfo(x));
        }

        [Fact]
        public void AnalyzeShouldReturnTestAssemblies()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, options);

            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core.Tests");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core.TestLibrary");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "xunit.core");
        }

        [Fact(Skip ="Fails in AppVeyor")]
        public void AnalyzeShouldReturnSystemAssemblies()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, options);

            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "mscorlib");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "netstandard");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "System");
        }

        [Fact]
        public void AnalyzeShouldNotReturnSystemAssembliesWhenFlagIsSet()
        {
            var altOptions = new VisualizerOptions
            {
                SkipSystem = true
            };
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, altOptions);

            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "mscorlib");
            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "netstandard");
            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "System");
        }

        [Fact]
        public void AnalyzeShouldReturnDependencies()
        {
            var exampleClass = new ExampleClass();
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, options);

            var tests = result.Assemblies.Values.Single(x => x.AssemblyName.Name == "AsmSpy.Core.Tests");

            Assert.Contains(tests.References, x => x.AssemblyName.Name == "AsmSpy.Core");
            Assert.Contains(tests.References, x => x.AssemblyName.Name == "AsmSpy.Core.TestLibrary");
            Assert.Contains(tests.References, x => x.AssemblyName.Name == "xunit.core");
            foreach(var reference in tests.References)
            {
                output.WriteLine(reference.AssemblyName.Name);
            }
        }

        [Fact]
        public void AnalyzeShouldReturnCorrectAssemblySource()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, options);

            var tests = result.Assemblies.Values.Single(x => x.AssemblyName.Name == "AsmSpy.Core.Tests");

            var mscorlib = tests.References.Single(x => x.AssemblyName.Name == "mscorlib");
            Assert.Equal(AssemblySource.GlobalAssemblyCache, mscorlib.AssemblySource);
        }
    }
}
