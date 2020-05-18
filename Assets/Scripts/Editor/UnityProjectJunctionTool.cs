using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

namespace Davinet
{
    public class UnityProjectJunctionTool : MonoBehaviour
    {
        [MenuItem("Davinet/Create Junction Unity Project")]
        private static void CreateJunctionUnityProject()
        {
            DirectoryInfo currentDirectoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            string junctionDirectory = $"{currentDirectoryInfo.Name}-Junction";
            string junctionPath = Path.Combine(currentDirectoryInfo.Parent.FullName, junctionDirectory);

            if (Directory.Exists(junctionPath))
            {
                UnityEngine.Debug.LogError($"Junction project directory already exists at {junctionPath}.");
            }
            else
            {
                DirectoryInfo junctionDirectoryInfo = Directory.CreateDirectory(junctionPath);

                string linkAssets = Path.Combine(junctionDirectoryInfo.FullName, "Assets");
                string linkProjectSettings = Path.Combine(junctionDirectoryInfo.FullName, "ProjectSettings");
                string linkPackages = Path.Combine(junctionDirectoryInfo.FullName, "Packages");

                string targetAssets = Path.Combine(currentDirectoryInfo.FullName, "Assets");
                string targetProjectSettings = Path.Combine(currentDirectoryInfo.FullName, "ProjectSettings");
                string targetPackages = Path.Combine(currentDirectoryInfo.FullName, "Packages");

                CreateJunction(linkAssets, targetAssets);
                CreateJunction(linkProjectSettings, targetProjectSettings);
                CreateJunction(linkPackages, targetPackages);

                UnityEngine.Debug.Log($"Created junction project at {junctionPath}.");
            }
        }

        private static void CreateJunction(string link, string target)
        {
            string command = $"/C mklink /J \"{link}\" \"{target}\"";
            Process process = Process.Start("cmd.exe", command);
        }
    }
}
