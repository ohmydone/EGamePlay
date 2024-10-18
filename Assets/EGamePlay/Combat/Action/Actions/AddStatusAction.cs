using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using EGamePlay.Combat;
using ET;
using GameUtils;

namespace EGamePlay.Combat
{
    public class AddStatusActionAbility : Entity, IActionAbility
    {
        public CombatEntity OwnerEntity { get { return GetParent<CombatEntity>(); } set { } }
        public bool Enable { get; set; }


        public bool TryMakeAction(out AddStatusAction action)
        {
            if (Enable == false)
            {
                action = null;
            }
            else
            {
                action = OwnerEntity.AddChild<AddStatusAction>();
                action.ActionAbility = this;
                action.Creator = OwnerEntity;
            }
            return Enable;
        }
    }

    /// <summary>
    /// ʩ��״̬�ж�
    /// </summary>
    public class AddStatusAction : Entity, IActionExecute
    {
        public Entity SourceAbility { get; set; }
        public AddStatusEffect AddStatusEffect => SourceAssignAction.AbilityEffect.EffectConfig as AddStatusEffect;
        public Ability BuffAbility { get; set; }

        /// �ж�����
        public Entity ActionAbility { get; set; }
        /// Ч�������ж�Դ
        public EffectAssignAction SourceAssignAction { get; set; }
        /// �ж�ʵ��
        public CombatEntity Creator { get; set; }
        /// Ŀ�����
        public Entity Target { get; set; }


        public void FinishAction()
        {
            Entity.Destroy(this);
        }

        //ǰ�ô���
        private void PreProcess()
        {

        }

        public void ApplyAddStatus()
        {
            PreProcess();

//#if EGAMEPLAY_EXCEL
//            var statusConfig = AddStatusEffect.AddStatusConfig;
//            var canStack = statusConfig.CanStack == "��";
//            var enabledLogicTrigger = statusConfig.EnabledLogicTrigger();
//#else
            var buffObject = AddStatusEffect.AddStatus;
            if (buffObject == null)
            {
                var statusId = AddStatusEffect.AddStatusId;
                buffObject = AssetUtils.LoadObject<AbilityConfigObject>($"{AbilityManagerObject.BuffResFolder}/Buff_{statusId}");
            }
            var buffConfig = AbilityConfigCategory.Instance.Get(buffObject.Id);
            var canStack = buffConfig.CanStack == "��";
//#endif
            var statusComp = Target.GetComponent<StatusComponent>();
            if (canStack == false)
            {
                if (statusComp.HasStatus(buffConfig.KeyName))
                {
                    var status = statusComp.GetStatus(buffConfig.KeyName);
                    var lifeComp = status.GetComponent<AbilityLifeTimeComponent>();
                    if (lifeComp != null)
                    {
                        var statusLifeTimer = lifeComp.LifeTimer;
                        statusLifeTimer.MaxTime = AddStatusEffect.Duration;
                        statusLifeTimer.Reset();
                    }
                    FinishAction();
                    return;
                }
            }

            BuffAbility = statusComp.AttachStatus(buffObject);
            BuffAbility.OwnerEntity = Creator;
            BuffAbility.GetComponent<AbilityLevelComponent>().Level = SourceAbility.GetComponent<AbilityLevelComponent>().Level;
            //Log.Debug($"ApplyAddStatus BuffAbility={BuffAbility.Config.KeyName}");
            ProcessInputKVParams(BuffAbility, AddStatusEffect.Params);

            if (AddStatusEffect.Duration > 0)
            {
                BuffAbility.AddComponent<AbilityLifeTimeComponent>(AddStatusEffect.Duration);
            }
            BuffAbility.TryActivateAbility();

            PostProcess();

            FinishAction();
        }

        //���ô���
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ActionPointType.PostGiveStatus, this);
            Target.GetComponent<ActionPointComponent>().TriggerActionPoint(ActionPointType.PostReceiveStatus, this);
        }

        /// ���ﴦ���ܴ���Ĳ�����ֵ�滻
        public void ProcessInputKVParams(Ability ability, Dictionary<string, string> Params)
        {
            foreach (var abilityTrigger in ability.GetComponent<AbilityTriggerComponent>().AbilityTriggers)
            {
                var effect = abilityTrigger.TriggerConfig;

                if (!string.IsNullOrEmpty(effect.ConditionParam))
                {
                    abilityTrigger.ConditionParamValue = ProcessReplaceKV(effect.ConditionParam, Params);
                }
            }

            foreach (var abilityEffect in ability.GetComponent<AbilityEffectComponent>().AbilityEffects)
            {
                var effect = abilityEffect.EffectConfig;

                if (effect is AttributeModifyEffect attributeModify && abilityEffect.TryGet(out EffectAttributeModifyComponent attributeModifyComponent))
                {
                    attributeModifyComponent.ModifyValueFormula = ProcessReplaceKV(attributeModify.NumericValue, Params);
                }
                if (effect is DamageEffect damage && abilityEffect.TryGet(out EffectDamageComponent damageComponent))
                {
                    damageComponent.DamageValueFormula = ProcessReplaceKV(damage.DamageValueFormula, Params);
                }
                if (effect is CureEffect cure && abilityEffect.TryGet(out EffectCureComponent cureComponent))
                {
                    cureComponent.CureValueProperty = ProcessReplaceKV(cure.CureValueFormula, Params);
                }
            }
        }

        private string ProcessReplaceKV(string originValue, Dictionary<string, string> Params)
        {
            foreach (var aInputKVItem in Params)
            {
                if (!string.IsNullOrEmpty(originValue))
                {
                    originValue = originValue.Replace(aInputKVItem.Key, aInputKVItem.Value);
                }
            }
            return originValue;
        }
    }
}