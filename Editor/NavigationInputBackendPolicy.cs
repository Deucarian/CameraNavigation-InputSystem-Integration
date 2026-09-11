using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Deucarian.CameraNavigation.InputSystemIntegration.Editor
{
    /// <summary>Installing the device adapter keeps legacy UI and native Input System backends enabled.</summary>
    [InitializeOnLoad]
    public static class NavigationInputBackendPolicy
    {
        private static readonly Type SettingsHelper = typeof(InputSystem).Assembly.GetType(
            "UnityEngine.InputSystem.Editor.EditorPlayerSettingHelpers", throwOnError: true);
        private static readonly PropertyInfo NewBackend = SettingsHelper.GetProperty("newSystemBackendsEnabled");
        private static readonly PropertyInfo OldBackend = SettingsHelper.GetProperty("oldSystemBackendsEnabled");

        static NavigationInputBackendPolicy()
        {
            var monitor = new NavigationInputBackendMonitor(EnsureBoth);
            monitor.Start();
        }

        /// <summary>Uses Input System's editor adapter, including its Unity 6 active Build Profile handling.</summary>
        public static void EnsureBoth()
        {
            if (NewBackend == null || OldBackend == null)
                throw new NotSupportedException("This Input System version does not expose its editor backend settings adapter.");
            EnableMissingBackends((bool)NewBackend.GetValue(null), (bool)OldBackend.GetValue(null),
                () => NewBackend.SetValue(null, true), () => OldBackend.SetValue(null, true));
        }

        public static bool EnableMissingBackends(bool newEnabled, bool oldEnabled, Action enableNew, Action enableOld)
        {
            if (!newEnabled) enableNew();
            if (!oldEnabled) enableOld();
            return !newEnabled || !oldEnabled;
        }
    }
}
