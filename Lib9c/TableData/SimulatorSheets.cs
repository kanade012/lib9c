using Nekoyume.TableData.Event;

namespace Nekoyume.TableData
{
    public abstract class SimulatorSheets
    {
        public readonly MaterialItemSheet MaterialItemSheet;
        public readonly SkillSheet SkillSheet;
        public readonly SkillBuffSheet SkillBuffSheet;
        public readonly BuffSheet BuffSheet;
        public readonly CharacterSheet CharacterSheet;
        public readonly CharacterLevelSheet CharacterLevelSheet;
        public readonly EquipmentItemSetEffectSheet EquipmentItemSetEffectSheet;

        protected SimulatorSheets(
            MaterialItemSheet materialItemSheet,
            SkillSheet skillSheet,
            SkillBuffSheet skillBuffSheet,
            BuffSheet buffSheet,
            CharacterSheet characterSheet,
            CharacterLevelSheet characterLevelSheet,
            EquipmentItemSetEffectSheet equipmentItemSetEffectSheet
        )
        {
            MaterialItemSheet = materialItemSheet;
            SkillSheet = skillSheet;
            SkillBuffSheet = skillBuffSheet;
            BuffSheet = buffSheet;
            CharacterSheet = characterSheet;
            CharacterLevelSheet = characterLevelSheet;
            EquipmentItemSetEffectSheet = equipmentItemSetEffectSheet;
        }
    }

    public class StageSimulatorSheets : SimulatorSheets
    {
        public readonly StageSheet StageSheet;
        public readonly StageWaveSheet StageWaveSheet;
        public readonly EnemySkillSheet EnemySkillSheet;

        public StageSimulatorSheets(
            MaterialItemSheet materialItemSheet,
            SkillSheet skillSheet,
            SkillBuffSheet skillBuffSheet,
            BuffSheet buffSheet,
            CharacterSheet characterSheet,
            CharacterLevelSheet characterLevelSheet,
            EquipmentItemSetEffectSheet equipmentItemSetEffectSheet,
            StageSheet stageSheet,
            StageWaveSheet stageWaveSheet,
            EnemySkillSheet enemySkillSheet
        ) : base(
            materialItemSheet,
            skillSheet,
            skillBuffSheet,
            buffSheet,
            characterSheet,
            characterLevelSheet,
            equipmentItemSetEffectSheet
        )
        {
            StageSheet = stageSheet;
            StageWaveSheet = stageWaveSheet;
            EnemySkillSheet = enemySkillSheet;
        }
    }

    public class EventDungeonBattleSimulatorSheets : SimulatorSheets
    {
        public readonly EventDungeonStageSheet EventDungeonStageSheet;
        public readonly EventDungeonStageWaveSheet EventDungeonStageWaveSheet;
        public readonly EnemySkillSheet EnemySkillSheet;
        public readonly CostumeStatSheet CostumeStatSheet;

        public EventDungeonBattleSimulatorSheets(
            MaterialItemSheet materialItemSheet,
            SkillSheet skillSheet,
            SkillBuffSheet skillBuffSheet,
            BuffSheet buffSheet,
            CharacterSheet characterSheet,
            CharacterLevelSheet characterLevelSheet,
            EquipmentItemSetEffectSheet equipmentItemSetEffectSheet,
            EventDungeonStageSheet eventDungeonStageSheet,
            EventDungeonStageWaveSheet eventDungeonStageWaveSheet,
            EnemySkillSheet enemySkillSheet,
            CostumeStatSheet costumeStatSheet
        ) : base(
            materialItemSheet,
            skillSheet,
            skillBuffSheet,
            buffSheet,
            characterSheet,
            characterLevelSheet,
            equipmentItemSetEffectSheet
        )
        {
            EventDungeonStageSheet = eventDungeonStageSheet;
            EventDungeonStageWaveSheet = eventDungeonStageWaveSheet;
            EnemySkillSheet = enemySkillSheet;
            CostumeStatSheet = costumeStatSheet;
        }
    }

    public class RankingSimulatorSheets : SimulatorSheets
    {
        public readonly WeeklyArenaRewardSheet WeeklyArenaRewardSheet;

        public RankingSimulatorSheets(
            MaterialItemSheet materialItemSheet,
            SkillSheet skillSheet,
            SkillBuffSheet skillBuffSheet,
            BuffSheet buffSheet,
            CharacterSheet characterSheet,
            CharacterLevelSheet characterLevelSheet,
            EquipmentItemSetEffectSheet equipmentItemSetEffectSheet,
            WeeklyArenaRewardSheet weeklyArenaRewardSheet
        ) : base(
            materialItemSheet,
            skillSheet,
            skillBuffSheet,
            buffSheet,
            characterSheet,
            characterLevelSheet,
            equipmentItemSetEffectSheet
        )
        {
            WeeklyArenaRewardSheet = weeklyArenaRewardSheet;
        }
    }

    public class ArenaSimulatorSheets : SimulatorSheets
    {
        public CostumeStatSheet CostumeStatSheet { get; }
        public WeeklyArenaRewardSheet WeeklyArenaRewardSheet { get; }

        public ArenaSimulatorSheets(
            MaterialItemSheet materialItemSheet,
            SkillSheet skillSheet,
            SkillBuffSheet skillBuffSheet,
            BuffSheet buffSheet,
            CharacterSheet characterSheet,
            CharacterLevelSheet characterLevelSheet,
            EquipmentItemSetEffectSheet equipmentItemSetEffectSheet,
            CostumeStatSheet costumeStatSheet,
            WeeklyArenaRewardSheet weeklyArenaRewardSheet
        ) : base(materialItemSheet,
            skillSheet,
            skillBuffSheet,
            buffSheet,
            characterSheet,
            characterLevelSheet,
            equipmentItemSetEffectSheet)
        {
            CostumeStatSheet = costumeStatSheet;
            WeeklyArenaRewardSheet = weeklyArenaRewardSheet;

        }
    }
}
