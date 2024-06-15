using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AsmSpy.Core.Tests
{
    [TestFixture]
    public class DependencyAnalyzerTests
    {
        private NullLogger logger;
        private IEnumerable<FileInfo> filesToAnalyse;
        private VisualizerOptions options = new VisualizerOptions();

        [SetUp]
        public void SetUp()
        {
            logger = new NullLogger();
            var thisAssembly = Assembly.GetExecutingAssembly();
            var testBinDirectory = Path.GetDirectoryName(thisAssembly.Location);
            Debug.WriteLine(testBinDirectory);

            filesToAnalyse = Directory.GetFiles(testBinDirectory, "*.dll").Select(x => new FileInfo(x));
        }

        [Test]
        public void AnalyzeShouldReturnTestAssemblies()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, options, RuntimeEnvironment.GetRuntimeDirectory());

            Assert.That(result.Assemblies.Values.Any(x => x.AssemblyName.Name == "AsmSpy.Core"));
            Assert.That(result.Assemblies.Values.Any(x => x.AssemblyName.Name == "AsmSpy.Core.Tests"));
            Assert.That(result.Assemblies.Values.Any(x => x.AssemblyName.Name == "nunit.engine.core"));
        }

        [Test]
        public void AnalyzeShouldReturnSystemAssemblies()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, options, RuntimeEnvironment.GetRuntimeDirectory());

            Assert.That(result.Assemblies.Values.Any(x => x.AssemblyName.Name == "mscorlib"));
            Assert.That(result.Assemblies.Values.Any(x => x.AssemblyName.Name == "netstandard"));
            Assert.That(result.Assemblies.Values.Any(x => x.AssemblyName.Name == "System.Runtime"));
        }

        [Test]
        public void AnalyzeShouldNotReturnSystemAssembliesWhenFlagIsSet()
        {
            var altOptions = new VisualizerOptions
            {
                SkipSystem = true
            };
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, altOptions, RuntimeEnvironment.GetRuntimeDirectory());

            Assert.That(result.Assemblies.Values.All(x => x.AssemblyName.Name != "mscorlib"));
            Assert.That(result.Assemblies.Values.All(x => x.AssemblyName.Name != "netstandard"));
            Assert.That(result.Assemblies.Values.All(x => x.AssemblyName.Name != "System"));
        }

        [Test]
        public void AnalyzeShouldReturnDependencies()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, logger, options, RuntimeEnvironment.GetRuntimeDirectory());

            var tests = result.Assemblies.Values.Single(x => x.AssemblyName.Name == "AsmSpy.Core.Tests");

            Assert.That(tests.References.Any(x => x.AssemblyName.Name == "AsmSpy.Core"));
            Assert.That(tests.References.Any(x => x.AssemblyName.Name == "nunit.framework"));
        }
    }
}
