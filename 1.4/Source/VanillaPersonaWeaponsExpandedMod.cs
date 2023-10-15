using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaPersonaWeaponsExpanded
{
    public class VanillaPersonaWeaponsExpandedMod : Mod
    {
        public VanillaPersonaWeaponsExpandedMod(ModContentPack pack) : base(pack)
        {
            new Harmony("VanillaPersonaWeaponsExpanded.Mod").PatchAll();
        }


        [SyncMethod]
        public static void RemoveUsedLetter(int id)
        {
            int index = Current.Game.GetComponent<GameComponent_PersonaWeapons>().unresolvedLetters.FirstIndexOf(letter => letter.ID == id);
            if (index != -1)
            {
                ChoiceLetter_ChoosePersonaWeapon letter = Current.Game.GetComponent<GameComponent_PersonaWeapons>().unresolvedLetters[index];
                Current.Game.GetComponent<GameComponent_PersonaWeapons>().unresolvedLetters.Remove(letter);
                Find.Archive.Remove(letter);
            }
        }

        [SyncMethod]
        public static void CloseDialogWindowForPawn(Pawn pawn)
        {
            Find.WindowStack.Windows.FirstOrDefault(x => x.GetType() == typeof(Dialog_ChoosePersonaWeapon) && (x as Dialog_ChoosePersonaWeapon).pawn == pawn && x.IsOpen)?.Close(false);
        }
    }

    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), "OnPostTitleChanged")]
    public static class Pawn_RoyaltyTracker_OnPostTitleChanged
    {
        public static void Postfix(Pawn_RoyaltyTracker __instance, Faction faction, RoyalTitleDef prevTitle, RoyalTitleDef newTitle)
        {
            if (newTitle != null && __instance.pawn.IsColonist && PawnGenerator.IsBeingGenerated(__instance.pawn) is false 
                && Current.CreatingWorld is null && __instance.pawn.Dead is false
                && (prevTitle is null || prevTitle.seniority < VPWE_DefOf.Baron.seniority) 
                && newTitle.seniority >= VPWE_DefOf.Baron.seniority && faction == Faction.OfEmpire)
            {
                var letter = LetterMaker.MakeLetter("VPWE.GainedPersonaWeaponTitle".Translate(__instance.pawn.Named("PAWN")),
                    "VPWE.GainedPersonaWeaponDesc".Translate(__instance.pawn.Named("PAWN"), newTitle.GetLabelFor(__instance.pawn.gender)),
                    VPWE_DefOf.VPWE_ChoosePersonaWeapon, faction) as ChoiceLetter_ChoosePersonaWeapon;
                letter.pawn = __instance.pawn;
                Find.LetterStack.ReceiveLetter(letter);
                Current.Game.GetComponent<GameComponent_PersonaWeapons>().unresolvedLetters.Add(letter);
            }
        }
    }
}
