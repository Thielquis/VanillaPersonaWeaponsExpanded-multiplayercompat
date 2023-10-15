using GraphicCustomization;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaPersonaWeaponsExpanded
{
    [HotSwappable]
    public class Dialog_AdjustPersonaWeapon : Dialog_GraphicCustomization
    {
        public Dialog_AdjustPersonaWeapon(ExtendedGraphicComp comp, Pawn pawn = null) : base(comp, pawn)
        {
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = DrawTitle(ref inRect);
            float scrollHeight = GetScrollHeight();
            Rect outRect = new Rect(inRect.x, rect.yMax + 30f, inRect.width, 350f);
            Rect viewRect = new Rect(inRect.x, outRect.y, inRect.width - 16f, scrollHeight);
            Rect itemTextureRect = new Rect(inRect.x + 10f, viewRect.y, 250f, 250f);
            DrawItem(itemTextureRect);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            DrawCustomizationArea(itemTextureRect);
            Widgets.EndScrollView();
            Rect rect2 = new Rect(inRect.width / 2f - 155f, inRect.height - 32f, 150f, 32f);
            if (Widgets.ButtonText(rect2, "VEF.Cancel".Translate()))
            {
                Close();
            }

            Rect confirmRect = new Rect(inRect.width / 2f + 5f, inRect.height - 32f, 150f, 32f);
            DrawConfirmButton(confirmRect, "Confirm".Translate(), delegate
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
                    else
                    {
                        overrideExist.Add(false);
                        chances.Add(0);
                        complete.Add("");
                        complete.Add("");
                    }
                    overrideChances.Add(variant.chanceOverride);
                }

                GameComponent_PersonaWeapons.SetCustomWeaponGraphicForPawn(this.pawn, complete, overrideExist, chances, overrideChances, currentName);
                Close();
            });
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            GameComponent_PersonaWeapons.RemoveWeaponForPawn(this.pawn);
        }
    }
}
