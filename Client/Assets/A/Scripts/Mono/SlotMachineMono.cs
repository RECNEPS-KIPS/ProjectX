using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using Random = UnityEngine.Random;
using GameFramework;
using DG.Tweening;

namespace GameLogic
{
    /// <summary>
    /// 老虎机MonoBehaviour类 - 三个竖轴滚动实现
    /// </summary>
    public class SlotMachineMono : MonoBehaviour
    {
        [Header("==== SlotMachine Config ====")]
        [SerializeField]
        private int m_reelCount = 3;
        [SerializeField]
        private Transform[] m_reelRoots;
        [SerializeField]
        private List<List<SlotSymbol>> m_symbols;
        [SerializeField]
        private GameObject m_symbolPrefab;
        [SerializeField]
        private float m_symbolHeight = 100;
        [SerializeField]
        private float m_symbolSpace = 20;

        [Header("==== Animation Config ====")]
        [SerializeField]
        private float m_spinDuration = 3.0f;   // 旋转持续时间
        [SerializeField]
        private float m_spinSpeed = 1000f;     // 旋转速度
        [SerializeField]
        private float m_spinSlowDownDuration = 1.0f;  // 减速时间
        [SerializeField]
        private Button m_spinButton;           // 开始按钮
        [SerializeField]
        private TextMeshProUGUI m_resultText;  // 结果文本

        [Header("==== Other Animation Effect ====")]
        [SerializeField]
        private Transform m_joyStick;
        [SerializeField]
        private float m_joyStickDuration = 0.1f;
        [SerializeField]
        private float m_slotMachineDuration = 0.1f;
        [SerializeField]
        private float m_slotMachineStrength = 10f;
        [SerializeField]
        private int m_slotMachineVibrato = 10;
        [SerializeField]
        private float m_slotMachineRandomnessMode = 90f;
        [SerializeField]
        private bool m_slotMachinefadeOut = true;
        private Tween m_joystickTween;
        private Tween m_slotMachineShakeTween;

        // 轮盘状态
        private enum ReelState
        {
            Idle,       // 静止状态
            Spinning,   // 旋转状态
            SlowingDown // 减速状态
        }

        private ReelState m_currentState = ReelState.Idle;
        private List<GameObject>[] m_symbolObjects;  // 符号对象
        private int[] m_targetSymbolIndices;         // 目标符号索引
        private float m_currentSpinTime = 0;         // 当前旋转时间
        private float[] m_reelPositions;            // 轮盘位置
        private float[] m_reelSpeeds;               // 轮盘速度

