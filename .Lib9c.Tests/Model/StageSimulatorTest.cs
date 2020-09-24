namespace Lib9c.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Lib9c.Tests.Action;
    using Libplanet.Action;
    using Nekoyume.Battle;
    using Nekoyume.Model.BattleStatus;
    using Nekoyume.Model.State;
    using Xunit;

    public class StageSimulatorTest
    {
        private readonly TableSheets _tableSheets;
        private readonly IRandom _random;
        private readonly AvatarState _avatarState;

        public StageSimulatorTest()
        {
            _tableSheets = new TableSheets(TableSheetsImporter.ImportSheets());
            _random = new ItemEnhancementTest.TestRandom();

            _avatarState = new AvatarState(
                default,
                default,
                0,
                _tableSheets.GetAvatarSheets(),
                new GameConfigState(),
                default
            );
        }

        [Fact]
        public void Simulate()
        {
            var simulator = new StageSimulator(
                _random,
                _avatarState,
                new List<Guid>(),
                1,
                3,
                _tableSheets.GetStageSimulatorSheets()
            );
            simulator.Simulate();
            var filtered =
                simulator.Log.Where(e => e.GetType() != typeof(GetReward) || e.GetType() != typeof(DropBox));
            Assert.Equal(typeof(WaveTurnEnd), filtered.Last().GetType());
            Assert.Equal(1, simulator.Log.OfType<WaveTurnEnd>().First().TurnNumber);
        }
    }
}