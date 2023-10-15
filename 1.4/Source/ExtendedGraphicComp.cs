using GraphicCustomization;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanillaPersonaWeaponsExpanded
{
    public class ExtendedGraphicComp: CompGraphicCustomization
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (Props.customizable)
            {
                yield return new FloatMenuOption("VEF.Customize".Translate(parent.LabelShort), delegate
                {
                    Find.WindowStack.Add(new Dialog_AdjustPersonaWeapon(this, selPawn));

                    Pawn pawn = this.parent.TryGetComp<CompBladelinkWeapon>().CodedPawn;
                    GameComponent_PersonaWeapons.AddWeaponForPawn(pawn, this.parent.def, this.parent);
                });
            }
        }
    }
}