        private void InitSymbol()
        {
            var gameConfigModule = GameRoot.Instance.GetGameModule<GameConfigModule>();
            var symbolConfigList = gameConfigModule.GetConfig<SymbolConfigVO>();
            m_symbols = new List<List<SlotSymbol>>();
            for (int i = 0; i < m_reelCount; i++)
            {
                m_symbols.Add(new List<SlotSymbol>());
                for (int j = 0; j < symbolConfigList.Count; j++)
                {
                    m_symbols[i].Add(
                        new SlotSymbol()
                        {
                            id = symbolConfigList[j].SymbolID,
                            name = symbolConfigList[j].SpriteName,
                            weight = symbolConfigList[j].Weight,
                            reward = symbolConfigList[j].BuyCoin
                        });
                }
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            InitSymbol();
            InitReelObj();
            InitSimpleAnimation();

            // 初始化数组
            m_reelPositions = new float[m_reelCount];
            m_reelSpeeds = new float[m_reelCount];
            m_targetSymbolIndices = new int[m_reelCount];

            // 设置按钮点击事件
            if (m_spinButton != null)
            {
                m_spinButton.onClick.AddListener(OnSpinButtonClicked);
            }

            // 重置文本
            if (m_resultText != null)
            {
                m_resultText.text = "点击按钮开始!";
            }
        }

        private void InitSimpleAnimation()
        {
            m_joystickTween = m_joyStick.DORotate(new Vector3(0, 0, -30), m_joyStickDuration, RotateMode.Fast)
            .SetLoops(2, LoopType.Yoyo)
            .SetAutoKill(false)
            .Pause();

            m_slotMachineShakeTween = transform.DOShakeRotation(m_slotMachineDuration, m_slotMachineStrength, m_slotMachineVibrato, m_slotMachineRandomnessMode, m_slotMachinefadeOut)
            .SetLoops(-1, LoopType.Restart)
            .SetAutoKill(false)
            .Pause();
        }

        private void InitReelObj()
        {
            if (m_reelRoots == null || m_reelRoots.Length == 0 || m_reelRoots.Length != m_reelCount)
            {
                Debug.LogError("m_reelRoots 初始化失败");
                return;
            }

            m_symbolObjects = new List<GameObject>[m_reelCount];
            GameResourceLoadModule gameResourceLoadModule = GameRoot.Instance.GetGameModule<GameResourceLoadModule>();
            for (int i = 0; i < m_reelCount; i++)
            {
                Transform reelRoot = m_reelRoots[i];
                m_symbolObjects[i] = new List<GameObject>();

                for (int j = 0; j < m_symbols[i].Count; j++)
                {
                    SlotSymbol symbol = m_symbols[i][j];
                    GameObject symbolObj = Instantiate(m_symbolPrefab, reelRoot);
                    symbolObj.transform.localPosition = new Vector3(0, j * (m_symbolHeight + m_symbolSpace), 0);

                    // 设置符号显示
                    var img = symbolObj.GetComponent<Image>();
                    img.sprite = gameResourceLoadModule.LoadSprite(symbol.name, "Atlas/SlotMachine");

                    m_symbolObjects[i].Add(symbolObj);
                }
            }
        }

        private void Update()
        {
            switch (m_currentState)
            {
                case ReelState.Idle:
                    // 空闲状态不做任何处理
                    break;

                case ReelState.Spinning:
                    // 处理旋转
                    UpdateSpinning();
                    break;

                case ReelState.SlowingDown:
                    // 处理减速
                    UpdateSlowingDown();
                    break;
            }
        }

        private void OnSpinButtonClicked()
        {
            // 如果当前不在静止状态，忽略点击
            if (m_currentState != ReelState.Idle)
                return;
            // 开始旋转
            StartSpin();
        }

        private void StartSpin()
        {
            // 设置状态为旋转中
            m_currentState = ReelState.Spinning;
            StartSpin_AnimationEffect();
            // 禁用旋转按钮
            if (m_spinButton != null)
            {
                m_spinButton.interactable = false;
            }

            // 更新结果文本
            if (m_resultText != null)
            {
                m_resultText.text = "旋转中...";
            }

            // 随机生成结果
            GenerateRandomResult();

            // 初始化旋转数据
            m_currentSpinTime = 0;
            for (int i = 0; i < m_reelCount; i++)
            {
                // 设置每个轮盘的初始速度（可以有些微差异）
                m_reelSpeeds[i] = m_spinSpeed * (1f + Random.Range(-0.1f, 0.1f));
            }
        }

        private void UpdateSpinning()
        {
            // 更新旋转时间
            m_currentSpinTime += Time.deltaTime;
            // 每个轮盘旋转
            for (int i = 0; i < m_reelCount; i++)
            {
                // 更新位置
                m_reelPositions[i] += m_reelSpeeds[i] * Time.deltaTime;
                // 应用位置到符号
                ApplyReelPosition(i);
            }
            // 检查是否应该开始减速
            if (m_currentSpinTime >= m_spinDuration)
            {
                // 切换到减速状态
                m_currentState = ReelState.SlowingDown;
            }
        }

        private void UpdateSlowingDown()
        {
            // 晃动效果结束
            EndSpin_AnimationEffect();
            // 更新旋转时间
            m_currentSpinTime += Time.deltaTime;
            // 计算减速因子 (0 到 1)
            float slowDownFactor = Mathf.Clamp01((m_currentSpinTime - m_spinDuration) / m_spinSlowDownDuration);
            bool allReelsStopped = true;
            // 每个轮盘减速
            for (int i = 0; i < m_reelCount; i++)
            {
                // 有序减速，让轮盘依次停下
                float reelSlowDownFactor = Mathf.Clamp01((slowDownFactor * m_reelCount - i) / (m_reelCount * 0.5f));
                // 计算当前速度
                float targetSpeed = 0;
                m_reelSpeeds[i] = Mathf.Lerp(m_reelSpeeds[i], targetSpeed, reelSlowDownFactor);
                // 如果速度接近零，则对齐到目标结果
                if (Mathf.Abs(m_reelSpeeds[i]) < 10f)
                {
                    // 计算目标位置
                    float targetPosition = CalculateTargetPosition(i, m_targetSymbolIndices[i]);
                    m_reelPositions[i] = Mathf.Lerp(m_reelPositions[i], targetPosition, reelSlowDownFactor);
                    if (Mathf.Abs(m_reelPositions[i] - targetPosition) < 0.1f)
                    {
                        m_reelPositions[i] = targetPosition;
                        m_reelSpeeds[i] = 0;
                    }
                    else
                    {
                        allReelsStopped = false;
                    }
                }
                else
                {
                    // 更新位置
                    m_reelPositions[i] += m_reelSpeeds[i] * Time.deltaTime;
                    allReelsStopped = false;
                }
                // 应用位置到符号
                ApplyReelPosition(i);
            }
            // 检查是否所有轮盘已停止
            if (allReelsStopped)
            {
                OnSpinCompleted();
            }
        }

        private void ApplyReelPosition(int reelIndex)
        {
            if (m_symbolObjects == null || reelIndex >= m_symbolObjects.Length)
                return;
            // 获取符号总高度
            float totalHeight = m_symbols[reelIndex].Count * (m_symbolHeight + m_symbolSpace);
            int symbolCount = m_symbols[reelIndex].Count;
            // 应用位置到每个符号
            for (int j = 0; j < symbolCount; j++)
            {
                GameObject symbolObj = m_symbolObjects[reelIndex][j];
                // 计算初始位置
                float initialY = j * (m_symbolHeight + m_symbolSpace);
                // 应用旋转偏移
                float offsetY = m_reelPositions[reelIndex] % totalHeight;
                float finalY = initialY - offsetY;
                // 循环显示
                if (finalY < -m_symbolHeight * 2)
                {
                    finalY += totalHeight;
                }
                // 设置位置
                symbolObj.transform.localPosition = new Vector3(0, finalY, 0);
            }
        }

        private float CalculateTargetPosition(int reelIndex, int targetSymbolIndex)
        {
            // 计算符号总高度
            float totalHeight = m_symbols[reelIndex].Count * (m_symbolHeight + m_symbolSpace);
            // 计算目标符号的初始位置
            float initialY = targetSymbolIndex * (m_symbolHeight + m_symbolSpace);
            // 确保目标符号在中间位置
            return initialY % totalHeight;
        }

        private void GenerateRandomResult()
        {
            // 为每个轮盘随机生成符号索引
            for (int i = 0; i < m_reelCount; i++)
            {
                // 基于权重随机选择符号
                int randomIndex = GetRandomSymbolIndex(i);
                m_targetSymbolIndices[i] = randomIndex;
            }
        }

        private int GetRandomSymbolIndex(int reelIndex)
        {
            // 计算总权重
            int totalWeight = 0;
            for (int i = 0; i < m_symbols[reelIndex].Count; i++)
            {
                totalWeight += m_symbols[reelIndex][i].weight;
            }
            // 随机选择一个权重值
            int randomWeight = Random.Range(0, totalWeight);
            // 找到对应的符号索引
            int currentWeight = 0;
            for (int i = 0; i < m_symbols[reelIndex].Count; i++)
            {
                currentWeight += m_symbols[reelIndex][i].weight;
                if (randomWeight < currentWeight)
                {
                    return i;
                }
            }
            // 默认返回第一个符号
            return 0;
        }

        private void OnSpinCompleted()
        {
            // 设置状态为静止
            m_currentState = ReelState.Idle;
            // 启用旋转按钮
            if (m_spinButton != null)
            {
                m_spinButton.interactable = true;
            }

            // 计算结果
            CalculateAndShowResult();
        }

        private void CalculateAndShowResult()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("结果: ");

            // 显示每个轮盘的符号
            for (int i = 0; i < m_reelCount; i++)
            {
                int symbolIndex = m_targetSymbolIndices[i];
                string symbolName = m_symbols[i][symbolIndex].name;
                sb.AppendLine($"轮盘 {i + 1}: {symbolName}");
            }

            // 检查是否获胜
            bool isWin = CheckWin();
            if (isWin)
            {
                sb.AppendLine("恭喜你赢了!");
            }
            else
            {
                sb.AppendLine("再试一次!");
            }

            // 更新结果文本
            if (m_resultText != null)
            {
                m_resultText.text = sb.ToString();
            }
        }

        private bool CheckWin()
        {
            // 检查所有轮盘是否显示相同符号
            int firstSymbolIndex = m_targetSymbolIndices[0];
            string firstSymbolName = m_symbols[0][firstSymbolIndex].name;

            for (int i = 1; i < m_reelCount; i++)
            {
                int symbolIndex = m_targetSymbolIndices[i];
                string symbolName = m_symbols[i][symbolIndex].name;

                if (symbolName != firstSymbolName)
                {
                    return false;
                }
            }

            return true;
        }


        #region Animation Effect
        private void StartSpin_AnimationEffect()
        {
            // 摇杆动画效果
            m_joystickTween.Restart();
            // 老虎机晃动效果
            m_slotMachineShakeTween.Restart();
        }

        private void EndSpin_AnimationEffect()
        {
            m_slotMachineShakeTween.Rewind();
        }
        #endregion
    }
    /// <summary>
    /// 老虎机符号类
    /// </summary>
    [Serializable]
    public class SlotSymbol
    {
        public string id;        // 符号ID
        public string name;   // 符号名称
        public int weight;    // 随机权重
        public int reward;    // 奖励值
    }
}