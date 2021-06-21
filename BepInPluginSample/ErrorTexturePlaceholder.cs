using BepInEx;
using BepInEx.Configuration;
using COM3D2.Lilly.Plugin;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.ErrorTexturePlaceholder.Plugin
{
    [BepInPlugin(MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_VERSION)]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    //[BepInPlugin("COM3D2.Sample.Plugin", "COM3D2.Sample.Plugin", "21.6.6")]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    [BepInProcess("COM3D2x64.exe")]
    public class ErrorTexturePlaceholder : BaseUnityPlugin
    {

        Harmony harmony;

        public void OnEnable()
        {
            MyLog.LogMessage("OnEnable");

            // 하모니 패치
            harmony = Harmony.CreateAndPatchAll(typeof(ErrorTexturePlaceholderPatch));

        }

        public void OnDisable()
        {
            MyLog.LogMessage("OnDisable");

            harmony.UnpatchSelf();// ==harmony.UnpatchAll(harmony.Id);
            //harmony.UnpatchAll(); // 정대 사용 금지. 다름 플러그인이 패치한것까지 다 풀려버림
        }

    }
}
