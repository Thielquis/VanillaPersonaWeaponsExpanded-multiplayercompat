using RimWorld;
using System.Collections.Generic;
using Verse;
using Multiplayer.API;
using Verse.Noise;
using GraphicCustomization;
using Verse.AI;

namespace VanillaPersonaWeaponsExpanded
{
    public class GameComponent_PersonaWeapons : GameComponent
    {
        public List<ChoiceLetter_ChoosePersonaWeapon> unresolvedLetters = new List<ChoiceLetter_ChoosePersonaWeapon>();

        public static Dictionary<Pawn, Thing> weaponsCurrentlyEditing = new Dictionary<Pawn, Thing>();

        public GameComponent_PersonaWeapons(Game game)
        {
            
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            for (var i = unresolvedLetters.Count - 1; i >= 0; i--)
            {
                var letter = unresolvedLetters[i];
                if (letter?.pawn is null)
                {
                    unresolvedLetters.RemoveAt(i);
                }
                else
                {
                    if (!Find.LetterStack.LettersListForReading.Contains(letter))
                    {
                        var diff = Find.TickManager.TicksGame - letter.tickWhenOpened;
                        if (diff >= GenDate.TicksPerDay * 7)
                        {
                            if (letter.pawn.IsColonist)
                            {
                                var map = letter.pawn.MapHeld ?? Find.AnyPlayerHomeMap;
                                if (map != null)
                                {
                                    Find.LetterStack.ReceiveLetter(letter);
                                }
                            }
                            else
                            {
                                unresolvedLetters.RemoveAt(i);
                            }
                        }
                    }
                }

            }
        }

        [SyncMethod]
        public static void AddWeaponForPawn(Pawn p, ThingDef weaponDef, Thing weapon = null)
        {
            if (weapon == null)
            {
                weaponsCurrentlyEditing.Add(p, ThingMaker.MakeThing(weaponDef));
            }
            else {
                weaponsCurrentlyEditing.Add(p, weapon);
            }
        }

        [SyncMethod]
        public static void RemoveWeaponForPawn(Pawn p)
        {
            weaponsCurrentlyEditing.Remove(p);
        }


        [SyncMethod]
        public static void SetCustomWeaponGraphicForPawn(Pawn p, List<string> complete, List<bool> overrideExist, 
            List<float> chances, List<float> overrideChances, string name) {
            List<TextureVariant> list = new List<TextureVariant>();
            for(int i = 0; i < complete.Count / 5; i++)
            {
                TextureVariant temp = new TextureVariant();
                temp.texName = complete[i * 5];
                temp.texture = complete[(i * 5) + 1];
                temp.outline = complete[(i * 5) + 2];
                if (overrideExist[i]) {
                    temp.textureVariantOverride = new TextureVariantOverride();
                    temp.textureVariantOverride.chance = chances[i];
                    temp.textureVariantOverride.groupName = complete[(i * 5) + 3];
                    temp.textureVariantOverride.texName = complete[(i * 5) + 4];
                }
                temp.chanceOverride = overrideChances[i];
                list.Add(temp);
            }

            CompGraphicCustomization comp = weaponsCurrentlyEditing[p].TryGetComp<ExtendedGraphicComp>();
            comp.TryInit();
            comp.texVariantsToCustomize = list;
            comp.Customize();

            CompGeneratedNames compGeneratedName = weaponsCurrentlyEditing[p].TryGetComp<CompGeneratedNames>();
            compGeneratedName.name = name;


        }

        [SyncMethod]
        public static void SetWeaponTraitForPawn(Pawn p, WeaponTraitDef weaponTraitDef)
        {
            CompBladelinkWeapon compBladelink = weaponsCurrentlyEditing[p].TryGetComp<CompBladelinkWeapon>();
            compBladelink.traits.Clear();
            compBladelink.traits.Add(weaponTraitDef);
            compBladelink.CodeFor(p);
        }

        [SyncMethod]
        public static void SetWeaponQualityForPawn(Pawn p)
        {
            CompQuality qualityComp = weaponsCurrentlyEditing[p].TryGetComp<CompQuality>();
            if (qualityComp != null)
            {
                qualityComp.SetQuality(QualityCategory.Excellent, ArtGenerationContext.Outsider);
            }
        }

        [SyncMethod]
        public static void DropWeaponForPawn(Pawn p, ref Thing weapon) {
            Map map = p.MapHeld ?? Find.AnyPlayerHomeMap;
            weapon = weaponsCurrentlyEditing[p];
            DropPodUtility.DropThingsNear(map.Center, map, new List<Thing> { weapon }, 110, canInstaDropDuringInit: false, leaveSlag: true);
            weaponsCurrentlyEditing.Remove(p);
        }

        [SyncMethod]
        public static void CustomizeWeapon(Pawn p)
        {
            p.jobs.TryTakeOrderedJob(JobMaker.MakeJob(GraphicCustomization_DefOf.VEF_CustomizeItem, weaponsCurrentlyEditing[p]), JobTag.Misc);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref unresolvedLetters, "unresolvedLetters", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                unresolvedLetters ??= new List<ChoiceLetter_ChoosePersonaWeapon>();
            }
        }
    }
}
