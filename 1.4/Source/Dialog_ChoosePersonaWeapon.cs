using GraphicCustomization;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaPersonaWeaponsExpanded
{
    [HotSwappable]
    public class Dialog_ChoosePersonaWeapon : Dialog_GraphicCustomization
    {
        public Thing currentWeapon;

        public ChoiceLetter_ChoosePersonaWeapon choiceLetter;
        public override Vector2 InitialSize => new Vector2(700, 600);

        public List<Thing> allWeapons;

        public List<WeaponTraitDef> allWeaponTraits;

        public WeaponTraitDef currentWeaponTrait;

        public Dialog_ChoosePersonaWeapon(ChoiceLetter_ChoosePersonaWeapon choiceLetter, List<Thing> allWeapons,
            ExtendedGraphicComp comp, Pawn pawn = null) : base(comp, pawn)
        {
            this.allWeapons = allWeapons;
            this.currentWeapon = comp.parent;
            this.choiceLetter = choiceLetter;
            this.allWeaponTraits = DefDatabase<WeaponTraitDef>.AllDefsListForReading;
            this.currentWeaponTrait = allWeaponTraits.RandomElement();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = DrawTitle(ref inRect);

            var floatMenuButtonsRect = new Rect(inRect.x + 150, titleRect.yMax, inRect.width - 300, 32);
            MakeFloatOptionButtons(floatMenuButtonsRect,
                leftAction: delegate
                {
                    if (allWeapons.IndexOf(currentWeapon) > 0)
                    {
                        currentWeapon = allWeapons[allWeapons.IndexOf(currentWeapon) - 1];
                    }
                    else
                    {
                        currentWeapon = allWeapons[allWeapons.Count - 1];
                    }
                    this.Init(currentWeapon.TryGetComp<CompGraphicCustomization>());
                }, centerAction: delegate
                {
                    FloatMenuUtility.MakeMenu(allWeapons, entry => entry.def.LabelCap, (Thing variant) => delegate
                    {
                        currentWeapon = variant;
                        this.Init(currentWeapon.TryGetComp<CompGraphicCustomization>());
                    });
                }, centerButtonName: currentWeapon.def.LabelCap,
                rightAction: delegate
                {
                    if (allWeapons.IndexOf(currentWeapon) < allWeapons.Count - 1)
                    {
                        currentWeapon = allWeapons[allWeapons.IndexOf(currentWeapon) + 1];
                    }
                    else
                    {
                        currentWeapon = allWeapons[0];
                    }
                    this.Init(currentWeapon.TryGetComp<CompGraphicCustomization>());
                });

            var height = GetScrollHeight();
            Rect outerRect = new Rect(inRect.x, floatMenuButtonsRect.yMax + 15, inRect.width, 430);
            Rect viewArea = new Rect(inRect.x, outerRect.y, inRect.width - 16, height);
            Rect itemTextureRect = new Rect(inRect.x + 10, viewArea.y, 250, 250);

            DrawItem(itemTextureRect);
            Widgets.BeginScrollView(outerRect, ref scrollPosition, viewArea, true);
            DrawCustomizationArea(itemTextureRect);
            Widgets.EndScrollView();

            var cancelRect = new Rect((inRect.width / 2f) - 155, inRect.height - 32, 150, 32);
            if (Widgets.ButtonText(cancelRect, "VEF.Cancel".Translate()))
            {
                this.Close();
            }
            var confirmRect = new Rect((inRect.width / 2f) + 5, inRect.height - 32, 250, 32);
            DrawConfirmButton(confirmRect, "VPWE.ClaimFor".Translate(pawn.Named("PAWN")), delegate
            {
                List<string> complete = new List<string>();
                List<bool> overrideExist = new List<bool>();
                List<float> chances = new List<float>();
                List<float> overrideChances = new List<float>();
                foreach (TextureVariant variant in this.currentVariants)
                {
                    complete.Add(variant.texName);
                    complete.Add(variant.texture);
                    complete.Add(variant.outline);
                    if (variant.textureVariantOverride != null)
                    {
                        overrideExist.Add(true);
                        chances.Add(variant.textureVariantOverride.chance);
                        complete.Add(variant.textureVariantOverride.groupName);
                        complete.Add(variant.textureVariantOverride.texName);
                    }
                    else {
                        overrideExist.Add(false);
                        chances.Add(0);
                        complete.Add("");
                        complete.Add("");
                    }
                    overrideChances.Add(variant.chanceOverride);
                }

                GameComponent_PersonaWeapons.AddWeaponForPawn(pawn, currentWeapon.def);
                GameComponent_PersonaWeapons.SetCustomWeaponGraphicForPawn(this.pawn, complete, overrideExist, chances, overrideChances, currentName);
                GameComponent_PersonaWeapons.SetWeaponTraitForPawn(this.pawn, this.currentWeaponTrait);
                GameComponent_PersonaWeapons.SetWeaponQualityForPawn(this.pawn);
                GameComponent_PersonaWeapons.DropWeaponForPawn(this.pawn, ref currentWeapon);

                Find.LetterStack.ReceiveLetter("VPWE.ReceivedWeaponTitle".Translate(), "VPWE.ReceivedWeaponDesc".Translate(currentWeapon.Label, pawn.Named("PAWN")), LetterDefOf.NeutralEvent, currentWeapon);
                VanillaPersonaWeaponsExpandedMod.RemoveUsedLetter(choiceLetter.ID);
                this.Close();
                VanillaPersonaWeaponsExpandedMod.CloseDialogWindowForPawn(this.pawn);
            });
        }

        public override void DrawCustomizationArea(Rect itemTextureRect)
        {
            base.DrawCustomizationArea(itemTextureRect);
            var personaTitleRect = new Rect(itemTextureRect.x, itemTextureRect.yMax + 26, 150, 24);
            Widgets.Label(personaTitleRect, "VPWE.Persona".Translate());
            var floatMenuButtonsRect = new Rect(personaTitleRect.x, personaTitleRect.yMax, 250, 32);
            MakeFloatOptionButtons(floatMenuButtonsRect,
                leftAction: delegate
                {
                    if (allWeaponTraits.IndexOf(currentWeaponTrait) > 0)
                    {
                        currentWeaponTrait = allWeaponTraits[allWeaponTraits.IndexOf(currentWeaponTrait) - 1];
                    }
                    else
                    {
                        currentWeaponTrait = allWeaponTraits[allWeaponTraits.Count - 1];
                    }

                }, centerAction: delegate
                {
                    FloatMenuUtility.MakeMenu(allWeaponTraits, entry => entry.LabelCap, (WeaponTraitDef variant) => delegate
                    {
                        currentWeaponTrait = variant;
                    });
                }, centerButtonName: currentWeaponTrait.LabelCap,
                rightAction: delegate
                {
                    if (allWeaponTraits.IndexOf(currentWeaponTrait) < allWeaponTraits.Count - 1)
                    {
                        currentWeaponTrait = allWeaponTraits[allWeaponTraits.IndexOf(currentWeaponTrait) + 1];
                    }
                    else
                    {
                        currentWeaponTrait = allWeaponTraits[0];
                    }
                });
            
            var weaponTraitDescription = currentWeaponTrait.LabelCap + ": " + currentWeaponTrait.description.CapitalizeFirst();
            var height = Text.CalcHeight(weaponTraitDescription, floatMenuButtonsRect.width);
            var weaponTraitDescriptionRect = new Rect(floatMenuButtonsRect.x, floatMenuButtonsRect.yMax + 15, floatMenuButtonsRect.width, height);
            Widgets.Label(weaponTraitDescriptionRect, weaponTraitDescription);
        }

        public override float GetScrollHeight()
        {
            var baseNumber = base.GetScrollHeight();
            var weaponTraitDescription = currentWeaponTrait.LabelCap + ": " + currentWeaponTrait.description.CapitalizeFirst();
            var height = Text.CalcHeight(weaponTraitDescription, 250);
            return Mathf.Max(250 + 24 + 32 + height + 35, baseNumber);
        }
        
        public override void Randomize()
        {
            base.Randomize();
            this.currentWeaponTrait = allWeaponTraits.RandomElement();
        }
    }
}
