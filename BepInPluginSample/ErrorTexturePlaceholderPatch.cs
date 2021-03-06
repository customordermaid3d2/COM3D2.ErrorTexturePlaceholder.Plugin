using COM3D2.Lilly.Plugin;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace COM3D2.ErrorTexturePlaceholder.Plugin
{
    class ErrorTexturePlaceholderPatch
    {


        private const string MISSING_TEX =
            "iVBORw0KGgoAAAANSUhEUgAAAfQAAAH0BAMAAAA5+MK5AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv" +
            "8YQUAAAAwUExURQAAAP8A3AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAI" +
            "/r1v0AAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJ3SURBVHja7c8xEQAgAMQwLODfLIeNbzp1zbm/86sdO" +
            "jo6Ojo6+hC97kdHR0dHR0efuKY6CUZHR0dHR1+m1/3o6Ojo6OjoE9dUJ8Ho6Ojo6OjL9LofHR0dHR0d" +
            "feKa6iQYHR0dHR19mV73o6Ojo6Ojo09cU50Eo6Ojo6OjL9PrfnR0dHR0dPSJa6qTYHR0dHR09GV63Y+" +
            "Ojo6Ojo4+cU11EoyOjo6Ojr5Mr/vR0dHR0dHRJ66pToLR0dHR0dGX6XU/Ojo6Ojo6+sQ11UkwOjo6Oj" +
            "r6Mr3uR0dHR0dHR5+4pjoJRkdHR0dHX6bX/ejo6Ojo6OgT11Qnwejo6Ojo6Mv0uh8dHR0dHR194prqJ" +
            "BgdHR0dHX2ZXvejo6Ojo6OjT1xTnQSjo6Ojo6Mv0+t+dHR0dHR09IlrqpNgdHR0dHT0ZXrdj46Ojo6O" +
            "jj5xTXUSjI6Ojo6Ovkyv+9HR0dHR0dEnrqlOgtHR0dHR0ZfpdT86Ojo6Ojr6xDXVSTA6Ojo6Ovoyve5" +
            "HR0dHR0dHn7imOglGR0dHR0dfptf96Ojo6Ojo6BPXVCfB6Ojo6Ojoy/S6Hx0dHR0dHX3imuokGB0dHR" +
            "0dfZle96Ojo6Ojo6NPXFOdBKOjo6Ojoy/T6350dHR0dHT0iWuqk2B0dHR0dPRlet2Pjo6Ojo6OPnFNd" +
            "RKMjo6Ojo6+TK/70dHR0dHR0SeuqU6C0dHR0dHRl+l1Pzo6Ojo6OvrENdVJMDo6Ojo6+jK97kdHR0dH" +
            "R0efuKY6CUZHR0dHR1+m1/3o6Ojo6OjoE9dUJ8Ho6Ojo6OjL9LofHR0dHR0dfeKa6iQYHR0dHR19mV7" +
            "3o6Ojb9+9DzgHN1XYvWlQAAAAAElFTkSuQmCC";

        private static TextureResource tex = new TextureResource(1, 1, TextureFormat.ARGB32, new Rect[0], Convert.FromBase64String(MISSING_TEX));

        [HarmonyPatch(typeof(ImportCM), "LoadTexture")]
        [HarmonyFinalizer]
        public static void HandleTextureFail(ref Exception __exception, ref TextureResource __result, string f_strFileName)
        {
            if (__exception == null)
                return;
            Debug.LogWarning($"[ImportCM.LoadTexture] Failed to load texture `{f_strFileName}`: ({__exception.GetType()}) {__exception.Message}");
            __exception = null;
            __result = tex;
        }


        [HarmonyPatch(typeof(ImportCM), "LoadTexture")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CleanUpLoadFailMessages(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(NDebug), "Assert", new[] { typeof(string), typeof(bool) })))
                .SetAndAdvance(OpCodes.Nop, null)
                .Insert(
                    Transpilers.EmitDelegate<Action<string, bool>>((msg, valid) => {
                        if (!valid)
                            throw new Exception("File is not valid (it's likely missing or filesystem failed to load it)");
                    })
                )
                .MatchForward(false,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Debug), "LogError", new[] { typeof(object) })))
                .SetAndAdvance(OpCodes.Nop, null)
                .Insert(
                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    Transpilers.EmitDelegate<Action<Exception>>(ex => {
                        throw new Exception($"Failed to load texture (it's either missing or a mod is misconfigured!). Inner exception: ({ex.GetType()}) {ex.Message}");
                    })
                )
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldstr, "CM3D2_TEX"),
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Brfalse))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_3),
                    Transpilers.EmitDelegate<Action<string>>((txt) => {
                        throw new Exception($"Invalid header. Got: `{txt}`, should be `CM3D2_TEX`.");
                    })
                )
                .InstructionEnumeration();
        }
    }
}
