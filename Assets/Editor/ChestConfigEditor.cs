using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ChestConfigEditor : EditorWindow
{
    private const string DEFAULT_NAME = "ChestConfig";
    private const string PATH = "Assets/Configs/Chest/";
    private const string REWARDS_FIELD_NAME = "_rewards";

    private string _configName;
    private int _rewardsCount = 0;
    private List<RewardDebugInfo> _rewardInfo;
    private Vector2 _scrollPos;

    [MenuItem("Window/Chest config")]
    private static void OpenWindow()
    {
        ChestConfigEditor wnd = (ChestConfigEditor)GetWindow(typeof(ChestConfigEditor));
        wnd.minSize = new Vector2(400, 200);
        wnd.Show();
    }

    private void OnEnable()
    {
        _rewardInfo = new List<RewardDebugInfo>();
    }

    private void OnGUI()
    {
        CreateEmptyRewards();
        FindErorrsInRewards();
        ShowGUI();
    }

    private void CreateEmptyRewards()
    {
        if (_rewardInfo.Count < _rewardsCount)
        {
            for (int i = _rewardInfo.Count; i <= _rewardsCount; i++)
            {
                _rewardInfo.Add(new RewardDebugInfo() { reward = new ChestConfig.RewardInfo(), isExpanded = true });
            }
        }
    }

    private void ShowGUI()
    {
        GUILayout.BeginHorizontal();

        DrawLeftBar();

        EditorGUILayout.Space(10, false);

        DrawRightBar();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftBar()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false));

        int index = _rewardInfo.FindIndex(r => r.hasErrors);
        if (index != -1 && index < _rewardsCount)
        {
            ChangeColorIfError(true,
                () => EditorGUILayout.LabelField("Rewards have errors!\nLook at red labels.", GUILayout.Width(150), GUILayout.MaxHeight(30)));

        }
        else if (_rewardsCount == 0)
        {
            ChangeColorIfError(true,
                () => EditorGUILayout.LabelField("No rewards in config.", GUILayout.Width(150)));
        }
        else
        {
            if (GUILayout.Button("Create Config", GUILayout.Width(150)))
            {
                CreateAsset();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRightBar()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        SetLabel("Config name:");
        _configName = EditorGUILayout.TextField(_configName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        SetLabel("Rewards count:");
        _rewardsCount = EditorGUILayout.IntField(_rewardsCount);
        EditorGUILayout.EndHorizontal();

        if (_rewardInfo.Count >= _rewardsCount)
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _rewardsCount; i++)
            {
                var debugRewardInfo = _rewardInfo[i];
                ChangeColorIfError(debugRewardInfo.hasErrors,
                    () => debugRewardInfo.isExpanded =
                    EditorGUILayout.BeginFoldoutHeaderGroup(debugRewardInfo.isExpanded, $"Reward {i + 1}"));


                if (debugRewardInfo.isExpanded)
                {
                    EditorGUI.indentLevel += 2;
                    DrawItem(debugRewardInfo);
                    EditorGUI.indentLevel -= 2;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space(15);
            }

            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
    }

    private void CreateAsset()
    {
        var chestConfig = CreateInstance<ChestConfig>();

        var type = typeof(ChestConfig);
        FieldInfo fieldInfo = type.GetField(REWARDS_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
        var value = fieldInfo.GetValue(chestConfig);
        fieldInfo.SetValue(chestConfig, _rewardInfo.Take(_rewardsCount).Select(r => r.reward).ToArray());

        _configName = string.IsNullOrEmpty(_configName) ? DEFAULT_NAME : _configName;
        string path = AssetDatabase.GenerateUniqueAssetPath(PATH + _configName + ".asset");
        AssetDatabase.CreateAsset(chestConfig, path);

        _rewardInfo.Clear();
    }

    private void DrawItem(RewardDebugInfo rewardDebugInfo)
    {
        var reward = rewardDebugInfo.reward;

        EditorGUILayout.BeginHorizontal();
        SetLabel("Weight:");
        ChangeColorIfError(rewardDebugInfo.errors.fstError,
            () => reward.randomWeight = EditorGUILayout.Slider(reward.randomWeight, 0, 1));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        SetLabel("Reward type:");
        reward.type = (ChestConfig.RewardType)EditorGUILayout.EnumPopup(reward.type);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        switch (reward.type)
        {
            case ChestConfig.RewardType.Soft:
                EditorGUILayout.BeginHorizontal();
                ChangeColorIfError(rewardDebugInfo.errors.sndError, () => DrawSoftHardSliders(reward));
                EditorGUILayout.EndHorizontal();
                break;
            case ChestConfig.RewardType.Hard:
                EditorGUILayout.BeginHorizontal();
                ChangeColorIfError(rewardDebugInfo.errors.sndError, () => DrawHardSliders(reward));
                EditorGUILayout.EndHorizontal();
                break;
            case ChestConfig.RewardType.Chest:
                EditorGUILayout.BeginHorizontal();
                SetLabel("Chest config:");
                ChangeColorIfError(rewardDebugInfo.errors.sndError,
                    () => reward.chest = (ChestConfig)EditorGUILayout.ObjectField(reward.chest, typeof(ChestConfig), false));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                SetLabel("Chest count:");
                ChangeColorIfError(rewardDebugInfo.errors.trdError,
                    () => reward.chestCount = EditorGUILayout.IntField(reward.chestCount));
                EditorGUILayout.EndHorizontal();
                break;
            default:
                break;
        }

        void DrawHardSliders(ChestConfig.RewardInfo reward)
        {
            SetLabel("Hard min:");
            reward.hardMin = EditorGUILayout.Slider(reward.hardMin, 0, 1000);

            SetLabel("Hard max:");
            reward.hardMax = EditorGUILayout.Slider(reward.hardMax, reward.hardMin, 1000);
        }
    }

    private void DrawSoftHardSliders(ChestConfig.RewardInfo reward)
    {
        SetLabel("Soft min:");
        reward.softMin = EditorGUILayout.IntSlider(reward.softMin, 0, 1000);

        SetLabel("Soft max:");
        reward.softMax = EditorGUILayout.IntSlider(reward.softMax, reward.softMin, 1000);
    }

    private void FindErorrsInRewards()
    {
        for (int i = 0; i < _rewardsCount; i++)
        {
            _rewardInfo[i].errors = FindErrors(_rewardInfo[i].reward);
        }
    }

    private void ChangeColorIfError(bool hasError, Action drawGuiAction)
    {
        if (hasError)
        {
            GUI.contentColor = Color.red;
        }

        drawGuiAction();

        GUI.contentColor = Color.white;
    }

    private (bool firstError, bool secondError, bool thirdError) FindErrors(ChestConfig.RewardInfo reward)
    {
        switch (reward.type)
        {
            case ChestConfig.RewardType.Soft:
                return (reward.randomWeight == 0, reward.softMax == 0 && reward.softMin == 0, false); ;
            case ChestConfig.RewardType.Hard:
                return (reward.randomWeight == 0, reward.hardMax == 0 && reward.hardMin == 0, false); ;
            case ChestConfig.RewardType.Chest:
                return (reward.randomWeight == 0, reward.chest == null, reward.chestCount == 0);
            default:
                return (false, false, false);
        }
    }

    private void SetLabel(string text)
    {
        EditorGUILayout.LabelField(text, GUILayout.MinWidth(10), GUILayout.ExpandWidth(false));
    }

    private class RewardDebugInfo
    {
        public ChestConfig.RewardInfo reward;
        public (bool fstError, bool sndError, bool trdError) errors;
        public bool isExpanded;
        public bool hasErrors => errors != (false, false, false);
    }
}
