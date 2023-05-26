using System.Collections.Generic;
using UnityEditor;

public class ChestConfigEditor : EditorWindow
{
    private ChestConfig _chestConfig;
    private List<RewardToggleInfo> _rewardInfo;
    private int _rewardsCount = 0;

    [MenuItem("Window/Chest config")]
    private static void OpenWindow()
    {
        ChestConfigEditor wnd = (ChestConfigEditor)GetWindow(typeof(ChestConfigEditor));
        wnd.Show();
    }

    private void OnEnable()
    {
        _chestConfig = (ChestConfig)CreateInstance(typeof(ChestConfig));
        _rewardInfo = new List<RewardToggleInfo>();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Количество наград:");
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

            for (int i = 0; i < _rewardsCount; i++)
            {
                _rewardInfo[i].isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_rewardInfo[i].isExpanded, $"Награда {i + 1}");

                if (_rewardInfo[i].isExpanded)
                {
                    EditorGUI.indentLevel++;
                    var reward = _rewardInfo[i].reward;
                    DrawItem(reward);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space(15);
            }
        }
    }

    private void DrawItem(ChestConfig.RewardInfo reward)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Вес:");
        reward.randomWeight = EditorGUILayout.Slider(reward.randomWeight, 0, 1);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Тип награды:");
        reward.type = (ChestConfig.RewardType)EditorGUILayout.EnumPopup(reward.type);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        switch (reward.type)
        {
            case ChestConfig.RewardType.Soft:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Soft min:");
                reward.softMin = EditorGUILayout.IntSlider(reward.softMin, 0, 1000);

                EditorGUILayout.LabelField("Soft max:");
                reward.softMax = EditorGUILayout.IntSlider(reward.softMax, reward.softMin, 1000);
                EditorGUILayout.EndHorizontal();
                break;
            case ChestConfig.RewardType.Hard:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Hard min:");
                reward.hardMin = EditorGUILayout.Slider(reward.hardMin, 0, 1000);

                EditorGUILayout.LabelField("Hard max:");
                reward.hardMax = EditorGUILayout.Slider(reward.hardMax, reward.hardMin, 1000);
                EditorGUILayout.EndHorizontal();
                break;
            case ChestConfig.RewardType.Chest:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Chest config:");
                reward.chest = (ChestConfig)EditorGUILayout.ObjectField(reward.chest, typeof(ChestConfig), false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("chestCount:");
                reward.chestCount = EditorGUILayout.IntSlider(reward.chestCount, 1, 1000);
                EditorGUILayout.EndHorizontal();
                break;
            default:
                break;
        }
    }

    private class RewardToggleInfo
    {
        public ChestConfig.RewardInfo reward;
        public bool isExpanded;
    }


}
