using Deucarian.CameraNavigation.InputSystemIntegration.Editor;
using NUnit.Framework;

namespace Deucarian.CameraNavigation.InputSystemIntegration.Tests
{
    public sealed class NavigationInputBackendPolicyTests
    {
        [Test]
        public void MonitorThrottlesIdleChecksAndStopsOnDisposal()
        {
            int checks = 0;
            var monitor = new NavigationInputBackendMonitor(() => checks++);
            monitor.Tick(0, true);
            Assert.That(checks, Is.Zero);
            monitor.Tick(0, false);
            monitor.Tick(.5, false);
            Assert.That(checks, Is.EqualTo(1));
            monitor.Tick(1, true);
            monitor.Tick(1.1, false);
            Assert.That(checks, Is.EqualTo(2));
            monitor.Dispose();
            monitor.Dispose();
            monitor.Start();
            monitor.Tick(10, false);
            Assert.That(checks, Is.EqualTo(2));
        }

        [TestCase(false, true, 1, 0)]
        [TestCase(true, false, 0, 1)]
        [TestCase(false, false, 1, 1)]
        [TestCase(true, true, 0, 0)]
        public void PolicyOnlyEnablesMissingBackends(bool newEnabled, bool oldEnabled, int expectedNew, int expectedOld)
        {
            int newWrites = 0, oldWrites = 0;
            bool changed = NavigationInputBackendPolicy.EnableMissingBackends(newEnabled, oldEnabled,
                () => newWrites++, () => oldWrites++);
            Assert.That(newWrites, Is.EqualTo(expectedNew));
            Assert.That(oldWrites, Is.EqualTo(expectedOld));
            Assert.That(changed, Is.EqualTo(expectedNew + expectedOld > 0));
        }

        [Test]
        public void InstalledInputSystemEditorAdapterRemainsAvailable()
        {
            Assert.DoesNotThrow(NavigationInputBackendPolicy.EnsureBoth);
        }
    }
}
