using BepInEx;
using DataModel;
using HarmonyLib;

using System;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace LordAshes
{
    public partial class StreamViewsPlugin : BaseUnityPlugin
    {
        public static CutsceneData selectedCutSceneData =default(CutsceneData);

        [HarmonyPatch(typeof(CutsceneManager), "SetCutsceneFocus")]
        public static class SetCutsceneFocusPatch
        {
            public static bool Prefix(CutsceneData data)
            {
                Debug.Log("Stream Views Plugin: Patch: SetCutsceneFocus");
                selectedCutSceneData = data;
                return true;
            }

            public static void Postfix(CutsceneData data)
            {
            }
        }
    }
}
