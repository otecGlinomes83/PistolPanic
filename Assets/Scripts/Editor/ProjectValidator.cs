using System;
using System.Collections.Generic;
using PistolPanic.Core;
using PistolPanic.Meta;
using PistolPanic.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PistolPanic.EditorTools
{
    public static class ProjectValidator
    {
        private static readonly List<string> Problems = new List<string>();

        public static void ValidateAndReport()
        {
            ValidateConfigAssets();
            ValidateBootstrapScene();
            ValidateGameScene();
            ValidateShopScene();

            foreach (string problem in Problems)
            {
                Debug.LogError("VALIDATOR: " + problem);
            }

            Debug.Log("VALIDATOR: finished, problems=" + Problems.Count);

            EditorApplication.Exit(Problems.Count > 0 ? 1 : 0);
        }

        private static void ValidateConfigAssets()
        {
            WeaponCatalog weaponCatalog = LoadAsset<WeaponCatalog>("Assets/Configs/Weapon Catalog.asset");

            if (weaponCatalog == null)
            {
                AddProblem("Weapon Catalog asset missing at Assets/Configs/Weapon Catalog.asset");

                return;
            }

            if (weaponCatalog.Weapons.Count != 3)
            {
                AddProblem("WeaponCatalog.Weapons count=" + weaponCatalog.Weapons.Count + ", expected 3");
            }

            for (int i = 0; i < weaponCatalog.Weapons.Count; i++)
            {
                if (weaponCatalog.Weapons[i] == null)
                {
                    AddProblem("WeaponCatalog.Weapons[" + i + "] is null");
                }
                else
                {
                    Debug.Log("VALIDATOR: weapon[" + i + "]=" + weaponCatalog.Weapons[i].DisplayName + " pattern=" + weaponCatalog.Weapons[i].FirePattern);
                }
            }

            if (weaponCatalog.DefaultIndex < 0 || weaponCatalog.DefaultIndex >= weaponCatalog.Weapons.Count)
            {
                AddProblem("WeaponCatalog.DefaultIndex=" + weaponCatalog.DefaultIndex + " out of range");
            }

            EnemyCatalog enemyCatalog = LoadAsset<EnemyCatalog>("Assets/Configs/Enemy Catalog.asset");

            if (enemyCatalog == null)
            {
                AddProblem("Enemy Catalog asset missing at Assets/Configs/Enemy Catalog.asset");

                return;
            }

            if (enemyCatalog.Types.Count != 3)
            {
                AddProblem("EnemyCatalog.Types count=" + enemyCatalog.Types.Count + ", expected 3");
            }

            for (int i = 0; i < enemyCatalog.Types.Count; i++)
            {
                EnemyTypeConfig enemyType = enemyCatalog.Types[i];

                if (enemyType == null)
                {
                    AddProblem("EnemyCatalog.Types[" + i + "] is null");

                    continue;
                }

                if (enemyType.WeaponConfig == null)
                {
                    AddProblem("EnemyCatalog.Types[" + i + "].WeaponConfig is null (" + enemyType.name + ")");
                }
                else
                {
                    Debug.Log("VALIDATOR: enemyType[" + i + "]=" + enemyType.DisplayName + " weapon=" + enemyType.WeaponConfig.DisplayName + " pattern=" + enemyType.WeaponConfig.FirePattern);
                }

                if (enemyType.StageColors.Count == 0)
                {
                    AddProblem("EnemyCatalog.Types[" + i + "].StageColors is empty");
                }
            }

            ValidateWeaponAsset("Assets/Configs/Pistol Config.asset", FirePattern.Single);
            ValidateWeaponAsset("Assets/Configs/Automat Config.asset", FirePattern.Burst);
            ValidateWeaponAsset("Assets/Configs/Shotgun Config.asset", FirePattern.Shotgun);
            ValidateAudioAsset();
        }

        private static void ValidateWeaponAsset(string path, FirePattern expectedPattern)
        {
            WeaponConfig weapon = LoadAsset<WeaponConfig>(path);

            if (weapon == null)
            {
                AddProblem(path + " missing");

                return;
            }

            if (weapon.FirePattern != expectedPattern)
            {
                AddProblem(path + " pattern=" + weapon.FirePattern + ", expected " + expectedPattern);
            }

            if (string.IsNullOrEmpty(weapon.DisplayName))
            {
                AddProblem(path + " DisplayName is empty");
            }
        }

        private static void ValidateAudioAsset()
        {
            AudioConfig audioConfig = LoadAsset<AudioConfig>("Assets/Configs/Audio Config.asset");

            if (audioConfig == null)
            {
                AddProblem("Audio Config asset missing at Assets/Configs/Audio Config.asset");
            }
        }

        private static void ValidateBootstrapScene()
        {
            OpenScene("Assets/Scenes/Bootstrap.unity");

            ProjectLifetimeScope projectScope = UnityEngine.Object.FindObjectOfType<ProjectLifetimeScope>();

            if (projectScope == null)
            {
                AddProblem("Bootstrap scene has no ProjectLifetimeScope");

                return;
            }

            SerializedObject serialized = new SerializedObject(projectScope);

            CheckSerialized(serialized, "_economyConfig");
            CheckSerialized(serialized, "_simulationConfig");
            CheckSerialized(serialized, "_weaponCatalog");
            CheckSerialized(serialized, "_audioConfig");

            if (UnityEngine.Object.FindObjectOfType<Bootstrapper>() == null)
            {
                AddProblem("Bootstrap scene has no Bootstrapper");
            }

            Camera bootstrapCamera = UnityEngine.Object.FindObjectOfType<Camera>();

            if (bootstrapCamera == null)
            {
                AddProblem("Bootstrap scene has no camera");
            }
        }

        private static void ValidateGameScene()
        {
            OpenScene("Assets/Scenes/Game.unity");

            GameLifetimeScope gameScope = UnityEngine.Object.FindObjectOfType<GameLifetimeScope>();

            if (gameScope == null)
            {
                AddProblem("Game scene has no GameLifetimeScope");

                return;
            }

            SerializedObject serialized = new SerializedObject(gameScope);

            CheckSerialized(serialized, "_mapConfig");
            CheckSerialized(serialized, "_weaponConfig");
            CheckSerialized(serialized, "_explosionConfig");
            CheckSerialized(serialized, "_physicsConfig");
            CheckSerialized(serialized, "_playerConfig");
            CheckSerialized(serialized, "_enemyConfig");
            CheckSerialized(serialized, "_enemyCatalog");

            if (UnityEngine.Object.FindObjectOfType<Camera>() == null)
            {
                AddProblem("Game scene has no camera");
            }
        }

        private static void ValidateShopScene()
        {
            OpenScene("Assets/Scenes/Shop.unity");

            if (UnityEngine.Object.FindObjectOfType<SceneSwapDebugView>() == null)
            {
                AddProblem("Shop scene has no SceneSwapDebugView");
            }
        }

        private static void CheckSerialized(SerializedObject serialized, string fieldName)
        {
            SerializedProperty property = serialized.FindProperty(fieldName);

            if (property == null)
            {
                AddProblem("serialized field missing: " + fieldName);

                return;
            }

            if (property.objectReferenceValue == null)
            {
                AddProblem("serialized field not assigned: " + fieldName);
            }
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static void OpenScene(string path)
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        private static void AddProblem(string problem)
        {
            Problems.Add(problem);
        }
    }
}
