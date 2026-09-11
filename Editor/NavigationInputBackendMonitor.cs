using System;
using System.Runtime.CompilerServices;
using UnityEditor;

[assembly: InternalsVisibleTo("Deucarian.CameraNavigation.InputSystemIntegration.Tests.EditMode")]

namespace Deucarian.CameraNavigation.InputSystemIntegration.Editor
{
    internal sealed class NavigationInputBackendMonitor : IDisposable
    {
        private readonly Action ensureBoth;
        private double nextCheck;
        private bool started;
        private bool disposed;

        internal NavigationInputBackendMonitor(Action ensureBoth)
        {
            this.ensureBoth = ensureBoth ?? throw new ArgumentNullException(nameof(ensureBoth));
        }

        internal void Start()
        {
            if (started || disposed) return;
            started = true;
            EditorApplication.delayCall += EnsureOnStartup;
            EditorApplication.update += CheckWhenIdle;
            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
        }

        private void EnsureOnStartup() { if (!disposed) ensureBoth(); }

        private void CheckWhenIdle() => Tick(EditorApplication.timeSinceStartup,
            EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode);

        internal void Tick(double now, bool busy)
        {
            if (disposed || busy || now < nextCheck) return;
            nextCheck = now + 1;
            ensureBoth();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            EditorApplication.delayCall -= EnsureOnStartup;
            EditorApplication.update -= CheckWhenIdle;
            AssemblyReloadEvents.beforeAssemblyReload -= Dispose;
        }
    }
}
