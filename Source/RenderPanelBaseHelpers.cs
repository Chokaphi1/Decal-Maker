using System.Collections.Generic;

namespace VAM_Decal_Maker
{
    internal static class RenderPanelBaseHelpers
    {

        private static readonly Dictionary<string, int[]> femaleTextureIndex = new Dictionary<string, int[]>()
        {
            {BodyRegionEnum.Torso, new int[]{15,18,19,20,21,27,29}},
            {BodyRegionEnum.Face, new int[]{2,5,11} },
            {BodyRegionEnum.Limbs, new int[]{0,12,14,16,17,22,23}},
            {BodyRegionEnum.Genitals, new int[]{28}}
        };
        private static readonly Dictionary<string, int[]> maleTextureIndex = new Dictionary<string, int[]>()
        {
            {BodyRegionEnum.Torso, new int[]{15,16,19,20,21,22,30,32}},
            {BodyRegionEnum.Face, new int[]{2,5,11}},
            {BodyRegionEnum.Limbs, new int[]{0,12,14,17,18,23,24}},
            {BodyRegionEnum.Genitals, new int[]{28,29}}
        };
    }
}