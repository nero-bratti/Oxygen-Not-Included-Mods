using HarmonyLib;
using Klei.AI;
using System.Collections.Generic;
using TUNING;

public static class Patches
{
    private static Dictionary<string, Patches.DupeInfo> canonicalTraits = new Dictionary<string, Patches.DupeInfo>();

    public class DupeInfo
    {
        public List<string> traits;
        public List<string> aptitudes;
        public List<int> bonus;

        public DupeInfo(List<string> traits, List<string> aptitudes, List<int> bonus)
        {
            this.traits = traits;
            this.aptitudes = aptitudes;
            this.bonus = bonus;
        }
    }

    [HarmonyPatch(typeof(ModifierSet), "LoadTraits")]
    public static class AdicionalTraitsPatch
    {
        public static void Prefix()
        {
            TRAITS.TRAIT_CREATORS.Add(TraitUtil.CreateDisabledTaskTrait("Catalina", "Catalina", "Catalina is fried.", "Research", true));
        }
    }

    [HarmonyPatch(typeof(Db), "Initialize")]
    public static class CanonicalTraitsAttribution
    {
        public static void Postfix()
        {
            Patches.canonicalTraits["ELLIE"] = new Patches.DupeInfo(new List<string>()
      {
        "Ellie",
        "CaringDown"
      }, new List<string>() { "Decorating", "Research" }, new List<int>()
      {
        3,
        2
      });
            Patches.canonicalTraits["JOSHUA"] = new Patches.DupeInfo(new List<string>()
      {
        "Joshua"
      }, new List<string>() { "Decorating", "Research" }, new List<int>()
      {
        3,
        2
      });
            Patches.canonicalTraits["CATALINA"] = new Patches.DupeInfo(new List<string>()
      {
        "Catalina"
      }, new List<string>() { "Decorating" }, new List<int>()
      {
        3,
        2
      });
        }
    }

    [HarmonyPatch(typeof(MinionStartingStats), "GenerateTraits")]
    public static class ApplyCanonicalTraitsPatch
    {
        public static bool Prefix(
          ref int __result,
          MinionStartingStats __instance,
          bool is_starter_minion,
          List<ChoreGroup> disabled_chore_groups,
          string guaranteedAptitudeID = null,
          string guaranteedTraitID = null,
          bool isDebugMinion = false)
        {
            UnityEngine.Debug.Log((object)"GenerateTraits start");
            Patches.DupeInfo dupeInfo;
            if (!Patches.canonicalTraits.TryGetValue(__instance.personality.Id, out dupeInfo))
                return true;
            __instance.stressTrait = Db.Get().traits.Get(__instance.personality.stresstrait);
            __instance.joyTrait = Db.Get().traits.Get(__instance.personality.joyTrait);
            __instance.stickerType = __instance.personality.stickerType;
            Trait trait1 = Db.Get().traits.TryGet(__instance.personality.congenitaltrait);
            __instance.congenitaltrait = trait1 == null || trait1.Name == "None" ? (Trait)null : trait1;
            foreach (string trait2 in dupeInfo.traits)
            {
                Trait trait3 = Db.Get().traits.TryGet(trait2);
                if (trait3 != null)
                    __instance.Traits.Add(trait3);
            }
            UnityEngine.Debug.Log((object)"GenerateTraits end");
            return false;
        }
    }
}