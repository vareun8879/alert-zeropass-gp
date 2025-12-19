using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CustomAction
{
    [RunInstaller(true)]
    public partial class CustomInstaller : System.Configuration.Install.Installer
    {

        private const string ExecutableName = "ZeroPassAlert.exe";
        private const string ShortcutName = "Zero Pass Alert.lnk";
        private const string ShortcutDescription = "Zero Pass Alert";
        
        public CustomInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);            
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            CreateDesktopShortcut();
            CreateStartMenuShortcut();
            CreateStartupFolderShortcut();
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            RemoveDesktopShortcut();
            RemoveStartMenuShortcut();
            RemoveStartupFolderShortcut();
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            RemoveDesktopShortcut();
            RemoveStartMenuShortcut();
            RemoveStartupFolderShortcut();
        }

        private void CreateDesktopShortcut()
        {
            string targetDir = Context.Parameters["targetdir"];
            string exePath = Path.Combine(targetDir, ExecutableName);
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, ShortcutName);

            CreateShortcut(shortcutPath, exePath, ShortcutDescription);
        }

        private void CreateStartMenuShortcut()
        {
            string targetDir = Context.Parameters["targetdir"];
            string exePath = Path.Combine(targetDir, ExecutableName);
            string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", ShortcutDescription);
            if (!Directory.Exists(startMenuPath))
            {
                Directory.CreateDirectory(startMenuPath);
            }
            string shortcutPath = Path.Combine(startMenuPath, ShortcutName);

            CreateShortcut(shortcutPath, exePath, ShortcutDescription);
        }

        private void CreateStartupFolderShortcut()
        {
            string targetDir = Context.Parameters["targetdir"];
            string exePath = Path.Combine(targetDir, ExecutableName);
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolderPath, ShortcutName);

            CreateShortcut(shortcutPath, exePath, ShortcutDescription);
        }

        private void RemoveDesktopShortcut()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, ShortcutName);

            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }

        private void RemoveStartMenuShortcut()
        {
            string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", ShortcutDescription);
            string shortcutPath = Path.Combine(startMenuPath, ShortcutName);

            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }

        private void RemoveStartupFolderShortcut()
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolderPath, ShortcutName);

            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.Description = description;
            shortcut.TargetPath = targetPath;

            // icon.ico 파일 경로 설정
            string iconPath = Path.Combine(Path.GetDirectoryName(targetPath), "ZeroPassIcon.ico");
            shortcut.IconLocation = iconPath;  // 아이콘 파일 경로 지정

            shortcut.Save();
        }
    }
}
