using GraphicCustomization;
using HarmonyLib;
using Multiplayer.API;
using System;
using System.Xml.Linq;
using UnityEngine.UI;
using UnityEngine;
using Verse;

namespace VanillaPersonaWeaponsExpanded
{
    [StaticConstructorOnStartup]
    internal static class Multiplayer
    {
        static Multiplayer()
        {
            if (!MP.enabled)
            {
                return;
            }

            MP.RegisterAll();
        }
    }
}
