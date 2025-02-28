﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace EGamePlay.Combat
{
    /// <summary>
    /// 行为节点，一次战斗行动<see cref="IActionExecute"/>会触发战斗实体一系列的行为节点<see cref="BehaviourPoint"/>
    /// </summary>
    public sealed class BehaviourPoint
    {
        public List<Action<Entity>> Listeners { get; set; } = new List<Action<Entity>>();
        public List<Entity> Observers { get; set; } = new List<Entity>();


        public void AddListener(Action<Entity> action)
        {
            Listeners.Add(action);
        }

        public void RemoveListener(Action<Entity> action)
        {
            Listeners.Remove(action);
        }

        public void AddObserver(Entity observer)
        {
            Observers.Add(observer);
        }

        public void RemoveObserver(Entity observer)
        {
            Observers.Remove(observer);
        }

        public void TriggerAllObservers(Entity actionExecution)
        {
            if (Listeners.Count > 0)
            {
                for (int i = Listeners.Count - 1; i >= 0; i--)
                {
                    var item = Listeners[i];
                    item.Invoke(actionExecution);
                }
            }
            if (Observers.Count > 0)
            {
                for (int i = Observers.Count - 1; i >= 0; i--)
                {
                    var item = Observers[i];
                    item.GetComponent<BehaviourObserveComponent>().OnTrigger(actionExecution);
                }
            }
        }
    }

    [Flags]
    [LabelText("行动点类型")]
    public enum ActionPointType
    {
        [LabelText("（空）")]
        None = 0,

        [LabelText("执行 伤害行动前")]
        PreExecuteDamage = 1 << 1,
        [LabelText("遭遇 伤害行动前")]
        PreSufferDamage = 1 << 2,

        [LabelText("执行 伤害行动后")]
        PostExecuteDamage = 1 << 3,
        [LabelText("遭遇 伤害行动后")]
        PostSufferDamage = 1 << 4,

        [LabelText("执行 治疗行动后")]
        PostExecuteCure = 1 << 5,
        [LabelText("遭遇 治疗行动后")]
        PostSufferCure = 1 << 6,

        [LabelText("执行 赋给效果行动后")]
        ExecuteAssignEffect = 1 << 7,
        [LabelText("遭遇 赋给效果行动后")]
        SufferAssignEffect = 1 << 8,

        [LabelText("执行 赋加状态行动后")]
        PostExecuteStatus = 1 << 9,
        [LabelText("遭遇 赋加状态行动后")]
        PostSufferStatus = 1 << 10,

        [LabelText("给予普攻前")]
        PreExecuteAttack = 1 << 11,
        [LabelText("给予普攻后")]
        PostExecuteAttack = 1 << 12,

        [LabelText("遭受普攻前")]
        PreSufferAttack = 1 << 13,
        [LabelText("遭受普攻后")]
        PostSufferAttack = 1 << 14,

        [LabelText("起跳前")]
        PreExecuteJumpTo = 1 << 15,
        [LabelText("起跳后")]
        PostExecuteJumpTo = 1 << 16,

        [LabelText("施法前")]
        PreExecuteSpell = 1 << 17,
        [LabelText("施法后")]
        PostExecuteSpell = 1 << 18,

        //[LabelText("赋给普攻效果前")]
        //PreGiveAttackEffect = 1 << 19,
        //[LabelText("赋给普攻效果后")]
        //PostGiveAttackEffect = 1 << 20,
        //[LabelText("承受普攻效果前")]
        //PreReceiveAttackEffect = 1 << 21,
        //[LabelText("承受普攻效果后")]
        //PostReceiveAttackEffect = 1 << 22,

        Max,
    }

    [Flags]
    [LabelText("施效点类型")]
    public enum ApplyPointType
    {
        [LabelText("（空）")]
        None = 0,

        [LabelText("造成 伤害效果前")]
        PreCauseDamage = 1 << 1,
        [LabelText("承受 伤害效果前")]
        PreReceiveDamage = 1 << 2,

        [LabelText("造成 伤害效果后")]
        PostCauseDamage = 1 << 3,
        [LabelText("承受 伤害效果后")]
        PostReceiveDamage = 1 << 4,

        [LabelText("给予 治疗效果后")]
        PostGiveCure = 1 << 5,
        [LabelText("接受 治疗效果后")]
        PostReceiveCure = 1 << 6,

        [LabelText("赋给技能效果")]
        AssignEffect = 1 << 7,
        [LabelText("接受技能效果")]
        ReceiveEffect = 1 << 8,

        [LabelText("赋加状态后")]
        PostGiveStatus = 1 << 9,
        [LabelText("承受状态后")]
        PostReceiveStatus = 1 << 10,

        [LabelText("给予普攻前")]
        PreGiveAttack = 1 << 11,
        [LabelText("给予普攻后")]
        PostGiveAttack = 1 << 12,

        [LabelText("遭受普攻前")]
        PreReceiveAttack = 1 << 13,
        [LabelText("遭受普攻后")]
        PostReceiveAttack = 1 << 14,

        [LabelText("起跳前")]
        PreJumpTo = 1 << 15,
        [LabelText("起跳后")]
        PostJumpTo = 1 << 16,

        [LabelText("施法前")]
        PreSpell = 1 << 17,
        [LabelText("施法后")]
        PostSpell = 1 << 18,

        //[LabelText("赋给普攻效果前")]
        //PreGiveAttackEffect = 1 << 19,
        //[LabelText("赋给普攻效果后")]
        //PostGiveAttackEffect = 1 << 20,
        //[LabelText("承受普攻效果前")]
        //PreReceiveAttackEffect = 1 << 21,
        //[LabelText("承受普攻效果后")]
        //PostReceiveAttackEffect = 1 << 22,

        Max,
    }

    /// <summary>
    /// 行为节点管理器，在这里管理一个战斗实体所有行为节点的添加监听、移除监听、触发流程
    /// </summary>
    public sealed class BehaviourPointComponent : Component
    {
        //行动点
        private Dictionary<ActionPointType, BehaviourPoint> ActionPoints { get; set; } = new Dictionary<ActionPointType, BehaviourPoint>();
        //施效点
        private Dictionary<ApplyPointType, BehaviourPoint> ApplyPoints { get; set; } = new Dictionary<ApplyPointType, BehaviourPoint>();


        public void AddListener(ActionPointType actionPointType, Action<Entity> action)
        {
            if (!ActionPoints.ContainsKey(actionPointType))
            {
                ActionPoints.Add(actionPointType, new BehaviourPoint());
            }
            ActionPoints[actionPointType].AddListener(action);
        }

        public void RemoveListener(ActionPointType actionPointType, Action<Entity> action)
        {
            if (ActionPoints.ContainsKey(actionPointType))
            {
                ActionPoints[actionPointType].RemoveListener(action);
            }
        }

        public void AddListener(ApplyPointType applyPointType, Action<Entity> action)
        {
            if (!ApplyPoints.ContainsKey(applyPointType))
            {
                ApplyPoints.Add(applyPointType, new BehaviourPoint());
            }
            ApplyPoints[applyPointType].AddListener(action);
        }

        public void RemoveListener(ApplyPointType applyPointType, Action<Entity> action)
        {
            if (ApplyPoints.ContainsKey(applyPointType))
            {
                ApplyPoints[applyPointType].RemoveListener(action);
            }
        }

        public void AddObserver(ActionPointType actionPointType, Entity observer)
        {
            if (!ActionPoints.ContainsKey(actionPointType))
            {
                ActionPoints.Add(actionPointType, new BehaviourPoint());
            }
            ActionPoints[actionPointType].AddObserver(observer);
        }

        public void RemoveObserver(ActionPointType actionPointType, Entity observer)
        {
            if (ActionPoints.ContainsKey(actionPointType))
            {
                ActionPoints[actionPointType].RemoveObserver(observer);
            }
        }

        public BehaviourPoint GetActionPoint(ActionPointType actionPointType)
        {
            if (ActionPoints.TryGetValue(actionPointType, out var actionPoint)) ;
            return actionPoint;
        }

        public void TriggerActionPoint(ActionPointType actionPointType, Entity actionExecution)
        {
            foreach (var item in ActionPoints)
            {
                if (item.Key.HasFlag(actionPointType))
                {
                    item.Value.TriggerAllObservers(actionExecution);
                }
            }
        }

        public void TriggerApplyPoint(ApplyPointType actionPointType, Entity actionExecution)
        {
            foreach (var item in ApplyPoints)
            {
                if (item.Key.HasFlag(actionPointType))
                {
                    item.Value.TriggerAllObservers(actionExecution);
                }
            }
        }
    }
}