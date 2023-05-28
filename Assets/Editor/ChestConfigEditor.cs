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
    private List<RewardToggleInfo> _rewardInfo;
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
        _rewardInfo = new List<RewardToggleInfo>();
    }

    private void OnGUI()
    {
        ShowGUI();
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
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(150));

        if (GUILayout.Button("Create Config"))
        {
            CreateAsset();
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

        if (_rewardsCount > 0)
        {
            if (_rewardInfo.Count < _rewardsCount)
            {
                for (int i = _rewardInfo.Count - 1; i < _rewardsCount; i++)
                {
                    _rewardInfo.Add(new RewardToggleInfo() { reward = new ChestConfig.RewardInfo(), isExpanded = true });
                }
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _rewardsCount; i++)
            {
                _rewardInfo[i].isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_rewardInfo[i].isExpanded, $"Reward {i + 1}");

                if (_rewardInfo[i].isExpanded)
                {
                    EditorGUI.indentLevel += 2;
                    var reward = _rewardInfo[i].reward;
                    DrawItem(reward);
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

    private void DrawItem(ChestConfig.RewardInfo reward)
    {
        EditorGUILayout.BeginHorizontal();
        SetLabel("Weight:");
        reward.randomWeight = EditorGUILayout.Slider(reward.randomWeight, 0, 1);
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
                SetLabel("Soft min:");
                reward.softMin = EditorGUILayout.IntSlider(reward.softMin, 0, 1000, GUILayout.ExpandWidth(false));

                SetLabel("Soft max:");
                reward.softMax = EditorGUILayout.IntSlider(reward.softMax, reward.softMin, 1000);
                EditorGUILayout.EndHorizontal();
                break;
            case ChestConfig.RewardType.Hard:
                EditorGUILayout.BeginHorizontal();
                SetLabel("Hard min:");
                reward.hardMin = EditorGUILayout.Slider(reward.hardMin, 0, 1000);

                SetLabel("Hard max:");
                reward.hardMax = EditorGUILayout.Slider(reward.hardMax, reward.hardMin, 1000);
                EditorGUILayout.EndHorizontal();
                break;
            case ChestConfig.RewardType.Chest:
                EditorGUILayout.BeginHorizontal();
                SetLabel("Chest config:");

                if (reward.chest == null)
                {
                    GUI.color = Color.red;
                }

                reward.chest = (ChestConfig)EditorGUILayout.ObjectField(reward.chest, typeof(ChestConfig), false);
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                SetLabel("Chest count:");

                if (reward.chestCount == 0)
                {
                    GUI.color = Color.red;
                }

                reward.chestCount = EditorGUILayout.IntField(reward.chestCount);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                break;
            default:
                break;
        }
    }

    private void HighlightError(bool isError, Action action)
    {
        if (isError)
        {
            GUI.color = Color.red;
        }

        action();
        GUI.color = Color.white;
    }

    private void SetLabel(string text)
    {
        EditorGUILayout.LabelField(text, GUILayout.MinWidth(10), GUILayout.ExpandWidth(false));
    }

    private class RewardToggleInfo
    {
        public ChestConfig.RewardInfo reward;
        public bool isExpanded;
    }
}
